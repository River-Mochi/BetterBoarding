// <copyright file="TransitWaitStatusSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/Status/TransitWaitStatusSystem.cs
// Purpose: Builds transit wait snapshots on demand for the Options UI status group.

namespace BetterBoarding
{
    using System;
    using System.Collections.Generic;
    using Game;
    using Game.Common;
    using Game.Creatures;
    using Game.Pathfind;
    using Game.Prefabs;
    using Game.Routes;
    using Game.Simulation;
    using Game.Tools;
    using Game.UI;
    using Game.Vehicles;
    using Unity.Collections;
    using Unity.Entities;

    public sealed partial class TransitWaitStatusSystem : GameSystemBase
    {
        private const int TopWorstStopCount = 3;

        public readonly struct WorstStopSnapshot
        {
            public WorstStopSnapshot(
                string stopName,
                Entity stopEntity,
                Entity waypointEntity,
                Entity lineEntity,
                string lineName,
                int averageWaitSeconds,
                long waitingPassengers,
                int lineWaitSeconds,
                int lineWaitingPassengers)
            {
                StopName = stopName ?? string.Empty;
                StopEntity = stopEntity;
                WaypointEntity = waypointEntity;
                LineEntity = lineEntity;
                LineName = lineName ?? string.Empty;
                AverageWaitSeconds = averageWaitSeconds;
                WaitingPassengers = waitingPassengers;
                LineWaitSeconds = lineWaitSeconds;
                LineWaitingPassengers = lineWaitingPassengers;
            }

            public string StopName { get; }

            public Entity StopEntity { get; }

            public Entity WaypointEntity { get; }

            public Entity LineEntity { get; }

            public string LineName { get; }

            public int AverageWaitSeconds { get; }

            public long WaitingPassengers { get; }

            public int LineWaitSeconds { get; }

            public int LineWaitingPassengers { get; }
        }

        public readonly struct FamilySnapshot
        {
            public FamilySnapshot(
                int stopCount,
                int activeQueueStops,
                long waitingPassengers,
                int averageWaitSeconds,
                int worstStopWaitSeconds,
                string worstStopName,
                Entity worstStopEntity,
                Entity worstWaypointEntity,
                Entity worstLineEntity,
                string worstLineName,
                int worstLineWaitSeconds,
                int worstLineWaitingPassengers,
                int lateGroupPassengers,
                int lateGroupGroups,
                int lateGroupVehicles,
                WorstStopSnapshot[] topWorstStops)
            {
                StopCount = stopCount;
                ActiveQueueStops = activeQueueStops;
                WaitingPassengers = waitingPassengers;
                AverageWaitSeconds = averageWaitSeconds;
                WorstStopWaitSeconds = worstStopWaitSeconds;
                WorstStopName = worstStopName ?? string.Empty;
                WorstStopEntity = worstStopEntity;
                WorstWaypointEntity = worstWaypointEntity;
                WorstLineEntity = worstLineEntity;
                WorstLineName = worstLineName ?? string.Empty;
                WorstLineWaitSeconds = worstLineWaitSeconds;
                WorstLineWaitingPassengers = worstLineWaitingPassengers;
                LateGroupPassengers = lateGroupPassengers;
                LateGroupGroups = lateGroupGroups;
                LateGroupVehicles = lateGroupVehicles;
                TopWorstStops = topWorstStops ?? Array.Empty<WorstStopSnapshot>();
            }

            public int StopCount { get; }

            public int ActiveQueueStops { get; }

            public long WaitingPassengers { get; }

            public int AverageWaitSeconds { get; }

            public int WorstStopWaitSeconds { get; }

            public string WorstStopName { get; }

            public Entity WorstStopEntity { get; }

            public Entity WorstWaypointEntity { get; }

            public Entity WorstLineEntity { get; }

            public string WorstLineName { get; }

            public int WorstLineWaitSeconds { get; }

            public int WorstLineWaitingPassengers { get; }

            public int LateGroupPassengers { get; }

            public int LateGroupGroups { get; }

            public int LateGroupVehicles { get; }

            public WorstStopSnapshot[] TopWorstStops { get; }
        }

        public readonly struct Snapshot
        {
            public Snapshot(
                FamilySnapshot bus,
                FamilySnapshot train,
                FamilySnapshot tram,
                FamilySnapshot subway,
                FamilySnapshot ship,
                FamilySnapshot ferry,
                FamilySnapshot air,
                int monthlyTourists,
                int monthlyCitizens)
            {
                Bus = bus;
                Train = train;
                Tram = tram;
                Subway = subway;
                Ship = ship;
                Ferry = ferry;
                Air = air;
                MonthlyTourists = monthlyTourists;
                MonthlyCitizens = monthlyCitizens;
            }

            public FamilySnapshot Bus { get; }

            public FamilySnapshot Train { get; }

            public FamilySnapshot Tram { get; }

            public FamilySnapshot Subway { get; }

            public FamilySnapshot Ship { get; }

            public FamilySnapshot Ferry { get; }

            public FamilySnapshot Air { get; }

            public int MonthlyTourists { get; }

            public int MonthlyCitizens { get; }
        }

