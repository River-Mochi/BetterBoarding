// <copyright file="LateGroupBoardingSystem.Assist.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/LateGroupBoardingSystem.Assist.cs
// Purpose: Group leader release and lagging-member boarding assistance.

namespace BetterBoarding
{
    using System;
    using Game.Common;
    using Game.Creatures;
    using Game.Pathfind;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;

    public partial class LateGroupBoardingSystem
    {
        private bool TryQueueOutsideGroupLeaderCancellation(
            ref EntityCommandBuffer ecb,
            ref bool hasCommandBuffer,
            Entity leader,
            Entity assignedVehicle)
        {

            if (!EntityManager.HasComponent<CurrentVehicle>(leader) ||
                !EntityManager.HasComponent<Game.Creatures.Resident>(leader) ||
                !EntityManager.HasComponent<Human>(leader) ||
                !EntityManager.HasComponent<PathOwner>(leader) ||
                !EntityManager.HasBuffer<PathElement>(leader) ||
                !VehicleContainsPassenger(assignedVehicle, leader))
            {
                return false;
            }

            CurrentVehicle currentVehicle =
                EntityManager.GetComponentData<CurrentVehicle>(leader);

            if (currentVehicle.m_Vehicle != assignedVehicle ||
                (currentVehicle.m_Flags &
                    (CreatureVehicleFlags.Ready | CreatureVehicleFlags.Entering)) != 0)
            {
                return false;
            }


            PathOwner pathOwner = EntityManager.GetComponentData<PathOwner>(leader);
            DynamicBuffer<PathElement> pathElements =
                EntityManager.GetBuffer<PathElement>(leader, isReadOnly: true);

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

            if (!hasCommandBuffer)
            {
                ecb = new EntityCommandBuffer(Allocator.Temp);
                hasCommandBuffer = true;
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

    }
}
