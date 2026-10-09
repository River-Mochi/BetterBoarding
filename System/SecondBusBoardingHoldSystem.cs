// <copyright file="SecondBusBoardingHoldSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/SecondBusBoardingHoldSystem.cs
// Purpose: Keeps an admitted following bus stationary while passengers use it.

namespace BetterBoarding
{
    using Game;
    using Game.Common;
    using Game.Objects;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine.Scripting;

    public sealed partial class SecondBusBoardingHoldSystem : GameSystemBase
    {
        private EntityQuery m_ActiveFollowers;

        private ComponentLookup<CarNavigation> m_Navigation;

        private ComponentLookup<Moving> m_Moving;

        [Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            m_ActiveFollowers = GetEntityQuery(
                ComponentType.ReadOnly<SecondBusBoardingSession>(),
                ComponentType.ReadWrite<CarNavigation>(),
                ComponentType.ReadWrite<Moving>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<Temp>());
            m_Navigation = GetComponentLookup<CarNavigation>(false);
            m_Moving = GetComponentLookup<Moving>(false);
            RequireForUpdate(m_ActiveFollowers);
        }

        [Preserve]
        protected override void OnUpdate()
        {
            m_Navigation.Update(this);
            m_Moving.Update(this);
            NativeList<Entity> followers =
                m_ActiveFollowers.ToEntityListAsync(
                    Allocator.TempJob,
                    out JobHandle listed);

            JobHandle handle = new HoldJob
            {
                Followers = followers,
                Navigation = m_Navigation,
                Moving = m_Moving,
            }.Schedule(JobHandle.CombineDependencies(Dependency, listed));

            handle = followers.Dispose(handle);
            Dependency = handle;
        }

        [BurstCompile]
        private struct HoldJob : IJob
        {
            [ReadOnly]
            public NativeList<Entity> Followers;

            public ComponentLookup<CarNavigation> Navigation;

            public ComponentLookup<Moving> Moving;

            public void Execute()
            {
                for (int i = 0; i < Followers.Length; i++)
                {
                    Entity follower = Followers[i];
                    CarNavigation navigation = Navigation[follower];
                    navigation.m_MaxSpeed = 0f;
                    Navigation[follower] = navigation;

                    Moving moving = Moving[follower];
                    moving.m_Velocity = float3.zero;
                    moving.m_AngularVelocity = float3.zero;
                    Moving[follower] = moving;
                }
            }
        }
    }
}