        private struct LateGroupStats
        {
            public int Passengers;

            public int Groups;

            public int Vehicles;
        }

        private readonly struct MonthlyPassengerTotals
        {
            public MonthlyPassengerTotals(int tourists, int citizens)
            {
                Tourists = tourists;
                Citizens = citizens;
            }

            public int Tourists { get; }

            public int Citizens { get; }
        }

        public enum FollowUpOutcome
        {
            SameVehicle,
            DifferentVehicle,
            HasPathNotAssignedYet,
            Other,
        }

        public enum FollowUpTargetKind
        {
            None,
            Stop,
            Waypoint,
            Lane,
            Vehicle,
            Target,
        }

        public readonly struct FollowUpSnapshot
        {
            public FollowUpSnapshot(
                FollowUpOutcome outcome,
                string outcomeLabel,
                Entity currentVehicle,
                CreatureVehicleFlags currentVehicleFlags,
                int pathElementCount,
                int pathIndex,
                Entity nextTargetEntity,
                FollowUpTargetKind nextTargetKind,
                string nextTargetName,
                Entity nextWaypointEntity,
                Entity nextStopEntity,
                string nextStopName,
                Entity nextLineEntity,
                string nextLineName,
                string hint)
            {
                Outcome = outcome;
                OutcomeLabel = outcomeLabel ?? string.Empty;
                CurrentVehicle = currentVehicle;
                CurrentVehicleFlags = currentVehicleFlags;
                PathElementCount = pathElementCount;
                PathIndex = pathIndex;
                NextTargetEntity = nextTargetEntity;
                NextTargetKind = nextTargetKind;
                NextTargetName = nextTargetName ?? string.Empty;
                NextWaypointEntity = nextWaypointEntity;
                NextStopEntity = nextStopEntity;
                NextStopName = nextStopName ?? string.Empty;
                NextLineEntity = nextLineEntity;
                NextLineName = nextLineName ?? string.Empty;
                Hint = hint ?? string.Empty;
            }

            public FollowUpOutcome Outcome { get; }

            public string OutcomeLabel { get; }

            public Entity CurrentVehicle { get; }

            public CreatureVehicleFlags CurrentVehicleFlags { get; }

            public int PathElementCount { get; }

            public int PathIndex { get; }

            public Entity NextTargetEntity { get; }

            public FollowUpTargetKind NextTargetKind { get; }

            public string NextTargetName { get; }

            public Entity NextWaypointEntity { get; }

            public Entity NextStopEntity { get; }

            public string NextStopName { get; }

            public Entity NextLineEntity { get; }

            public string NextLineName { get; }

            public string Hint { get; }

            public string CurrentVehicleText => CurrentVehicle == Entity.Null
                ? "none"
                : $"{CurrentVehicle} ({CurrentVehicleFlags})";
        }

        private readonly struct FollowUpTargetInfo
        {
            public FollowUpTargetInfo(
                FollowUpTargetKind nextTargetKind,
                string nextTargetName,
                Entity nextWaypointEntity,
                Entity nextStopEntity,
                string nextStopName,
                Entity nextLineEntity,
                string nextLineName)
            {
                NextTargetKind = nextTargetKind;
                NextTargetName = nextTargetName ?? string.Empty;
                NextWaypointEntity = nextWaypointEntity;
                NextStopEntity = nextStopEntity;
                NextStopName = nextStopName ?? string.Empty;
                NextLineEntity = nextLineEntity;
                NextLineName = nextLineName ?? string.Empty;
            }

            public FollowUpTargetKind NextTargetKind { get; }

            public string NextTargetName { get; }

            public Entity NextWaypointEntity { get; }

            public Entity NextStopEntity { get; }

            public string NextStopName { get; }

            public Entity NextLineEntity { get; }

            public string NextLineName { get; }
        }

        private struct StopAggregate
        {
            public long WaitingPassengers;

            public long WeightedWaitSeconds;

            public string Name;

            public Entity WorstWaypointEntity;

            public Entity WorstLineEntity;

            public string WorstLineName;

            public int WorstLineWaitSeconds;

            public int WorstLineWaitingPassengers;
        }

        private EntityQuery m_WaitingWaypointQuery;
        private EntityQuery m_BoardingVehicleQuery;
        private EntityQuery m_TransportConfigQuery;
        private NameSystem m_NameSystem = null!;
        private SimulationSystem m_SimulationSystem = null!;
        private CityStatisticsSystem m_CityStatisticsSystem = null!;
        private PrefabSystem m_PrefabSystem = null!;
        private UITransportConfigurationPrefab? m_TransportConfig;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_NameSystem = World.GetOrCreateSystemManaged<NameSystem>();
            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
            m_CityStatisticsSystem = World.GetOrCreateSystemManaged<CityStatisticsSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

