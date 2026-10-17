// <copyright file="LateGroupBoardingSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/LateGroupBoardingSystem.cs
// Purpose: Main update loop for late family/group boarding assistance.

namespace BetterBoarding
{
    using System;
    using System.Collections.Generic;
    using CS2Shared.RiverMochi;
    using Game;
    using Game.Common;
    using Game.Creatures;
    using Game.Prefabs;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;

    /// <summary>
    /// Handles the group/family cases we exclude from the normal late-passenger pass.
    ///
    /// If leader is still outside, the leader is safely detached and vanilla cancels the
    /// followers after seeing their leader no longer has CurrentVehicle. If the leader
    /// has boarded, walking members are prompted to enter and assigned members receive vanilla's
    /// existing boarding-timeout nudge. ResidentAISystem or PetAISystem still performs every
    /// actual enter/finish transition and its bookkeeping.
    /// </summary>
    public partial class LateGroupBoardingSystem : GameSystemBase
    {
        public const int UpdatesPerDay = 2048;

        // Give groups one extra boarding-assist interval beyond the solo grace.
        // Run Sooner already begins up to 512 frames before departure for human members.
        private const uint kLateGroupGraceFrames = 256u;

        // Vanilla ResidentAISystem and PetAISystem both use 250 as the group-boarding fallback.
        private const int kVanillaBoardingTimeout = 250;

        // Keep work bounded in very large cities and crowded interchange stations.
        private const int kMaxGroupsPerUpdate = 32;

        private EntityQuery m_VehicleQuery;
        private readonly List<float3> m_CachedCarPositions = new List<float3>(8);

        private Game.Simulation.SimulationSystem? m_SimulationSystem;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 262144 / UpdatesPerDay;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            m_SimulationSystem =
                World.GetOrCreateSystemManaged<Game.Simulation.SimulationSystem>();

            // Start from public-transport controllers, then inspect only their passenger buffers.
            m_VehicleQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Vehicles.PublicTransport>()
                .WithNone<Deleted, Destroyed, Temp, Overridden>()
                .Build();

            RequireForUpdate(m_VehicleQuery);
        }

