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
    using Game.Net;
    using Game.Objects;
    using Game.Pathfind;
    using Game.Rendering;
    using Game.Simulation;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Handles the group/family cases deliberately excluded from the normal late-passenger pass.
    ///
    /// Vanilla keeps a group leader not-ready until every GroupCreature is ready. That can hold
    /// a public-transport vehicle for a long time when one child or pet is still boarding.
    ///
    /// This system uses two conservative paths:
    /// 1. If the group leader is still physically outside, cancel only the leader. Vanilla group
    ///    member AI then sees that the leader no longer has CurrentVehicle and uses its own
    ///    CancelEnterVehicle path for the followers.
    /// 2. If the leader has already physically boarded, finish only the lagging members using
    ///    the same core state transition vanilla uses for completed boarding, then mark the
    ///    leader ready once the whole group is logically aboard.
    ///
    /// It never disables or replaces a vanilla transport AI system.
    /// </summary>
    public sealed class LateGroupBoardingSystem : GameSystemBase
    {
        public const int UpdatesPerDay = 2048;

        // Give groups one extra boarding-assist interval beyond the solo grace.
        // Run Sooner already begins up to 512 frames before departure for human members.
        private const uint kLateGroupGraceFrames = 256u;

        // Keep structural work bounded in very large cities.
        private const int kMaxGroupsPerUpdate = 32;

        private EntityQuery m_GroupLeaderQuery;
        private EntityQuery m_CurrentVehicleQuery;
        private EntityQuery m_HumanLaneQuery;
        private EntityQuery m_AnimalLaneQuery;
        private EntityQuery m_PassengerBufferQuery;
        private EntityQuery m_PathQuery;
        private EntityQuery m_CreatureQuery;
        private EntityQuery m_QueueQuery;
        private EntityQuery m_LaneObjectQuery;

        private SimulationSystem? m_SimulationSystem;

        private ComponentTypeSet m_HumanCurrentLaneTypes;
        private ComponentTypeSet m_AnimalCurrentLaneTypes;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 262144 / UpdatesPerDay;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();

            // A group leader owns GroupCreature and is assigned through CurrentVehicle.
            m_GroupLeaderQuery = SystemAPI.QueryBuilder()
                .WithAll<GroupCreature, CurrentVehicle>()
                .WithNone<Deleted, Destroyed, Temp, Overridden>()
                .Build();

            // Dependency-only queries for data read or edited directly below.
            m_CurrentVehicleQuery = SystemAPI.QueryBuilder()
                .WithAll<CurrentVehicle>()
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

            m_PathQuery = SystemAPI.QueryBuilder()
                .WithAll<PathOwner, PathElement>()
                .Build();

            m_CreatureQuery = SystemAPI.QueryBuilder()
                .WithAll<Creature>()
                .Build();

            m_QueueQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Creatures.Queue>()
                .Build();

            m_LaneObjectQuery = SystemAPI.QueryBuilder()
                .WithAll<LaneObject>()
                .Build();

            // Match the component sets vanilla removes when a human/pet finishes boarding.
            m_HumanCurrentLaneTypes = new ComponentTypeSet(new ComponentType[6]
            {
                ComponentType.ReadWrite<Moving>(),
                ComponentType.ReadWrite<TransformFrame>(),
                ComponentType.ReadWrite<InterpolatedTransform>(),
                ComponentType.ReadWrite<HumanNavigation>(),
                ComponentType.ReadWrite<HumanCurrentLane>(),
                ComponentType.ReadWrite<Blocker>(),
            });

            m_AnimalCurrentLaneTypes = new ComponentTypeSet(new ComponentType[6]
            {
                ComponentType.ReadWrite<Moving>(),
                ComponentType.ReadWrite<TransformFrame>(),
                ComponentType.ReadWrite<InterpolatedTransform>(),
                ComponentType.ReadWrite<AnimalNavigation>(),
                ComponentType.ReadWrite<AnimalCurrentLane>(),
                ComponentType.ReadWrite<Blocker>(),
            });

            RequireForUpdate(m_GroupLeaderQuery);
        }

        protected override void OnUpdate()
        {
            if (!BoardingRuntimeSettings.CancelLateBoarders ||
                m_SimulationSystem == null)
            {
                return;
            }

            try
            {
                CompleteDependencies();

                uint frame = m_SimulationSystem.frameIndex;
                int groupsProcessed = 0;
                int groupsReleased = 0;
                int groupsFinished = 0;
                int membersFinished = 0;

                using NativeArray<Entity> leaders =
                    m_GroupLeaderQuery.ToEntityArray(Allocator.Temp);

                for (int i = 0;
                    i < leaders.Length && groupsProcessed < kMaxGroupsPerUpdate;
                    i++)
                {
                    Entity leader = leaders[i];

                    if (!TryGetLateGroupContext(
                            leader,
                            frame,
                            out Entity assignedVehicle,
                            out Entity controllerVehicle,
                            out DynamicBuffer<GroupCreature> group))
                    {
                        continue;
                    }

                    groupsProcessed++;

                    // Human leader still has a lane: the leader has not physically finished boarding.
                    // Remove only the leader assignment. Vanilla child/pet AI will see the missing
                    // leader CurrentVehicle and use its own cancellation path for followers.
                    if (EntityManager.HasComponent<HumanCurrentLane>(leader))
                    {
                        if (TryCancelOutsideGroupLeader(leader, assignedVehicle))
                        {
                            groupsReleased++;
                        }

                        continue;
                    }

                    // Leader has already finished entering. At this point canceling the leader would
                    // require reconstructing its world/lane state. Instead, finish the remaining
                    // members aboard so the family stays together and the vehicle can depart.
                    if (TryFinishLateGroup(
                            leader,
                            controllerVehicle,
                            group,
                            out int finishedMembers))
                    {
                        groupsFinished++;
                        membersFinished += finishedMembers;
                    }
                }

                if (BoardingRuntimeSettings.EnableVerboseLogging &&
                    (groupsReleased > 0 || groupsFinished > 0))
                {
                    LogUtils.Info(
                        Mod.s_Log,
                        () =>
                            $"{Mod.ModTag} Late groups: processed={groupsProcessed}, " +
                            $"released={groupsReleased}, finishedAboard={groupsFinished}, " +
                            $"membersFinished={membersFinished}");
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
        }

        private bool TryGetLateGroupContext(
            Entity leader,
            uint frame,
            out Entity assignedVehicle,
            out Entity controllerVehicle,
            out DynamicBuffer<GroupCreature> group)
        {
            assignedVehicle = Entity.Null;
            controllerVehicle = Entity.Null;
            group = default;

            if (!EntityManager.Exists(leader) ||
                !EntityManager.HasComponent<CurrentVehicle>(leader) ||
                !EntityManager.HasBuffer<GroupCreature>(leader))
            {
                return false;
            }

            CurrentVehicle currentVehicle =
                EntityManager.GetComponentData<CurrentVehicle>(leader);

            if ((currentVehicle.m_Flags & CreatureVehicleFlags.Ready) != 0 ||
                currentVehicle.m_Vehicle == Entity.Null)
            {
                return false;
            }

            assignedVehicle = currentVehicle.m_Vehicle;
            controllerVehicle = GetControllerVehicle(assignedVehicle);

            if (controllerVehicle == Entity.Null ||
                !EntityManager.Exists(controllerVehicle) ||
                !EntityManager.HasComponent<Game.Vehicles.PublicTransport>(
                    controllerVehicle))
            {
                return false;
            }

            Game.Vehicles.PublicTransport publicTransport =
                EntityManager.GetComponentData<Game.Vehicles.PublicTransport>(
                    controllerVehicle);

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

            if (departureFrame == 0 ||
                frame < departureFrame ||
                frame - departureFrame < kLateGroupGraceFrames)
            {
                return false;
            }

            group = EntityManager.GetBuffer<GroupCreature>(leader);
            return group.Length > 0;
        }

        private bool TryCancelOutsideGroupLeader(
            Entity leader,
            Entity assignedVehicle)
        {
            if (!EntityManager.HasComponent<Game.Creatures.Resident>(leader) ||
                !EntityManager.HasComponent<Human>(leader) ||
                !EntityManager.HasComponent<PathOwner>(leader) ||
                !EntityManager.HasBuffer<PathElement>(leader))
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

            RemovePassengerFromVehicle(assignedVehicle, leader);
            EntityManager.RemoveComponent<CurrentVehicle>(leader);

            Game.Creatures.Resident resident =
                EntityManager.GetComponentData<Game.Creatures.Resident>(leader);
            resident.m_Flags &= ~ResidentFlags.InVehicle;
            resident.m_Timer = 0;
            EntityManager.SetComponentData(leader, resident);

            Human human = EntityManager.GetComponentData<Human>(leader);
            human.m_Flags &= ~(HumanFlags.Run | HumanFlags.Emergency);
            EntityManager.SetComponentData(leader, human);

            // Match vanilla CancelEnterVehicle: drop the missed vehicle leg and continue onward.
            pathElements.RemoveRange(0, vehiclePathIndex + 1);
            pathOwner.m_ElementIndex = 0;
            EntityManager.SetComponentData(leader, pathOwner);

            return true;
        }

        private bool TryFinishLateGroup(
            Entity leader,
            Entity controllerVehicle,
            DynamicBuffer<GroupCreature> group,
            out int finishedMembers)
        {
            finishedMembers = 0;

            // Validate every unfinished member first. If one member is in an unknown state,
            // leave the entire group to vanilla rather than partially modifying it.
            for (int i = 0; i < group.Length; i++)
            {
                Entity member = group[i].m_Creature;

                if (!CanFinishGroupMember(member, leader, controllerVehicle))
                {
                    return false;
                }
            }

            for (int i = 0; i < group.Length; i++)
            {
                Entity member = group[i].m_Creature;
                CurrentVehicle memberVehicle =
                    EntityManager.GetComponentData<CurrentVehicle>(member);

                if ((memberVehicle.m_Flags & CreatureVehicleFlags.Ready) != 0)
                {
                    continue;
                }

                if (FinishGroupMember(member, ref memberVehicle))
                {
                    finishedMembers++;
                }
                else
                {
                    // Validation above should make this unreachable. If the entity changed between
                    // validation and mutation, stop and let vanilla continue from the resulting state.
                    return false;
                }
            }

            // Vanilla normally marks the leader Ready only after HasEveryoneBoarded(group) succeeds.
            // We have just completed every unfinished member, so finish that final state transition.
            if (!EntityManager.HasComponent<CurrentVehicle>(leader))
            {
                return false;
            }

            CurrentVehicle leaderVehicle =
                EntityManager.GetComponentData<CurrentVehicle>(leader);

            leaderVehicle.m_Flags &= ~CreatureVehicleFlags.Entering;
            leaderVehicle.m_Flags |= CreatureVehicleFlags.Ready;
            EntityManager.SetComponentData(leader, leaderVehicle);

            return true;
        }

        private bool CanFinishGroupMember(
            Entity member,
            Entity expectedLeader,
            Entity controllerVehicle)
        {
            if (!EntityManager.Exists(member) ||
                EntityManager.HasComponent<Deleted>(member) ||
                EntityManager.HasComponent<Destroyed>(member) ||
                EntityManager.HasComponent<Temp>(member) ||
                EntityManager.HasComponent<Overridden>(member) ||
                !EntityManager.HasComponent<GroupMember>(member) ||
                !EntityManager.HasComponent<CurrentVehicle>(member))
            {
                return false;
            }

            GroupMember groupMember =
                EntityManager.GetComponentData<GroupMember>(member);

            if (groupMember.m_Leader != expectedLeader)
            {
                return false;
            }

            CurrentVehicle currentVehicle =
                EntityManager.GetComponentData<CurrentVehicle>(member);

            if (currentVehicle.m_Vehicle == Entity.Null ||
                GetControllerVehicle(currentVehicle.m_Vehicle) != controllerVehicle)
            {
                return false;
            }

            if ((currentVehicle.m_Flags & CreatureVehicleFlags.Ready) != 0)
            {
                return true;
            }

            if (EntityManager.HasComponent<HumanCurrentLane>(member))
            {
                HumanCurrentLane lane =
                    EntityManager.GetComponentData<HumanCurrentLane>(member);

                return lane.m_Lane != Entity.Null &&
                    EntityManager.HasBuffer<LaneObject>(lane.m_Lane) &&
                    EntityManager.HasComponent<Human>(member) &&
                    EntityManager.HasComponent<Game.Creatures.Resident>(member) &&
                    EntityManager.HasComponent<Creature>(member);
            }

            if (EntityManager.HasComponent<AnimalCurrentLane>(member))
            {
                AnimalCurrentLane lane =
                    EntityManager.GetComponentData<AnimalCurrentLane>(member);

                return lane.m_Lane != Entity.Null &&
                    EntityManager.HasBuffer<LaneObject>(lane.m_Lane) &&
                    EntityManager.HasComponent<Game.Creatures.Pet>(member) &&
                    EntityManager.HasComponent<Creature>(member);
            }

            // No current lane means vanilla has already completed the physical enter step.
            // Only accept known human or pet group members before fixing the Ready flag.
            return
                (EntityManager.HasComponent<Human>(member) &&
                 EntityManager.HasComponent<Game.Creatures.Resident>(member)) ||
                EntityManager.HasComponent<Game.Creatures.Pet>(member);
        }

        private bool FinishGroupMember(
            Entity member,
            ref CurrentVehicle currentVehicle)
        {
            if (EntityManager.HasComponent<HumanCurrentLane>(member))
            {
                HumanCurrentLane lane =
                    EntityManager.GetComponentData<HumanCurrentLane>(member);

                if (!RemoveLaneObject(lane.m_Lane, member))
                {
                    return false;
                }

                ClearCreatureQueue(member);

                if (EntityManager.HasBuffer<Game.Creatures.Queue>(member))
                {
                    EntityManager.GetBuffer<Game.Creatures.Queue>(member).Clear();
                }

                EntityManager.RemoveComponent(member, in m_HumanCurrentLaneTypes);

                if (!EntityManager.HasComponent<Unspawned>(member))
                {
                    EntityManager.AddComponent<Unspawned>(member);
                }

                if (!EntityManager.HasComponent<Updated>(member))
                {
                    EntityManager.AddComponent<Updated>(member);
                }
            }
            else if (EntityManager.HasComponent<AnimalCurrentLane>(member))
            {
                AnimalCurrentLane lane =
                    EntityManager.GetComponentData<AnimalCurrentLane>(member);

                if (!RemoveLaneObject(lane.m_Lane, member))
                {
                    return false;
                }

                ClearCreatureQueue(member);
                EntityManager.RemoveComponent(member, in m_AnimalCurrentLaneTypes);

                if (!EntityManager.HasComponent<Unspawned>(member))
                {
                    EntityManager.AddComponent<Unspawned>(member);
                }

                if (!EntityManager.HasComponent<Updated>(member))
                {
                    EntityManager.AddComponent<Updated>(member);
                }
            }

            currentVehicle.m_Flags &= ~CreatureVehicleFlags.Entering;
            currentVehicle.m_Flags |= CreatureVehicleFlags.Ready;
            EntityManager.SetComponentData(member, currentVehicle);

            return true;
        }

        private void ClearCreatureQueue(Entity member)
        {
            if (!EntityManager.HasComponent<Creature>(member))
            {
                return;
            }

            Creature creature = EntityManager.GetComponentData<Creature>(member);
            creature.m_QueueEntity = Entity.Null;
            creature.m_QueueArea = default;
            EntityManager.SetComponentData(member, creature);
        }

        private bool RemoveLaneObject(Entity laneEntity, Entity member)
        {
            if (laneEntity == Entity.Null ||
                !EntityManager.HasBuffer<LaneObject>(laneEntity))
            {
                return false;
            }

            DynamicBuffer<LaneObject> laneObjects =
                EntityManager.GetBuffer<LaneObject>(laneEntity);

            for (int i = laneObjects.Length - 1; i >= 0; i--)
            {
                if (laneObjects[i].m_LaneObject == member)
                {
                    laneObjects.RemoveAt(i);
                    return true;
                }
            }

            // Vanilla can also track creatures in its object search tree when a lane buffer
            // is unavailable. We intentionally do not emulate that private search-tree path.
            return false;
        }

        private void RemovePassengerFromVehicle(Entity vehicle, Entity passenger)
        {
            if (vehicle == Entity.Null ||
                !EntityManager.HasBuffer<Passenger>(vehicle))
            {
                return;
            }

            DynamicBuffer<Passenger> passengers =
                EntityManager.GetBuffer<Passenger>(vehicle);

            for (int i = passengers.Length - 1; i >= 0; i--)
            {
                if (passengers[i].m_Passenger == passenger)
                {
                    passengers.RemoveAt(i);
                    return;
                }
            }
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
            m_GroupLeaderQuery.CompleteDependency();
            m_CurrentVehicleQuery.CompleteDependency();
            m_HumanLaneQuery.CompleteDependency();
            m_AnimalLaneQuery.CompleteDependency();
            m_PassengerBufferQuery.CompleteDependency();
            m_PathQuery.CompleteDependency();
            m_CreatureQuery.CompleteDependency();
            m_QueueQuery.CompleteDependency();
            m_LaneObjectQuery.CompleteDependency();
        }
    }
}
