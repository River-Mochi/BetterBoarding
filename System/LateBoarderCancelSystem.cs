// <copyright file="LateBoarderCancelSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/LateBoarderCancelSystem.cs
// Purpose: Main update loop and boarding-assist pass for late solo passengers.

namespace BoardingNow
{
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Creatures;
    using Game.Pathfind;
    using Game.SceneFlow;
    using Game.Simulation;
    using Game.Tools;
    using Game.Vehicles;
    using System;
    using System.Collections.Generic;
    using Unity.Collections;
    using Unity.Entities;
    using TransportType = Game.Prefabs.TransportType;

    public partial class LateBoarderCancelSystem : GameSystemBase
    {
        // High-level flow:
        // 1. Scan vehicles while vanilla still has them in the boarding state.
        // 2. Optionally set vanilla's Run flag shortly before bus/tram departure so assigned cims hurry sooner.
        // 3. After departure, optionally detach safe solo late passengers whose remaining path still contains that exact vehicle.
        // 4. Optionally record delayed "what happened next?" samples for verbose diagnostics.
        // 2048/day means every 128 simulation frames (~42 game seconds); cancellation work is also capped below.
        public const int UpdatesPerDay = 2048;

        // 128 keeps the old daily throughput ceiling after halving UpdatesPerDay from 4096 to 2048.
        // Higher caps process large crowds faster but can create larger one-frame edits.
        private const int MaxCancellationsPerUpdate = 128;

        // Wait one boarding-assist interval after vanilla departure before trimming paths.
        // This gives vanilla and other mods one extra assist interval to settle same-frame boarding edits.
        private const uint LatePassengerCancelGraceFrames = 128u;

        private EntityQuery m_VehicleQuery;
        private EntityQuery m_CurrentVehicleQuery;
        private EntityQuery m_HumanResidentQuery;
        private EntityQuery m_PassengerBufferQuery;
        private EntityQuery m_PathBufferQuery;
        private SimulationSystem? m_SimulationSystem;
        private ToolSystem? m_ToolSystem;
        private DefaultToolSystem? m_DefaultToolSystem;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            // CS2 uses 262144 simulation frames per in-game day.
            return 262144 / UpdatesPerDay;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_DefaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();

            // Passenger is not required because some multi-car layouts store passengers on child vehicles.
            m_VehicleQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Vehicles.PublicTransport>()
                .WithNone<Deleted, Destroyed, Temp, Overridden>()
                .Build();

            // These queries are only used to complete dependencies for data this system edits directly.
            m_CurrentVehicleQuery = SystemAPI.QueryBuilder()
                .WithAll<CurrentVehicle>()
                .Build();
            m_HumanResidentQuery = SystemAPI.QueryBuilder()
                .WithAll<Human, Game.Creatures.Resident>()
                .Build();
            m_PassengerBufferQuery = SystemAPI.QueryBuilder()
                .WithAll<Passenger>()
                .Build();
            m_PathBufferQuery = SystemAPI.QueryBuilder()
                .WithAll<PathOwner, PathElement>()
                .Build();

            RequireForUpdate(m_VehicleQuery);
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (!IsRealGameLoad(purpose, mode))
            {
                return;
            }

            // Saved checkboxes do not fire Options UI setters during load, so restore live system state here.
            ResetDiagnosticsForCityLoad();
            Enabled = BoardingRuntimeSettings.BoardingAssistEnabled;
        }

        protected override void OnUpdate()
        {
            try
            {
                if (!BoardingRuntimeSettings.BoardingAssistEnabled)
                {
                    // Options UI setter wakes this system only when a boarding assist is enabled.
                    Enabled = false;
                    return;
                }

                LogActiveOnce();

                if (IsPlayerUsingTool())
                {
                    // Avoid passenger-buffer edits while tools may delete or rebuild entities.
                    m_SkippedForTool++;
                    uint frame = m_SimulationSystem?.frameIndex ?? 0;
                    LogPassSummary(frame, new PassStats(0, 0, 0, 0, 0), "paused-tool");
                    LogFollowUps(frame);
                    return;
                }

                // One pass handles both behavior toggles, then older samples are checked separately.
                PassStats stats = RunCancellationPass();
                uint currentFrame = m_SimulationSystem?.frameIndex ?? 0;
                LogPassSummary(currentFrame, stats, "pass");
                LogFollowUps(currentFrame);
            }
            catch (Exception ex)
            {
                Enabled = false;
                BoardingRuntimeSettings.SetCancelLateBoarders(false);
                BoardingRuntimeSettings.SetCimsRunSoonerToCatchBuses(false);

                Mod.WarnOnce(
                    "FB_LATE_BOARDER_CANCEL_EXCEPTION",
                    () => $"{Mod.ModTag} Boarding assist disabled after {ex.GetType().Name}: {ex.Message}");
            }
        }

