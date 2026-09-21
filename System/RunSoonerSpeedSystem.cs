// <copyright file="RunSoonerSpeedSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/RunSoonerSpeedSystem.cs
// Purpose: Boost only active bus/tram/train/subway passengers that are hurrying to board.

namespace BetterBoarding
{
    using Game;
    using Game.Common;
    using Game.Creatures;
    using Game.Simulation;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    public partial class RunSoonerSpeedSystem : GameSystemBase
    {
        // Only boost when vanilla is already allowing full running speed.
        // This avoids overriding braking, queues, blockers, or stop/connection behavior.
        private const float kFullRunThreshold = 0.95f;

        private const CreatureLaneFlags kNoBoostLaneFlags =
            CreatureLaneFlags.EndOfPath |
            CreatureLaneFlags.EndReached |
            CreatureLaneFlags.TransformTarget |
            CreatureLaneFlags.ParkingSpace |
            CreatureLaneFlags.Obsolete |
            CreatureLaneFlags.Connection |
            CreatureLaneFlags.WaitSignal |
            CreatureLaneFlags.FindLane |
            CreatureLaneFlags.Stuck |
            CreatureLaneFlags.Area |
            CreatureLaneFlags.Hangaround |
            CreatureLaneFlags.WaitPosition |
            CreatureLaneFlags.EmergeUnspawned;

        // Temporary low-frequency diagnostics for the 1x-vs-4x Run Sooner test.
        // Counts are job passes/writes, not unique cims.
        private const double kDiagnosticSummaryIntervalSeconds = 60.0;
        private const int kDiagnosticChecked = 0;
        private const int kDiagnosticNotRunning = 1;
        private const int kDiagnosticVehicleState = 2;
        private const int kDiagnosticNoPublicTransport = 3;
        private const int kDiagnosticNotActiveBoarding = 4;
        private const int kDiagnosticUnsupportedTransport = 5;
        private const int kDiagnosticUnsafeLane = 6;
        private const int kDiagnosticBlocked = 7;
        private const int kDiagnosticMissingRunSpeed = 8;
        private const int kDiagnosticTargetActivity = 9;
        private const int kDiagnosticNotFullRunSpeed = 10;
        private const int kDiagnosticBoostedWrites = 11;
        private const int kDiagnosticChangedWrites = 12;
        private const int kDiagnosticAlreadyBoosted = 13;
        private const int kDiagnosticBoostedBus = 14;
        private const int kDiagnosticBoostedTram = 15;
        private const int kDiagnosticBoostedTrain = 16;
        private const int kDiagnosticBoostedSubway = 17;
        private const int kDiagnosticCounterCount = 18;

        private EntityQuery m_PassengerQuery;
        private SimulationSystem? m_SimulationSystem;
        private NativeArray<int> m_DiagnosticCounters;
        private NativeArray<float> m_DiagnosticSpeedSample;
        private double m_DiagnosticLastSummaryRealtime;
        private int m_DiagnosticFactor;
        private bool m_DiagnosticWasEnabled;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();

            m_DiagnosticCounters = new NativeArray<int>(
                kDiagnosticCounterCount,
                Allocator.Persistent,
                NativeArrayOptions.ClearMemory);

            m_DiagnosticSpeedSample = new NativeArray<float>(
                3,
                Allocator.Persistent,
                NativeArrayOptions.ClearMemory);

            m_DiagnosticLastSummaryRealtime =
                UnityEngine.Time.realtimeSinceStartupAsDouble;

            // CurrentVehicle keeps this query much smaller than an all-citizen query.
            // UpdateFrame lets us match vanilla HumanNavigation/HumanMove cadence.
            m_PassengerQuery = SystemAPI.QueryBuilder()
                .WithAll<
                    Human,
                    CurrentVehicle,
                    HumanNavigation,
                    HumanCurrentLane,
                    Blocker,
                    Game.Prefabs.PrefabRef,
                    UpdateFrame>()
                .WithNone<Deleted, Destroyed, Temp, Overridden>()
                .Build();

            RequireForUpdate(m_PassengerQuery);
        }

        protected override void OnDestroy()
        {
            Dependency.Complete();

            if (m_DiagnosticCounters.IsCreated)
            {
                m_DiagnosticCounters.Dispose();
            }

            if (m_DiagnosticSpeedSample.IsCreated)
            {
                m_DiagnosticSpeedSample.Dispose();
            }

            base.OnDestroy();
        }

