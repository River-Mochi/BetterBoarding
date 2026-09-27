// <copyright file="LateGroupBoardingSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/LateGroupBoardingSystem.cs
// Purpose: Resolve late family/group boarding without replacing vanilla transport AI systems.

namespace BetterBoarding
{
    using System;
    using CS2Shared.RiverMochi;
    using Game;
    using Game.Common;
    using Game.Creatures;
    using Game.Pathfind;
    using Game.Simulation;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Handles the group/family cases deliberately excluded from the normal late-passenger pass.
    ///
    /// If the leader is still outside, the leader is safely detached and vanilla cancels the
    /// followers after observing that their leader no longer has CurrentVehicle. If the leader
    /// has boarded, walking members are prompted to enter and assigned members receive vanilla's
    /// existing boarding-timeout nudge. ResidentAISystem or PetAISystem still performs every
    /// actual enter/finish transition and its bookkeeping.
    /// </summary>
    public sealed partial class LateGroupBoardingSystem : GameSystemBase
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
        private EntityQuery m_CurrentVehicleQuery;
        private EntityQuery m_HumanResidentQuery;
        private EntityQuery m_HumanLaneQuery;
        private EntityQuery m_AnimalLaneQuery;
        private EntityQuery m_PassengerBufferQuery;
        private EntityQuery m_LayoutBufferQuery;
        private EntityQuery m_PathQuery;
        private EntityQuery m_GroupCreatureQuery;
        private EntityQuery m_GroupMemberQuery;

        private SimulationSystem? m_SimulationSystem;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 262144 / UpdatesPerDay;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();

            // Start from public-transport controllers, then inspect only their passenger buffers.
            m_VehicleQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Vehicles.PublicTransport>()
                .WithNone<Deleted, Destroyed, Temp, Overridden>()
                .Build();

            // Dependency-only queries for data read or edited directly below.
            m_CurrentVehicleQuery = SystemAPI.QueryBuilder()
                .WithAll<CurrentVehicle>()
                .Build();

            m_HumanResidentQuery = SystemAPI.QueryBuilder()
                .WithAll<Human, Game.Creatures.Resident>()
                .Build();

            m_HumanLaneQuery = SystemAPI.QueryBuilder()
                .WithAll<HumanCurrentLane>()
                .Build();

            m_AnimalLaneQuery = SystemAPI.QueryBuilder()
                .WithAll<AnimalCurrentLane>()
                .Build();

            m_PassengerBufferQuery = SystemAPI.QueryBuilder()
                .WithAll<Passenger>()
                .Build();

            m_LayoutBufferQuery = SystemAPI.QueryBuilder()
                .WithAll<LayoutElement>()
                .Build();

            m_PathQuery = SystemAPI.QueryBuilder()
                .WithAll<PathOwner, PathElement>()
                .Build();

            m_GroupCreatureQuery = SystemAPI.QueryBuilder()
                .WithAll<GroupCreature>()
                .Build();

            m_GroupMemberQuery = SystemAPI.QueryBuilder()
                .WithAll<GroupMember>()
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