        private PassStats RunCancellationPass()
        {
            NativeArray<Entity> vehicles = default;
            EntityCommandBuffer ecb = default;
            bool hasCommandBuffer = false;
            int cancellationsThisUpdate = 0;
            int runSoonerAssists = 0;
            int vehiclesScanned = 0;
            int passengersScanned = 0;
            int candidates = 0;
            int busCanceled = 0;
            int trainCanceled = 0;
            int tramCanceled = 0;
            int subwayCanceled = 0;
            int shipCanceled = 0;
            int ferryCanceled = 0;
            int airCanceled = 0;
            int busRunSoonerAssists = 0;
            int tramRunSoonerAssists = 0;
            int sampledRunSoonerPassengerCount = 0;
            int busSampleCount = 0;
            int trainSampleCount = 0;
            int tramSampleCount = 0;
            int subwaySampleCount = 0;
            int shipSampleCount = 0;
            int ferrySampleCount = 0;
            int airSampleCount = 0;
            CanceledPassengerSample[]? sampledCanceledPassengers = null;
            int sampledCanceledPassengerCount = 0;
            uint frame = m_SimulationSystem?.frameIndex ?? 0;
            bool cancelLateBoarders = BoardingRuntimeSettings.CancelLateBoarders;
            bool cimsRunSoonerToCatchBuses = BoardingRuntimeSettings.CimsRunSoonerToCatchBuses;
            var pendingCancellation = new HashSet<Entity>();
            var canceledPassengers = new HashSet<Entity>();

            try
            {
                // This pass reads vehicle/passenger data directly on the main thread.
                // Safety: wait for earlier ECS jobs touching the same vehicle/passenger/path data first.
                CompleteBoardingAssistDependencies();
                vehicles = m_VehicleQuery.ToEntityArray(Allocator.Temp);
                ecb = new EntityCommandBuffer(Allocator.Temp);
                hasCommandBuffer = true;

                foreach (var vehicleEntity in vehicles)
                {
                    vehiclesScanned++;

                    if (!EntityManager.Exists(vehicleEntity) ||
                        EntityManager.HasComponent<Destroyed>(vehicleEntity) ||
                        EntityManager.HasComponent<Overridden>(vehicleEntity))
                    {
                        continue;
                    }

                    var publicTransport = EntityManager.GetComponentData<Game.Vehicles.PublicTransport>(vehicleEntity);
                    if ((publicTransport.m_State & PublicTransportFlags.Boarding) == 0)
                    {
                        // Only intervene while vanilla says the vehicle is in a boarding state.
                        continue;
                    }

                    if ((publicTransport.m_State & (PublicTransportFlags.Evacuating | PublicTransportFlags.PrisonerTransport)) != 0)
                    {
                        // These flags are special gameplay flows, not normal public boarding.
                        continue;
                    }

                    TransportType transportType = GetTransportType(vehicleEntity);
                    uint latestDepartureFrame = GetLatestDepartureFrame(vehicleEntity, publicTransport);

                    if (cimsRunSoonerToCatchBuses)
                    {
                        int queuedRunSooner = QueueRoadTransitPassengersRunSooner(
                            ref ecb,
                            vehicleEntity,
                            transportType,
                            publicTransport,
                            frame,
                            latestDepartureFrame,
                            ref sampledRunSoonerPassengerCount);
                        runSoonerAssists += queuedRunSooner;

                        if (transportType == TransportType.Bus)
                        {
                            busRunSoonerAssists += queuedRunSooner;
                        }
                        else if (transportType == TransportType.Tram)
                        {
                            tramRunSoonerAssists += queuedRunSooner;
                        }
                    }

                    if (m_SimulationSystem == null ||
                        !IsPastLateCancelGraceFrame(latestDepartureFrame, frame))
                    {
                        // Late-passenger cancellation waits a short grace after vanilla departure.
                        continue;
                    }

                    bool hasPassengerBuffer = EntityManager.HasBuffer<Passenger>(vehicleEntity);
                    DynamicBuffer<Passenger> passengers = hasPassengerBuffer
                        ? EntityManager.GetBuffer<Passenger>(vehicleEntity)
                        : default;

                    if (!hasPassengerBuffer || passengers.Length == 0)
                    {
                        continue;
                    }

                    // Collect first, then mutate afterward, so the passenger buffer is not edited while scanned.
                    // Reused managed sets avoid per-vehicle GC while staying simple for this main-thread pass.
                    pendingCancellation.Clear();

                    for (var i = 0; i < passengers.Length; i++)
                    {
                        passengersScanned++;
                        var passenger = passengers[i].m_Passenger;
                        if (!EntityManager.Exists(passenger) ||
                            EntityManager.HasComponent<Deleted>(passenger) ||
                            EntityManager.HasComponent<Destroyed>(passenger) ||
                            EntityManager.HasComponent<Temp>(passenger) ||
                            EntityManager.HasComponent<Overridden>(passenger) ||
                            !EntityManager.HasComponent<CurrentVehicle>(passenger))
                        {
                            continue;
                        }

                        var currentVehicle = EntityManager.GetComponentData<CurrentVehicle>(passenger);
                        if (currentVehicle.m_Vehicle != vehicleEntity)
                        {
                            continue;
                        }

                        if ((currentVehicle.m_Flags & CreatureVehicleFlags.Ready) != 0)
                        {
                            continue;
                        }

                        if (IsGroupPassenger(passenger))
                        {
                            continue;
                        }

                        if (!cancelLateBoarders)
                        {
                            continue;
                        }

                        if (CanSafelyCancelPassenger(vehicleEntity, passenger))
                        {
                            pendingCancellation.Add(passenger);
                            candidates++;
                        }
                    }

                    int canceledForVehicle = 0;
                    canceledPassengers.Clear();
                    foreach (var passenger in pendingCancellation)
                    {
                        if (cancellationsThisUpdate >= MaxCancellationsPerUpdate)
                        {
                            // Spread very large crowds over multiple ticks instead of spiking one frame.
                            break;
                        }

                        if (QueuePassengerCancellation(ref ecb, vehicleEntity, passenger))
                        {
                            canceledPassengers.Add(passenger);
                            cancellationsThisUpdate++;
                            canceledForVehicle++;

                            if (sampledCanceledPassengerCount < MaxSampledCimsPerUpdate &&
                                TryCountCanceledSample(
                                    transportType,
                                    ref busSampleCount,
                                    ref trainSampleCount,
                                    ref tramSampleCount,
                                    ref subwaySampleCount,
                                    ref shipSampleCount,
                                    ref ferrySampleCount,
                                    ref airSampleCount))
                            {
                                // Keep a few entity IDs per mode for the on-demand troubleshooting report.
                                sampledCanceledPassengers ??= new CanceledPassengerSample[MaxSampledCimsPerUpdate];
                                sampledCanceledPassengers[sampledCanceledPassengerCount++] =
                                    new CanceledPassengerSample(transportType, vehicleEntity, passenger);
                            }
                        }
                    }

                    if (canceledForVehicle > 0)
                    {
                        QueueVehiclePassengerBuffer(ref ecb, vehicleEntity, passengers, canceledPassengers);
                        AccumulateCanceledCount(
                            transportType,
                            canceledForVehicle,
                            ref busCanceled,
                            ref trainCanceled,
                            ref tramCanceled,
                            ref subwayCanceled,
                            ref shipCanceled,
                            ref ferryCanceled,
                            ref airCanceled);
                    }

                    if (cancellationsThisUpdate >= MaxCancellationsPerUpdate)
                    {
                        break;
                    }
                }

                // Mutate after scanning so buffers/components are not edited while enumerated.
                ecb.Playback(EntityManager);

                // Update UI counters only after all queued ECS edits have succeeded.
                RecordCanceledCounts(
                    busCanceled,
                    trainCanceled,
                    tramCanceled,
                    subwayCanceled,
                    shipCanceled,
                    ferryCanceled,
                    airCanceled);
                TransitWaitStatus.RecordRunSoonerAssists(World, TransportType.Bus, busRunSoonerAssists);
                TransitWaitStatus.RecordRunSoonerAssists(World, TransportType.Tram, tramRunSoonerAssists);

                if (sampledCanceledPassengers != null)
                {
                    for (int i = 0; i < sampledCanceledPassengerCount; i++)
                    {
                        CanceledPassengerSample sample = sampledCanceledPassengers[i];
                        TransitWaitStatus.RecordLateBoarderSample(
                            World,
                            sample.TransportType,
                            sample.Vehicle,
                            sample.Passenger);

                        if (ShouldLogDiagnostics())
                        {
                            TrackFollowUpSample(sample);
                        }
                    }
                }

                return new PassStats(vehiclesScanned, passengersScanned, candidates, cancellationsThisUpdate, runSoonerAssists);
            }
            finally
            {
                if (hasCommandBuffer)
                {
                    ecb.Dispose();
                }

                if (vehicles.IsCreated)
                {
                    vehicles.Dispose();
                }
            }
        }

        private void CompleteBoardingAssistDependencies()
        {
            m_VehicleQuery.CompleteDependency();
            m_CurrentVehicleQuery.CompleteDependency();
            m_HumanResidentQuery.CompleteDependency();
            m_PassengerBufferQuery.CompleteDependency();
            m_PathBufferQuery.CompleteDependency();
        }

        private static bool IsRealGameLoad(Purpose purpose, GameMode mode)
        {
            return mode == GameMode.Game &&
                (purpose == Purpose.NewGame || purpose == Purpose.LoadGame);
        }
    }
}
