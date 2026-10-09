// <copyright file="SecondBusBoardingProvisionSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/SecondBusBoardingProvisionSystem.cs
// Purpose: Adds disabled transient session state before simulation jobs need it.

namespace BetterBoarding
{
    using Game;
    using Game.Common;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Collections;
    using Unity.Entities;
    using VehiclePublicTransport = Game.Vehicles.PublicTransport;

    public sealed partial class SecondBusBoardingProvisionSystem : GameSystemBase
    {
        private EntityQuery m_WithoutSession;

        private ModificationBarrier1? m_Barrier;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_WithoutSession = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VehiclePublicTransport>(),
                    ComponentType.ReadOnly<CarCurrentLane>(),
                },
                Absent = new[]
                {
                    ComponentType.ReadOnly<SecondBusBoardingSession>(),
                },
                None = new[]
                {
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Temp>(),
                },
            });
            m_Barrier = World.GetOrCreateSystemManaged<ModificationBarrier1>();
        }

        protected override void OnUpdate()
        {
            if (!SecondBusBoardingCompatibility.EffectiveEnabled ||
                m_WithoutSession.IsEmptyIgnoreFilter ||
                m_Barrier == null)
            {
                return;
            }

            using NativeArray<Entity> vehicles =
                m_WithoutSession.ToEntityArray(Allocator.Temp);
            EntityCommandBuffer commandBuffer = m_Barrier.CreateCommandBuffer();
            commandBuffer.AddComponent<SecondBusBoardingSession>(vehicles);
            for (int i = 0; i < vehicles.Length; i++)
            {
                commandBuffer.SetComponentEnabled<SecondBusBoardingSession>(
                    vehicles[i],
                    false);
            }
        }
    }
}