            m_WaitingWaypointQuery = SystemAPI.QueryBuilder()
                .WithAll<WaitingPassengers, Connected, Owner>()
                .WithNone<Deleted, Destroyed, Temp, Overridden>()
                .Build();

            m_BoardingVehicleQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Vehicles.PublicTransport, Passenger>()
                .WithNone<Deleted, Destroyed, Temp, Overridden>()
                .Build();

            m_TransportConfigQuery = GetEntityQuery(ComponentType.ReadOnly<UITransportConfigurationData>());
        }

        protected override void OnUpdate()
        {
            // Intentionally empty: snapshot work only runs when the Options UI asks for it.
        }

        public Snapshot BuildSnapshot()
        {
            // Vanilla wait data lives on line waypoints, not on the stop entities themselves.
            // We group those waypoint queues back onto the connected stop so the UI reads like a stop report.
            Dictionary<Entity, StopAggregate> busStops = new();
            Dictionary<Entity, StopAggregate> trainStops = new();
            Dictionary<Entity, StopAggregate> tramStops = new();
            Dictionary<Entity, StopAggregate> subwayStops = new();
            Dictionary<Entity, StopAggregate> shipStops = new();
            Dictionary<Entity, StopAggregate> ferryStops = new();
            Dictionary<Entity, StopAggregate> airStops = new();
            LateGroupStats busLateGroups = default;
            LateGroupStats trainLateGroups = default;
            LateGroupStats tramLateGroups = default;
            LateGroupStats subwayLateGroups = default;
            LateGroupStats shipLateGroups = default;
            LateGroupStats ferryLateGroups = default;
            LateGroupStats airLateGroups = default;

            using NativeArray<Entity> waypointEntities = m_WaitingWaypointQuery.ToEntityArray(Allocator.Temp);
            using NativeArray<WaitingPassengers> waitingData = m_WaitingWaypointQuery.ToComponentDataArray<WaitingPassengers>(Allocator.Temp);
            using NativeArray<Connected> connectedData = m_WaitingWaypointQuery.ToComponentDataArray<Connected>(Allocator.Temp);
            using NativeArray<Owner> ownerData = m_WaitingWaypointQuery.ToComponentDataArray<Owner>(Allocator.Temp);

            // Each waypoint belongs to a transit line and points back to its served stop/terminal.
            for (int i = 0; i < waypointEntities.Length; i++)
            {
                if (!TryGetTransportType(ownerData[i].m_Owner, out TransportType transportType))
                {
                    continue;
                }

                Entity stopEntity = connectedData[i].m_Connected != Entity.Null
                    ? connectedData[i].m_Connected
                    : waypointEntities[i];

                switch (transportType)
                {
                    case TransportType.Bus:
                        AccumulateStop(busStops, stopEntity, waypointEntities[i], ownerData[i].m_Owner, transportType, waitingData[i]);
                        break;
                    case TransportType.Train:
                        AccumulateStop(trainStops, stopEntity, waypointEntities[i], ownerData[i].m_Owner, transportType, waitingData[i]);
                        break;
                    case TransportType.Tram:
                        AccumulateStop(tramStops, stopEntity, waypointEntities[i], ownerData[i].m_Owner, transportType, waitingData[i]);
                        break;
                    case TransportType.Subway:
                        AccumulateStop(subwayStops, stopEntity, waypointEntities[i], ownerData[i].m_Owner, transportType, waitingData[i]);
                        break;
                    case TransportType.Ship:
                        AccumulateStop(shipStops, stopEntity, waypointEntities[i], ownerData[i].m_Owner, transportType, waitingData[i]);
                        break;
                    case TransportType.Ferry:
                        AccumulateStop(ferryStops, stopEntity, waypointEntities[i], ownerData[i].m_Owner, transportType, waitingData[i]);
                        break;
                    case TransportType.Airplane:
                        AccumulateStop(airStops, stopEntity, waypointEntities[i], ownerData[i].m_Owner, transportType, waitingData[i]);
                        break;
                }
            }

            BuildLateGroupStats(
                ref busLateGroups,
                ref trainLateGroups,
                ref tramLateGroups,
                ref subwayLateGroups,
                ref shipLateGroups,
                ref ferryLateGroups,
                ref airLateGroups);

            FamilySnapshot bus = BuildFamilySnapshot(busStops, busLateGroups);
            FamilySnapshot train = BuildFamilySnapshot(trainStops, trainLateGroups);
            FamilySnapshot tram = BuildFamilySnapshot(tramStops, tramLateGroups);
            FamilySnapshot subway = BuildFamilySnapshot(subwayStops, subwayLateGroups);
            FamilySnapshot ship = BuildFamilySnapshot(shipStops, shipLateGroups);
            FamilySnapshot ferry = BuildFamilySnapshot(ferryStops, ferryLateGroups);
            FamilySnapshot air = BuildFamilySnapshot(airStops, airLateGroups);
            MonthlyPassengerTotals monthlyTotals = BuildMonthlyPassengerTotals();

            return new Snapshot(
                bus,
                train,
                tram,
                subway,
                ship,
                ferry,
                air,
                monthlyTotals.Tourists,
                monthlyTotals.Citizens);
        }