        private void PrepareDiagnostics(int speedFactor, bool enabled)
        {
            if (m_DiagnosticFactor == speedFactor &&
                m_DiagnosticWasEnabled == enabled)
            {
                return;
            }

            // The scheduled job owns these arrays while running.
            Dependency.Complete();
            ResetDiagnosticCounters();

            m_DiagnosticFactor = speedFactor;
            m_DiagnosticWasEnabled = enabled;
            m_DiagnosticLastSummaryRealtime =
                UnityEngine.Time.realtimeSinceStartupAsDouble;

            if (enabled)
            {
                CS2Shared.RiverMochi.LogUtils.Info(
                    Mod.s_Log,
                    () =>
                        $"{Mod.ModTag} Run Speed Boost diagnostics started: " +
                        $"factor={speedFactor}x, summaryInterval=" +
                        $"{kDiagnosticSummaryIntervalSeconds:F0}s");
            }
        }

        private void TryLogDiagnostics(int speedFactor, bool enabled)
        {
            if (!enabled)
            {
                return;
            }

            double now = UnityEngine.Time.realtimeSinceStartupAsDouble;
            if (now - m_DiagnosticLastSummaryRealtime <
                kDiagnosticSummaryIntervalSeconds)
            {
                return;
            }

            // This happens only once per minute while verbose logging is enabled.
            // Complete first so the job is no longer writing the diagnostic arrays.
            Dependency.Complete();

            int checkedCount = m_DiagnosticCounters[kDiagnosticChecked];
            int notRunning = m_DiagnosticCounters[kDiagnosticNotRunning];
            int vehicleState = m_DiagnosticCounters[kDiagnosticVehicleState];
            int noPublicTransport = m_DiagnosticCounters[kDiagnosticNoPublicTransport];
            int notActiveBoarding = m_DiagnosticCounters[kDiagnosticNotActiveBoarding];
            int unsupportedTransport = m_DiagnosticCounters[kDiagnosticUnsupportedTransport];
            int unsafeLane = m_DiagnosticCounters[kDiagnosticUnsafeLane];
            int blocked = m_DiagnosticCounters[kDiagnosticBlocked];
            int missingRunSpeed = m_DiagnosticCounters[kDiagnosticMissingRunSpeed];
            int targetActivity = m_DiagnosticCounters[kDiagnosticTargetActivity];
            int notFullRunSpeed = m_DiagnosticCounters[kDiagnosticNotFullRunSpeed];
            int boostedWrites = m_DiagnosticCounters[kDiagnosticBoostedWrites];
            int changedWrites = m_DiagnosticCounters[kDiagnosticChangedWrites];
            int alreadyBoosted = m_DiagnosticCounters[kDiagnosticAlreadyBoosted];
            int boostedBus = m_DiagnosticCounters[kDiagnosticBoostedBus];
            int boostedTram = m_DiagnosticCounters[kDiagnosticBoostedTram];
            int boostedTrain = m_DiagnosticCounters[kDiagnosticBoostedTrain];
            int boostedSubway = m_DiagnosticCounters[kDiagnosticBoostedSubway];

            float sampleRunSpeed = m_DiagnosticSpeedSample[0];
            float sampleMaxBefore = m_DiagnosticSpeedSample[1];
            float sampleMaxAfter = m_DiagnosticSpeedSample[2];

            CS2Shared.RiverMochi.LogUtils.Info(
                Mod.s_Log,
                () =>
                    $"{Mod.ModTag} Run Speed Boost: factor={speedFactor}x | " +
                    $"checked={checkedCount}, boostedWrites={boostedWrites}, " +
                    $"changedWrites={changedWrites}, alreadyAtBoost={alreadyBoosted} | " +
                    $"rejected[notRunning={notRunning}, vehicleState={vehicleState}, " +
                    $"noPublicTransport={noPublicTransport}, " +
                    $"notActiveBoarding={notActiveBoarding}, " +
                    $"unsupportedTransport={unsupportedTransport}, " +
                    $"unsafeLaneOrQueue={unsafeLane}, blocked={blocked}, " +
                    $"missingRunSpeed={missingRunSpeed}, targetActivity={targetActivity}, " +
                    $"notFullRunSpeed={notFullRunSpeed}] | " +
                    $"boostedByMode[bus={boostedBus}, tram={boostedTram}, " +
                    $"train={boostedTrain}, subway={boostedSubway}] | " +
                    $"sample[vanillaRun={sampleRunSpeed:F3}m/s, " +
                    $"navMaxBefore={sampleMaxBefore:F3}m/s, " +
                    $"navMaxAfter={sampleMaxAfter:F3}m/s]");

            ResetDiagnosticCounters();
            m_DiagnosticLastSummaryRealtime = now;
        }

