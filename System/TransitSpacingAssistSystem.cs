// <copyright file="TransitSpacingAssistSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/TransitSpacingAssistSystem.cs
// Purpose: Helps vanilla unbunching use current boarding progress instead of a stale scheduled departure.

namespace BetterBoarding
{
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

    /// <summary>
    /// Keeps each route waypoint's last-departure timing current while a passenger
    /// vehicle is still boarding. The next vehicle can then use vanilla's own
    /// unbunching formula with an actual departure reference instead of the earlier
    /// departure time that was scheduled when boarding began.
    /// </summary>
    public sealed partial class TransitSpacingAssistSystem : GameSystemBase
    {
        public const int UpdatesPerDay = 8192;

        private const int kRefreshCount = 0;
        private const int kBusStopCount = 1;
        private const int kTramStopCount = 2;
        private const int kTrainStopCount = 3;
        private const int kSubwayStopCount = 4;
        private const int kCounterCount = 5;

        private EntityQuery m_VehicleQuery;
        private SimulationSystem? m_SimulationSystem;
        private NativeArray<long> m_Counters;
        private uint m_LastSimulationFrame = uint.MaxValue;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            // Frequent enough to follow the one-second minimum vanilla boarding state,
            // while the work itself stays off the main thread in one small Burst job.
            return 262144 / UpdatesPerDay;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
            m_Counters = new NativeArray<long>(
                kCounterCount,
                Allocator.Persistent,
                NativeArrayOptions.ClearMemory);

            m_VehicleQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Vehicles.PublicTransport, CurrentRoute, Target>()
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
                Dependency.Complete();
                ClearCounters();
                m_LastSimulationFrame = uint.MaxValue;
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
                // Also defend against a city switch while this optional system was disabled.
                Dependency.Complete();
                ClearCounters();
            }

            m_LastSimulationFrame = frame;

            RefreshDepartureTimingJob job = new RefreshDepartureTimingJob
            {
                m_Frame = frame,
                m_RoutePrefabRefs = SystemAPI.GetComponentLookup<PrefabRef>(isReadOnly: true),
                m_TransportLineData = SystemAPI.GetComponentLookup<TransportLineData>(isReadOnly: true),
                m_VehicleTiming = SystemAPI.GetComponentLookup<VehicleTiming>(isReadOnly: false),
                m_Counters = m_Counters
            };

            // Sequential scheduling is deliberate: a physical stop admits one boarding
            // vehicle at a time, but a writable random-access lookup should still have a
            // single writer. The job remains off the main thread and Burst compiled.
            Dependency = job.Schedule(m_VehicleQuery, Dependency);
        }

        protected override void OnDestroy()
        {
            Dependency.Complete();

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
                m_Counters[kRefreshCount],
                m_Counters[kBusStopCount],
                m_Counters[kTramStopCount],
                m_Counters[kTrainStopCount],
                m_Counters[kSubwayStopCount]);
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

        [BurstCompile]
        private partial struct RefreshDepartureTimingJob : IJobEntity
        {
            public uint m_Frame;

            [ReadOnly]
            public ComponentLookup<PrefabRef> m_RoutePrefabRefs;

            [ReadOnly]
            public ComponentLookup<TransportLineData> m_TransportLineData;

            public ComponentLookup<VehicleTiming> m_VehicleTiming;

            public NativeArray<long> m_Counters;

            private void Execute(
                in Game.Vehicles.PublicTransport publicTransport,
                in CurrentRoute currentRoute,
                in Target target)
            {
                Game.Vehicles.PublicTransportFlags state = publicTransport.m_State;
                if ((state & Game.Vehicles.PublicTransportFlags.Boarding) == 0 ||
                    (state & Game.Vehicles.PublicTransportFlags.EnRoute) == 0 ||
                    (state &
                        (Game.Vehicles.PublicTransportFlags.Evacuating |
                         Game.Vehicles.PublicTransportFlags.PrisonerTransport)) != 0)
                {
                    return;
                }

                Entity route = currentRoute.m_Route;
                Entity waypoint = target.m_Target;
                if (route == Entity.Null || waypoint == Entity.Null ||
                    !m_RoutePrefabRefs.HasComponent(route) ||
                    !m_VehicleTiming.HasComponent(waypoint))
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

                VehicleTiming timing = m_VehicleTiming[waypoint];

                uint previousTiming = timing.m_LastDepartureFrame;
                if (previousTiming == m_Frame)
                {
                    return;
                }

                // BeginBoarding writes a scheduled departure into VehicleTiming. Once the
                // scheduled frame is still ahead of this first observation, count one newly
                // observed stop. Later refreshes hold the previous observation frame and do
                // not increment this stop counter again.
                if (unchecked((int)(previousTiming - m_Frame)) > 0)
                {
                    IncrementStopCounter(lineData.m_TransportType);
                }

                timing.m_LastDepartureFrame = m_Frame;
                m_VehicleTiming[waypoint] = timing;
                m_Counters[kRefreshCount] = m_Counters[kRefreshCount] + 1;
            }

            private void IncrementStopCounter(TransportType transportType)
            {
                int index;
                switch (transportType)
                {
                    case TransportType.Bus:
                        index = kBusStopCount;
                        break;
                    case TransportType.Tram:
                        index = kTramStopCount;
                        break;
                    case TransportType.Train:
                        index = kTrainStopCount;
                        break;
                    case TransportType.Subway:
                        index = kSubwayStopCount;
                        break;
                    default:
                        return;
                }

                m_Counters[index] = m_Counters[index] + 1;
            }

            private static bool IsSupportedTransportType(TransportType transportType)
            {
                return transportType == TransportType.Bus ||
                    transportType == TransportType.Tram ||
                    transportType == TransportType.Train ||
                    transportType == TransportType.Subway;
            }
        }

        internal readonly struct StatisticsSnapshot
        {
            public StatisticsSnapshot(
                long refreshes,
                long busStops,
                long tramStops,
                long trainStops,
                long subwayStops)
            {
                Refreshes = refreshes;
                BusStops = busStops;
                TramStops = tramStops;
                TrainStops = trainStops;
                SubwayStops = subwayStops;
            }

            public long Refreshes { get; }

            public long BusStops { get; }

            public long TramStops { get; }

            public long TrainStops { get; }

            public long SubwayStops { get; }

            public long TotalStops => BusStops + TramStops + TrainStops + SubwayStops;
        }
    }
}
