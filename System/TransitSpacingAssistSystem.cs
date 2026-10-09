// <copyright file="TransitSpacingAssistSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/TransitSpacingAssistSystem.cs
// Purpose: Applies a bounded headway hold at one control stop per passenger line.

namespace BetterBoarding
{
    using System;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Routes;
    using Game.Simulation;
    using Game.Tools;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;

    /// <summary>
    /// Uses the first valid boarding stop on each passenger line as a control
    /// point. Consecutive vehicles are released closer to the line's target
    /// interval there, without changing the shared timing at every stop.
    /// </summary>
    public sealed partial class TransitSpacingAssistSystem : GameSystemBase
    {
        public const int UpdatesPerDay = 16384;

        private const uint kMaximumAddedHoldFrames = 512u;
        private const uint kSameVisitToleranceFrames = 512u;
        private const uint kMaximumMeasuredGapFrames = 524288u;
        private const int kLineStateCapacity = 4096;

        private const int kControlVisitCount = 0;
        private const int kHoldCount = 1;
        private const int kWriteCount = 2;
        private const int kTotalAddedHoldFrames = 3;
        private const int kMaximumAddedHold = 4;
        private const int kCappedHoldCount = 5;
        private const int kGapSampleCount = 6;
        private const int kTotalGapFrames = 7;
        private const int kTotalTargetGapFrames = 8;
        private const int kUnderHalfTargetCount = 9;
        private const int kBusVisitCount = 10;
        private const int kTramVisitCount = 11;
        private const int kTrainVisitCount = 12;
        private const int kSubwayVisitCount = 13;
        private const int kLineStateCapacityMissCount = 14;
        private const int kCounterCount = 15;

        private EntityQuery m_VehicleQuery;
        private SimulationSystem? m_SimulationSystem;
        private NativeArray<long> m_Counters;
        private NativeParallelHashMap<Entity, LineSpacingState> m_LineStates;
        private uint m_LastSimulationFrame = uint.MaxValue;
        private DateTime m_CollectionStartedLocalTime;
        private uint m_CollectionStartedSimulationFrame = uint.MaxValue;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            // A bunched follower can receive no vanilla dwell and leave at the
            // next 16-frame transport-AI pass. Matching that cadence ensures the
            // control point sees those short visits before release.
            return 262144 / UpdatesPerDay;
        }

        public override int GetUpdateOffset(SystemUpdatePhase phase)
        {
            // Cars update at offset 1 and trains at offset 3. Offset 0 observes
            // their new boarding state before either AI's next 16-frame pass.
            return 0;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
            m_Counters = new NativeArray<long>(
                kCounterCount,
                Allocator.Persistent,
                NativeArrayOptions.ClearMemory);
            m_LineStates = new NativeParallelHashMap<Entity, LineSpacingState>(
                kLineStateCapacity,
                Allocator.Persistent);

            m_VehicleQuery = SystemAPI.QueryBuilder()
                .WithAllRW<Game.Vehicles.PublicTransport>()
                .WithAll<CurrentRoute, Target>()
                .WithNone<Deleted, Destroyed, Temp, Overridden>()
                .Build();

            RequireForUpdate(m_VehicleQuery);
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (mode == GameMode.Game &&
                (purpose == Purpose.NewGame || purpose == Purpose.LoadGame))
            {
                if (BoardingRuntimeSettings.SpacingAssist && m_SimulationSystem != null)
                {
                    RestartStatisticsCollection(m_SimulationSystem.frameIndex);
                }
                else
                {
                    ClearStatisticsCollection();
                }
            }
        }