        private void ResetDiagnosticCounters()
        {
            for (int i = 0; i < m_DiagnosticCounters.Length; i++)
            {
                m_DiagnosticCounters[i] = 0;
            }

            for (int i = 0; i < m_DiagnosticSpeedSample.Length; i++)
            {
                m_DiagnosticSpeedSample[i] = 0f;
            }
        }

        protected override void OnUpdate()
        {
            if (!BoardingRuntimeSettings.RunSoonerSpeedBoostEnabled)
            {
                Enabled = false;
                return;
            }

            int speedFactor = BoardingRuntimeSettings.PassengerRunSpeedFactor;
            bool collectDiagnostics =
                BoardingRuntimeSettings.EnableVerboseLogging;

            PrepareDiagnostics(speedFactor, collectDiagnostics);
            TryLogDiagnostics(speedFactor, collectDiagnostics);

            uint updateFrame = (m_SimulationSystem?.frameIndex ?? 0) % 16;

            // Vanilla HumanNavigationSystem and HumanMoveSystem use this same rotating
            // 16-way filter, so only the passengers being moved this frame are touched.
            m_PassengerQuery.ResetFilter();
            m_PassengerQuery.SetSharedComponentFilter(new UpdateFrame(updateFrame));

            JobHandle handle = new RunSoonerSpeedJob
            {
                m_HumanType =
                    SystemAPI.GetComponentTypeHandle<Human>(isReadOnly: true),

                m_CurrentVehicleType =
                    SystemAPI.GetComponentTypeHandle<CurrentVehicle>(isReadOnly: true),

                m_NavigationType =
                    SystemAPI.GetComponentTypeHandle<HumanNavigation>(isReadOnly: false),

                m_CurrentLaneType =
                    SystemAPI.GetComponentTypeHandle<HumanCurrentLane>(isReadOnly: true),

                m_BlockerType =
                    SystemAPI.GetComponentTypeHandle<Blocker>(isReadOnly: true),

                m_PrefabRefType =
                    SystemAPI.GetComponentTypeHandle<Game.Prefabs.PrefabRef>(isReadOnly: true),

                m_ControllerData =
                    SystemAPI.GetComponentLookup<Controller>(isReadOnly: true),

                m_PublicTransportData =
                    SystemAPI.GetComponentLookup<Game.Vehicles.PublicTransport>(isReadOnly: true),

                m_PrefabRefData =
                    SystemAPI.GetComponentLookup<Game.Prefabs.PrefabRef>(isReadOnly: true),

                m_PublicTransportVehicleData =
                    SystemAPI.GetComponentLookup<Game.Prefabs.PublicTransportVehicleData>(isReadOnly: true),

                m_PrefabHumanData =
                    SystemAPI.GetComponentLookup<Game.Prefabs.HumanData>(isReadOnly: true),

                m_SpeedFactor = speedFactor,
                m_CollectDiagnostics = collectDiagnostics,
                m_DiagnosticCounters = m_DiagnosticCounters,
                m_DiagnosticSpeedSample = m_DiagnosticSpeedSample,
            }.Schedule(m_PassengerQuery, Dependency);

            Dependency = handle;
        }

        [BurstCompile]
        private struct RunSoonerSpeedJob : IJobChunk
        {
            [ReadOnly]
            public ComponentTypeHandle<Human> m_HumanType;

            [ReadOnly]
            public ComponentTypeHandle<CurrentVehicle> m_CurrentVehicleType;

            public ComponentTypeHandle<HumanNavigation> m_NavigationType;

            [ReadOnly]
            public ComponentTypeHandle<HumanCurrentLane> m_CurrentLaneType;

            [ReadOnly]
            public ComponentTypeHandle<Blocker> m_BlockerType;

            [ReadOnly]
            public ComponentTypeHandle<Game.Prefabs.PrefabRef> m_PrefabRefType;

            [ReadOnly]
            public ComponentLookup<Controller> m_ControllerData;

            [ReadOnly]
            public ComponentLookup<Game.Vehicles.PublicTransport> m_PublicTransportData;

            [ReadOnly]
            public ComponentLookup<Game.Prefabs.PrefabRef> m_PrefabRefData;

            [ReadOnly]
            public ComponentLookup<Game.Prefabs.PublicTransportVehicleData> m_PublicTransportVehicleData;

            [ReadOnly]
            public ComponentLookup<Game.Prefabs.HumanData> m_PrefabHumanData;

