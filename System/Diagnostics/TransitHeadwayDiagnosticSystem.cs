// <copyright file="TransitHeadwayDiagnosticSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/Diagnostics/TransitHeadwayDiagnosticSystem.cs
// Purpose: Read-only measurements of vanilla transit-line headway and boarding timing.

namespace BetterBoarding
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Colossal.Serialization.Entities;
    using CS2Shared.RiverMochi;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Routes;
    using Game.SceneFlow;
    using Game.Simulation;
    using Game.Tools;
    using Game.UI;
    using Unity.Collections;
    using Unity.Entities;
    using TransportType = Game.Prefabs.TransportType;

    /// <summary>
    /// Observes vanilla boarding transitions without changing route or vehicle state.
    /// Measurements are collected only while verbose diagnostics are enabled.
    /// </summary>
    public sealed partial class TransitHeadwayDiagnosticSystem : GameSystemBase
    {
        public const int UpdatesPerDay = 8192;

        private const float kFramesPerSecond = 60f;
        private const int kMaxReportedLinesPerTransportType = 3;
        private const int kReportHeaderWidth = 60;
        private const int kReportFieldWidth = 24;

        private readonly Dictionary<Entity, VehicleObservation> m_VehicleObservations =
            new Dictionary<Entity, VehicleObservation>();
        private readonly Dictionary<RouteWaypointKey, BoardingStartObservation> m_LastBoardingStarts =
            new Dictionary<RouteWaypointKey, BoardingStartObservation>();
        private readonly Dictionary<Entity, LineStatistics> m_LineStatistics =
            new Dictionary<Entity, LineStatistics>();
        private readonly HashSet<Entity> m_SeenVehicles = new HashSet<Entity>();
        private readonly List<Entity> m_StaleVehicles = new List<Entity>();
        private readonly List<Entity> m_StaleLines = new List<Entity>();
        private readonly List<LineStatistics> m_ReportLines = new List<LineStatistics>();
        private readonly List<LineStatistics> m_ModeReportLines = new List<LineStatistics>();

        private EntityQuery m_VehicleQuery;
        private SimulationSystem? m_SimulationSystem;
        private NameSystem? m_NameSystem;
        private uint m_LastSimulationFrame = uint.MaxValue;
        private bool m_WasCollecting;
        private bool m_CollectionFailed;

        private readonly struct RouteWaypointKey : IEquatable<RouteWaypointKey>
        {
            public RouteWaypointKey(Entity route, Entity waypoint)
            {
                Route = route;
                Waypoint = waypoint;
            }

            public Entity Route { get; }

            public Entity Waypoint { get; }

            public bool Equals(RouteWaypointKey other)
            {
                return Route == other.Route && Waypoint == other.Waypoint;
            }

            public override bool Equals(object? obj)
            {
                return obj is RouteWaypointKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (Route.GetHashCode() * 397) ^ Waypoint.GetHashCode();
            }
        }

        private readonly struct BoardingStartObservation
        {
            public BoardingStartObservation(
                uint frame,
                Entity vehicle,
                bool usedVanillaDepartureCalculation)
            {
                Frame = frame;
                Vehicle = vehicle;
                UsedVanillaDepartureCalculation = usedVanillaDepartureCalculation;
            }

            public uint Frame { get; }

            public Entity Vehicle { get; }

            public bool UsedVanillaDepartureCalculation { get; }
        }

        private struct VehicleObservation
        {
            public bool WasBoarding;

            public bool TrackBoardingEnd;

            public Entity Route;

            public uint ScheduledDepartureFrame;
        }

        private sealed class LineStatistics
        {
            public LineStatistics(Entity route, TransportType transportType)
            {
                Route = route;
                TransportType = transportType;
            }

            public Entity Route { get; }

            public TransportType TransportType { get; }

            public int BoardingStarts;

            public int CalculatedBoardingStarts;

            public int NonEnRouteBoardingStarts;

            public int HeadwaySamples;

            public int TargetComparisonSamples;

            public ulong HeadwaySumFrames;

            public uint MinimumHeadwayFrames = uint.MaxValue;

            public uint MaximumHeadwayFrames;

            public double TargetIntervalSumSeconds;

            public double HeadwayRatioSum;

            public double MinimumHeadwayRatio = double.MaxValue;

            public float MinimumHeadwayRatioTargetSeconds;

            public int UnderHalfTarget;

            public int UnderQuarterTarget;

            public Entity MinimumRatioPreviousVehicle;

            public Entity MinimumRatioCurrentVehicle;

            public int MinimumRatioScheduledHoldFrames;

            public int ScheduledHoldSamples;

            public long ScheduledHoldSumFrames;

            public int MinimumScheduledHoldFrames = int.MaxValue;

            public int MaximumScheduledHoldFrames = int.MinValue;

            public int BoardingEndSamples;

            public int PositiveBoardingEndOvershootSamples;

            public long PositiveBoardingEndOvershootSumFrames;

            public int MaximumPositiveBoardingEndOvershootFrames;

            public int BoardingEndsOnOrBeforeSchedule;

            public int BoardingEndsOver128FramesLate;

            public int BoardingEndsOver512FramesLate;

            public int BoardingEndsOver1024FramesLate;
        }

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            // 8192 updates/day gives one read-only observation every 32 simulation frames.
            return 262144 / UpdatesPerDay;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
            m_NameSystem = World.GetOrCreateSystemManaged<NameSystem>();
            m_VehicleQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Vehicles.PublicTransport, CurrentRoute, Target>()
                .WithNone<Deleted, Destroyed, Temp, Overridden>()
                .Build();
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (mode == GameMode.Game &&
                (purpose == Purpose.NewGame || purpose == Purpose.LoadGame))
            {
                ResetForCityLoad();
            }
        }

        protected override void OnUpdate()
        {
            if (!ShouldCollectDiagnostics())
            {
                if (m_WasCollecting)
                {
                    // Preserve completed aggregates, but do not bridge a disabled period
                    // with a false giant headway when verbose collection resumes.
                    ClearTransitionTracking();
                    m_WasCollecting = false;
                }

                return;
            }

            if (m_SimulationSystem == null || m_CollectionFailed)
            {
                return;
            }

            uint frame = m_SimulationSystem.frameIndex;
            if (m_LastSimulationFrame != uint.MaxValue && frame < m_LastSimulationFrame)
            {
                ResetForCityLoad();
            }

            m_LastSimulationFrame = frame;
            m_WasCollecting = true;

            try
            {
                ScanVehicles(frame);
            }
            catch (Exception ex)
            {
                m_CollectionFailed = true;
                ClearTransitionTracking();
                LogUtils.WarnOnce(
                    "BB_HEADWAY_DIAGNOSTIC_EXCEPTION",
                    () => $"{Mod.ModTag} Headway diagnostics stopped after {ex.GetType().Name}: {ex.Message}",
                    ex);
            }
        }

        internal void AppendReport(StringBuilder sb)
        {
            try
            {
                AppendReportCore(sb);
            }
            catch (Exception ex)
            {
                AppendSectionHeader(sb, "Vanilla headway / un-bunching diagnostics");
                AppendField(sb, "Status", $"report unavailable: {ex.GetType().Name}");
                LogUtils.WarnOnce(
                    "BB_HEADWAY_REPORT_EXCEPTION",
                    () => $"{Mod.ModTag} Headway diagnostic report failed: {ex.GetType().Name}: {ex.Message}",
                    ex);
            }
        }

        private void ScanVehicles(uint frame)
        {
            // The system is registered in the back band, after vanilla transport AI.
            // Complete tracked readers/writers before observing their final state this pass.
            m_VehicleQuery.CompleteDependency();

            m_SeenVehicles.Clear();
            using NativeArray<Entity> vehicles = m_VehicleQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < vehicles.Length; i++)
            {
                Entity vehicle = vehicles[i];
                m_SeenVehicles.Add(vehicle);

                if (!EntityManager.Exists(vehicle))
                {
                    continue;
                }

                Game.Vehicles.PublicTransport publicTransport =
                    EntityManager.GetComponentData<Game.Vehicles.PublicTransport>(vehicle);
                bool isBoarding =
                    (publicTransport.m_State & Game.Vehicles.PublicTransportFlags.Boarding) != 0;

                if (!m_VehicleObservations.TryGetValue(vehicle, out VehicleObservation previous))
                {
                    // A vehicle already boarding on the first observation is a bootstrap state,
                    // not a measured boarding start.
                    m_VehicleObservations.Add(
                        vehicle,
                        new VehicleObservation { WasBoarding = isBoarding });
                    continue;
                }

                if (previous.WasBoarding && !isBoarding && previous.TrackBoardingEnd)
                {
                    RecordBoardingEnd(previous.Route, previous.ScheduledDepartureFrame, frame);
                }

                if (!previous.WasBoarding && isBoarding &&
                    TryGetBoardingContext(
                        vehicle,
                        publicTransport,
                        out Entity route,
                        out Entity waypoint,
                        out TransportType transportType,
                        out Game.Routes.TransportLine transportLine))
                {
                    bool usedVanillaDepartureCalculation =
                        (publicTransport.m_State & Game.Vehicles.PublicTransportFlags.EnRoute) != 0;
                    if (!usedVanillaDepartureCalculation &&
                        EntityManager.HasComponent<Game.Vehicles.CargoTransport>(vehicle))
                    {
                        Game.Vehicles.CargoTransport cargoTransport =
                            EntityManager.GetComponentData<Game.Vehicles.CargoTransport>(vehicle);
                        usedVanillaDepartureCalculation =
                            (cargoTransport.m_State & Game.Vehicles.CargoTransportFlags.EnRoute) != 0;
                    }

                    RecordBoardingStart(
                        route,
                        waypoint,
                        vehicle,
                        transportType,
                        transportLine,
                        publicTransport.m_DepartureFrame,
                        usedVanillaDepartureCalculation,
                        frame);

                    previous.WasBoarding = true;
                    previous.TrackBoardingEnd = usedVanillaDepartureCalculation;
                    previous.Route = route;
                    previous.ScheduledDepartureFrame = publicTransport.m_DepartureFrame;
                    m_VehicleObservations[vehicle] = previous;
                    continue;
                }

                previous.WasBoarding = isBoarding;
                if (!isBoarding)
                {
                    previous.TrackBoardingEnd = false;
                    previous.Route = Entity.Null;
                    previous.ScheduledDepartureFrame = 0;
                }

                m_VehicleObservations[vehicle] = previous;
            }

            RemoveStaleVehicleObservations();
        }

        private bool TryGetBoardingContext(
            Entity vehicle,
            Game.Vehicles.PublicTransport publicTransport,
            out Entity route,
            out Entity waypoint,
            out TransportType transportType,
            out Game.Routes.TransportLine transportLine)
        {
            route = Entity.Null;
            waypoint = Entity.Null;
            transportType = default;
            transportLine = default;

            if ((publicTransport.m_State &
                    (Game.Vehicles.PublicTransportFlags.Evacuating |
                     Game.Vehicles.PublicTransportFlags.PrisonerTransport)) != 0)
            {
                return false;
            }

            CurrentRoute currentRoute = EntityManager.GetComponentData<CurrentRoute>(vehicle);
            Target target = EntityManager.GetComponentData<Target>(vehicle);
            route = currentRoute.m_Route;
            waypoint = target.m_Target;

            if (route == Entity.Null || waypoint == Entity.Null ||
                !EntityManager.Exists(route) || !EntityManager.Exists(waypoint) ||
                !EntityManager.HasComponent<Game.Routes.TransportLine>(route) ||
                !EntityManager.HasComponent<PrefabRef>(route) ||
                !EntityManager.HasComponent<Connected>(waypoint) ||
                !EntityManager.HasComponent<VehicleTiming>(waypoint))
            {
                return false;
            }

            PrefabRef routePrefabRef = EntityManager.GetComponentData<PrefabRef>(route);
            if (routePrefabRef.m_Prefab == Entity.Null ||
                !EntityManager.Exists(routePrefabRef.m_Prefab) ||
                !EntityManager.HasComponent<TransportLineData>(routePrefabRef.m_Prefab))
            {
                return false;
            }

            TransportLineData lineData =
                EntityManager.GetComponentData<TransportLineData>(routePrefabRef.m_Prefab);
            if (!lineData.m_PassengerTransport || !IsSupportedTransportType(lineData.m_TransportType))
            {
                return false;
            }

            transportType = lineData.m_TransportType;
            transportLine = EntityManager.GetComponentData<Game.Routes.TransportLine>(route);
            return true;
        }

        private void RecordBoardingStart(
            Entity route,
            Entity waypoint,
            Entity vehicle,
            TransportType transportType,
            Game.Routes.TransportLine transportLine,
            uint scheduledDepartureFrame,
            bool usedVanillaDepartureCalculation,
            uint frame)
        {
            LineStatistics statistics = GetOrCreateLineStatistics(route, transportType);
            statistics.BoardingStarts++;

            RouteWaypointKey key = new RouteWaypointKey(route, waypoint);
            if (!usedVanillaDepartureCalculation)
            {
                // Vanilla uses simulationFrame + 60 for this route-entry boarding and does
                // not call CalculateDepartureFrame or update VehicleTiming.LastDepartureFrame.
                statistics.NonEnRouteBoardingStarts++;
                m_LastBoardingStarts[key] =
                    new BoardingStartObservation(
                        frame,
                        vehicle,
                        usedVanillaDepartureCalculation: false);
                return;
            }

            statistics.CalculatedBoardingStarts++;
            int scheduledHoldFrames = Math.Max(
                0,
                SignedFrameDelta(scheduledDepartureFrame, frame));
            RecordScheduledHold(
                statistics,
                scheduledDepartureFrame,
                scheduledHoldFrames);

            if (m_LastBoardingStarts.TryGetValue(key, out BoardingStartObservation lastStart) &&
                lastStart.UsedVanillaDepartureCalculation)
            {
                uint gapFrames = unchecked(frame - lastStart.Frame);
                if (gapFrames > 0 && gapFrames <= (uint)int.MaxValue)
                {
                    RecordHeadwaySample(
                        statistics,
                        gapFrames,
                        transportLine.m_VehicleInterval,
                        lastStart.Vehicle,
                        vehicle,
                        scheduledHoldFrames);
                }
            }

            m_LastBoardingStarts[key] =
                new BoardingStartObservation(
                    frame,
                    vehicle,
                    usedVanillaDepartureCalculation: true);
        }

        private static void RecordHeadwaySample(
            LineStatistics statistics,
            uint gapFrames,
            float targetIntervalSeconds,
            Entity previousVehicle,
            Entity currentVehicle,
            int scheduledHoldFrames)
        {
            statistics.HeadwaySamples++;
            statistics.HeadwaySumFrames += gapFrames;
            statistics.MaximumHeadwayFrames =
                Math.Max(statistics.MaximumHeadwayFrames, gapFrames);

            if (gapFrames < statistics.MinimumHeadwayFrames)
            {
                statistics.MinimumHeadwayFrames = gapFrames;
            }

            if (targetIntervalSeconds <= 0f)
            {
                return;
            }

            float gapSeconds = gapFrames / kFramesPerSecond;
            double ratio = gapSeconds / targetIntervalSeconds;
            statistics.TargetComparisonSamples++;
            statistics.TargetIntervalSumSeconds += targetIntervalSeconds;
            statistics.HeadwayRatioSum += ratio;

            if (ratio < statistics.MinimumHeadwayRatio)
            {
                statistics.MinimumHeadwayRatio = ratio;
                statistics.MinimumRatioPreviousVehicle = previousVehicle;
                statistics.MinimumRatioCurrentVehicle = currentVehicle;
                statistics.MinimumHeadwayRatioTargetSeconds = targetIntervalSeconds;
                statistics.MinimumRatioScheduledHoldFrames = scheduledHoldFrames;
            }

            if (gapSeconds < targetIntervalSeconds * 0.5f)
            {
                statistics.UnderHalfTarget++;
            }

            if (gapSeconds < targetIntervalSeconds * 0.25f)
            {
                statistics.UnderQuarterTarget++;
            }
        }

        private static void RecordScheduledHold(
            LineStatistics statistics,
            uint scheduledDepartureFrame,
            int scheduledHoldFrames)
        {
            if (scheduledDepartureFrame == 0)
            {
                return;
            }

            statistics.ScheduledHoldSamples++;
            statistics.ScheduledHoldSumFrames += scheduledHoldFrames;
            statistics.MinimumScheduledHoldFrames =
                Math.Min(statistics.MinimumScheduledHoldFrames, scheduledHoldFrames);
            statistics.MaximumScheduledHoldFrames =
                Math.Max(statistics.MaximumScheduledHoldFrames, scheduledHoldFrames);
        }

        private void RecordBoardingEnd(
            Entity route,
            uint scheduledDepartureFrame,
            uint observedBoardingEndFrame)
        {
            if (route == Entity.Null || scheduledDepartureFrame == 0 ||
                !m_LineStatistics.TryGetValue(route, out LineStatistics statistics))
            {
                return;
            }

            int signedDeltaFrames =
                SignedFrameDelta(observedBoardingEndFrame, scheduledDepartureFrame);
            statistics.BoardingEndSamples++;

            if (signedDeltaFrames <= 0)
            {
                statistics.BoardingEndsOnOrBeforeSchedule++;
                return;
            }

            int positiveOvershootFrames = Math.Max(0, signedDeltaFrames);
            statistics.PositiveBoardingEndOvershootSamples++;
            statistics.PositiveBoardingEndOvershootSumFrames += positiveOvershootFrames;
            statistics.MaximumPositiveBoardingEndOvershootFrames =
                Math.Max(
                    statistics.MaximumPositiveBoardingEndOvershootFrames,
                    positiveOvershootFrames);

            if (positiveOvershootFrames > 128)
            {
                statistics.BoardingEndsOver128FramesLate++;
            }

            if (positiveOvershootFrames > 512)
            {
                statistics.BoardingEndsOver512FramesLate++;
            }

            if (positiveOvershootFrames > 1024)
            {
                statistics.BoardingEndsOver1024FramesLate++;
            }
        }

        private LineStatistics GetOrCreateLineStatistics(
            Entity route,
            TransportType transportType)
        {
            if (!m_LineStatistics.TryGetValue(route, out LineStatistics statistics))
            {
                statistics = new LineStatistics(route, transportType);
                m_LineStatistics.Add(route, statistics);
            }

            return statistics;
        }

        private void RemoveStaleVehicleObservations()
        {
            m_StaleVehicles.Clear();
            foreach (KeyValuePair<Entity, VehicleObservation> pair in m_VehicleObservations)
            {
                if (!m_SeenVehicles.Contains(pair.Key))
                {
                    m_StaleVehicles.Add(pair.Key);
                }
            }

            for (int i = 0; i < m_StaleVehicles.Count; i++)
            {
                m_VehicleObservations.Remove(m_StaleVehicles[i]);
            }
        }

        private void AppendReportCore(StringBuilder sb)
        {
            AppendSectionHeader(sb, "Vanilla headway / un-bunching diagnostics");

            string status = m_CollectionFailed
                ? "stopped after an error"
                : ShouldCollectDiagnostics()
                    ? "collecting (verbose ON)"
                    : "paused (verbose OFF)";

            AppendField(sb, "Status", status);
            AppendField(sb, "Sampling", "every 32 simulation frames; transition times may read 0-31f late");
            AppendField(sb, "Scope", "read-only; Bus, Tram, Train, Subway passenger lines");
            AppendField(sb, "Interpretation", "short-headway thresholds are diagnostic, not proof of a game bug");
            AppendField(sb, "Overshoot scope", "EnRoute starts using vanilla's calculated departure only");
            AppendField(sb, "Ranking", "per mode: <25% rate/count, <50% rate/count, minimum ratio, late-end tie-breakers");

            BuildReportLineList();
            AppendField(sb, "Lines with observations", LocaleUtils.FormatN0(m_ReportLines.Count));

            if (m_ReportLines.Count == 0)
            {
                sb.AppendLine("No qualifying boarding transitions have been observed yet.");
                return;
            }

            AppendField(
                sb,
                "Lines shown",
                $"up to {kMaxReportedLinesPerTransportType} diagnostic-ranked lines per transport type");

            AppendTransportTypeReports(sb, TransportType.Bus);
            AppendTransportTypeReports(sb, TransportType.Tram);
            AppendTransportTypeReports(sb, TransportType.Train);
            AppendTransportTypeReports(sb, TransportType.Subway);
        }

        private void BuildReportLineList()
        {
            m_ReportLines.Clear();
            m_StaleLines.Clear();

            foreach (KeyValuePair<Entity, LineStatistics> pair in m_LineStatistics)
            {
                Entity route = pair.Key;
                if (!EntityManager.Exists(route) ||
                    !EntityManager.HasComponent<Game.Routes.TransportLine>(route))
                {
                    m_StaleLines.Add(route);
                    continue;
                }

                m_ReportLines.Add(pair.Value);
            }

            for (int i = 0; i < m_StaleLines.Count; i++)
            {
                m_LineStatistics.Remove(m_StaleLines[i]);
            }
        }

        private void AppendTransportTypeReports(
            StringBuilder sb,
            TransportType transportType)
        {
            m_ModeReportLines.Clear();
            for (int i = 0; i < m_ReportLines.Count; i++)
            {
                LineStatistics statistics = m_ReportLines[i];
                if (statistics.TransportType == transportType)
                {
                    m_ModeReportLines.Add(statistics);
                }
            }

            if (m_ModeReportLines.Count == 0)
            {
                AppendField(sb, $"{transportType} lines", "none observed");
                return;
            }

            m_ModeReportLines.Sort(CompareDiagnosticPriority);
            int reportCount = Math.Min(
                kMaxReportedLinesPerTransportType,
                m_ModeReportLines.Count);
            AppendField(
                sb,
                $"{transportType} lines",
                $"showing {LocaleUtils.FormatN0(reportCount)} of {LocaleUtils.FormatN0(m_ModeReportLines.Count)}");

            for (int i = 0; i < reportCount; i++)
            {
                AppendLineReport(sb, m_ModeReportLines[i]);
            }
        }

        private void AppendLineReport(StringBuilder sb, LineStatistics statistics)
        {
            Entity route = statistics.Route;
            Game.Routes.TransportLine transportLine =
                EntityManager.GetComponentData<Game.Routes.TransportLine>(route);
            int vehicleCount = EntityManager.HasBuffer<RouteVehicle>(route)
                ? EntityManager.GetBuffer<RouteVehicle>(route, isReadOnly: true).Length
                : 0;

            string lineName = ResolveLineName(route);
            sb.AppendLine();
            sb.Append("-- ").Append(statistics.TransportType).Append(" — ")
                .Append(lineName).Append(" [").Append(route).AppendLine("] --");
            AppendField(sb, "Route vehicles", LocaleUtils.FormatN0(vehicleCount));
            AppendField(sb, "Vanilla interval", $"{transportLine.m_VehicleInterval:F1}s");
            AppendField(sb, "Unbunching factor", transportLine.m_UnbunchingFactor.ToString("F2"));
            AppendField(sb, "Boarding starts", LocaleUtils.FormatN0(statistics.BoardingStarts));
            AppendField(
                sb,
                "Vanilla departure calc",
                $"{LocaleUtils.FormatN0(statistics.CalculatedBoardingStarts)} | initial/non-EnRoute {LocaleUtils.FormatN0(statistics.NonEnRouteBoardingStarts)}");
            AppendField(sb, "Headway samples", LocaleUtils.FormatN0(statistics.HeadwaySamples));

            if (statistics.HeadwaySamples > 0)
            {
                double averageHeadwaySeconds =
                    statistics.HeadwaySumFrames / (kFramesPerSecond * statistics.HeadwaySamples);
                AppendField(
                    sb,
                    "Actual headway",
                    $"avg {averageHeadwaySeconds:F1}s | min {statistics.MinimumHeadwayFrames / kFramesPerSecond:F1}s | max {statistics.MaximumHeadwayFrames / kFramesPerSecond:F1}s");

                if (statistics.TargetComparisonSamples > 0)
                {
                    double averageTargetSeconds =
                        statistics.TargetIntervalSumSeconds / statistics.TargetComparisonSamples;
                    double averageRatio =
                        statistics.HeadwayRatioSum / statistics.TargetComparisonSamples;

                    AppendField(
                        sb,
                        "Target at samples",
                        $"avg {averageTargetSeconds:F1}s | avg actual/target {averageRatio:F2}x");
                    AppendField(
                        sb,
                        "Under target",
                        $"<50% {LocaleUtils.FormatN0(statistics.UnderHalfTarget)} | <25% {LocaleUtils.FormatN0(statistics.UnderQuarterTarget)}");
                    AppendField(
                        sb,
                        "Worst ratio pair",
                        $"{statistics.MinimumRatioPreviousVehicle} -> {statistics.MinimumRatioCurrentVehicle}");
                    AppendField(
                        sb,
                        "Headway at worst ratio",
                        $"{statistics.MinimumHeadwayRatio:F2}x target");
                    AppendField(
                        sb,
                        "Target at worst ratio",
                        $"{statistics.MinimumHeadwayRatioTargetSeconds:F1}s");
                    AppendField(
                        sb,
                        "Following vehicle hold",
                        $"{statistics.MinimumRatioScheduledHoldFrames / kFramesPerSecond:F1}s ({statistics.MinimumRatioScheduledHoldFrames}f) for {statistics.MinimumRatioCurrentVehicle}");
                }
                else
                {
                    AppendField(sb, "Target comparison", "unavailable; sampled interval was not positive");
                }
            }
            else
            {
                AppendField(sb, "Actual headway", "not enough consecutive EnRoute starts yet");
            }

            if (statistics.ScheduledHoldSamples > 0)
            {
                double averageHoldFrames =
                    (double)statistics.ScheduledHoldSumFrames / statistics.ScheduledHoldSamples;
                AppendField(
                    sb,
                    "Vanilla scheduled hold",
                    $"avg {averageHoldFrames / kFramesPerSecond:F1}s | min {statistics.MinimumScheduledHoldFrames / kFramesPerSecond:F1}s | max {statistics.MaximumScheduledHoldFrames / kFramesPerSecond:F1}s");
            }
            else
            {
                AppendField(sb, "Vanilla scheduled hold", "no EnRoute samples yet");
            }

            if (statistics.BoardingEndSamples > 0)
            {
                AppendField(
                    sb,
                    "Boarding ends observed",
                    LocaleUtils.FormatN0(statistics.BoardingEndSamples));
                AppendField(
                    sb,
                    "On/before schedule",
                    LocaleUtils.FormatN0(statistics.BoardingEndsOnOrBeforeSchedule));

                if (statistics.PositiveBoardingEndOvershootSamples > 0)
                {
                    double averagePositiveOvershootFrames =
                        (double)statistics.PositiveBoardingEndOvershootSumFrames /
                        statistics.PositiveBoardingEndOvershootSamples;
                    AppendField(
                        sb,
                        "Positive overshoot",
                        $"avg {averagePositiveOvershootFrames:F1}f | max {statistics.MaximumPositiveBoardingEndOvershootFrames}f");
                }
                else
                {
                    AppendField(sb, "Positive overshoot", "none observed");
                }

                AppendField(
                    sb,
                    ">128f",
                    LocaleUtils.FormatN0(statistics.BoardingEndsOver128FramesLate));
                AppendField(
                    sb,
                    ">512f",
                    LocaleUtils.FormatN0(statistics.BoardingEndsOver512FramesLate));
                AppendField(
                    sb,
                    ">1024f",
                    LocaleUtils.FormatN0(statistics.BoardingEndsOver1024FramesLate));
            }
            else
            {
                AppendField(sb, "Boarding ends observed", "none yet");
            }
        }

        private string ResolveLineName(Entity route)
        {
            string name = m_NameSystem?.GetRenderedLabelName(route) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = m_NameSystem?.GetDebugName(route) ?? string.Empty;
            }

            return string.IsNullOrWhiteSpace(name) ? "(unnamed line)" : name.Trim();
        }

        private static int CompareDiagnosticPriority(
            LineStatistics left,
            LineStatistics right)
        {
            int comparison = CompareRateDescending(
                left.UnderQuarterTarget,
                left.TargetComparisonSamples,
                right.UnderQuarterTarget,
                right.TargetComparisonSamples);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = right.UnderQuarterTarget.CompareTo(left.UnderQuarterTarget);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = CompareRateDescending(
                left.UnderHalfTarget,
                left.TargetComparisonSamples,
                right.UnderHalfTarget,
                right.TargetComparisonSamples);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = right.UnderHalfTarget.CompareTo(left.UnderHalfTarget);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = left.MinimumHeadwayRatio.CompareTo(right.MinimumHeadwayRatio);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = right.BoardingEndsOver1024FramesLate.CompareTo(
                left.BoardingEndsOver1024FramesLate);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = right.BoardingEndsOver512FramesLate.CompareTo(
                left.BoardingEndsOver512FramesLate);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = right.BoardingEndsOver128FramesLate.CompareTo(
                left.BoardingEndsOver128FramesLate);
            if (comparison != 0)
            {
                return comparison;
            }

            return right.HeadwaySamples.CompareTo(left.HeadwaySamples);
        }

        private static int CompareRateDescending(
            int leftCount,
            int leftSampleCount,
            int rightCount,
            int rightSampleCount)
        {
            double leftRate = leftSampleCount > 0
                ? (double)leftCount / leftSampleCount
                : -1d;
            double rightRate = rightSampleCount > 0
                ? (double)rightCount / rightSampleCount
                : -1d;

            return rightRate.CompareTo(leftRate);
        }

        private void ResetForCityLoad()
        {
            m_LineStatistics.Clear();
            ClearTransitionTracking();
            m_LastSimulationFrame = uint.MaxValue;
            m_WasCollecting = false;
            m_CollectionFailed = false;
        }

        private void ClearTransitionTracking()
        {
            m_VehicleObservations.Clear();
            m_LastBoardingStarts.Clear();
            m_SeenVehicles.Clear();
            m_StaleVehicles.Clear();
        }

        private static int SignedFrameDelta(uint later, uint earlier)
        {
            return unchecked((int)(later - earlier));
        }

        private static bool IsSupportedTransportType(TransportType transportType)
        {
            return transportType == TransportType.Bus ||
                transportType == TransportType.Tram ||
                transportType == TransportType.Train ||
                transportType == TransportType.Subway;
        }

        private static bool ShouldCollectDiagnostics()
        {
#if DEBUG
            return true;
#else
            return BoardingRuntimeSettings.EnableVerboseLogging;
#endif
        }

        private static void AppendSectionHeader(StringBuilder sb, string title)
        {
            sb.AppendLine(new string('=', kReportHeaderWidth));
            sb.AppendLine(title);
            sb.AppendLine(new string('=', kReportHeaderWidth));
        }

        private static void AppendField(StringBuilder sb, string label, string value)
        {
            sb.Append(label);
            if (label.Length < kReportFieldWidth)
            {
                sb.Append('.', kReportFieldWidth - label.Length);
            }

            sb.Append(": ").AppendLine(value);
        }
    }
}
