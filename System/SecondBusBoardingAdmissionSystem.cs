// <copyright file="SecondBusBoardingAdmissionSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/SecondBusBoardingAdmissionSystem.cs
// Purpose: Admits one stopped follower and protects the native lead bus before vehicle AI.

namespace BetterBoarding
{
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Routes;
    using Game.Simulation;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine.Scripting;
    using VehiclePublicTransport = Game.Vehicles.PublicTransport;

    public sealed partial class SecondBusBoardingAdmissionSystem : GameSystemBase
    {
        private const int kPairCandidates = 0;
        private const int kSessionsStarted = 1;
        private const int kRejectedMoving = 2;
        private const int kRejectedDistance = 3;
        private const int kRejectedContext = 4;
        private const int kArrivingSessionsStarted = 5;
        private const int kCounterCount = 6;

        private EntityQuery m_Buses;

        private SimulationSystem? m_SimulationSystem;

        private SecondBusBoardingLookups m_Lookups;

        private ComponentLookup<SecondBusBoardingSession> m_Sessions;

        private NativeArray<int> m_Counters;

        private NativeArray<Entity> m_LastSession;

        private JobHandle m_PreviousJob;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 16;
        }

        [Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Buses = GetEntityQuery(
                ComponentType.ReadOnly<VehiclePublicTransport>(),
                ComponentType.ReadOnly<PrefabRef>(),
                ComponentType.ReadOnly<CurrentRoute>(),
                ComponentType.ReadOnly<Target>(),
                ComponentType.ReadOnly<Transform>(),
                ComponentType.ReadOnly<Moving>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<Temp>(),
                ComponentType.Exclude<TripSource>(),
                ComponentType.Exclude<OutOfControl>());
            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
            m_Lookups = SecondBusBoardingLookups.Create(this);
            m_Sessions = GetComponentLookup<SecondBusBoardingSession>(false);
            m_Counters = new NativeArray<int>(kCounterCount, Allocator.Persistent);
            m_LastSession = new NativeArray<Entity>(4, Allocator.Persistent);
            RequireForUpdate(m_Buses);
        }

