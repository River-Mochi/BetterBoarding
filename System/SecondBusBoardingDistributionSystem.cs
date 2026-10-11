// <copyright file="SecondBusBoardingDistributionSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/SecondBusBoardingDistributionSystem.cs
// Purpose: Presents one safe passenger-facing stop slot and completes managed follower service.

namespace BetterBoarding
{
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Creatures;
    using Game.Pathfind;
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
        private const int kManagedCompletions = 0;
        private const int kWindowCompletions = 1;
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

        private ComponentLookup<PathOwner> m_PathOwners;

        private ComponentLookup<Waypoint> m_Waypoints;

        private BufferLookup<RouteWaypoint> m_RouteWaypoints;

        private ComponentLookup<VehicleTiming> m_VehicleTimings;

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
            m_PathOwners = GetComponentLookup<PathOwner>(false);
            m_Waypoints = GetComponentLookup<Waypoint>(true);
            m_RouteWaypoints = GetBufferLookup<RouteWaypoint>(true);
            m_VehicleTimings = GetComponentLookup<VehicleTiming>(false);
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
            m_PathOwners.Update(this);
            m_Waypoints.Update(this);
            m_RouteWaypoints.Update(this);
            m_VehicleTimings.Update(this);
            NativeList<Entity> followers =
                m_ActiveSessions.ToEntityListAsync(
                    Allocator.TempJob,
                    out JobHandle listed);