        public FollowUpSnapshot BuildLateBoarderFollowUpSnapshot(
            Entity passenger,
            TransportType transportType,
            Entity missedVehicle)
        {
            if (passenger == Entity.Null || !EntityManager.Exists(passenger))
            {
                return new FollowUpSnapshot(
                    FollowUpOutcome.Other,
                    "entity missing",
                    Entity.Null,
                    default,
                    -1,
                    -1,
                    Entity.Null,
                    FollowUpTargetKind.None,
                    string.Empty,
                    Entity.Null,
                    Entity.Null,
                    string.Empty,
                    Entity.Null,
                    string.Empty,
                    "entity missing");
            }

            if (EntityManager.HasComponent<Deleted>(passenger) ||
                EntityManager.HasComponent<Destroyed>(passenger))
            {
                return new FollowUpSnapshot(
                    FollowUpOutcome.Other,
                    "deleted/destroyed",
                    Entity.Null,
                    default,
                    -1,
                    -1,
                    Entity.Null,
                    FollowUpTargetKind.None,
                    string.Empty,
                    Entity.Null,
                    Entity.Null,
                    string.Empty,
                    Entity.Null,
                    string.Empty,
                    "deleted/destroyed");
            }

            Entity currentVehicle = Entity.Null;
            CreatureVehicleFlags currentVehicleFlags = default;
            if (EntityManager.HasComponent<CurrentVehicle>(passenger))
            {
                CurrentVehicle currentVehicleData = EntityManager.GetComponentData<CurrentVehicle>(passenger);
                currentVehicle = currentVehicleData.m_Vehicle;
                currentVehicleFlags = currentVehicleData.m_Flags;
            }

            int pathElementCount = -1;
            int pathIndex = EntityManager.HasComponent<PathOwner>(passenger)
                ? EntityManager.GetComponentData<PathOwner>(passenger).m_ElementIndex
                : -1;
            Entity nextTargetEntity = Entity.Null;
            if (EntityManager.HasBuffer<PathElement>(passenger))
            {
                DynamicBuffer<PathElement> pathElements = EntityManager.GetBuffer<PathElement>(passenger);
                pathElementCount = pathElements.Length;
                if (pathElements.Length > 0)
                {
                    int safeIndex = Math.Min(Math.Max(pathIndex, 0), pathElements.Length - 1);
                    nextTargetEntity = pathElements[safeIndex].m_Target;
                }
            }

            FollowUpTargetInfo targetInfo = ResolveFollowUpTarget(nextTargetEntity, transportType);

            // Keep outcomes simple for testers: same vehicle, different vehicle, has path but not assigned, or other.
            FollowUpOutcome outcome;
            string outcomeLabel;
            string hint;
            if (currentVehicle != Entity.Null)
            {
                outcome = currentVehicle == missedVehicle
                    ? FollowUpOutcome.SameVehicle
                    : FollowUpOutcome.DifferentVehicle;
                outcomeLabel = currentVehicle == missedVehicle
                    ? "same vehicle"
                    : "different vehicle";
                hint = "assigned";
            }
            else if (pathElementCount > 0)
            {
                outcome = FollowUpOutcome.HasPathNotAssignedYet;
                outcomeLabel = "has path, not assigned yet";
                hint = "has path";
            }
            else
            {
                outcome = FollowUpOutcome.Other;
                outcomeLabel = "no path yet";
                hint = "no path yet";
            }

            return new FollowUpSnapshot(
                outcome,
                outcomeLabel,
                currentVehicle,
                currentVehicleFlags,
                pathElementCount,
                pathIndex,
                nextTargetEntity,
                targetInfo.NextTargetKind,
                targetInfo.NextTargetName,
                targetInfo.NextWaypointEntity,
                targetInfo.NextStopEntity,
                targetInfo.NextStopName,
                targetInfo.NextLineEntity,
                targetInfo.NextLineName,
                hint);
        }

        private MonthlyPassengerTotals BuildMonthlyPassengerTotals()
        {
            UITransportConfigurationPrefab? config = GetTransportConfig();
            if (config == null)
            {
                return new MonthlyPassengerTotals(0, 0);
            }

            int totalTourists = 0;
            int totalCitizens = 0;
            UITransportSummaryItem[] items = config.m_PassengerSummaryItems;
            for (int i = 0; i < items.Length; i++)
            {
                UITransportSummaryItem item = items[i];

                try
                {
                    // This matches the vanilla Transportation infoview passenger summary.
                    totalCitizens += m_CityStatisticsSystem.GetStatisticValue(item.m_Statistic);
                    totalTourists += m_CityStatisticsSystem.GetStatisticValue(item.m_Statistic, 1);
                }
                catch
                {
                    // Missing stats should not break the Options UI status snapshot.
                }
            }

            return new MonthlyPassengerTotals(totalTourists, totalCitizens);
        }

