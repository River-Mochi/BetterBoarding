// <copyright file="SecondBusBoardingDistributionSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/SecondBusBoardingDistributionSystem.cs
// Purpose: Presents one safe passenger-facing stop slot and returns followers to vanilla.

namespace BetterBoarding
{
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
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

    public sealed partial class SecondBusBoardingDistributionSystem : GameSystemBase
    {
        private const int kLeadDepartedReleases = 0;
        private const int kWindowReleases = 1;
        private const int kHardReleases = 2;
        private const int kInvalidReleases = 3;
        private const int kDisabledReleases = 4;
        private const int kSessionsWithPassengerChanges = 5;
        private const int kBoardedNet = 6;
        private const int kAlightedNet = 7;
        private const int kCounterCount = 8;
        private const int kRecentOutcomeCapacity = 8;
        private const int kOutcomeWriteIndex = 0;
        private const int kOutcomeCount = 1;

        private EntityQuery m_ActiveSessions;

        private SimulationSystem? m_SimulationSystem;

        private SecondBusBoardingLookups m_Lookups;

        private ComponentLookup<SecondBusBoardingSession> m_Sessions;

        private NativeArray<int> m_Counters;

        private NativeArray<SessionOutcome> m_RecentOutcomes;

        private NativeArray<int> m_OutcomeState;

        private JobHandle m_PreviousJob;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 16;
        }

        public override int GetUpdateOffset(SystemUpdatePhase phase)
        {
            // Run on the exact TransportCarAISystem tick. UpdateAfter places this
            // system after the bus AI but does not inherit its interval or offset.
            return 1;
        }

        [Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            m_ActiveSessions = GetEntityQuery(
                ComponentType.ReadWrite<SecondBusBoardingSession>(),
                ComponentType.ReadWrite<VehiclePublicTransport>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<Temp>());
            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
            m_Lookups = SecondBusBoardingLookups.Create(this);
            m_Sessions = GetComponentLookup<SecondBusBoardingSession>(false);
            m_Counters = new NativeArray<int>(kCounterCount, Allocator.Persistent);
            m_RecentOutcomes =
                new NativeArray<SessionOutcome>(kRecentOutcomeCapacity, Allocator.Persistent);
            m_OutcomeState = new NativeArray<int>(2, Allocator.Persistent);
            RequireForUpdate(m_ActiveSessions);
        }

