// <copyright file="BoardingVehicleFilterJob.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/BoardingVehicleFilterJob.cs
// Purpose: Burst-filter public transport entities before managed boarding logic.

namespace BetterBoarding
{
    using Game.Vehicles;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Performs the cheap, contiguous first-stage public-transport scan in Burst.
    /// The existing managed systems still perform all safety-sensitive boarding logic.
    /// </summary>
    [BurstCompile]
    internal struct BoardingVehicleFilterJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle m_EntityType;

        [ReadOnly]
        public ComponentTypeHandle<PublicTransport> m_PublicTransportType;

        [ReadOnly]
        public ComponentTypeHandle<Controller> m_ControllerType;

        [ReadOnly]
        public ComponentTypeHandle<CargoTransport> m_CargoTransportType;

        public NativeList<Entity>.ParallelWriter m_Results;

        public PublicTransportFlags m_ExcludedStates;

        public bool m_SkipChildControllers;

        public bool m_RequirePastDeparture;

        public uint m_Frame;

        public uint m_MinimumPastDepartureFrames;

        public void Execute(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask)
        {
            _ = unfilteredChunkIndex;
            _ = useEnabledMask;
            _ = chunkEnabledMask;

            NativeArray<Entity> entities = chunk.GetNativeArray(m_EntityType);
            NativeArray<PublicTransport> publicTransports =
                chunk.GetNativeArray(ref m_PublicTransportType);

            bool hasController =
                m_SkipChildControllers && chunk.Has(ref m_ControllerType);
            NativeArray<Controller> controllers = hasController
                ? chunk.GetNativeArray(ref m_ControllerType)
                : default;

            bool hasCargoTransport =
                m_RequirePastDeparture && chunk.Has(ref m_CargoTransportType);
            NativeArray<CargoTransport> cargoTransports = hasCargoTransport
                ? chunk.GetNativeArray(ref m_CargoTransportType)
                : default;

            for (int i = 0; i < entities.Length; i++)
            {
                PublicTransport publicTransport = publicTransports[i];

                if ((publicTransport.m_State & PublicTransportFlags.Boarding) == 0 ||
                    (publicTransport.m_State & m_ExcludedStates) != 0)
                {
                    continue;
                }

                Entity entity = entities[i];

                if (hasController)
                {
                    Entity controller = controllers[i].m_Controller;
                    if (controller != Entity.Null && controller != entity)
                    {
                        continue;
                    }
                }

                if (m_RequirePastDeparture)
                {
                    uint departureFrame = publicTransport.m_DepartureFrame;
                    if (hasCargoTransport &&
                        cargoTransports[i].m_DepartureFrame > departureFrame)
                    {
                        departureFrame = cargoTransports[i].m_DepartureFrame;
                    }

                    if (departureFrame == 0 ||
                        m_Frame < departureFrame ||
                        m_Frame - departureFrame < m_MinimumPastDepartureFrames)
                    {
                        continue;
                    }
                }

                m_Results.AddNoResize(entity);
            }
        }
    }
}