        private UITransportConfigurationPrefab? GetTransportConfig()
        {
            if (m_TransportConfig != null)
            {
                return m_TransportConfig;
            }

            try
            {
                m_TransportConfig =
                    m_PrefabSystem.GetSingletonPrefab<UITransportConfigurationPrefab>(m_TransportConfigQuery);
            }
            catch
            {
                m_TransportConfig = null;
            }

            return m_TransportConfig;
        }

        private void AccumulateStop(
            Dictionary<Entity, StopAggregate> stops,
            Entity stopEntity,
            Entity waypointEntity,
            Entity lineEntity,
            TransportType transportType,
            WaitingPassengers waiting)
        {
            if (!stops.TryGetValue(stopEntity, out StopAggregate aggregate))
            {
                aggregate = new StopAggregate
                {
                    Name = ResolveStopName(stopEntity, transportType),
                    WorstWaypointEntity = Entity.Null,
                    WorstLineEntity = Entity.Null,
                    WorstLineName = string.Empty
                };
            }

            aggregate.WaitingPassengers += waiting.m_Count;
            aggregate.WeightedWaitSeconds += (long)waiting.m_Count * waiting.m_AverageWaitingTime;

            if (waiting.m_Count > 0 && waiting.m_AverageWaitingTime > aggregate.WorstLineWaitSeconds)
            {
                // Keep a line/waypoint hint for the detailed log report.
                aggregate.WorstWaypointEntity = waypointEntity;
                aggregate.WorstLineEntity = lineEntity;
                aggregate.WorstLineName = ResolveLineName(lineEntity);
                aggregate.WorstLineWaitSeconds = waiting.m_AverageWaitingTime;
                aggregate.WorstLineWaitingPassengers = waiting.m_Count;
            }

            stops[stopEntity] = aggregate;
        }

        private FamilySnapshot BuildFamilySnapshot(Dictionary<Entity, StopAggregate> stops, LateGroupStats lateGroupStats)
        {
            int activeQueueStops = 0;
            long waitingPassengers = 0;
            long weightedWaitSeconds = 0;
            int worstStopWaitSeconds = 0;
            string worstStopName = string.Empty;
            Entity worstStopEntity = Entity.Null;
            Entity worstWaypointEntity = Entity.Null;
            Entity worstLineEntity = Entity.Null;
            string worstLineName = string.Empty;
            int worstLineWaitSeconds = 0;
            int worstLineWaitingPassengers = 0;
            List<WorstStopSnapshot> topWorstStops = new List<WorstStopSnapshot>(TopWorstStopCount);

            foreach (KeyValuePair<Entity, StopAggregate> pair in stops)
            {
                Entity stopEntity = pair.Key;
                StopAggregate aggregate = pair.Value;
                waitingPassengers += aggregate.WaitingPassengers;
                weightedWaitSeconds += aggregate.WeightedWaitSeconds;

                if (aggregate.WaitingPassengers <= 0)
                {
                    continue;
                }

                activeQueueStops++;
                int stopAverageWaitSeconds = (int)(aggregate.WeightedWaitSeconds / aggregate.WaitingPassengers);
                InsertWorstStop(
                    topWorstStops,
                    new WorstStopSnapshot(
                        aggregate.Name,
                        stopEntity,
                        aggregate.WorstWaypointEntity,
                        aggregate.WorstLineEntity,
                        aggregate.WorstLineName,
                        stopAverageWaitSeconds,
                        aggregate.WaitingPassengers,
                        aggregate.WorstLineWaitSeconds,
                        aggregate.WorstLineWaitingPassengers));

                if (stopAverageWaitSeconds > worstStopWaitSeconds)
                {
                    // Worst stop is based on stop-level average, not just one line waypoint.
                    worstStopWaitSeconds = stopAverageWaitSeconds;
                    worstStopName = aggregate.Name;
                    worstStopEntity = stopEntity;
                    worstWaypointEntity = aggregate.WorstWaypointEntity;
                    worstLineEntity = aggregate.WorstLineEntity;
                    worstLineName = aggregate.WorstLineName;
                    worstLineWaitSeconds = aggregate.WorstLineWaitSeconds;
                    worstLineWaitingPassengers = aggregate.WorstLineWaitingPassengers;
                }
            }

            int averageWaitSeconds = waitingPassengers > 0
                ? (int)(weightedWaitSeconds / waitingPassengers)
                : 0;

            return new FamilySnapshot(
                stops.Count,
                activeQueueStops,
                waitingPassengers,
                averageWaitSeconds,
                worstStopWaitSeconds,
                worstStopName,
                worstStopEntity,
                worstWaypointEntity,
                worstLineEntity,
                worstLineName,
                worstLineWaitSeconds,
                worstLineWaitingPassengers,
                lateGroupStats.Passengers,
                lateGroupStats.Groups,
                lateGroupStats.Vehicles,
                topWorstStops.ToArray());
        }