        [Preserve]
        protected override void OnDestroy()
        {
            m_PreviousJob.Complete();
            if (m_Counters.IsCreated)
            {
                m_Counters.Dispose();
            }

            if (m_RecentOutcomes.IsCreated)
            {
                m_RecentOutcomes.Dispose();
            }

            if (m_OutcomeState.IsCreated)
            {
                m_OutcomeState.Dispose();
            }

            base.OnDestroy();
        }

        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);
            ReleaseAllActiveSessions();
            ResetCounters();
        }

        [Preserve]
        protected override void OnUpdate()
        {
            m_Lookups.Update(this);
            m_Sessions.Update(this);
            NativeList<Entity> followers =
                m_ActiveSessions.ToEntityListAsync(
                    Allocator.TempJob,
                    out JobHandle listed);

            JobHandle handle = new DistributionJob
            {
                Followers = followers,
                Lookups = m_Lookups,
                Sessions = m_Sessions,
                Counters = m_Counters,
                RecentOutcomes = m_RecentOutcomes,
                OutcomeState = m_OutcomeState,
                Frame = m_SimulationSystem?.frameIndex ?? 0u,
                FeatureEnabled = SecondBusBoardingCompatibility.EffectiveEnabled,
            }.Schedule(JobHandle.CombineDependencies(Dependency, listed));

            handle = followers.Dispose(handle);
            m_PreviousJob = handle;
            Dependency = handle;
        }

        public StatisticsSnapshot GetStatisticsSnapshot()
        {
            m_PreviousJob.Complete();
            using NativeArray<Entity> active =
                m_ActiveSessions.ToEntityArray(Allocator.Temp);
            int outcomeCount = m_OutcomeState[kOutcomeCount];
            SessionOutcomeSnapshot[] recent = new SessionOutcomeSnapshot[outcomeCount];
            int writeIndex = m_OutcomeState[kOutcomeWriteIndex];
            for (int i = 0; i < outcomeCount; i++)
            {
                int index = writeIndex - 1 - i;
                if (index < 0)
                {
                    index += kRecentOutcomeCapacity;
                }

                SessionOutcome outcome = m_RecentOutcomes[index];
                recent[i] = new SessionOutcomeSnapshot(
                    outcome.Route,
                    outcome.Stop,
                    outcome.LeadBus,
                    outcome.FollowerBus,
                    outcome.DurationFrames,
                    outcome.LeadActiveUpdates,
                    outcome.FollowerSlotPresentations,
                    outcome.SawWaitingPassenger != 0,
                    outcome.ClosestWaitingDistance,
                    outcome.ConcurrentBoarded,
                    outcome.ConcurrentAlighted,
                    outcome.Reason);
            }

            return new StatisticsSnapshot(
                active.Length,
                m_Counters[kLeadDepartedReleases],
                m_Counters[kWindowReleases],
                m_Counters[kHardReleases],
                m_Counters[kInvalidReleases],
                m_Counters[kDisabledReleases],
                m_Counters[kSessionsWithPassengerChanges],
                m_Counters[kBoardedNet],
                m_Counters[kAlightedNet],
                recent);
        }

        public void RestartStatisticsCollection()
        {
            m_PreviousJob.Complete();
            ResetCounters();
        }

        public void ReleaseAllActiveSessions()
        {
            m_PreviousJob.Complete();
            using NativeArray<Entity> followers =
                m_ActiveSessions.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < followers.Length; i++)
            {
                Entity follower = followers[i];
                if (!EntityManager.Exists(follower) ||
                    !EntityManager.HasComponent<SecondBusBoardingSession>(follower))
                {
                    continue;
                }

                SecondBusBoardingSession session =
                    EntityManager.GetComponentData<SecondBusBoardingSession>(follower);
                RestoreFollowerToVanilla(follower, session);
                EntityManager.SetComponentEnabled<SecondBusBoardingSession>(follower, false);
            }
        }

        private void RestoreFollowerToVanilla(
            Entity follower,
            SecondBusBoardingSession session)
        {
            if (EntityManager.HasComponent<VehiclePublicTransport>(follower))
            {
                VehiclePublicTransport transport =
                    EntityManager.GetComponentData<VehiclePublicTransport>(follower);
                transport.m_State &= ~(
                    PublicTransportFlags.Boarding |
                    PublicTransportFlags.Arriving);
                transport.m_State |=
                    PublicTransportFlags.EnRoute |
                    PublicTransportFlags.Testing |
                    PublicTransportFlags.RequireStop;
                transport.m_DepartureFrame = 0u;
                transport.m_MaxBoardingDistance = float.MaxValue;
                transport.m_MinWaitingDistance = float.MaxValue;
                EntityManager.SetComponentData(follower, transport);
            }

            if (session.Stop != Entity.Null &&
                EntityManager.Exists(session.Stop) &&
                EntityManager.HasComponent<BoardingVehicle>(session.Stop))
            {
                BoardingVehicle slot =
                    EntityManager.GetComponentData<BoardingVehicle>(session.Stop);
                if (slot.m_Vehicle == follower)
                {
                    slot.m_Vehicle = Entity.Null;
                }

                if (slot.m_Testing == Entity.Null || slot.m_Testing == follower)
                {
                    slot.m_Testing = follower;
                }

                EntityManager.SetComponentData(session.Stop, slot);
            }
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

            for (int i = 0; i < m_RecentOutcomes.Length; i++)
            {
                m_RecentOutcomes[i] = default;
            }

            for (int i = 0; i < m_OutcomeState.Length; i++)
            {
                m_OutcomeState[i] = 0;
            }
        }

        public enum SessionReleaseReason : byte
        {
            LeadDeparted,
            WindowEnded,
            HardSafety,
            Invalid,
            OptionOrConflict,
        }

        public readonly struct SessionOutcomeSnapshot
        {
            public SessionOutcomeSnapshot(
                Entity route,
                Entity stop,
                Entity leadBus,
                Entity followerBus,
                uint durationFrames,
                int leadActiveUpdates,
                int followerSlotPresentations,
                bool sawWaitingPassenger,
                float closestWaitingDistance,
                int concurrentBoarded,
                int concurrentAlighted,
                SessionReleaseReason reason)
            {
                Route = route;
                Stop = stop;
                LeadBus = leadBus;
                FollowerBus = followerBus;
                DurationFrames = durationFrames;
                LeadActiveUpdates = leadActiveUpdates;
                FollowerSlotPresentations = followerSlotPresentations;
                SawWaitingPassenger = sawWaitingPassenger;
                ClosestWaitingDistance = closestWaitingDistance;
                ConcurrentBoarded = concurrentBoarded;
                ConcurrentAlighted = concurrentAlighted;
                Reason = reason;
            }

            public Entity Route { get; }

            public Entity Stop { get; }

            public Entity LeadBus { get; }

            public Entity FollowerBus { get; }

            public uint DurationFrames { get; }

            public int LeadActiveUpdates { get; }

            public int FollowerSlotPresentations { get; }

            public bool SawWaitingPassenger { get; }

            public float ClosestWaitingDistance { get; }

            public int ConcurrentBoarded { get; }

            public int ConcurrentAlighted { get; }

            public SessionReleaseReason Reason { get; }
        }

        private struct SessionOutcome
        {
            public Entity Route;

            public Entity Stop;

            public Entity LeadBus;

            public Entity FollowerBus;

            public uint DurationFrames;

            public int LeadActiveUpdates;

            public int FollowerSlotPresentations;

            public float ClosestWaitingDistance;

            public int ConcurrentBoarded;

            public int ConcurrentAlighted;

            public byte SawWaitingPassenger;

            public SessionReleaseReason Reason;
        }

        public readonly struct StatisticsSnapshot
        {
            public StatisticsSnapshot(
                int activeSessions,
                int leadDepartedReleases,
                int windowReleases,
                int hardReleases,
                int invalidReleases,
                int disabledReleases,
                int sessionsWithPassengerChanges,
                int boardedNet,
                int alightedNet,
                SessionOutcomeSnapshot[] recentOutcomes)
            {
                ActiveSessions = activeSessions;
                LeadDepartedReleases = leadDepartedReleases;
                WindowReleases = windowReleases;
                HardReleases = hardReleases;
                InvalidReleases = invalidReleases;
                DisabledReleases = disabledReleases;
                SessionsWithPassengerChanges = sessionsWithPassengerChanges;
                BoardedNet = boardedNet;
                AlightedNet = alightedNet;
                RecentOutcomes = recentOutcomes;
            }

            public int ActiveSessions { get; }

            public int LeadDepartedReleases { get; }

            public int WindowReleases { get; }

            public int HardReleases { get; }

            public int InvalidReleases { get; }

            public int DisabledReleases { get; }

            public int SessionsWithPassengerChanges { get; }

            public int BoardedNet { get; }

            public int AlightedNet { get; }

            public SessionOutcomeSnapshot[] RecentOutcomes { get; }
        }

        [BurstCompile]
        private struct DistributionJob : IJob
        {
            [ReadOnly]
            public NativeList<Entity> Followers;

            public SecondBusBoardingLookups Lookups;

            public ComponentLookup<SecondBusBoardingSession> Sessions;

            public NativeArray<int> Counters;

            public NativeArray<SessionOutcome> RecentOutcomes;

            public NativeArray<int> OutcomeState;

            public uint Frame;

            public bool FeatureEnabled;

            public void Execute()
            {
                for (int i = 0; i < Followers.Length; i++)
                {
                    UpdateSession(Followers[i]);
                }
            }

            private void UpdateSession(Entity follower)
            {
                SecondBusBoardingSession session = Sessions[follower];
                if (!FeatureEnabled)
                {
                    Release(follower, session, SessionReleaseReason.OptionOrConflict);
                    return;
                }

                if (session.ReleaseRequested != 0 ||
                    !IsFollowerContextValid(follower, session))
                {
                    Release(follower, session, SessionReleaseReason.Invalid);
                    return;
                }

                bool leadActive = IsLeadStillBoarding(follower, session);
                if (leadActive)
                {
                    session.LeadActiveUpdates++;
                    // A delta first observed after the lead leaves may be ordinary
                    // single-bus service, so it is not proof of concurrent exchange.
                    TrackPassengerChanges(follower, ref session);
                }

                uint age = Frame >= session.AdmittedFrame
                    ? Frame - session.AdmittedFrame
                    : 0u;
                bool windowEnded = age >= SecondBusBoardingPolicy.SessionWindowFrames;
                bool hardDeadline = age >= SecondBusBoardingPolicy.HardReleaseFrames;
                bool followerReady = Lookups.ArePassengersReady(follower);

                if ((!leadActive || windowEnded) && (followerReady || hardDeadline))
                {
                    SessionReleaseReason reason = hardDeadline && !followerReady
                        ? SessionReleaseReason.HardSafety
                        : leadActive
                            ? SessionReleaseReason.WindowEnded
                            : SessionReleaseReason.LeadDeparted;
                    Release(follower, session, reason);
                    return;
                }

                VehiclePublicTransport followerTransport =
                    Lookups.PublicTransport[follower];
                followerTransport.m_State &= ~(
                    PublicTransportFlags.Arriving |
                    PublicTransportFlags.Testing |
                    PublicTransportFlags.RequireStop);
                followerTransport.m_State |=
                    PublicTransportFlags.EnRoute |
                    PublicTransportFlags.Boarding;

                if (Frame - session.LastRatchetFrame >=
                    SecondBusBoardingPolicy.BoardingRatchetFrames)
                {
                    float observedMinimum = followerTransport.m_MinWaitingDistance;
                    if (observedMinimum != float.MaxValue && observedMinimum > 0f)
                    {
                        session.SawWaitingPassenger = 1;
                        session.ClosestWaitingDistance =
                            math.min(session.ClosestWaitingDistance, observedMinimum);
                    }

                    if (session.HasPresentedBoardingWindow != 0)
                    {
                        followerTransport.m_MaxBoardingDistance =
                            observedMinimum == float.MaxValue || observedMinimum == 0f
                                ? float.MaxValue
                                : observedMinimum + 1f;
                    }
                    else
                    {
                        session.HasPresentedBoardingWindow = 1;
                    }

                    followerTransport.m_MinWaitingDistance = float.MaxValue;
                    session.LastRatchetFrame = Frame;
                }

                Lookups.PublicTransport[follower] = followerTransport;

                BoardingVehicle slot = Lookups.BoardingVehicle[session.Stop];
                if (slot.m_Vehicle != Entity.Null &&
                    slot.m_Vehicle != follower &&
                    slot.m_Vehicle != session.LeadBus)
                {
                    Release(follower, session, SessionReleaseReason.Invalid);
                    return;
                }

                Entity selected = follower;
                if (leadActive && !Lookups.ArePassengersReady(session.LeadBus))
                {
                    // A cim already climbing into the lead can finish only while the single
                    // passenger-facing slot still names that lead bus.
                    selected = session.LeadBus;
                }

                if (slot.m_Vehicle == Entity.Null ||
                    slot.m_Vehicle == follower ||
                    slot.m_Vehicle == session.LeadBus)
                {
                    slot.m_Vehicle = selected;
                    if (slot.m_Testing == follower)
                    {
                        slot.m_Testing = Entity.Null;
                    }

                    Lookups.BoardingVehicle[session.Stop] = slot;
                    if (selected == follower)
                    {
                        session.FollowerSlotPresentations++;
                    }
                }

                Sessions[follower] = session;
            }

            private void TrackPassengerChanges(
                Entity follower,
                ref SecondBusBoardingSession session)
            {
                int passengerCount = Lookups.GetPassengerCount(follower);
                int delta = passengerCount - session.LastPassengerCount;
                if (delta == 0)
                {
                    return;
                }

                if (session.SawPassengerCountChange == 0)
                {
                    session.SawPassengerCountChange = 1;
                    Counters[kSessionsWithPassengerChanges]++;
                }

                if (delta > 0)
                {
                    Counters[kBoardedNet] += delta;
                    session.ConcurrentBoarded += delta;
                }
                else
                {
                    Counters[kAlightedNet] -= delta;
                    session.ConcurrentAlighted -= delta;
                }

                session.LastPassengerCount = passengerCount;
            }

            private bool IsFollowerContextValid(
                Entity follower,
                SecondBusBoardingSession session)
            {
                return Lookups.IsBus(follower) &&
                    Lookups.IsUsable(session.Stop) &&
                    Lookups.BoardingVehicle.HasComponent(session.Stop) &&
                    Lookups.PublicTransport.HasComponent(follower) &&
                    Lookups.TryGetStop(follower, out Entity stop) &&
                    stop == session.Stop &&
                    Lookups.CurrentRoute.TryGetComponent(
                        follower,
                        out CurrentRoute route) &&
                    route.m_Route == session.Route;
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

            private void Release(
                Entity follower,
                SecondBusBoardingSession session,
                SessionReleaseReason reason)
            {
                RecordOutcome(follower, session, reason);

                if (Lookups.PublicTransport.TryGetComponent(
                    follower,
                    out VehiclePublicTransport transport))
                {
                    transport.m_State &= ~(
                        PublicTransportFlags.Boarding |
                        PublicTransportFlags.Arriving);
                    transport.m_State |=
                        PublicTransportFlags.EnRoute |
                        PublicTransportFlags.Testing |
                        PublicTransportFlags.RequireStop;
                    transport.m_DepartureFrame = 0u;
                    transport.m_MaxBoardingDistance = float.MaxValue;
                    transport.m_MinWaitingDistance = float.MaxValue;
                    Lookups.PublicTransport[follower] = transport;
                }

                if (Lookups.IsUsable(session.Stop) &&
                    Lookups.BoardingVehicle.HasComponent(session.Stop))
                {
                    BoardingVehicle slot = Lookups.BoardingVehicle[session.Stop];
                    if (slot.m_Vehicle == follower)
                    {
                        slot.m_Vehicle = Entity.Null;
                    }

                    if (slot.m_Testing == Entity.Null || slot.m_Testing == follower)
                    {
                        slot.m_Testing = follower;
                    }

                    Lookups.BoardingVehicle[session.Stop] = slot;
                }

                Sessions.SetComponentEnabled(follower, false);
                switch (reason)
                {
                    case SessionReleaseReason.LeadDeparted:
                        Counters[kLeadDepartedReleases]++;
                        break;
                    case SessionReleaseReason.WindowEnded:
                        Counters[kWindowReleases]++;
                        break;
                    case SessionReleaseReason.HardSafety:
                        Counters[kHardReleases]++;
                        break;
                    case SessionReleaseReason.Invalid:
                        Counters[kInvalidReleases]++;
                        break;
                    case SessionReleaseReason.OptionOrConflict:
                        Counters[kDisabledReleases]++;
                        break;
                }
            }

            private void RecordOutcome(
                Entity follower,
                SecondBusBoardingSession session,
                SessionReleaseReason reason)
            {
                int writeIndex = OutcomeState[kOutcomeWriteIndex];
                uint duration = Frame >= session.AdmittedFrame
                    ? Frame - session.AdmittedFrame
                    : 0u;
                RecentOutcomes[writeIndex] = new SessionOutcome
                {
                    Route = session.Route,
                    Stop = session.Stop,
                    LeadBus = session.LeadBus,
                    FollowerBus = follower,
                    DurationFrames = duration,
                    LeadActiveUpdates = session.LeadActiveUpdates,
                    FollowerSlotPresentations = session.FollowerSlotPresentations,
                    ClosestWaitingDistance = session.ClosestWaitingDistance,
                    ConcurrentBoarded = session.ConcurrentBoarded,
                    ConcurrentAlighted = session.ConcurrentAlighted,
                    SawWaitingPassenger = session.SawWaitingPassenger,
                    Reason = reason,
                };

                OutcomeState[kOutcomeWriteIndex] =
                    (writeIndex + 1) % kRecentOutcomeCapacity;
                OutcomeState[kOutcomeCount] =
                    math.min(kRecentOutcomeCapacity, OutcomeState[kOutcomeCount] + 1);
            }
        }
    }
}