        protected override void OnUpdate()
        {
            if (!BoardingRuntimeSettings.SpacingAssist || m_SimulationSystem == null)
            {
                return;
            }

            uint frame = m_SimulationSystem.frameIndex;
            if (m_LastSimulationFrame != uint.MaxValue && frame < m_LastSimulationFrame)
            {
                // Defend against a city switch while the optional system was off.
                RestartStatisticsCollection(frame);
            }
            else if (m_CollectionStartedLocalTime == default)
            {
                RestartStatisticsCollection(frame);
            }

            m_LastSimulationFrame = frame;

            PaceControlPointJob job = new PaceControlPointJob
            {
                m_Frame = frame,
                m_RoutePrefabRefs = SystemAPI.GetComponentLookup<PrefabRef>(isReadOnly: true),
                m_TransportLineData = SystemAPI.GetComponentLookup<TransportLineData>(isReadOnly: true),
                m_TransportLines = SystemAPI.GetComponentLookup<TransportLine>(isReadOnly: true),
                m_Connected = SystemAPI.GetComponentLookup<Connected>(isReadOnly: true),
                m_BoardingVehicles = SystemAPI.GetComponentLookup<BoardingVehicle>(isReadOnly: true),
                m_RouteWaypoints = SystemAPI.GetBufferLookup<RouteWaypoint>(isReadOnly: true),
                m_LineStates = m_LineStates,
                m_Counters = m_Counters
            };

            // The route-state map is shared by vehicles on the same line. A
            // sequential Burst job avoids write races while keeping the scan off
            // the main thread; the work per boarding vehicle is very small.
            Dependency = job.Schedule(m_VehicleQuery, Dependency);
        }

        protected override void OnDestroy()
        {
            // Undo a live control-point hold on hot unload when the entity still
            // carries the exact value written by this system.
            ReleaseManagedHolds();

            if (m_LineStates.IsCreated)
            {
                m_LineStates.Dispose();
            }

            if (m_Counters.IsCreated)
            {
                m_Counters.Dispose();
            }

            base.OnDestroy();
        }

        internal StatisticsSnapshot GetStatisticsSnapshot()
        {
            Dependency.Complete();

            if (!m_Counters.IsCreated)
            {
                return default;
            }

            return new StatisticsSnapshot(
                m_Counters[kControlVisitCount],
                m_Counters[kHoldCount],
                m_Counters[kWriteCount],
                m_Counters[kTotalAddedHoldFrames],
                m_Counters[kMaximumAddedHold],
                m_Counters[kCappedHoldCount],
                m_Counters[kGapSampleCount],
                m_Counters[kTotalGapFrames],
                m_Counters[kTotalTargetGapFrames],
                m_Counters[kUnderHalfTargetCount],
                m_Counters[kBusVisitCount],
                m_Counters[kTramVisitCount],
                m_Counters[kTrainVisitCount],
                m_Counters[kSubwayVisitCount],
                m_Counters[kLineStateCapacityMissCount],
                m_CollectionStartedLocalTime,
                m_CollectionStartedSimulationFrame);
        }

        internal void RestartStatisticsCollection()
        {
            uint frame = m_SimulationSystem?.frameIndex ?? uint.MaxValue;
            RestartStatisticsCollection(frame);
        }

        internal void ReleaseManagedHolds()
        {
            Dependency.Complete();

            if (!m_LineStates.IsCreated)
            {
                return;
            }

            NativeArray<LineSpacingState> states =
                m_LineStates.GetValueArray(Allocator.Temp);
            try
            {
                for (int i = 0; i < states.Length; i++)
                {
                    LineSpacingState state = states[i];
                    if (state.m_HoldApplied == 0 ||
                        state.m_ActiveVehicle == Entity.Null ||
                        !EntityManager.Exists(state.m_ActiveVehicle) ||
                        !EntityManager.HasComponent<Game.Vehicles.PublicTransport>(
                            state.m_ActiveVehicle))
                    {
                        continue;
                    }

                    Game.Vehicles.PublicTransport publicTransport =
                        EntityManager.GetComponentData<Game.Vehicles.PublicTransport>(
                            state.m_ActiveVehicle);
                    if ((publicTransport.m_State &
                            Game.Vehicles.PublicTransportFlags.Boarding) != 0 &&
                        publicTransport.m_DepartureFrame == state.m_PlannedDepartureFrame)
                    {
                        publicTransport.m_DepartureFrame = state.m_OriginalDepartureFrame;
                        EntityManager.SetComponentData(
                            state.m_ActiveVehicle,
                            publicTransport);
                    }
                }
            }
            finally
            {
                states.Dispose();
            }

            m_LineStates.Clear();
        }