            JobHandle handle = new DistributionJob
            {
                Followers = followers,
                Lookups = m_Lookups,
                Sessions = m_Sessions,
                PathOwners = m_PathOwners,
                Waypoints = m_Waypoints,
                RouteWaypoints = m_RouteWaypoints,
                VehicleTimings = m_VehicleTimings,
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
                    outcome.ConcurrentEnteringPeak,
                    outcome.ConcurrentExitingPeak,
                    outcome.Reason);
            }

            return new StatisticsSnapshot(
                active.Length,
                m_Counters[kManagedCompletions],
                m_Counters[kWindowCompletions],
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
                RepayExtraDwellMainThread(session);
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

        private void RepayExtraDwellMainThread(SecondBusBoardingSession session)
        {
            uint frame = m_SimulationSystem?.frameIndex ?? 0u;
            uint age = frame >= session.AdmittedFrame
                ? frame - session.AdmittedFrame
                : 0u;
            if (age <= SecondBusBoardingPolicy.ManagedDepartureFrames ||
                session.Waypoint == Entity.Null ||
                !EntityManager.Exists(session.Waypoint) ||
                !EntityManager.HasComponent<VehicleTiming>(session.Waypoint))
            {
                return;
            }

            VehicleTiming timing =
                EntityManager.GetComponentData<VehicleTiming>(session.Waypoint);
            float repayment = age - SecondBusBoardingPolicy.ManagedDepartureFrames;
            timing.m_AverageTravelTime =
                math.max(0f, timing.m_AverageTravelTime - repayment);
            EntityManager.SetComponentData(session.Waypoint, timing);
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
            ServiceCompleted,
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
                int concurrentEnteringPeak,
                int concurrentExitingPeak,
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
                ConcurrentEnteringPeak = concurrentEnteringPeak;
                ConcurrentExitingPeak = concurrentExitingPeak;
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

            public int ConcurrentEnteringPeak { get; }

            public int ConcurrentExitingPeak { get; }

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

            public int ConcurrentEnteringPeak;

            public int ConcurrentExitingPeak;

            public byte SawWaitingPassenger;

            public SessionReleaseReason Reason;
        }

        public readonly struct StatisticsSnapshot
        {
            public StatisticsSnapshot(
                int activeSessions,
                int managedCompletions,
                int windowCompletions,
                int hardReleases,
                int invalidReleases,
                int disabledReleases,
                int sessionsWithPassengerChanges,
                int boardedNet,
                int alightedNet,
                SessionOutcomeSnapshot[] recentOutcomes)
            {
                ActiveSessions = activeSessions;
                ManagedCompletions = managedCompletions;
                WindowCompletions = windowCompletions;
                HardReleases = hardReleases;
                InvalidReleases = invalidReleases;
                DisabledReleases = disabledReleases;
                SessionsWithPassengerChanges = sessionsWithPassengerChanges;
                BoardedNet = boardedNet;
                AlightedNet = alightedNet;
                RecentOutcomes = recentOutcomes;
            }

            public int ActiveSessions { get; }

            public int ManagedCompletions { get; }

            public int WindowCompletions { get; }

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

            public ComponentLookup<PathOwner> PathOwners;

            [ReadOnly]
            public ComponentLookup<Waypoint> Waypoints;

            [ReadOnly]
            public BufferLookup<RouteWaypoint> RouteWaypoints;

            public ComponentLookup<VehicleTiming> VehicleTimings;

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
                }

                // The follower remains a managed stop session after the native lead departs.
                // Otherwise vanilla makes it pull forward and repeat this same stop before it can
                // exchange passengers, which defeats concurrent boarding.
                TrackPassengerChanges(follower, ref session);
                TrackConcurrentPassengerAnimations(follower, ref session);

                uint age = Frame >= session.AdmittedFrame
                    ? Frame - session.AdmittedFrame
                    : 0u;
                bool windowEnded = age >= SecondBusBoardingPolicy.SessionWindowFrames;
                bool hardDeadline = age >= SecondBusBoardingPolicy.HardReleaseFrames;

                VehiclePublicTransport followerTransport =
                    Lookups.PublicTransport[follower];
                followerTransport.m_State &= ~(
                    PublicTransportFlags.Arriving |
                    PublicTransportFlags.Testing |
                    PublicTransportFlags.RequireStop);
                followerTransport.m_State |=
                    PublicTransportFlags.EnRoute |
                    PublicTransportFlags.Boarding;

                if (session.PreviousFollowerSelected != 0 &&
                    Frame - session.LastRatchetFrame >=
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

                if (windowEnded)
                {
                    // Close the doors before departure so no new cim starts boarding while an
                    // in-flight passenger is finishing the animation.
                    session.DoorsClosing = 1;
                    followerTransport.m_MaxBoardingDistance = 0f;
                }

                bool followerReady = Lookups.ArePassengersReady(follower);
                bool normalCompletion = session.DoorsClosing == 0 &&
                    session.HasPresentedBoardingWindow != 0 &&
                    session.FollowerSlotPresentations > 0 &&
                    Frame >= followerTransport.m_DepartureFrame &&
                    followerTransport.m_MaxBoardingDistance == float.MaxValue &&
                    followerReady;
                bool closedCompletion = session.DoorsClosing != 0 &&
                    session.HasPresentedBoardingWindow != 0 &&
                    followerReady;
                if ((normalCompletion || closedCompletion) &&
                    TryAdvanceToNextWaypoint(follower, session))
                {
                    Release(
                        follower,
                        session,
                        normalCompletion
                            ? SessionReleaseReason.ServiceCompleted
                            : SessionReleaseReason.WindowEnded,
                        completedStop: true);
                    return;
                }

                if (hardDeadline)
                {
                    // If managed completion cannot safely advance the route, give the vehicle back
                    // to vanilla at this stop instead of risking a stranded bus or passenger.
                    Release(
                        follower,
                        session,
                        SessionReleaseReason.HardSafety,
                        completedStop: false);
                    return;
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

                bool followerSelected;
                if (session.FollowerSelected != 0 && !followerReady)
                {
                    // A cim already climbing into the follower must finish before the slot moves.
                    followerSelected = true;
                }
                else if (session.FollowerSelected == 0 &&
                    leadActive &&
                    !Lookups.ArePassengersReady(session.LeadBus))
                {
                    // Apply the same safety rule to the native lead.
                    followerSelected = false;
                }
                else if (!leadActive)
                {
                    followerSelected = true;
                }
                else
                {
                    // Passenger processing happens after vehicle AI. Alternate from the previous
                    // resident sweep so a newly admitted follower gets the first safe opportunity.
                    followerSelected = session.FollowerSelected == 0;
                }

                session.FollowerSelected = followerSelected ? (byte)1 : (byte)0;
                Entity selected = followerSelected ? follower : session.LeadBus;

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

                    // Passenger-count changes are observed on the following system tick. Remember
                    // whether this follower-facing resident sweep was genuinely concurrent so the
                    // report does not credit exchange that happened after the lead had departed.
                    session.PassengerWindowLeadActive =
                        selected == follower && leadActive ? (byte)1 : (byte)0;
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
                    if (session.PassengerWindowLeadActive != 0)
                    {
                        session.ConcurrentBoarded += delta;
                    }
                }
                else
                {
                    Counters[kAlightedNet] -= delta;
                    if (session.PassengerWindowLeadActive != 0)
                    {
                        session.ConcurrentAlighted -= delta;
                    }
                }

                session.LastPassengerCount = passengerCount;
            }

            private void TrackConcurrentPassengerAnimations(
                Entity follower,
                ref SecondBusBoardingSession session)
            {
                if (session.PassengerWindowLeadActive == 0 ||
                    !Lookups.Passengers.TryGetBuffer(
                        follower,
                        out DynamicBuffer<Passenger> passengers))
                {
                    return;
                }

                int entering = 0;
                int exiting = 0;
                for (int i = 0; i < passengers.Length; i++)
                {
                    Entity passenger = passengers[i].m_Passenger;
                    if (!Lookups.CurrentVehicle.TryGetComponent(
                            passenger,
                            out CurrentVehicle currentVehicle) ||
                        currentVehicle.m_Vehicle != follower)
                    {
                        continue;
                    }

                    if ((currentVehicle.m_Flags & CreatureVehicleFlags.Entering) != 0)
                    {
                        entering++;
                    }

                    if ((currentVehicle.m_Flags & CreatureVehicleFlags.Exiting) != 0)
                    {
                        exiting++;
                    }
                }

                session.ConcurrentEnteringPeak =
                    math.max(session.ConcurrentEnteringPeak, entering);
                session.ConcurrentExitingPeak =
                    math.max(session.ConcurrentExitingPeak, exiting);
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

            private bool TryAdvanceToNextWaypoint(
                Entity follower,
                SecondBusBoardingSession session)
            {
                if (!PathOwners.TryGetComponent(follower, out var pathOwner) ||
                    !Lookups.Target.TryGetComponent(follower, out Target target) ||
                    !Waypoints.TryGetComponent(target.m_Target, out Waypoint waypoint) ||
                    !RouteWaypoints.TryGetBuffer(
                        session.Route,
                        out DynamicBuffer<RouteWaypoint> routeWaypoints) ||
                    routeWaypoints.Length == 0 ||
                    waypoint.m_Index < 0 ||
                    waypoint.m_Index >= routeWaypoints.Length ||
                    routeWaypoints[waypoint.m_Index].m_Waypoint != target.m_Target)
                {
                    return false;
                }

                Entity nextWaypoint =
                    routeWaypoints[(waypoint.m_Index + 1) % routeWaypoints.Length].m_Waypoint;
                if (nextWaypoint == Entity.Null ||
                    nextWaypoint == target.m_Target ||
                    !Lookups.IsUsable(nextWaypoint) ||
                    !Waypoints.HasComponent(nextWaypoint))
                {
                    return false;
                }

                VehicleUtils.SetTarget(ref pathOwner, ref target, nextWaypoint);
                PathOwners[follower] = pathOwner;
                Lookups.Target[follower] = target;
                return true;
            }

            private void Release(
                Entity follower,
                SecondBusBoardingSession session,
                SessionReleaseReason reason,
                bool completedStop = false)
            {
                RepayExtraDwell(session);
                RecordOutcome(follower, session, reason);

                if (Lookups.PublicTransport.TryGetComponent(
                    follower,
                    out VehiclePublicTransport transport))
                {
                    transport.m_State &= ~(
                        PublicTransportFlags.Boarding |
                        PublicTransportFlags.Arriving |
                        PublicTransportFlags.Testing |
                        PublicTransportFlags.RequireStop);
                    transport.m_State |= PublicTransportFlags.EnRoute;
                    if (!completedStop)
                    {
                        transport.m_State |=
                            PublicTransportFlags.Testing |
                            PublicTransportFlags.RequireStop;
                    }

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

                    if (!completedStop &&
                        (slot.m_Testing == Entity.Null || slot.m_Testing == follower))
                    {
                        slot.m_Testing = follower;
                    }
                    else if (completedStop && slot.m_Testing == follower)
                    {
                        slot.m_Testing = Entity.Null;
                    }

                    Lookups.BoardingVehicle[session.Stop] = slot;
                }

                Sessions.SetComponentEnabled(follower, false);
                switch (reason)
                {
                    case SessionReleaseReason.ServiceCompleted:
                        Counters[kManagedCompletions]++;
                        break;
                    case SessionReleaseReason.WindowEnded:
                        Counters[kWindowCompletions]++;
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

            private void RepayExtraDwell(SecondBusBoardingSession session)
            {
                uint age = Frame >= session.AdmittedFrame
                    ? Frame - session.AdmittedFrame
                    : 0u;
                if (age <= SecondBusBoardingPolicy.ManagedDepartureFrames ||
                    session.Waypoint == Entity.Null ||
                    !VehicleTimings.TryGetComponent(
                        session.Waypoint,
                        out VehicleTiming timing))
                {
                    return;
                }

                float repayment = age - SecondBusBoardingPolicy.ManagedDepartureFrames;
                timing.m_AverageTravelTime =
                    math.max(0f, timing.m_AverageTravelTime - repayment);
                VehicleTimings[session.Waypoint] = timing;
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
                    ConcurrentEnteringPeak = session.ConcurrentEnteringPeak,
                    ConcurrentExitingPeak = session.ConcurrentExitingPeak,
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