        protected override void OnUpdate()
        {
            if (!BoardingRuntimeSettings.CancelLateBoarders ||
                m_SimulationSystem == null)
            {
                return;
            }

            EntityCommandBuffer ecb = default;
            bool hasCommandBuffer = false;
#if DEBUG
            bool playedCommandBuffer = false;
#endif
            int controllersScanned = 0;
            int candidateCount = 0;
            int groupsReleased = 0;
            int groupsAssisted = 0;
            int membersPrompted = 0;
#if DEBUG
            long performanceSampleStarted = BoardingPerformanceStats.BeginSample();
#endif

            try
            {
                uint frame = m_SimulationSystem.frameIndex;

                // Burst-filter the broad public-transport query first. Keep only root
                // controllers that are boarding and already past the group grace period.
                int sourceControllerCount = m_VehicleQuery.CalculateEntityCount();

                using NativeList<Entity> controllers =
                    new NativeList<Entity>(
                        sourceControllerCount,
                        Allocator.TempJob);

                Dependency = new BoardingVehicleFilterJob
                {
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_PublicTransportType =
                        SystemAPI.GetComponentTypeHandle<Game.Vehicles.PublicTransport>(
                            isReadOnly: true),
                    m_ControllerType =
                        SystemAPI.GetComponentTypeHandle<Controller>(
                            isReadOnly: true),
                    m_CargoTransportType =
                        SystemAPI.GetComponentTypeHandle<Game.Vehicles.CargoTransport>(
                            isReadOnly: true),
                    m_Results = controllers.AsParallelWriter(),
                    m_ExcludedStates =
                        PublicTransportFlags.Evacuating |
                        PublicTransportFlags.PrisonerTransport |
                        PublicTransportFlags.Refueling,
                    m_SkipChildControllers = true,
                    m_RequirePastDeparture = true,
                    m_Frame = frame,
                    m_MinimumPastDepartureFrames = kLateGroupGraceFrames,
                }.ScheduleParallel(m_VehicleQuery, Dependency);

                Dependency.Complete();

                using NativeList<GroupCandidate> candidates =
                    new NativeList<GroupCandidate>(kMaxGroupsPerUpdate, Allocator.Temp);
                using NativeList<Entity> releasedLeaders =
                    new NativeList<Entity>(kMaxGroupsPerUpdate, Allocator.Temp);
                using NativeList<Entity> releasedVehicles =
                    new NativeList<Entity>(kMaxGroupsPerUpdate, Allocator.Temp);

                controllersScanned = controllers.Length;

                for (int i = 0;
                    i < controllers.Length && candidates.Length < kMaxGroupsPerUpdate;
                    i++)
                {
                    Entity controllerVehicle = controllers[i];

                  if (!IsLateBoardingController(controllerVehicle, frame))
                    {
                        continue;
                    }

                    CollectLateGroupCandidates(controllerVehicle, candidates);
                }

                candidateCount = candidates.Length;

                Entity cachedController = Entity.Null;
                TransportType cachedMode = TransportType.None;
                bool stopContextValid = false;
                LatePassengerProximityPolicy.StopContext stopContext = default;
                bool carsCached = false;
                bool carsValid = false;
                List<float3> carPositions = m_CachedCarPositions;

                for (int i = 0; i < candidates.Length; i++)
                {
                    GroupCandidate candidate = candidates[i];

                    if (!IsValidGroup(
                            candidate.Leader,
                            candidate.AssignedVehicle,
                            candidate.ControllerVehicle))
                    {
                        continue;
                    }

                    // A human leader with a lane has not physically completed boarding.
                    // Cancel only the leader; vanilla then cancels each follower through its
                    // existing missing-leader path.
                    if (EntityManager.HasComponent<HumanCurrentLane>(candidate.Leader))
                    {
                        if (candidate.ControllerVehicle != cachedController)
                        {
                            cachedController = candidate.ControllerVehicle;
                            cachedMode = LatePassengerProximityPolicy.GetTransportType(
                                EntityManager,
                                cachedController);
                            bool isRail = cachedMode == TransportType.Train ||
                                cachedMode == TransportType.Subway;
                            bool isBusOrTram = cachedMode == TransportType.Bus ||
                                cachedMode == TransportType.Tram;
                            stopContextValid = (isRail || isBusOrTram) &&
                                LatePassengerProximityPolicy.TryCreateStopContext(
                                    EntityManager,
                                    cachedController,
                                    requireStationOwner: isRail,
                                    out stopContext);
                            carPositions.Clear();
                            carsCached = false;
                            carsValid = false;
                        }

                        bool rail = cachedMode == TransportType.Train ||
                            cachedMode == TransportType.Subway;
                        if (!stopContextValid ||
                            !LatePassengerProximityPolicy.IsClearlyFarWithCachedStop(
                                EntityManager,
                                candidate.ControllerVehicle,
                                candidate.Leader,
                                stopContext,
                                rail,
                                carPositions,
                                ref carsCached,
                                ref carsValid))
                        {
                            // Keep nearby and unmeasurable leaders assigned. Sea, air and
                            // unknown modes have no verified proximity rule yet.
                            continue;
                        }

                        if (ContainsEntity(releasedLeaders, candidate.Leader))
                        {
                            continue;
                        }

                        if (TryQueueOutsideGroupLeaderCancellation(
                                ref ecb,
                                ref hasCommandBuffer,
                                candidate.Leader,
                                candidate.AssignedVehicle))
                        {
                            releasedLeaders.Add(candidate.Leader);
                            AddUniqueEntity(releasedVehicles, candidate.AssignedVehicle);
                            groupsReleased++;
                        }

                        continue;
                    }

                    // A boarded leader has no current lane. Prompt walking members to join, and
                    // raise vanilla's own timeout for members already assigned to the vehicle.
                    // Vanilla retains every enter/finish transition and its bookkeeping.
                    if (IsBoardedHuman(candidate.Leader))
                    {
                        DynamicBuffer<GroupCreature> group =
                            EntityManager.GetBuffer<GroupCreature>(
                                candidate.Leader,
                                isReadOnly: true);

                        if (TryAssistLateGroup(
                                candidate.Leader,
                                candidate.ControllerVehicle,
                                group,
                                out int promptedMembers))
                        {
                            groupsAssisted++;
                            membersPrompted += promptedMembers;
                        }
                    }
                }

                if (hasCommandBuffer)
                {
                    QueueReleasedLeaderPassengerBuffers(
                        ref ecb,
                        releasedVehicles,
                        releasedLeaders);

                    // Apply structural and buffer changes only after every live source buffer
                    // used by this pass has finished being read.
                    ecb.Playback(EntityManager);
#if DEBUG
                    playedCommandBuffer = true;
#endif
                }

                int groupFamilyAssists = groupsReleased + groupsAssisted;
                if (groupFamilyAssists > 0)
                {
                    WaitStatus.RecordGroupFamilyAssists(
                        World,
                        groupFamilyAssists);
                }

                if (BoardingRuntimeSettings.EnableVerboseLogging &&
                    (groupsReleased > 0 || groupsAssisted > 0))
                {
                    LogUtils.Info(
                        Mod.s_Log,
                        () =>
                            $"{Mod.ModTag} Late groups: candidates={candidateCount}, " +
                            $"released={groupsReleased}, assisted={groupsAssisted}, " +
                            $"membersPrompted={membersPrompted}");
                }
            }
            catch (Exception ex)
            {
                // Group handling is an enhancement. Do not disable the normal solo boarding assist.
                Enabled = false;

                LogUtils.WarnOnce(
                    "BBOARD_LATE_GROUP_EXCEPTION",
                    () =>
                        $"{Mod.ModTag} Late group assist disabled after " +
                        $"{ex.GetType().Name}: {ex.Message}",
                    ex);
            }
            finally
            {
                if (hasCommandBuffer)
                {
                    ecb.Dispose();
                }

#if DEBUG
                BoardingPerformanceStats.RecordLateGroups(
                    performanceSampleStarted,
                    controllersScanned,
                    candidateCount,
                    groupsReleased,
                    groupsAssisted,
                    membersPrompted,
                    playedCommandBuffer);
#endif
            }
        }