        private void RestartStatisticsCollection(uint frame)
        {
            Dependency.Complete();
            ClearCounters();
            ClearLineStates();
            m_CollectionStartedLocalTime = DateTime.Now;
            m_CollectionStartedSimulationFrame = frame;
            m_LastSimulationFrame = frame;
        }

        private void ClearStatisticsCollection()
        {
            Dependency.Complete();
            ClearCounters();
            ClearLineStates();
            m_CollectionStartedLocalTime = default;
            m_CollectionStartedSimulationFrame = uint.MaxValue;
            m_LastSimulationFrame = uint.MaxValue;
        }

        private void ClearCounters()
        {
            if (!m_Counters.IsCreated)
            {
                return;
            }

            for (int i = 0; i < m_Counters.Length; i++)
            {
                m_Counters[i] = 0;
            }
        }

        private void ClearLineStates()
        {
            if (m_LineStates.IsCreated)
            {
                m_LineStates.Clear();
            }
        }

        [BurstCompile]
        private partial struct PaceControlPointJob : IJobEntity
        {
            public uint m_Frame;

            [ReadOnly]
            public ComponentLookup<PrefabRef> m_RoutePrefabRefs;

            [ReadOnly]
            public ComponentLookup<TransportLineData> m_TransportLineData;

            [ReadOnly]
            public ComponentLookup<TransportLine> m_TransportLines;

            [ReadOnly]
            public ComponentLookup<Connected> m_Connected;

            [ReadOnly]
            public ComponentLookup<BoardingVehicle> m_BoardingVehicles;

            [ReadOnly]
            public BufferLookup<RouteWaypoint> m_RouteWaypoints;

            public NativeParallelHashMap<Entity, LineSpacingState> m_LineStates;

            public NativeArray<long> m_Counters;

            private void Execute(
                Entity vehicleEntity,
                ref Game.Vehicles.PublicTransport publicTransport,
                in CurrentRoute currentRoute,
                in Target target)
            {
                Game.Vehicles.PublicTransportFlags stateFlags = publicTransport.m_State;
                if ((stateFlags & Game.Vehicles.PublicTransportFlags.Boarding) == 0 ||
                    (stateFlags & Game.Vehicles.PublicTransportFlags.EnRoute) == 0 ||
                    (stateFlags &
                        (Game.Vehicles.PublicTransportFlags.Evacuating |
                         Game.Vehicles.PublicTransportFlags.PrisonerTransport |
                         Game.Vehicles.PublicTransportFlags.Refueling)) != 0)
                {
                    return;
                }

                Entity route = currentRoute.m_Route;
                Entity waypoint = target.m_Target;
                if (route == Entity.Null || waypoint == Entity.Null ||
                    !m_RoutePrefabRefs.HasComponent(route) ||
                    !m_TransportLines.HasComponent(route) ||
                    !m_RouteWaypoints.HasBuffer(route))
                {
                    return;
                }

                PrefabRef routePrefabRef = m_RoutePrefabRefs[route];
                if (routePrefabRef.m_Prefab == Entity.Null ||
                    !m_TransportLineData.HasComponent(routePrefabRef.m_Prefab))
                {
                    return;
                }

                TransportLineData lineData =
                    m_TransportLineData[routePrefabRef.m_Prefab];
                if (!lineData.m_PassengerTransport ||
                    !IsSupportedTransportType(lineData.m_TransportType))
                {
                    return;
                }

                bool hadLineState = m_LineStates.TryGetValue(
                    route,
                    out LineSpacingState lineState);
                int controlWaypointIndex;
                Entity controlWaypoint;
                if (hadLineState && IsCachedControlWaypointValid(route, lineState))
                {
                    controlWaypoint = lineState.m_ControlWaypoint;
                    controlWaypointIndex = lineState.m_ControlWaypointIndex;
                }
                else
                {
                    controlWaypoint = FindControlWaypoint(
                        route,
                        out controlWaypointIndex);
                }

                if (controlWaypoint == Entity.Null || waypoint != controlWaypoint ||
                    !OwnsBoardingSlot(vehicleEntity, waypoint))
                {
                    return;
                }

                if (!hadLineState)
                {
                    lineState = LineSpacingState.Create(
                        controlWaypoint,
                        controlWaypointIndex);
                    if (!m_LineStates.TryAdd(route, lineState))
                    {
                        IncrementCounter(kLineStateCapacityMissCount);
                        return;
                    }
                }
                else if (lineState.m_ControlWaypoint != controlWaypoint ||
                    lineState.m_ControlWaypointIndex != controlWaypointIndex)
                {
                    // A route edit can change the first valid stop. Do not carry a
                    // departure anchor from the old control point into the new one.
                    lineState = LineSpacingState.Create(
                        controlWaypoint,
                        controlWaypointIndex);
                }

                uint observationGap =
                    m_Frame - lineState.m_LastObservedFrame;
                bool sameVisit =
                    lineState.m_ActiveVehicle == vehicleEntity &&
                    lineState.m_LastObservedFrame != uint.MaxValue &&
                    observationGap <= kSameVisitToleranceFrames;

                if (sameVisit)
                {
                    lineState.m_LastObservedFrame = m_Frame;
                    ReassertActiveHold(ref publicTransport, ref lineState);
                    m_LineStates[route] = lineState;
                    return;
                }

                uint targetHeadwayFrames = GetTargetHeadwayFrames(
                    m_TransportLines[route]);
                RecordCompletedDeparture(ref lineState, targetHeadwayFrames);

                lineState.m_ActiveVehicle = vehicleEntity;
                lineState.m_LastObservedFrame = m_Frame;
                lineState.m_OriginalDepartureFrame =
                    publicTransport.m_DepartureFrame;
                lineState.m_PlannedDepartureFrame =
                    publicTransport.m_DepartureFrame;
                lineState.m_HoldApplied = 0;

                IncrementCounter(kControlVisitCount);
                IncrementModeCounter(lineData.m_TransportType);

                if (lineState.m_HasLastDeparture != 0)
                {
                    ApplyBoundedHold(
                        ref publicTransport,
                        ref lineState,
                        targetHeadwayFrames);
                }

                m_LineStates[route] = lineState;
            }