        private static void InsertWorstStop(List<WorstStopSnapshot> topWorstStops, WorstStopSnapshot candidate)
        {
            // Keep a tiny sorted top-N list instead of sorting every stop in the city.
            int insertAt = topWorstStops.Count;
            for (int i = 0; i < topWorstStops.Count; i++)
            {
                WorstStopSnapshot existing = topWorstStops[i];
                bool candidateIsWorse =
                    candidate.AverageWaitSeconds > existing.AverageWaitSeconds ||
                    (candidate.AverageWaitSeconds == existing.AverageWaitSeconds &&
                     candidate.WaitingPassengers > existing.WaitingPassengers);

                if (candidateIsWorse)
                {
                    insertAt = i;
                    break;
                }
            }

            topWorstStops.Insert(insertAt, candidate);

            if (topWorstStops.Count > TopWorstStopCount)
            {
                topWorstStops.RemoveAt(topWorstStops.Count - 1);
            }
        }

        private void BuildLateGroupStats(
            ref LateGroupStats bus,
            ref LateGroupStats train,
            ref LateGroupStats tram,
            ref LateGroupStats subway,
            ref LateGroupStats ship,
            ref LateGroupStats ferry,
            ref LateGroupStats air)
        {
            uint frame = m_SimulationSystem.frameIndex;
            // Managed sets are fine here because this is an on-demand diagnostic scan, not scheduled job work.
            HashSet<Entity> busGroups = new HashSet<Entity>();
            HashSet<Entity> trainGroups = new HashSet<Entity>();
            HashSet<Entity> tramGroups = new HashSet<Entity>();
            HashSet<Entity> subwayGroups = new HashSet<Entity>();
            HashSet<Entity> shipGroups = new HashSet<Entity>();
            HashSet<Entity> ferryGroups = new HashSet<Entity>();
            HashSet<Entity> airGroups = new HashSet<Entity>();

            // This is a point-in-time diagnostic for the log report, not a gameplay mutation.
            m_BoardingVehicleQuery.CompleteDependency();
            using NativeArray<Entity> vehicles = m_BoardingVehicleQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < vehicles.Length; i++)
            {
                Entity vehicleEntity = vehicles[i];
                if (!EntityManager.Exists(vehicleEntity) ||
                    !EntityManager.HasComponent<Game.Vehicles.PublicTransport>(vehicleEntity) ||
                    !EntityManager.HasBuffer<Passenger>(vehicleEntity))
                {
                    continue;
                }

                Game.Vehicles.PublicTransport publicTransport = EntityManager.GetComponentData<Game.Vehicles.PublicTransport>(vehicleEntity);
                if ((publicTransport.m_State & PublicTransportFlags.Boarding) == 0 ||
                    frame < publicTransport.m_DepartureFrame)
                {
                    continue;
                }

                if (!TryGetVehicleTransportType(vehicleEntity, out TransportType transportType))
                {
                    continue;
                }

                DynamicBuffer<Passenger> passengers = EntityManager.GetBuffer<Passenger>(vehicleEntity);
                int lateGroupPassengersForVehicle = 0;
                HashSet<Entity> groupsForVehicle = GetGroupSet(transportType, busGroups, trainGroups, tramGroups, subwayGroups, shipGroups, ferryGroups, airGroups);

                for (int j = 0; j < passengers.Length; j++)
                {
                    Entity passenger = passengers[j].m_Passenger;
                    if (!IsLateGroupPassenger(vehicleEntity, passenger, out Entity groupKey))
                    {
                        continue;
                    }

                    lateGroupPassengersForVehicle++;
                    groupsForVehicle.Add(groupKey);
                }

                if (lateGroupPassengersForVehicle <= 0)
                {
                    continue;
                }

                AddLateGroupPassengers(
                    transportType,
                    lateGroupPassengersForVehicle,
                    ref bus,
                    ref train,
                    ref tram,
                    ref subway,
                    ref ship,
                    ref ferry,
                    ref air);
            }