        [Preserve]
        protected override void OnDestroy()
        {
            m_PreviousJob.Complete();
            if (m_Counters.IsCreated)
            {
                m_Counters.Dispose();
            }

            if (m_LastSession.IsCreated)
            {
                m_LastSession.Dispose();
            }

            base.OnDestroy();
        }

        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);
            m_PreviousJob.Complete();
            ResetCounters();
        }

        [Preserve]
        protected override void OnUpdate()
        {
            if (!SecondBusBoardingCompatibility.EffectiveEnabled)
            {
                Enabled = false;
                return;
            }

            m_Lookups.Update(this);
            m_Sessions.Update(this);
            NativeList<Entity> buses =
                m_Buses.ToEntityListAsync(Allocator.TempJob, out JobHandle listed);

            JobHandle handle = new AdmissionJob
            {
                Buses = buses,
                Lookups = m_Lookups,
                Sessions = m_Sessions,
                Counters = m_Counters,
                LastSession = m_LastSession,
                Frame = m_SimulationSystem?.frameIndex ?? 0u,
            }.Schedule(JobHandle.CombineDependencies(Dependency, listed));

            handle = buses.Dispose(handle);
            m_PreviousJob = handle;
            Dependency = handle;
        }

        public StatisticsSnapshot GetStatisticsSnapshot()
        {
            m_PreviousJob.Complete();
            return new StatisticsSnapshot(
                m_Counters[kPairCandidates],
                m_Counters[kSessionsStarted],
                m_Counters[kRejectedMoving],
                m_Counters[kRejectedDistance],
                m_Counters[kRejectedContext],
                m_Counters[kArrivingSessionsStarted],
                m_LastSession[0],
                m_LastSession[1],
                m_LastSession[2],
                m_LastSession[3]);
        }

        public void RestartStatisticsCollection()
        {
            m_PreviousJob.Complete();
            ResetCounters();
        }

        private void ResetCounters()
        {
            if (!m_Counters.IsCreated)
            {
                return;
            }

            for (int i = 0; i < m_Counters.Length; i++)
            {
                m_Counters[i] = 0;
            }

            for (int i = 0; i < m_LastSession.Length; i++)
            {
                m_LastSession[i] = Entity.Null;
            }
        }

        public readonly struct StatisticsSnapshot
        {
            public StatisticsSnapshot(
                int pairCandidates,
                int sessionsStarted,
                int rejectedMoving,
                int rejectedDistance,
                int rejectedContext,
                int arrivingSessionsStarted,
                Entity lastRoute,
                Entity lastStop,
                Entity lastLeadBus,
                Entity lastFollowerBus)
            {
                PairCandidates = pairCandidates;
                SessionsStarted = sessionsStarted;
                RejectedMoving = rejectedMoving;
                RejectedDistance = rejectedDistance;
                RejectedContext = rejectedContext;
                ArrivingSessionsStarted = arrivingSessionsStarted;
                LastRoute = lastRoute;
                LastStop = lastStop;
                LastLeadBus = lastLeadBus;
                LastFollowerBus = lastFollowerBus;
            }

            public int PairCandidates { get; }

            public int SessionsStarted { get; }

            public int RejectedMoving { get; }

            public int RejectedDistance { get; }

            public int RejectedContext { get; }

            public int ArrivingSessionsStarted { get; }

            public Entity LastRoute { get; }

            public Entity LastStop { get; }

            public Entity LastLeadBus { get; }

            public Entity LastFollowerBus { get; }
        }

        [BurstCompile]
        private struct AdmissionJob : IJob
        {
            [ReadOnly]
            public NativeList<Entity> Buses;

            public SecondBusBoardingLookups Lookups;

            public ComponentLookup<SecondBusBoardingSession> Sessions;

            public NativeArray<int> Counters;

            public NativeArray<Entity> LastSession;

            public uint Frame;

            public void Execute()
            {
                NativeHashMap<Entity, byte> activeStops =
                    new NativeHashMap<Entity, byte>(math.max(1, Buses.Length), Allocator.Temp);
                try
                {
                    PrepareActiveSessions(ref activeStops);
                    AdmitFollowers(ref activeStops);
                }
                finally
                {
                    activeStops.Dispose();
                }
            }

            private void PrepareActiveSessions(ref NativeHashMap<Entity, byte> activeStops)
            {
                for (int i = 0; i < Buses.Length; i++)
                {
                    Entity follower = Buses[i];
                    if (!Sessions.HasComponent(follower) ||
                        !Sessions.IsComponentEnabled(follower))
                    {
                        continue;
                    }

                    SecondBusBoardingSession session = Sessions[follower];
                    if (session.Stop != Entity.Null)
                    {
                        if (!activeStops.TryAdd(session.Stop, 1))
                        {
                            // A stop is strictly one native lead plus one managed follower.
                            // Repair any duplicated transient state by releasing the later entry.
                            session.ReleaseRequested = 1;
                        }
                    }

                    bool valid = IsActiveContextValid(follower, session);
                    if (!valid)
                    {
                        session.ReleaseRequested = 1;
                    }

                    Sessions[follower] = session;

                    if (Lookups.PublicTransport.TryGetComponent(
                        follower,
                        out VehiclePublicTransport followerTransport))
                    {
                        // A synthetic follower must not look like a native boarding vehicle to
                        // vehicle AI. If it did, the AI would force-complete it short of the stop.
                        followerTransport.m_State &= ~(
                            PublicTransportFlags.Boarding |
                            PublicTransportFlags.Arriving |
                            PublicTransportFlags.Testing |
                            PublicTransportFlags.RequireStop);
                        followerTransport.m_State |= PublicTransportFlags.EnRoute;
                        Lookups.PublicTransport[follower] = followerTransport;
                    }

                    if (valid && IsLeadStillBoarding(follower, session))
                    {
                        BoardingVehicle slot = Lookups.BoardingVehicle[session.Stop];
                        slot.m_Vehicle = session.LeadBus;
                        if (slot.m_Testing == follower)
                        {
                            slot.m_Testing = Entity.Null;
                        }

                        Lookups.BoardingVehicle[session.Stop] = slot;
                    }
                }
            }

            private void AdmitFollowers(ref NativeHashMap<Entity, byte> activeStops)
            {
                for (int i = 0; i < Buses.Length; i++)
                {
                    Entity follower = Buses[i];
                    if (!Sessions.HasComponent(follower) ||
                        Sessions.IsComponentEnabled(follower) ||
                        !Lookups.IsBus(follower) ||
                        !Lookups.TryGetStop(follower, out Entity stop) ||
                        activeStops.ContainsKey(stop))
                    {
                        continue;
                    }

                    BoardingVehicle slot = Lookups.BoardingVehicle[stop];
                    if (slot.m_Vehicle == Entity.Null ||
                        slot.m_Vehicle == follower)
                    {
                        continue;
                    }

                    VehiclePublicTransport transport = Lookups.PublicTransport[follower];
                    bool exactTestingPair = slot.m_Testing == follower;
                    // Vanilla clears the one-frame testing slot and leaves a blocked bus in
                    // Arriving while the lead owns the stop. Recover that same follower once it
                    // has safely slowed instead of raising the 1 m/s synthetic-admission limit.
                    bool arrivingRetry = slot.m_Testing == Entity.Null &&
                        (transport.m_State &
                            (PublicTransportFlags.EnRoute |
                             PublicTransportFlags.Arriving |
                             PublicTransportFlags.RequireStop)) ==
                            (PublicTransportFlags.EnRoute |
                             PublicTransportFlags.Arriving |
                             PublicTransportFlags.RequireStop);
                    if (!exactTestingPair && !arrivingRetry)
                    {
                        continue;
                    }

                    if (exactTestingPair)
                    {
                        Counters[kPairCandidates]++;
                    }

                    Entity lead = slot.m_Vehicle;
                    if (!CanShareStop(follower, lead, stop, out Entity route))
                    {
                        if (exactTestingPair)
                        {
                            Counters[kRejectedContext]++;
                        }

                        continue;
                    }

                    Moving followerMoving = Lookups.Moving[follower];
                    if (!SecondBusBoardingPolicy.IsFollowerSlow(
                        followerMoving.m_Velocity))
                    {
                        if (exactTestingPair)
                        {
                            Counters[kRejectedMoving]++;
                        }

                        continue;
                    }

                    Transform leadTransform = Lookups.Transform[lead];
                    Transform followerTransform = Lookups.Transform[follower];
                    if (!SecondBusBoardingPolicy.IsPairClose(
                        leadTransform.m_Position,
                        followerTransform.m_Position))
                    {
                        if (exactTestingPair)
                        {
                            Counters[kRejectedDistance]++;
                        }

                        continue;
                    }

                    transport.m_State &= ~(
                        PublicTransportFlags.Boarding |
                        PublicTransportFlags.Arriving |
                        PublicTransportFlags.Testing |
                        PublicTransportFlags.RequireStop);
                    transport.m_State |= PublicTransportFlags.EnRoute;
                    transport.m_DepartureFrame = Frame + 64u;
                    transport.m_MaxBoardingDistance = 0f;
                    transport.m_MinWaitingDistance = float.MaxValue;
                    Lookups.PublicTransport[follower] = transport;

                    slot.m_Testing = Entity.Null;
                    slot.m_Vehicle = lead;
                    Lookups.BoardingVehicle[stop] = slot;

                    Sessions[follower] = new SecondBusBoardingSession
                    {
                        Stop = stop,
                        LeadBus = lead,
                        Route = route,
                        AdmittedFrame = Frame,
                        LastRatchetFrame = Frame,
                        LastPassengerCount = Lookups.GetPassengerCount(follower),
                    };
                    Sessions.SetComponentEnabled(follower, true);
                    activeStops.TryAdd(stop, 1);
                    Counters[kSessionsStarted]++;
                    if (arrivingRetry)
                    {
                        Counters[kArrivingSessionsStarted]++;
                    }

                    LastSession[0] = route;
                    LastSession[1] = stop;
                    LastSession[2] = lead;
                    LastSession[3] = follower;
                }
            }

            private bool CanShareStop(
                Entity follower,
                Entity lead,
                Entity stop,
                out Entity route)
            {
                route = Entity.Null;
                if (!Lookups.IsBus(lead) ||
                    !Lookups.PublicTransport.TryGetComponent(
                        follower,
                        out VehiclePublicTransport followerTransport) ||
                    !Lookups.PublicTransport.TryGetComponent(
                        lead,
                        out VehiclePublicTransport leadTransport) ||
                    !Lookups.Transform.HasComponent(lead) ||
                    !Lookups.Transform.HasComponent(follower) ||
                    !Lookups.Moving.HasComponent(follower) ||
                    !Lookups.TryGetStop(lead, out Entity leadStop) ||
                    leadStop != stop ||
                    !Lookups.AreSameRoute(follower, lead, out route))
                {
                    return false;
                }

                bool leadBoarding =
                    (leadTransport.m_State &
                        (PublicTransportFlags.EnRoute | PublicTransportFlags.Boarding)) ==
                    (PublicTransportFlags.EnRoute | PublicTransportFlags.Boarding);
                bool followerApproaching =
                    (followerTransport.m_State & PublicTransportFlags.EnRoute) != 0 &&
                    (followerTransport.m_State &
                        (PublicTransportFlags.Testing |
                         PublicTransportFlags.RequireStop)) != 0;
                return leadBoarding && followerApproaching;
            }

            private bool IsActiveContextValid(
                Entity follower,
                SecondBusBoardingSession session)
            {
                if (!Lookups.IsBus(follower) ||
                    !Lookups.IsUsable(session.Stop) ||
                    !Lookups.BoardingVehicle.HasComponent(session.Stop) ||
                    !Lookups.TryGetStop(follower, out Entity followerStop) ||
                    followerStop != session.Stop ||
                    !Lookups.CurrentRoute.TryGetComponent(
                        follower,
                        out CurrentRoute currentRoute) ||
                    currentRoute.m_Route != session.Route)
                {
                    return false;
                }

                BoardingVehicle slot = Lookups.BoardingVehicle[session.Stop];
                if (slot.m_Vehicle != Entity.Null &&
                    slot.m_Vehicle != follower &&
                    slot.m_Vehicle != session.LeadBus)
                {
                    return false;
                }

                return true;
            }

            private bool IsLeadStillBoarding(
                Entity follower,
                SecondBusBoardingSession session)
            {
                if (!Lookups.IsBus(session.LeadBus) ||
                    !Lookups.PublicTransport.TryGetComponent(
                        session.LeadBus,
                        out VehiclePublicTransport leadTransport) ||
                    !Lookups.TryGetStop(session.LeadBus, out Entity leadStop) ||
                    leadStop != session.Stop ||
                    !Lookups.AreSameRoute(follower, session.LeadBus, out Entity route) ||
                    route != session.Route)
                {
                    return false;
                }

                return (leadTransport.m_State & PublicTransportFlags.Boarding) != 0;
            }
        }
    }
}