            private Entity FindControlWaypoint(Entity route, out int waypointIndex)
            {
                DynamicBuffer<RouteWaypoint> waypoints = m_RouteWaypoints[route];
                for (int i = 0; i < waypoints.Length; i++)
                {
                    Entity candidate = waypoints[i].m_Waypoint;
                    if (candidate == Entity.Null ||
                        !m_Connected.HasComponent(candidate))
                    {
                        continue;
                    }

                    Entity stop = m_Connected[candidate].m_Connected;
                    if (stop != Entity.Null &&
                        m_BoardingVehicles.HasComponent(stop))
                    {
                        waypointIndex = i;
                        return candidate;
                    }
                }

                waypointIndex = -1;
                return Entity.Null;
            }

            private bool IsCachedControlWaypointValid(
                Entity route,
                LineSpacingState lineState)
            {
                DynamicBuffer<RouteWaypoint> waypoints = m_RouteWaypoints[route];
                int index = lineState.m_ControlWaypointIndex;
                if (index < 0 || index >= waypoints.Length ||
                    waypoints[index].m_Waypoint != lineState.m_ControlWaypoint ||
                    !m_Connected.HasComponent(lineState.m_ControlWaypoint))
                {
                    return false;
                }

                Entity stop = m_Connected[lineState.m_ControlWaypoint].m_Connected;
                return stop != Entity.Null &&
                    m_BoardingVehicles.HasComponent(stop);
            }

            private bool OwnsBoardingSlot(Entity vehicleEntity, Entity waypoint)
            {
                if (!m_Connected.HasComponent(waypoint))
                {
                    return false;
                }

                Entity stop = m_Connected[waypoint].m_Connected;
                return stop != Entity.Null &&
                    m_BoardingVehicles.HasComponent(stop) &&
                    m_BoardingVehicles[stop].m_Vehicle == vehicleEntity;
            }