        private bool IsLateBoardingController(Entity controllerVehicle, uint frame)
        {
            if (!EntityManager.Exists(controllerVehicle) ||
                !EntityManager.HasComponent<Game.Vehicles.PublicTransport>(controllerVehicle))
            {
                return false;
            }

            Game.Vehicles.PublicTransport publicTransport =
                EntityManager.GetComponentData<Game.Vehicles.PublicTransport>(controllerVehicle);

            if ((publicTransport.m_State & PublicTransportFlags.Boarding) == 0 ||
                (publicTransport.m_State &
                    (PublicTransportFlags.Evacuating |
                     PublicTransportFlags.PrisonerTransport |
                     PublicTransportFlags.Refueling)) != 0)
            {
                return false;
            }

            uint departureFrame = publicTransport.m_DepartureFrame;
            if (EntityManager.HasComponent<Game.Vehicles.CargoTransport>(controllerVehicle))
            {
                Game.Vehicles.CargoTransport cargo =
                    EntityManager.GetComponentData<Game.Vehicles.CargoTransport>(controllerVehicle);

                if (cargo.m_DepartureFrame > departureFrame)
                {
                    departureFrame = cargo.m_DepartureFrame;
                }
            }

            if (departureFrame == 0 ||
                frame < departureFrame ||
                frame - departureFrame < kLateGroupGraceFrames)
            {
                return false;
            }

            // Fail closed if a custom vehicle cannot be classified. Rail leaders are
            // protected by the station-proximity gate; boarded leaders can still receive
            // the family assist below.
            if (!EntityManager.HasComponent<PrefabRef>(controllerVehicle))
            {
                return false;
            }

            Entity prefab =
                EntityManager.GetComponentData<PrefabRef>(controllerVehicle).m_Prefab;
            return EntityManager.HasComponent<PublicTransportVehicleData>(prefab);
        }

    }
}