            bus.Groups = busGroups.Count;
            train.Groups = trainGroups.Count;
            tram.Groups = tramGroups.Count;
            subway.Groups = subwayGroups.Count;
            ship.Groups = shipGroups.Count;
            ferry.Groups = ferryGroups.Count;
            air.Groups = airGroups.Count;
        }

        private bool IsLateGroupPassenger(Entity vehicleEntity, Entity passenger, out Entity groupKey)
        {
            groupKey = Entity.Null;
            if (!EntityManager.Exists(passenger) ||
                EntityManager.HasComponent<Deleted>(passenger) ||
                EntityManager.HasComponent<Destroyed>(passenger) ||
                EntityManager.HasComponent<Temp>(passenger) ||
                EntityManager.HasComponent<Overridden>(passenger) ||
                !EntityManager.HasComponent<CurrentVehicle>(passenger))
            {
                return false;
            }

            CurrentVehicle currentVehicle = EntityManager.GetComponentData<CurrentVehicle>(passenger);
            if (currentVehicle.m_Vehicle != vehicleEntity ||
                (currentVehicle.m_Flags & CreatureVehicleFlags.Ready) != 0)
            {
                return false;
            }

            if (EntityManager.HasComponent<GroupMember>(passenger))
            {
                // Report group members, but leave them to vanilla until group boarding is fully researched.
                groupKey = EntityManager.GetComponentData<GroupMember>(passenger).m_Leader;
                return true;
            }

            if (EntityManager.HasBuffer<GroupCreature>(passenger))
            {
                // Group leaders own this buffer, so count them as group passengers too.
                groupKey = passenger;
                return true;
            }

            return false;
        }

        private static HashSet<Entity> GetGroupSet(
            TransportType transportType,
            HashSet<Entity> bus,
            HashSet<Entity> train,
            HashSet<Entity> tram,
            HashSet<Entity> subway,
            HashSet<Entity> ship,
            HashSet<Entity> ferry,
            HashSet<Entity> air)
        {
            switch (transportType)
            {
                case TransportType.Bus:
                    return bus;
                case TransportType.Train:
                    return train;
                case TransportType.Tram:
                    return tram;
                case TransportType.Subway:
                    return subway;
                case TransportType.Ship:
                    return ship;
                case TransportType.Ferry:
                    return ferry;
                case TransportType.Airplane:
                    return air;
                default:
                    return bus;
            }
        }

        private static void AddLateGroupPassengers(
            TransportType transportType,
            int passengers,
            ref LateGroupStats bus,
            ref LateGroupStats train,
            ref LateGroupStats tram,
            ref LateGroupStats subway,
            ref LateGroupStats ship,
            ref LateGroupStats ferry,
            ref LateGroupStats air)
        {
            switch (transportType)
            {
                case TransportType.Bus:
                    bus.Passengers += passengers;
                    bus.Vehicles++;
                    break;
                case TransportType.Train:
                    train.Passengers += passengers;
                    train.Vehicles++;
                    break;
                case TransportType.Tram:
                    tram.Passengers += passengers;
                    tram.Vehicles++;
                    break;
                case TransportType.Subway:
                    subway.Passengers += passengers;
                    subway.Vehicles++;
                    break;
                case TransportType.Ship:
                    ship.Passengers += passengers;
                    ship.Vehicles++;
                    break;
                case TransportType.Ferry:
                    ferry.Passengers += passengers;
                    ferry.Vehicles++;
                    break;
                case TransportType.Airplane:
                    air.Passengers += passengers;
                    air.Vehicles++;
                    break;
            }
        }

        private bool TryGetTransportType(Entity lineEntity, out TransportType transportType)
        {
            transportType = TransportType.None;
            if (lineEntity == Entity.Null || !EntityManager.HasComponent<PrefabRef>(lineEntity))
            {
                return false;
            }

            PrefabRef prefabRef = EntityManager.GetComponentData<PrefabRef>(lineEntity);
            if (!EntityManager.HasComponent<TransportLineData>(prefabRef.m_Prefab))
            {
                return false;
            }

            TransportLineData lineData = EntityManager.GetComponentData<TransportLineData>(prefabRef.m_Prefab);
            if (!lineData.m_PassengerTransport)
            {
                return false;
            }

            transportType = lineData.m_TransportType;
            return true;
        }

        private bool TryGetVehicleTransportType(Entity vehicleEntity, out TransportType transportType)
        {
            transportType = TransportType.None;
            if (!EntityManager.HasComponent<PrefabRef>(vehicleEntity))
            {
                return false;
            }

            Entity prefabEntity = EntityManager.GetComponentData<PrefabRef>(vehicleEntity).m_Prefab;
            if (!EntityManager.HasComponent<PublicTransportVehicleData>(prefabEntity))
            {
                return false;
            }

            transportType = EntityManager.GetComponentData<PublicTransportVehicleData>(prefabEntity).m_TransportType;
            return true;
        }

        private string ResolveStopName(Entity stopEntity, TransportType transportType)
        {
            string name = m_NameSystem.GetRenderedLabelName(stopEntity);
            if (!string.IsNullOrWhiteSpace(name))
            {
                return SimplifyStopName(name, transportType);
            }

            return SimplifyStopName(m_NameSystem.GetDebugName(stopEntity), transportType);
        }

        private string ResolveLineName(Entity lineEntity)
        {
            if (lineEntity == Entity.Null)
            {
                return string.Empty;
            }

            string name = m_NameSystem.GetRenderedLabelName(lineEntity);
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = NormalizeWhitespace(name);
                return IsGenericLineToolName(name)
                    ? string.Empty
                    : name;
            }

            string debugName = NormalizeWhitespace(m_NameSystem.GetDebugName(lineEntity));
            return IsGenericLineToolName(debugName)
                ? string.Empty
                : debugName;
        }

        private FollowUpTargetInfo ResolveFollowUpTarget(Entity nextTargetEntity, TransportType transportType)
        {
            if (nextTargetEntity == Entity.Null || !EntityManager.Exists(nextTargetEntity))
            {
                return new FollowUpTargetInfo(
                    FollowUpTargetKind.None,
                    string.Empty,
                    Entity.Null,
                    Entity.Null,
                    string.Empty,
                    Entity.Null,
                    string.Empty);
            }

            Entity waypointEntity = Entity.Null;
            Entity stopEntity = Entity.Null;
            Entity lineEntity = Entity.Null;

            Entity current = nextTargetEntity;
            // Walk a short owner chain because the next target is often a lane/waypoint, not the stop itself.
            for (int depth = 0; depth < 6; depth++)
            {
                if (current == Entity.Null || !EntityManager.Exists(current))
                {
                    break;
                }

                if (EntityManager.HasComponent<Connected>(current))
                {
                    waypointEntity = waypointEntity == Entity.Null ? current : waypointEntity;
                    Entity connectedStop = EntityManager.GetComponentData<Connected>(current).m_Connected;
                    if (connectedStop != Entity.Null)
                    {
                        stopEntity = connectedStop;
                    }
                }

                if (lineEntity == Entity.Null && TryGetTransportType(current, out _))
                {
                    lineEntity = current;
                }

                if (!EntityManager.HasComponent<Owner>(current))
                {
                    break;
                }

                current = EntityManager.GetComponentData<Owner>(current).m_Owner;
            }

            string targetName = m_NameSystem.GetRenderedLabelName(nextTargetEntity);
            if (string.IsNullOrWhiteSpace(targetName))
            {
                targetName = m_NameSystem.GetDebugName(nextTargetEntity);
            }

            targetName = string.IsNullOrWhiteSpace(targetName)
                ? string.Empty
                : NormalizeWhitespace(targetName);

            string stopName = stopEntity != Entity.Null
                ? ResolveStopName(stopEntity, transportType)
                : string.Empty;

            if (string.IsNullOrWhiteSpace(stopName))
            {
                stopName = string.Empty;
            }

            string lineName = lineEntity != Entity.Null
                ? ResolveLineName(lineEntity)
                : string.Empty;

            FollowUpTargetKind targetKind;
            string targetLabel;
            if (stopEntity != Entity.Null)
            {
                targetKind = FollowUpTargetKind.Stop;
                targetLabel = stopName;
            }
            else if (EntityManager.HasComponent<Game.Vehicles.PublicTransport>(nextTargetEntity))
            {
                targetKind = FollowUpTargetKind.Vehicle;
                targetLabel = targetName;
            }
            else if (waypointEntity != Entity.Null || EntityManager.HasComponent<Connected>(nextTargetEntity))
            {
                targetKind = FollowUpTargetKind.Waypoint;
                targetLabel = targetName;
            }
            else if (!string.IsNullOrWhiteSpace(targetName) &&
                targetName.IndexOf("Lane", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                targetKind = FollowUpTargetKind.Lane;
                targetLabel = targetName;
            }
            else
            {
                targetKind = FollowUpTargetKind.Target;
                targetLabel = targetName;
            }

            return new FollowUpTargetInfo(
                targetKind,
                targetLabel,
                waypointEntity,
                stopEntity,
                stopName,
                lineEntity,
                lineName);
        }

        private static bool IsGenericLineToolName(string name)
        {
            // Avoid misleading reports like "Bus Line Tool" when there is no player-facing line name.
            return name.EndsWith(" Line Tool", System.StringComparison.OrdinalIgnoreCase);
        }

        private static string SimplifyStopName(string name, TransportType transportType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            string simplified = RemoveWord(name, "Passenger");
            simplified = RemoveWord(simplified, transportType.ToString());
            simplified = NormalizeWhitespace(simplified);

            return string.IsNullOrWhiteSpace(simplified)
                ? name.Trim()
                : simplified;
        }

        private static string RemoveWord(string value, string word)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(word))
            {
                return value;
            }

            string[] parts = value.Split(' ');
            List<string> kept = new List<string>(parts.Length);
            for (int i = 0; i < parts.Length; i++)
            {
                if (string.Equals(parts[i], word, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                kept.Add(parts[i]);
            }

            return string.Join(" ", kept);
        }

        private static string NormalizeWhitespace(string value)
        {
            return string.Join(" ", value.Split(System.Array.Empty<char>(), System.StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