            private void RecordCompletedDeparture(
                ref LineSpacingState lineState,
                uint targetHeadwayFrames)
            {
                if (lineState.m_ActiveVehicle == Entity.Null ||
                    lineState.m_LastObservedFrame == uint.MaxValue)
                {
                    return;
                }

                uint completedDepartureFrame = lineState.m_LastObservedFrame;
                if (lineState.m_HasLastDeparture != 0)
                {
                    uint gap =
                        completedDepartureFrame - lineState.m_LastDepartureFrame;
                    if (gap > 0u && gap <= kMaximumMeasuredGapFrames)
                    {
                        IncrementCounter(kGapSampleCount);
                        AddCounter(kTotalGapFrames, gap);
                        AddCounter(kTotalTargetGapFrames, targetHeadwayFrames);
                        if (((ulong)gap * 2ul) < targetHeadwayFrames)
                        {
                            IncrementCounter(kUnderHalfTargetCount);
                        }
                    }
                }

                lineState.m_LastDepartureFrame = completedDepartureFrame;
                lineState.m_HasLastDeparture = 1;
            }

            private void ApplyBoundedHold(
                ref Game.Vehicles.PublicTransport publicTransport,
                ref LineSpacingState lineState,
                uint targetHeadwayFrames)
            {
                uint desiredDeparture =
                    lineState.m_LastDepartureFrame + targetHeadwayFrames;
                uint baselineDeparture = IsFrameAfter(
                    publicTransport.m_DepartureFrame,
                    m_Frame)
                    ? publicTransport.m_DepartureFrame
                    : m_Frame;
                int additionalFramesNeeded =
                    unchecked((int)(desiredDeparture - baselineDeparture));
                if (additionalFramesNeeded <= 0)
                {
                    return;
                }

                uint addedHold = math.min(
                    (uint)additionalFramesNeeded,
                    kMaximumAddedHoldFrames);
                uint plannedDeparture = baselineDeparture + addedHold;

                publicTransport.m_DepartureFrame = plannedDeparture;
                lineState.m_PlannedDepartureFrame = plannedDeparture;
                lineState.m_HoldApplied = 1;

                IncrementCounter(kHoldCount);
                IncrementCounter(kWriteCount);
                AddCounter(kTotalAddedHoldFrames, addedHold);
                SetMaximumCounter(kMaximumAddedHold, addedHold);
                if ((uint)additionalFramesNeeded > kMaximumAddedHoldFrames)
                {
                    IncrementCounter(kCappedHoldCount);
                }
            }

            private void ReassertActiveHold(
                ref Game.Vehicles.PublicTransport publicTransport,
                ref LineSpacingState lineState)
            {
                if (lineState.m_HoldApplied == 0)
                {
                    return;
                }

                if (IsFrameAfter(lineState.m_PlannedDepartureFrame, m_Frame))
                {
                    if (IsFrameAfter(
                            lineState.m_PlannedDepartureFrame,
                            publicTransport.m_DepartureFrame))
                    {
                        publicTransport.m_DepartureFrame =
                            lineState.m_PlannedDepartureFrame;
                        IncrementCounter(kWriteCount);
                    }

                    return;
                }

                // Once the intentional spacing hold has elapsed, restore the
                // original vanilla departure anchor if nobody else changed it.
                // Better Boarding's late-passenger grace and vanilla's failsafe
                // then count lateness from the original boarding schedule rather
                // than adding the spacing hold a second time.
                if (publicTransport.m_DepartureFrame ==
                    lineState.m_PlannedDepartureFrame)
                {
                    publicTransport.m_DepartureFrame =
                        lineState.m_OriginalDepartureFrame;
                    IncrementCounter(kWriteCount);
                }

                lineState.m_HoldApplied = 0;
            }

            private void IncrementModeCounter(TransportType transportType)
            {
                int index;
                switch (transportType)
                {
                    case TransportType.Bus:
                        index = kBusVisitCount;
                        break;
                    case TransportType.Tram:
                        index = kTramVisitCount;
                        break;
                    case TransportType.Train:
                        index = kTrainVisitCount;
                        break;
                    case TransportType.Subway:
                        index = kSubwayVisitCount;
                        break;
                    default:
                        return;
                }

                IncrementCounter(index);
            }

            private void IncrementCounter(int index)
            {
                m_Counters[index] = m_Counters[index] + 1;
            }

            private void AddCounter(int index, uint value)
            {
                m_Counters[index] = m_Counters[index] + value;
            }

