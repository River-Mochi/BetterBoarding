// <copyright file="LateGroupBoardingSystem.Candidates.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/LateGroupBoardingSystem.Candidates.cs
// Purpose: Candidate discovery and validation for late family/group boarding.

namespace BetterBoarding
{
    using Game.Common;
    using Game.Creatures;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Collections;
    using Unity.Entities;

    public partial class LateGroupBoardingSystem
    {
        private void CollectLateGroupCandidates(
            Entity controllerVehicle,
            NativeList<GroupCandidate> candidates)
        {
            if (EntityManager.HasBuffer<LayoutElement>(controllerVehicle))
            {
                DynamicBuffer<LayoutElement> layout =
                    EntityManager.GetBuffer<LayoutElement>(
                        controllerVehicle,
                        isReadOnly: true);

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
                EntityManager.GetBuffer<Passenger>(assignedVehicle, isReadOnly: true);

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
                    EntityManager.GetBuffer<GroupCreature>(leader, isReadOnly: true).Length == 0)
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
                EntityManager.GetBuffer<GroupCreature>(leader, isReadOnly: true);
            return group.Length > 0;
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