            [ReadOnly]
            public int m_SpeedFactor;

            [ReadOnly]
            public bool m_CollectDiagnostics;

            public NativeArray<int> m_DiagnosticCounters;

            public NativeArray<float> m_DiagnosticSpeedSample;

            public void Execute(
                in ArchetypeChunk chunk,
                int unfilteredChunkIndex,
                bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                _ = unfilteredChunkIndex;
                _ = useEnabledMask;
                _ = chunkEnabledMask;

                NativeArray<Human> humans =
                    chunk.GetNativeArray(ref m_HumanType);

                NativeArray<CurrentVehicle> currentVehicles =
                    chunk.GetNativeArray(ref m_CurrentVehicleType);

                NativeArray<HumanNavigation> navigations =
                    chunk.GetNativeArray(ref m_NavigationType);

                NativeArray<HumanCurrentLane> currentLanes =
                    chunk.GetNativeArray(ref m_CurrentLaneType);

                NativeArray<Blocker> blockers =
                    chunk.GetNativeArray(ref m_BlockerType);

                NativeArray<Game.Prefabs.PrefabRef> prefabRefs =
                    chunk.GetNativeArray(ref m_PrefabRefType);

                bool collectDiagnostics = m_CollectDiagnostics;

                for (int i = 0; i < chunk.Count; i++)
                {
                    if (collectDiagnostics)
                    {
                        m_DiagnosticCounters[kDiagnosticChecked]++;
                    }

                    Human human = humans[i];

                    // BetterBoarding starts this vanilla Run state sooner.
                    if ((human.m_Flags & HumanFlags.Run) == 0)
                    {
                        if (collectDiagnostics)
                        {
                            m_DiagnosticCounters[kDiagnosticNotRunning]++;
                        }

                        continue;
                    }

                    CurrentVehicle currentVehicle = currentVehicles[i];

                    // Once the cim starts entering, is ready, or is exiting,
                    // movement is back entirely under vanilla control.
                    if (currentVehicle.m_Vehicle == Entity.Null ||
                        (currentVehicle.m_Flags &
                            (CreatureVehicleFlags.Ready |
                             CreatureVehicleFlags.Entering |
                             CreatureVehicleFlags.Exiting)) != 0)
                    {
                        if (collectDiagnostics)
                        {
                            m_DiagnosticCounters[kDiagnosticVehicleState]++;
                        }

                        continue;
                    }

                    Entity assignedVehicle = currentVehicle.m_Vehicle;
                    Entity controllerVehicle = assignedVehicle;

                    // Multi-car trains/trams store CurrentVehicle against an individual
                    // car, while boarding state belongs to the controller vehicle.
                    if (m_ControllerData.TryGetComponent(
                            assignedVehicle,
                            out Controller controller) &&
                        controller.m_Controller != Entity.Null)
                    {
                        controllerVehicle = controller.m_Controller;
                    }

                    if (!m_PublicTransportData.TryGetComponent(
                            controllerVehicle,
                            out Game.Vehicles.PublicTransport publicTransport))
                    {
                        if (collectDiagnostics)
                        {
                            m_DiagnosticCounters[kDiagnosticNoPublicTransport]++;
                        }

                        continue;
                    }

                    if ((publicTransport.m_State & PublicTransportFlags.Boarding) == 0 ||
                        (publicTransport.m_State &
                            (PublicTransportFlags.Evacuating |
                             PublicTransportFlags.PrisonerTransport |
                             PublicTransportFlags.Refueling)) != 0)
                    {
                        if (collectDiagnostics)
                        {
                            m_DiagnosticCounters[kDiagnosticNotActiveBoarding]++;
                        }

                        continue;
                    }

                    if (!TryGetTransportType(
                            controllerVehicle,
                            assignedVehicle,
                            out Game.Prefabs.TransportType transportType) ||
                        (transportType != Game.Prefabs.TransportType.Bus &&
                         transportType != Game.Prefabs.TransportType.Tram &&
                         transportType != Game.Prefabs.TransportType.Train &&
                         transportType != Game.Prefabs.TransportType.Subway))
                    {
                        if (collectDiagnostics)
                        {
                            m_DiagnosticCounters[kDiagnosticUnsupportedTransport]++;
                        }

                        continue;
                    }

                    HumanCurrentLane currentLane = currentLanes[i];

                    // Do not override vanilla's special lane/stop/queue behavior.
                    if (currentLane.m_Lane == Entity.Null ||
                        (currentLane.m_Flags & kNoBoostLaneFlags) != 0 ||
                        currentLane.m_QueueEntity != Entity.Null ||
                        currentLane.m_QueueArea.radius > 0f)
                    {
                        if (collectDiagnostics)
                        {
                            m_DiagnosticCounters[kDiagnosticUnsafeLane]++;
                        }

                        continue;
                    }

                    Blocker blocker = blockers[i];
                    if (blocker.m_Blocker != Entity.Null)
                    {
                        if (collectDiagnostics)
                        {
                            m_DiagnosticCounters[kDiagnosticBlocked]++;
                        }

                        continue;
                    }

                    Game.Prefabs.PrefabRef prefabRef = prefabRefs[i];
                    if (prefabRef.m_Prefab == Entity.Null ||
                        !m_PrefabHumanData.TryGetComponent(
                            prefabRef.m_Prefab,
                            out Game.Prefabs.HumanData humanData) ||
                        humanData.m_RunSpeed <= 0f)
                    {
                        if (collectDiagnostics)
                        {
                            m_DiagnosticCounters[kDiagnosticMissingRunSpeed]++;
                        }

                        continue;
                    }

                    HumanNavigation navigation = navigations[i];

                    if (navigation.m_TargetActivity != 0)
                    {
                        if (collectDiagnostics)
                        {
                            m_DiagnosticCounters[kDiagnosticTargetActivity]++;
                        }

                        continue;
                    }

                    // Vanilla has already done its collision, queue and braking work.
                    // Only boost when it still permits essentially full running speed.
                    if (navigation.m_MaxSpeed <
                        humanData.m_RunSpeed * kFullRunThreshold)
                    {
                        if (collectDiagnostics)
                        {
                            m_DiagnosticCounters[kDiagnosticNotFullRunSpeed]++;
                        }

                        continue;
                    }

                    float maxSpeedBefore = navigation.m_MaxSpeed;
                    float boostedMaxSpeed =
                        humanData.m_RunSpeed * m_SpeedFactor;

                    navigation.m_MaxSpeed = boostedMaxSpeed;
                    navigations[i] = navigation;

                    if (collectDiagnostics)
                    {
                        m_DiagnosticCounters[kDiagnosticBoostedWrites]++;

                        if (maxSpeedBefore < boostedMaxSpeed - 0.01f ||
                            maxSpeedBefore > boostedMaxSpeed + 0.01f)
                        {
                            m_DiagnosticCounters[kDiagnosticChangedWrites]++;
                        }
                        else
                        {
                            m_DiagnosticCounters[kDiagnosticAlreadyBoosted]++;
                        }

                        switch (transportType)
                        {
                            case Game.Prefabs.TransportType.Bus:
                                m_DiagnosticCounters[kDiagnosticBoostedBus]++;
                                break;

                            case Game.Prefabs.TransportType.Tram:
                                m_DiagnosticCounters[kDiagnosticBoostedTram]++;
                                break;

                            case Game.Prefabs.TransportType.Train:
                                m_DiagnosticCounters[kDiagnosticBoostedTrain]++;
                                break;

                            case Game.Prefabs.TransportType.Subway:
                                m_DiagnosticCounters[kDiagnosticBoostedSubway]++;
                                break;
                        }

                        // Keep one recent proof sample showing the actual write.
                        m_DiagnosticSpeedSample[0] = humanData.m_RunSpeed;
                        m_DiagnosticSpeedSample[1] = maxSpeedBefore;
                        m_DiagnosticSpeedSample[2] = boostedMaxSpeed;
                    }
                }
            }

            private bool TryGetTransportType(
                Entity controllerVehicle,
                Entity assignedVehicle,
                out Game.Prefabs.TransportType transportType)
            {
                if (TryGetTransportType(controllerVehicle, out transportType))
                {
                    return true;
                }

                return TryGetTransportType(assignedVehicle, out transportType);
            }

            private bool TryGetTransportType(
                Entity vehicle,
                out Game.Prefabs.TransportType transportType)
            {
                transportType = Game.Prefabs.TransportType.None;

                if (!m_PrefabRefData.TryGetComponent(
                        vehicle,
                        out Game.Prefabs.PrefabRef prefabRef) ||
                    prefabRef.m_Prefab == Entity.Null ||
                    !m_PublicTransportVehicleData.TryGetComponent(
                        prefabRef.m_Prefab,
                        out Game.Prefabs.PublicTransportVehicleData transportData))
                {
                    return false;
                }

                transportType = transportData.m_TransportType;
                return true;
            }
        }
    }
}