            private void SetMaximumCounter(int index, uint value)
            {
                if (value > m_Counters[index])
                {
                    m_Counters[index] = value;
                }
            }

            private static uint GetTargetHeadwayFrames(TransportLine line)
            {
                float interval = line.m_VehicleInterval;
                if (!(interval >= 1f))
                {
                    interval = 1f;
                }

                interval = math.min(interval, 4369f);
                return (uint)math.round(interval * 60f);
            }

            private static bool IsFrameAfter(uint candidate, uint reference)
            {
                return unchecked((int)(candidate - reference)) > 0;
            }

            private static bool IsSupportedTransportType(TransportType transportType)
            {
                return transportType == TransportType.Bus ||
                    transportType == TransportType.Tram ||
                    transportType == TransportType.Train ||
                    transportType == TransportType.Subway;
            }
        }

        private struct LineSpacingState
        {
            public Entity m_ControlWaypoint;
            public Entity m_ActiveVehicle;
            public int m_ControlWaypointIndex;
            public uint m_LastObservedFrame;
            public uint m_LastDepartureFrame;
            public uint m_OriginalDepartureFrame;
            public uint m_PlannedDepartureFrame;
            public byte m_HasLastDeparture;
            public byte m_HoldApplied;

            public static LineSpacingState Create(
                Entity controlWaypoint,
                int controlWaypointIndex)
            {
                return new LineSpacingState
                {
                    m_ControlWaypoint = controlWaypoint,
                    m_ActiveVehicle = Entity.Null,
                    m_ControlWaypointIndex = controlWaypointIndex,
                    m_LastObservedFrame = uint.MaxValue
                };
            }
        }

        internal readonly struct StatisticsSnapshot
        {
            public StatisticsSnapshot(
                long controlVisits,
                long holds,
                long writes,
                long totalAddedHoldFrames,
                long maximumAddedHoldFrames,
                long cappedHolds,
                long gapSamples,
                long totalGapFrames,
                long totalTargetGapFrames,
                long underHalfTargetGaps,
                long busVisits,
                long tramVisits,
                long trainVisits,
                long subwayVisits,
                long lineStateCapacityMisses,
                DateTime collectionStartedLocalTime,
                uint collectionStartedSimulationFrame)
            {
                ControlVisits = controlVisits;
                Holds = holds;
                Writes = writes;
                TotalAddedHoldFrames = totalAddedHoldFrames;
                MaximumAddedHoldFrames = maximumAddedHoldFrames;
                CappedHolds = cappedHolds;
                GapSamples = gapSamples;
                TotalGapFrames = totalGapFrames;
                TotalTargetGapFrames = totalTargetGapFrames;
                UnderHalfTargetGaps = underHalfTargetGaps;
                BusVisits = busVisits;
                TramVisits = tramVisits;
                TrainVisits = trainVisits;
                SubwayVisits = subwayVisits;
                LineStateCapacityMisses = lineStateCapacityMisses;
                CollectionStartedLocalTime = collectionStartedLocalTime;
                CollectionStartedSimulationFrame = collectionStartedSimulationFrame;
            }

            public long ControlVisits { get; }

            public long Holds { get; }

            public long Writes { get; }

            public long TotalAddedHoldFrames { get; }

            public long MaximumAddedHoldFrames { get; }

            public long CappedHolds { get; }

            public long GapSamples { get; }

            public long TotalGapFrames { get; }

            public long TotalTargetGapFrames { get; }

            public long UnderHalfTargetGaps { get; }

            public long BusVisits { get; }

            public long TramVisits { get; }

            public long TrainVisits { get; }

            public long SubwayVisits { get; }

            public long LineStateCapacityMisses { get; }

            public DateTime CollectionStartedLocalTime { get; }

            public uint CollectionStartedSimulationFrame { get; }

            public double AverageAddedHoldFrames =>
                Holds > 0 ? (double)TotalAddedHoldFrames / Holds : 0d;

            public double AverageGapFrames =>
                GapSamples > 0 ? (double)TotalGapFrames / GapSamples : 0d;

            public double AverageTargetGapFrames =>
                GapSamples > 0 ? (double)TotalTargetGapFrames / GapSamples : 0d;
        }
    }
}