            try
            {
                CompleteDependencies();

                uint frame = m_SimulationSystem.frameIndex;
                int groupsReleased = 0;
                int groupsAssisted = 0;
                int membersPrompted = 0;

                using NativeArray<Entity> controllers =
                    m_VehicleQuery.ToEntityArray(Allocator.Temp);
                using NativeList<GroupCandidate> candidates =
                    new NativeList<GroupCandidate>(kMaxGroupsPerUpdate, Allocator.Temp);
                using NativeList<Entity> releasedLeaders =
                    new NativeList<Entity>(kMaxGroupsPerUpdate, Allocator.Temp);
                using NativeList<Entity> releasedVehicles =
                    new NativeList<Entity>(kMaxGroupsPerUpdate, Allocator.Temp);

                for (int i = 0;
                    i < controllers.Length && candidates.Length < kMaxGroupsPerUpdate;
                    i++)
                {
                    Entity controllerVehicle = controllers[i];

                    if (GetControllerVehicle(controllerVehicle) != controllerVehicle ||
                        !IsLateBoardingController(controllerVehicle, frame))
                    {
                        continue;
                    }

                    CollectLateGroupCandidates(controllerVehicle, candidates);
                }

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
                        if (ContainsEntity(releasedLeaders, candidate.Leader))
                        {
                            continue;
                        }

                        if (!hasCommandBuffer)
                        {
                            ecb = new EntityCommandBuffer(Allocator.Temp);
                            hasCommandBuffer = true;
                        }

                        if (TryQueueOutsideGroupLeaderCancellation(
                                ref ecb,
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
                            EntityManager.GetBuffer<GroupCreature>(candidate.Leader);

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
                }

                if (BoardingRuntimeSettings.EnableVerboseLogging &&
                    (groupsReleased > 0 || groupsAssisted > 0))
                {
                    LogUtils.Info(
                        Mod.s_Log,
                        () =>
                            $"{Mod.ModTag} Late groups: candidates={candidates.Length}, " +
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
            if (EntityManager.HasComponent<CargoTransport>(controllerVehicle))
            {
                CargoTransport cargo =
                    EntityManager.GetComponentData<CargoTransport>(controllerVehicle);

                if (cargo.m_DepartureFrame > departureFrame)
                {
                    departureFrame = cargo.m_DepartureFrame;
                }
            }

            return departureFrame != 0 &&
                frame >= departureFrame &&
                frame - departureFrame >= kLateGroupGraceFrames;
        }

        private void CollectLateGroupCandidates(
            Entity controllerVehicle,
            NativeList<GroupCandidate> candidates)
        {
            if (EntityManager.HasBuffer<LayoutElement>(controllerVehicle))
            {
                DynamicBuffer<LayoutElement> layout =
                    EntityManager.GetBuffer<LayoutElement>(controllerVehicle);

                for (int i = 0;
                    i < layout.Length && candidates.Length < kMaxGroupsPerUpdate;
                    i++)
                {
                    CollectLateGroupCandidatesFromVehicle(
                        layout[i].m_Vehicle,
                        controllerVehicle,
                        candidates);
                }

                return;
            }

            CollectLateGroupCandidatesFromVehicle(
                controllerVehicle,
                controllerVehicle,
                candidates);
        }

        private void CollectLateGroupCandidatesFromVehicle(
            Entity assignedVehicle,
            Entity controllerVehicle,
            NativeList<GroupCandidate> candidates)
        {
            if (!EntityManager.Exists(assignedVehicle) ||
                !EntityManager.HasBuffer<Passenger>(assignedVehicle))
            {
                return;
            }

            DynamicBuffer<Passenger> passengers =
                EntityManager.GetBuffer<Passenger>(assignedVehicle);

            for (int i = 0;
                i < passengers.Length && candidates.Length < kMaxGroupsPerUpdate;
                i++)
            {
                Entity leader = passengers[i].m_Passenger;

                if (!EntityManager.Exists(leader) ||
                    EntityManager.HasComponent<Deleted>(leader) ||
                    EntityManager.HasComponent<Destroyed>(leader) ||
                    EntityManager.HasComponent<Temp>(leader) ||
                    EntityManager.HasComponent<Overridden>(leader) ||
                    !EntityManager.HasComponent<CurrentVehicle>(leader) ||
                    !EntityManager.HasBuffer<GroupCreature>(leader))
                {
                    continue;
                }

                CurrentVehicle currentVehicle =
                    EntityManager.GetComponentData<CurrentVehicle>(leader);

                if (currentVehicle.m_Vehicle != assignedVehicle ||
                    (currentVehicle.m_Flags & CreatureVehicleFlags.Ready) != 0 ||
                    EntityManager.GetBuffer<GroupCreature>(leader).Length == 0)
                {
                    continue;
                }

                candidates.Add(
                    new GroupCandidate(leader, assignedVehicle, controllerVehicle));
            }
        }

        private bool IsValidGroup(
            Entity leader,
            Entity assignedVehicle,
            Entity controllerVehicle)
        {
            if (!EntityManager.Exists(leader) ||
                !EntityManager.HasComponent<CurrentVehicle>(leader) ||
                !EntityManager.HasBuffer<GroupCreature>(leader))
            {
                return false;
            }

            CurrentVehicle currentVehicle =
                EntityManager.GetComponentData<CurrentVehicle>(leader);

            if (currentVehicle.m_Vehicle != assignedVehicle ||
                (currentVehicle.m_Flags & CreatureVehicleFlags.Ready) != 0 ||
                GetControllerVehicle(assignedVehicle) != controllerVehicle ||
                !VehicleContainsPassenger(assignedVehicle, leader))
            {
                return false;
            }

            DynamicBuffer<GroupCreature> group =
                EntityManager.GetBuffer<GroupCreature>(leader);
            return group.Length > 0;
        }

        private bool TryQueueOutsideGroupLeaderCancellation(
            ref EntityCommandBuffer ecb,
            Entity leader,
            Entity assignedVehicle)
        {
            if (!EntityManager.HasComponent<Game.Creatures.Resident>(leader) ||
                !EntityManager.HasComponent<Human>(leader) ||
                !EntityManager.HasComponent<PathOwner>(leader) ||
                !EntityManager.HasBuffer<PathElement>(leader) ||
                !VehicleContainsPassenger(assignedVehicle, leader))
            {
                return false;
            }

            PathOwner pathOwner = EntityManager.GetComponentData<PathOwner>(leader);
            DynamicBuffer<PathElement> pathElements =
                EntityManager.GetBuffer<PathElement>(leader);

            int startIndex = Math.Max(0, pathOwner.m_ElementIndex);
            int vehiclePathIndex = -1;

            for (int i = startIndex; i < pathElements.Length; i++)
            {
                if (pathElements[i].m_Target == assignedVehicle)
                {
                    vehiclePathIndex = i;
                    break;
                }
            }

            if (vehiclePathIndex < 0)
            {
                return false;
            }

            Game.Creatures.Resident resident =
                EntityManager.GetComponentData<Game.Creatures.Resident>(leader);
            resident.m_Flags &= ~ResidentFlags.InVehicle;
            resident.m_Timer = 0;
            ecb.SetComponent(leader, resident);

            Human human = EntityManager.GetComponentData<Human>(leader);
            human.m_Flags &= ~(HumanFlags.Run | HumanFlags.Emergency);
            ecb.SetComponent(leader, human);

            // Match vanilla CancelEnterVehicle without mutating the live source buffer. The ECB
            // owns this replacement buffer until playback after all source reads have completed.
            DynamicBuffer<PathElement> newPath = ecb.SetBuffer<PathElement>(leader);
            for (int i = vehiclePathIndex + 1; i < pathElements.Length; i++)
            {
                newPath.Add(pathElements[i]);
            }

            pathOwner.m_ElementIndex = 0;
            ecb.SetComponent(leader, pathOwner);

            // Queue the archetype change; never invalidate a live buffer during this scan.
            ecb.RemoveComponent<CurrentVehicle>(leader);
            return true;
        }

        private bool TryAssistLateGroup(
            Entity leader,
            Entity controllerVehicle,
            DynamicBuffer<GroupCreature> group,
            out int promptedMembers)
        {
            promptedMembers = 0;

            // Validate the complete group before changing any member. Unknown or inconsistent
            // state is left to vanilla instead of producing a partially modified family.
            for (int i = 0; i < group.Length; i++)
            {
                if (!CanAssistGroupMember(
                        group[i].m_Creature,
                        leader,
                        controllerVehicle))
                {
                    return false;
                }
            }

            bool changed = false;
            int waitingPets = 0;
            int petsPromptedToEnter = 0;

            for (int i = 0; i < group.Length; i++)
            {
                Entity member = group[i].m_Creature;
                bool hasCurrentVehicle =
                    EntityManager.HasComponent<CurrentVehicle>(member);

                if (hasCurrentVehicle &&
                    (EntityManager.GetComponentData<CurrentVehicle>(member).m_Flags &
                        CreatureVehicleFlags.Ready) != 0)
                {
                    continue;
                }

                if (EntityManager.HasComponent<HumanCurrentLane>(member))
                {
                    if (hasCurrentVehicle)
                    {
                        Game.Creatures.Resident resident =
                            EntityManager.GetComponentData<Game.Creatures.Resident>(member);

                        if (resident.m_Timer < kVanillaBoardingTimeout)
                        {
                            resident.m_Timer = kVanillaBoardingTimeout;
                            EntityManager.SetComponentData(member, resident);
                            promptedMembers++;
                            changed = true;
                        }
                    }
                    else
                    {
                        HumanCurrentLane lane =
                            EntityManager.GetComponentData<HumanCurrentLane>(member);
                        CreatureLaneFlags finishFlags =
                            CreatureLaneFlags.EndOfPath | CreatureLaneFlags.EndReached;

                        if ((lane.m_Flags & finishFlags) != finishFlags)
                        {
                            lane.m_Flags |= finishFlags;
                            EntityManager.SetComponentData(member, lane);
                            promptedMembers++;
                            changed = true;
                        }
                    }
                }
                else if (EntityManager.HasComponent<AnimalCurrentLane>(member))
                {
                    waitingPets++;

                    if (!hasCurrentVehicle)
                    {
                        AnimalCurrentLane lane =
                            EntityManager.GetComponentData<AnimalCurrentLane>(member);
                        CreatureLaneFlags finishFlags =
                            CreatureLaneFlags.EndOfPath | CreatureLaneFlags.EndReached;

                        if ((lane.m_Flags & finishFlags) != finishFlags)
                        {
                            lane.m_Flags |= finishFlags;
                            EntityManager.SetComponentData(member, lane);
                            petsPromptedToEnter++;
                            promptedMembers++;
                            changed = true;
                        }
                    }
                }
            }

            if (waitingPets > 0)
            {
                // PetAISystem intentionally reads the group leader's Resident timer. Setting it
                // now also covers a walking pet as soon as vanilla assigns that pet to the vehicle.
                Game.Creatures.Resident leaderResident =
                    EntityManager.GetComponentData<Game.Creatures.Resident>(leader);

                if (leaderResident.m_Timer < kVanillaBoardingTimeout)
                {
                    leaderResident.m_Timer = kVanillaBoardingTimeout;
                    EntityManager.SetComponentData(leader, leaderResident);
                    promptedMembers += waitingPets - petsPromptedToEnter;
                    changed = true;
                }
            }

            return changed;
        }

        private bool CanAssistGroupMember(
            Entity member,
            Entity expectedLeader,
            Entity controllerVehicle)
        {
            if (!EntityManager.Exists(member) ||
                EntityManager.HasComponent<Deleted>(member) ||
                EntityManager.HasComponent<Destroyed>(member) ||
                EntityManager.HasComponent<Temp>(member) ||
                EntityManager.HasComponent<Overridden>(member) ||
                !EntityManager.HasComponent<GroupMember>(member))
            {
                return false;
            }

            GroupMember groupMember =
                EntityManager.GetComponentData<GroupMember>(member);

            if (groupMember.m_Leader != expectedLeader)
            {
                return false;
            }

            bool hasHumanLane = EntityManager.HasComponent<HumanCurrentLane>(member);
            bool hasAnimalLane = EntityManager.HasComponent<AnimalCurrentLane>(member);

            if (hasHumanLane && hasAnimalLane)
            {
                return false;
            }

            bool hasCurrentVehicle = EntityManager.HasComponent<CurrentVehicle>(member);

            if (!hasCurrentVehicle)
            {
                // Vanilla group walking code assigns the follower only after EndReached. These
                // members are safe to prompt because the boarded leader selects the exact vehicle.
                return (hasHumanLane && IsValidHumanGroupMember(member)) ||
                    (hasAnimalLane && IsValidPetGroupMember(member));
            }

            CurrentVehicle currentVehicle =
                EntityManager.GetComponentData<CurrentVehicle>(member);

            if (currentVehicle.m_Vehicle == Entity.Null ||
                GetControllerVehicle(currentVehicle.m_Vehicle) != controllerVehicle ||
                !VehicleContainsPassenger(currentVehicle.m_Vehicle, member))
            {
                return false;
            }

            if ((currentVehicle.m_Flags & CreatureVehicleFlags.Ready) != 0)
            {
                // Vanilla sets Ready only after the lane is gone. Do not normalize an unknown
                // ready-but-still-spawned state because the vehicle may otherwise leave it behind.
                return !hasHumanLane && !hasAnimalLane;
            }

            if (hasHumanLane)
            {
                return IsValidHumanGroupMember(member);
            }

            if (hasAnimalLane)
            {
                return IsValidPetGroupMember(member);
            }

            bool isKnownHuman =
                EntityManager.HasComponent<Human>(member) &&
                EntityManager.HasComponent<Game.Creatures.Resident>(member);
            bool isKnownPet =
                EntityManager.HasComponent<Game.Creatures.Pet>(member) &&
                EntityManager.HasComponent<Creature>(member);
            bool isPhysicallyAboard =
                EntityManager.HasComponent<Game.Objects.Unspawned>(member) ||
                EntityManager.HasComponent<Game.Objects.Relative>(member);

            return (isKnownHuman || isKnownPet) && isPhysicallyAboard;
        }

        private bool IsValidHumanGroupMember(Entity member)
        {
            return EntityManager.HasComponent<Human>(member) &&
                EntityManager.HasComponent<Game.Creatures.Resident>(member) &&
                EntityManager.HasComponent<HumanNavigation>(member) &&
                EntityManager.HasComponent<PathOwner>(member) &&
                EntityManager.HasBuffer<PathElement>(member);
        }

        private bool IsValidPetGroupMember(Entity member)
        {
            return EntityManager.HasComponent<Game.Creatures.Pet>(member) &&
                EntityManager.HasComponent<Creature>(member) &&
                EntityManager.HasComponent<AnimalNavigation>(member);
        }

        private bool IsBoardedHuman(Entity leader)
        {
            return !EntityManager.HasComponent<HumanCurrentLane>(leader) &&
                !EntityManager.HasComponent<AnimalCurrentLane>(leader) &&
                EntityManager.HasComponent<Human>(leader) &&
                EntityManager.HasComponent<Game.Creatures.Resident>(leader) &&
                (EntityManager.HasComponent<Game.Objects.Unspawned>(leader) ||
                 EntityManager.HasComponent<Game.Objects.Relative>(leader));
        }

        private bool VehicleContainsPassenger(Entity vehicle, Entity passenger)
        {
            if (vehicle == Entity.Null ||
                !EntityManager.Exists(vehicle) ||
                !EntityManager.HasBuffer<Passenger>(vehicle))
            {
                return false;
            }

            DynamicBuffer<Passenger> passengers =
                EntityManager.GetBuffer<Passenger>(vehicle);

            for (int i = 0; i < passengers.Length; i++)
            {
                if (passengers[i].m_Passenger == passenger)
                {
                    return true;
                }
            }

            return false;
        }

        private void QueueReleasedLeaderPassengerBuffers(
            ref EntityCommandBuffer ecb,
            NativeList<Entity> releasedVehicles,
            NativeList<Entity> releasedLeaders)
        {
            for (int vehicleIndex = 0;
                vehicleIndex < releasedVehicles.Length;
                vehicleIndex++)
            {
                Entity vehicle = releasedVehicles[vehicleIndex];
                if (vehicle == Entity.Null ||
                    !EntityManager.Exists(vehicle) ||
                    !EntityManager.HasBuffer<Passenger>(vehicle))
                {
                    continue;
                }

                DynamicBuffer<Passenger> passengers =
                    EntityManager.GetBuffer<Passenger>(vehicle);
                DynamicBuffer<Passenger> newPassengers =
                    ecb.SetBuffer<Passenger>(vehicle);

                for (int i = 0; i < passengers.Length; i++)
                {
                    if (!ContainsEntity(
                            releasedLeaders,
                            passengers[i].m_Passenger))
                    {
                        newPassengers.Add(passengers[i]);
                    }
                }
            }
        }

        private static void AddUniqueEntity(
            NativeList<Entity> entities,
            Entity entity)
        {
            if (!ContainsEntity(entities, entity))
            {
                entities.Add(entity);
            }
        }

        private static bool ContainsEntity(
            NativeList<Entity> entities,
            Entity entity)
        {
            for (int i = 0; i < entities.Length; i++)
            {
                if (entities[i] == entity)
                {
                    return true;
                }
            }

            return false;
        }

        private Entity GetControllerVehicle(Entity vehicle)
        {
            if (vehicle == Entity.Null || !EntityManager.Exists(vehicle))
            {
                return Entity.Null;
            }

            if (EntityManager.HasComponent<Controller>(vehicle))
            {
                Controller controller =
                    EntityManager.GetComponentData<Controller>(vehicle);

                if (controller.m_Controller != Entity.Null)
                {
                    return controller.m_Controller;
                }
            }

            return vehicle;
        }

        private void CompleteDependencies()
        {
            m_VehicleQuery.CompleteDependency();
            m_CurrentVehicleQuery.CompleteDependency();
            m_HumanResidentQuery.CompleteDependency();
            m_HumanLaneQuery.CompleteDependency();
            m_AnimalLaneQuery.CompleteDependency();
            m_PassengerBufferQuery.CompleteDependency();
            m_LayoutBufferQuery.CompleteDependency();
            m_PathQuery.CompleteDependency();
            m_GroupCreatureQuery.CompleteDependency();
            m_GroupMemberQuery.CompleteDependency();
        }

        private readonly struct GroupCandidate
        {
            public GroupCandidate(
                Entity leader,
                Entity assignedVehicle,
                Entity controllerVehicle)
            {
                Leader = leader;
                AssignedVehicle = assignedVehicle;
                ControllerVehicle = controllerVehicle;
            }

            public Entity Leader { get; }

            public Entity AssignedVehicle { get; }

            public Entity ControllerVehicle { get; }
        }
    }
}
