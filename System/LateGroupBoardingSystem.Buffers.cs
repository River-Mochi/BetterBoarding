// <copyright file="LateGroupBoardingSystem.Buffers.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/LateGroupBoardingSystem.Buffers.cs
// Purpose: Passenger-buffer and entity-list helpers for late group boarding.

namespace BetterBoarding
{
    using Game.Vehicles;
    using Unity.Collections;
    using Unity.Entities;

    public partial class LateGroupBoardingSystem
    {
        private bool VehicleContainsPassenger(Entity vehicle, Entity passenger)
        {
            if (vehicle == Entity.Null ||
                !EntityManager.Exists(vehicle) ||
                !EntityManager.HasBuffer<Passenger>(vehicle))
            {
                return false;
            }

            DynamicBuffer<Passenger> passengers =
                EntityManager.GetBuffer<Passenger>(vehicle, isReadOnly: true);

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
                    EntityManager.GetBuffer<Passenger>(vehicle, isReadOnly: true);
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

    }
}
