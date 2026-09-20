// <copyright file="WaitStatus.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/Status/WaitStatus.cs
// Purpose: Cached Options UI status text for current transit wait snapshots.

namespace BetterBoarding
{
    using System;
    using System.Text;
    using CS2Shared.RiverMochi;
    using Game;
  //  using Game.Common;
    using Game.Creatures;
   // using Game.Pathfind;
    using Game.Prefabs;
    using Game.SceneFlow;
    using Game.Simulation;
    using Unity.Entities;
    using UnityEngine;

    public static class WaitStatus
    {
        public const string KeyStatusNotLoaded = "FB_STATUS_NOT_LOADED";
        public const string KeyNoCityLoaded = "FB_STATUS_NO_CITY_LOADED";
        public const string KeyNoStopsFound = "FB_STATUS_NO_STOPS";

        public const string KeyStatusLine = "FB_STATUS_LINE";
        public const string KeyStatusLateSkipped = "FB_STATUS_LATE_SKIPPED";
        public const string KeyStatusSkipOff = "FB_STATUS_SKIP_OFF";
        public const string KeyStatusOverviewLine = "FB_STATUS_OVERVIEW_LINE";
        public const string KeyStatusRunSoonerLine = "FB_STATUS_RUN_SOONER_LINE";
        public const string KeyStatusRunSoonerOff = "FB_STATUS_RUN_SOONER_OFF";

        public const string KeyReportNoCityLoaded = "FB_REPORT_NO_CITY_LOADED";
        public const string KeyReportTitle = "FB_REPORT_TITLE";
        public const string KeyReportSettings = "FB_REPORT_SETTINGS";
        public const string KeyReportNote = "FB_REPORT_NOTE";
        public const string KeyReportTesterHintsHeader = "FB_REPORT_TESTER_HINTS_HEADER";
        public const string KeyReportHintWorstStops = "FB_REPORT_HINT_WORST_STOPS";
        public const string KeyReportHintSkippedCims = "FB_REPORT_HINT_SKIPPED_CIMS";
        public const string KeyReportHintLateGroups = "FB_REPORT_HINT_LATE_GROUPS";
        public const string KeyReportFamilyHeader = "FB_REPORT_FAMILY_HEADER";
        public const string KeyReportServedStops = "FB_REPORT_SERVED_STOPS";
        public const string KeyReportStopsWithWaiting = "FB_REPORT_STOPS_WITH_WAITING";
        public const string KeyReportWaitingPassengers = "FB_REPORT_WAITING_PASSENGERS";
        public const string KeyReportAverageWait = "FB_REPORT_AVERAGE_WAIT";
        public const string KeyReportLateBoardersSkipped = "FB_REPORT_LATE_BOARDERS_SKIPPED";
        public const string KeyReportWorstStopNone = "FB_REPORT_WORST_STOP_NONE";
        public const string KeyReportWorstStopAverageWait = "FB_REPORT_WORST_STOP_AVERAGE_WAIT";
        public const string KeyReportWorstStopName = "FB_REPORT_WORST_STOP_NAME";
        public const string KeyReportWorstStopEntity = "FB_REPORT_WORST_STOP_ENTITY";
        public const string KeyReportWorstWaypointEntity = "FB_REPORT_WORST_WAYPOINT_ENTITY";
        public const string KeyReportWorstLineHint = "FB_REPORT_WORST_LINE_HINT";
        public const string KeyReportWorstLineEntity = "FB_REPORT_WORST_LINE_ENTITY";
        public const string KeyReportWorstLineWaypointAverage = "FB_REPORT_WORST_LINE_WAYPOINT_AVERAGE";
        public const string KeyReportTopWorstStopsHeader = "FB_REPORT_TOP_WORST_STOPS_HEADER";
        public const string KeyReportTopWorstStopLine = "FB_REPORT_TOP_WORST_STOP_LINE";
        public const string KeyReportLateGroups = "FB_REPORT_LATE_GROUPS";
        public const string KeyReportLastSkippedSamplesHeader = "FB_REPORT_LAST_SKIPPED_SAMPLES_HEADER";
        public const string KeyReportLastSkippedSampleLine = "FB_REPORT_LAST_SKIPPED_SAMPLE_LINE";
        public const string KeyReportNone = "FB_REPORT_NONE";
        public const string KeyReportUnknown = "FB_REPORT_UNKNOWN";


        private const int kReportHeaderWidth = 60;
        private const int kReportFieldWidth = 24;
        private const int kSkippedSampleCapacityPerMode = 3;

        public static int RefreshIntervalSeconds { get; set; } = 15;

        public static string OverviewSummary { get; private set; } = string.Empty;
        public static string CimsRunSoonerSummary { get; private set; } = string.Empty;
        public static string BusSummary { get; private set; } = string.Empty;
        public static string TrainSummary { get; private set; } = string.Empty;
        public static string TramSummary { get; private set; } = string.Empty;
        public static string SubwaySummary { get; private set; } = string.Empty;
        public static string ShipSummary { get; private set; } = string.Empty;
        public static string FerrySummary { get; private set; } = string.Empty;
        public static string AirSummary { get; private set; } = string.Empty;

        private static bool s_WasInGame;
        private static bool s_HasSnapshotThisCity;
        private static long s_LastRefreshTicksUtc;
        private static int s_LastUiFrame = -1;
        private static int s_CurrentDayKey = int.MinValue;
        private static uint s_LastSimulationFrame = uint.MaxValue;
        private static long s_BusLateBoardersToday;
        private static long s_TrainLateBoardersToday;
        private static long s_TramLateBoardersToday;
        private static long s_SubwayLateBoardersToday;
        private static long s_ShipLateBoardersToday;
        private static long s_FerryLateBoardersToday;
        private static long s_AirLateBoardersToday;
        private static long s_BusRunSoonerToday;
        private static long s_TrainRunSoonerToday;
        private static long s_TramRunSoonerToday;
        private static long s_SubwayRunSoonerToday;
        private static long s_ShipRunSoonerToday;
        private static long s_FerryRunSoonerToday;
        private static long s_AirRunSoonerToday;
        private static DateTime s_LastSnapshotLocalTime;
        // Outcome counters/rings back the detailed log report without bloating the Options UI rows.
        private static FollowUpOutcomeCounts s_BusFollowUpOutcomes;
        private static FollowUpOutcomeCounts s_TrainFollowUpOutcomes;
        private static FollowUpOutcomeCounts s_TramFollowUpOutcomes;
        private static FollowUpOutcomeCounts s_SubwayFollowUpOutcomes;
        private static FollowUpOutcomeCounts s_ShipFollowUpOutcomes;
        private static FollowUpOutcomeCounts s_FerryFollowUpOutcomes;
        private static FollowUpOutcomeCounts s_AirFollowUpOutcomes;
        private static readonly SkippedPassengerSampleRing s_BusSkippedSamples = new();
        private static readonly SkippedPassengerSampleRing s_TrainSkippedSamples = new();
        private static readonly SkippedPassengerSampleRing s_TramSkippedSamples = new();
        private static readonly SkippedPassengerSampleRing s_SubwaySkippedSamples = new();
        private static readonly SkippedPassengerSampleRing s_ShipSkippedSamples = new();
        private static readonly SkippedPassengerSampleRing s_FerrySkippedSamples = new();
        private static readonly SkippedPassengerSampleRing s_AirSkippedSamples = new();
        private static readonly LateBoarderFollowUpSampleRing s_BusFollowUpSamples = new();
        private static readonly LateBoarderFollowUpSampleRing s_TrainFollowUpSamples = new();
        private static readonly LateBoarderFollowUpSampleRing s_TramFollowUpSamples = new();
        private static readonly LateBoarderFollowUpSampleRing s_SubwayFollowUpSamples = new();
        private static readonly LateBoarderFollowUpSampleRing s_ShipFollowUpSamples = new();
        private static readonly LateBoarderFollowUpSampleRing s_FerryFollowUpSamples = new();
        private static readonly LateBoarderFollowUpSampleRing s_AirFollowUpSamples = new();

        private readonly struct SkippedPassengerSample
        {
            public SkippedPassengerSample(TransportType transportType, Entity vehicle, Entity passenger, uint frame, DateTime localTime)
            {
                TransportType = transportType;
                Vehicle = vehicle;
                Passenger = passenger;
                Frame = frame;
                LocalTime = localTime;
            }

            public TransportType TransportType { get; }

            public Entity Vehicle { get; }

            public Entity Passenger { get; }

            public uint Frame { get; }

            public DateTime LocalTime { get; }
        }

        private readonly struct LateBoarderFollowUpSample
        {
            public LateBoarderFollowUpSample(
                TransportType transportType,
                Entity passenger,
                Entity missedVehicle,
                DateTime skippedLocalTime,
                DateTime followUpLocalTime,
                TransitWaitStatusSystem.FollowUpSnapshot snapshot)
            {
                TransportType = transportType;
                Passenger = passenger;
                MissedVehicle = missedVehicle;
                SkippedLocalTime = skippedLocalTime;
                FollowUpLocalTime = followUpLocalTime;
                Outcome = snapshot.Outcome;
                OutcomeLabel = snapshot.OutcomeLabel;
                CurrentVehicle = snapshot.CurrentVehicle;
                CurrentVehicleFlags = snapshot.CurrentVehicleFlags;
                NextTargetEntity = snapshot.NextTargetEntity;
                NextTargetKind = snapshot.NextTargetKind;
                NextTargetName = snapshot.NextTargetName;
                NextStopEntity = snapshot.NextStopEntity;
                NextStopName = snapshot.NextStopName;
                NextWaypointEntity = snapshot.NextWaypointEntity;
                NextLineEntity = snapshot.NextLineEntity;
                NextLineName = snapshot.NextLineName;
            }

            public TransportType TransportType { get; }

            public Entity Passenger { get; }

            public Entity MissedVehicle { get; }

            public DateTime SkippedLocalTime { get; }

            public DateTime FollowUpLocalTime { get; }

            public TransitWaitStatusSystem.FollowUpOutcome Outcome { get; }

            public string OutcomeLabel { get; }

            public Entity CurrentVehicle { get; }

            public CreatureVehicleFlags CurrentVehicleFlags { get; }

            public Entity NextTargetEntity { get; }

            public TransitWaitStatusSystem.FollowUpTargetKind NextTargetKind { get; }

            public string NextTargetName { get; }

            public Entity NextStopEntity { get; }

            public string NextStopName { get; }

            public Entity NextWaypointEntity { get; }

            public Entity NextLineEntity { get; }

            public string NextLineName { get; }

            public string CurrentVehicleText => CurrentVehicle == Entity.Null
                ? "none"
                : $"{CurrentVehicle} ({CurrentVehicleFlags})";
        }

        private struct FollowUpOutcomeCounts
        {
            public int SameVehicle;

            public int DifferentVehicle;

            public int HasPathNotAssignedYet;

            public int Other;

            public void Record(TransitWaitStatusSystem.FollowUpOutcome outcome)
            {
                switch (outcome)
                {
                    case TransitWaitStatusSystem.FollowUpOutcome.SameVehicle:
                        SameVehicle++;
                        break;
                    case TransitWaitStatusSystem.FollowUpOutcome.DifferentVehicle:
                        DifferentVehicle++;
                        break;
                    case TransitWaitStatusSystem.FollowUpOutcome.HasPathNotAssignedYet:
                        HasPathNotAssignedYet++;
                        break;
                    default:
                        Other++;
                        break;
                }
            }
        }

        private sealed class SkippedPassengerSampleRing
        {
            private readonly SkippedPassengerSample[] m_Samples =
                new SkippedPassengerSample[kSkippedSampleCapacityPerMode];
            private int m_Count;
            private int m_NextIndex;

            public int Count => m_Count;

            public void Add(SkippedPassengerSample sample)
            {
                m_Samples[m_NextIndex] = sample;
                m_NextIndex = (m_NextIndex + 1) % m_Samples.Length;

                if (m_Count < m_Samples.Length)
                {
                    m_Count++;
                }
            }

            public void Clear()
            {
                m_Count = 0;
                m_NextIndex = 0;
            }

            public SkippedPassengerSample GetNewest(int newestIndex)
            {
                int index = (m_NextIndex - 1 - newestIndex + m_Samples.Length) % m_Samples.Length;
                return m_Samples[index];
            }
        }

        private sealed class LateBoarderFollowUpSampleRing
        {
            private readonly LateBoarderFollowUpSample[] m_Samples =
                new LateBoarderFollowUpSample[kSkippedSampleCapacityPerMode];
            private int m_Count;
            private int m_NextIndex;

            public int Count => m_Count;

            public void Add(LateBoarderFollowUpSample sample)
            {
                m_Samples[m_NextIndex] = sample;
                m_NextIndex = (m_NextIndex + 1) % m_Samples.Length;

                if (m_Count < m_Samples.Length)
                {
                    m_Count++;
                }
            }

            public void Clear()
            {
                m_Count = 0;
                m_NextIndex = 0;
            }

            public LateBoarderFollowUpSample GetNewest(int newestIndex)
            {
                int index = (m_NextIndex - 1 - newestIndex + m_Samples.Length) % m_Samples.Length;
                return m_Samples[index];
            }
        }

        public static void InvalidateCache()
        {
            s_HasSnapshotThisCity = false;
            s_LastRefreshTicksUtc = 0;
            s_LastUiFrame = -1;

            BusSummary = Localize(KeyStatusNotLoaded, "Status not loaded.");
            OverviewSummary = BusSummary;
            CimsRunSoonerSummary = string.Empty;
            TrainSummary = string.Empty;
            TramSummary = string.Empty;
            SubwaySummary = string.Empty;
            ShipSummary = string.Empty;
            FerrySummary = string.Empty;
            AirSummary = string.Empty;
        }

        public static void ResetForCityLoad()
        {
            // Called from a guaranteed city-load lifecycle hook so switched saves start clean,
            // while later Options UI refreshes do not erase counters already collected.
            s_WasInGame = true;
            s_CurrentDayKey = int.MinValue;
            s_LastSimulationFrame = uint.MaxValue;
            ResetDailyCounters();
            InvalidateCache();
        }

        public static void RefreshIfNeeded()
        {
            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                return;
            }

            // The Options UI polls each property separately, so refresh once per UI frame at most.
            int frame = Time.frameCount;
            if (frame == s_LastUiFrame)
            {
                return;
            }

            s_LastUiFrame = frame;

            GameManager gm = GameManager.instance;
            bool isGame = gm.gameMode.IsGame();

            if (isGame != s_WasInGame)
            {
                bool leftGame = s_WasInGame && !isGame;
                s_WasInGame = isGame;

                // Loading/unloading a city makes cached stop text stale. Do not reset skipped
                // counters when entering a city here, because the skip system may have already
                // recorded passengers before the player opens Options UI for the first time.
                InvalidateCache();

                if (leftGame)
                {
                    ResetDailyCounters();
                }
            }

            if (!isGame)
            {
                BusSummary = Localize(KeyNoCityLoaded, "No city loaded.");
                OverviewSummary = BusSummary;
                CimsRunSoonerSummary = string.Empty;
                TrainSummary = string.Empty;
                TramSummary = string.Empty;
                SubwaySummary = string.Empty;
                ShipSummary = string.Empty;
                FerrySummary = string.Empty;
                AirSummary = string.Empty;
                return;
            }

            EnsureCounterDay(world);

            long nowUtc = DateTime.UtcNow.Ticks;
            if (!s_HasSnapshotThisCity)
            {
                BuildSnapshotSafe(world);
                s_HasSnapshotThisCity = true;
                s_LastRefreshTicksUtc = nowUtc;
                return;
            }

            int intervalSeconds = Math.Max(1, RefreshIntervalSeconds);
            long nextAllowed = s_LastRefreshTicksUtc + TimeSpan.FromSeconds(intervalSeconds).Ticks;
            if (nowUtc < nextAllowed)
            {
                return;
            }

            BuildSnapshotSafe(world);
            s_LastRefreshTicksUtc = nowUtc;
        }

        private static void BuildSnapshotSafe(World world)
        {
            try
            {
                // Snapshot work is on demand; this does not run in the city per frame.
                TransitWaitStatusSystem system = world.GetOrCreateSystemManaged<TransitWaitStatusSystem>();
                TransitWaitStatusSystem.Snapshot snapshot = system.BuildSnapshot();
                ApplySnapshot(snapshot);
            }
            catch (Exception ex)
            {
                BusSummary = Localize(KeyStatusNotLoaded, "Status not loaded.");
                CimsRunSoonerSummary = string.Empty;
                TrainSummary = string.Empty;
                TramSummary = string.Empty;
                SubwaySummary = string.Empty;
                ShipSummary = string.Empty;
                FerrySummary = string.Empty;
                AirSummary = string.Empty;

                LogUtils.WarnOnce(
                    "FB_STATUS_SNAPSHOT_EXCEPTION",
                    () => $"{Mod.ModTag} Transit status snapshot failed: {ex.GetType().Name}: {ex.Message}",
                    ex);
            }
        }

        public static void LogDetailedReport()
        {
            World world = World.DefaultGameObjectInjectionWorld;
            GameManager gm = GameManager.instance;
            if (world == null || !world.IsCreated || !gm.gameMode.IsGame())
            {
                LogUtils.Info(Mod.s_Log, () => Localize(KeyReportNoCityLoaded, "[BBoard] Stats report requested, but no city is loaded."));
                return;
            }

            try
            {
                EnsureCounterDay(world);
                TransitWaitStatusSystem system = world.GetOrCreateSystemManaged<TransitWaitStatusSystem>();
                TransitWaitStatusSystem.Snapshot snapshot = system.BuildSnapshot();
                ApplySnapshot(snapshot);

                // Keep this verbose output in the log, not the cramped Options UI row.
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                AppendSectionHeader(sb, Localize(KeyReportTitle, "Better Boarding transit status report"));
                AppendField(sb, "Snapshot updated", s_LastSnapshotLocalTime == default ? "unknown" : s_LastSnapshotLocalTime.ToString("HH:mm:ss"));
                AppendField(sb, "Options Settings", BoardingRuntimeSettings.DescribeForLog());
                AppendField(sb, "Note", Localize(KeyReportNote, "Worst line is a hint from the highest-wait waypoint at the worst stop."));

                AppendTesterHints(sb);
                AppendSummaryReport(sb, snapshot);

                AppendFamilyReport(sb, "Bus", snapshot.Bus, s_BusLateBoardersToday, s_BusSkippedSamples, s_BusFollowUpOutcomes, s_BusFollowUpSamples);
                AppendFamilyReport(sb, "Tram", snapshot.Tram, s_TramLateBoardersToday, s_TramSkippedSamples, s_TramFollowUpOutcomes, s_TramFollowUpSamples);
                AppendFamilyReport(sb, "Train", snapshot.Train, s_TrainLateBoardersToday, s_TrainSkippedSamples, s_TrainFollowUpOutcomes, s_TrainFollowUpSamples);
                AppendFamilyReport(sb, "Subway", snapshot.Subway, s_SubwayLateBoardersToday, s_SubwaySkippedSamples, s_SubwayFollowUpOutcomes, s_SubwayFollowUpSamples);
                AppendFamilyReport(sb, "Ferry", snapshot.Ferry, s_FerryLateBoardersToday, s_FerrySkippedSamples, s_FerryFollowUpOutcomes, s_FerryFollowUpSamples);
                AppendFamilyReport(sb, "Ship", snapshot.Ship, s_ShipLateBoardersToday, s_ShipSkippedSamples, s_ShipFollowUpOutcomes, s_ShipFollowUpSamples);
                AppendFamilyReport(sb, "Airplane", snapshot.Air, s_AirLateBoardersToday, s_AirSkippedSamples, s_AirFollowUpOutcomes, s_AirFollowUpSamples);

                AppendDivider(sb);

                LogUtils.Info(Mod.s_Log, () => sb.ToString());
            }
            catch (Exception ex)
            {
                LogUtils.Warn(Mod.s_Log, () => $"{Mod.ModTag} Stats report failed: {ex.GetType().Name}: {ex.Message}", ex);
            }
        }

        private static void AppendTesterHints(StringBuilder sb)
        {
            sb.AppendLine();
            AppendSubHeader(sb, Localize(KeyReportTesterHintsHeader, "Tester hints"));
            sb.AppendLine("- " + Localize(
                KeyReportHintWorstStops,
                "Worst stops: inspect these first in-game or with Scene Explorer. Look for accidents, blocked traffic, bad stop placement, or a bugged stop."));
            sb.AppendLine("- " + Localize(
                KeyReportHintSkippedCims,
                "Skipped solo cims: later state should usually become 'has path' or 'assigned'. If it stays 'no path yet', inspect that cim entity after more time."));
            sb.AppendLine("- " + Localize(
                KeyReportHintLateGroups,
                ": families left to vanilla. High counts are clues for future safe group-travel support."));
        }

        internal static void RecordLateBoardersCanceled(World world, TransportType transportType, int count)
        {
            if (count <= 0)
            {
                return;
            }

            EnsureCounterDay(world);

            switch (transportType)
            {
                case TransportType.Bus:
                    s_BusLateBoardersToday += count;
                    break;
                case TransportType.Train:
                    s_TrainLateBoardersToday += count;
                    break;
                case TransportType.Tram:
                    s_TramLateBoardersToday += count;
                    break;
                case TransportType.Subway:
                    s_SubwayLateBoardersToday += count;
                    break;
                case TransportType.Ship:
                    s_ShipLateBoardersToday += count;
                    break;
                case TransportType.Ferry:
                    s_FerryLateBoardersToday += count;
                    break;
                case TransportType.Airplane:
                    s_AirLateBoardersToday += count;
                    break;
            }
        }

        internal static void RecordRunSoonerAssists(World world, TransportType transportType, int count)
        {
            if (count <= 0)
            {
                return;
            }

            EnsureCounterDay(world);

            switch (transportType)
            {
                case TransportType.Bus:
                    s_BusRunSoonerToday += count;
                    break;
                case TransportType.Train:
                    s_TrainRunSoonerToday += count;
                    break;
                case TransportType.Tram:
                    s_TramRunSoonerToday += count;
                    break;
                case TransportType.Subway:
                    s_SubwayRunSoonerToday += count;
                    break;
                case TransportType.Ship:
                    s_ShipRunSoonerToday += count;
                    break;
                case TransportType.Ferry:
                    s_FerryRunSoonerToday += count;
                    break;
                case TransportType.Airplane:
                    s_AirRunSoonerToday += count;
                    break;
            }
        }

        internal static void RecordLateBoarderSample(World world, TransportType transportType, Entity vehicle, Entity passenger)
        {
            EnsureCounterDay(world);

            uint frame = world.GetExistingSystemManaged<SimulationSystem>()?.frameIndex ?? 0;
            // Keep only the most recent few examples per mode so the report stays readable.
            GetSkippedSampleRing(transportType).Add(
                new SkippedPassengerSample(transportType, vehicle, passenger, frame, DateTime.Now));
        }

        internal static void RecordLateBoarderFollowUp(
            World world,
            TransportType transportType,
            Entity missedVehicle,
            Entity passenger,
            DateTime skippedLocalTime,
            DateTime followUpLocalTime,
            TransitWaitStatusSystem.FollowUpSnapshot snapshot)
        {
            EnsureCounterDay(world);

            RecordFollowUpOutcome(transportType, snapshot.Outcome);
            GetFollowUpSampleRing(transportType).Add(
                new LateBoarderFollowUpSample(
                    transportType,
                    passenger,
                    missedVehicle,
                    skippedLocalTime,
                    followUpLocalTime,
                    snapshot));
        }

        private static string FormatFamily(TransitWaitStatusSystem.FamilySnapshot family, long lateBoardersCanceledToday)
        {
            if (family.StopCount == 0)
            {
                return LocaleUtils.Localize(KeyNoStopsFound, "No stops found.");
            }

            string lateSkipText = BoardingRuntimeSettings.CancelLateBoarders
                ? LocaleUtils.SafeFormat(
                    KeyStatusLateSkipped,
                    "{0} late today",
                    LocaleUtils.FormatN0(lateBoardersCanceledToday))
                : LocaleUtils.Localize(KeyStatusSkipOff, "skip OFF");

            return LocaleUtils.SafeFormat(
                KeyStatusLine,
                "{0} waiting | avg {1} | worst {2} | {3}",
                LocaleUtils.FormatN0(family.WaitingPassengers),
                FormatDuration(family.AverageWaitSeconds),
                FormatDuration(family.WorstStopWaitSeconds),
                lateSkipText);
        }

        private static void ApplySnapshot(TransitWaitStatusSystem.Snapshot snapshot)
        {
            s_LastSnapshotLocalTime = DateTime.Now;
            OverviewSummary = FormatOverview(snapshot);
            CimsRunSoonerSummary = FormatRunSoonerSummary();
            BusSummary = FormatFamily(snapshot.Bus, s_BusLateBoardersToday);
            TrainSummary = FormatFamily(snapshot.Train, s_TrainLateBoardersToday);
            TramSummary = FormatFamily(snapshot.Tram, s_TramLateBoardersToday);
            SubwaySummary = FormatFamily(snapshot.Subway, s_SubwayLateBoardersToday);
            ShipSummary = FormatFamily(snapshot.Ship, s_ShipLateBoardersToday);
            FerrySummary = FormatFamily(snapshot.Ferry, s_FerryLateBoardersToday);
            AirSummary = FormatFamily(snapshot.Air, s_AirLateBoardersToday);
        }

        private static string FormatOverview(TransitWaitStatusSystem.Snapshot snapshot)
        {
            return LocaleUtils.SafeFormat(
                KeyStatusOverviewLine,
                "{0} tourist/mo | {1} citizens/mo | updated {2}",
                LocaleUtils.FormatN0(snapshot.MonthlyTourists),
                LocaleUtils.FormatN0(snapshot.MonthlyCitizens),
                s_LastSnapshotLocalTime.ToString("HH:mm:ss"));
        }

        private static string FormatRunSoonerSummary()
        {
            if (!BoardingRuntimeSettings.CimsRunSoonerToCatchBuses)
            {
                return Localize(KeyStatusRunSoonerOff, "run sooner OFF");
            }

            string summary =
                $"{LocaleUtils.FormatN0(s_BusRunSoonerToday)} bus | " +
                $"{LocaleUtils.FormatN0(s_TramRunSoonerToday)} tram | " +
                $"{LocaleUtils.FormatN0(s_TrainRunSoonerToday)} train";

            return LocaleUtils.SafeFormat(KeyStatusRunSoonerLine, "{0}", summary);
        }

        private static void AppendSummaryReport(StringBuilder sb, TransitWaitStatusSystem.Snapshot snapshot)
        {
            sb.AppendLine();
            AppendSectionHeader(sb, "Summary");
            AppendField(
                sb,
                "Total public transit",
                $"{LocaleUtils.FormatN0(snapshot.MonthlyTourists)} tourists/mo | {LocaleUtils.FormatN0(snapshot.MonthlyCitizens)} citizens/mo");
            AppendField(sb, "Cims run sooner", FormatRunSoonerSummary());
            AppendSummaryLine(sb, "Bus", snapshot.Bus, s_BusLateBoardersToday);
            AppendSummaryLine(sb, "Tram", snapshot.Tram, s_TramLateBoardersToday);
            AppendSummaryLine(sb, "Train", snapshot.Train, s_TrainLateBoardersToday);
            AppendSummaryLine(sb, "Subway", snapshot.Subway, s_SubwayLateBoardersToday);
            AppendSummaryLine(sb, "Ferry", snapshot.Ferry, s_FerryLateBoardersToday);
            AppendSummaryLine(sb, "Ship", snapshot.Ship, s_ShipLateBoardersToday);
            AppendSummaryLine(sb, "Airplane", snapshot.Air, s_AirLateBoardersToday);
        }

        private static void AppendSummaryLine(
            StringBuilder sb,
            string label,
            TransitWaitStatusSystem.FamilySnapshot family,
            long lateBoardersCanceledToday)
        {
            if (family.StopCount == 0)
            {
                AppendField(sb, label, Localize(KeyNoStopsFound, "No stops found."));
                return;
            }

            AppendField(
                sb,
                label,
                $"{LocaleUtils.FormatN0(family.WaitingPassengers)} waiting | avg wait {FormatDuration(family.AverageWaitSeconds)} | worst stop {FormatDuration(family.WorstStopWaitSeconds)} | queued/total stops {LocaleUtils.FormatN0(family.ActiveQueueStops)}/{LocaleUtils.FormatN0(family.StopCount)} | {LocaleUtils.FormatN0(lateBoardersCanceledToday)} skipped late passengers");
        }

        private static void AppendFamilyReport(
            StringBuilder sb,
            string label,
            TransitWaitStatusSystem.FamilySnapshot family,
            long lateBoardersCanceledToday,
            SkippedPassengerSampleRing skippedSamples,
            FollowUpOutcomeCounts followUpOutcomes,
            LateBoarderFollowUpSampleRing followUpSamples)
        {
            sb.AppendLine();
            AppendSectionHeader(sb, FormatReport(KeyReportFamilyHeader, "{0}", label));

            if (family.StopCount == 0)
            {
                sb.AppendLine(Localize(KeyNoStopsFound, "No stops found."));
                return;
            }

            AppendField(
                sb,
                "Status",
                $"{LocaleUtils.FormatN0(family.WaitingPassengers)} waiting | avg wait {FormatDuration(family.AverageWaitSeconds)} | worst stop {FormatDuration(family.WorstStopWaitSeconds)} | queued/total stops {LocaleUtils.FormatN0(family.ActiveQueueStops)}/{LocaleUtils.FormatN0(family.StopCount)}");
            AppendField(sb, "Late solo cims skipped", LocaleUtils.FormatN0(lateBoardersCanceledToday) + " today");
            AppendField(
                sb,
                "Late groups (families) not skipped",
                $"{LocaleUtils.FormatN0(family.LateGroupPassengers)} passengers | {LocaleUtils.FormatN0(family.LateGroupGroups)} groups | {LocaleUtils.FormatN0(family.LateGroupVehicles)} vehicles");
            AppendField(sb, "Follow-up outcomes (verbose)", FormatFollowUpOutcomes(followUpOutcomes));

            if (family.WaitingPassengers <= 0)
            {
                sb.AppendLine(Localize(KeyReportWorstStopNone, "Worst stop: none, no waiting passengers right now."));
            }
            else
            {
                AppendTopWorstStops(sb, family);
            }

            AppendSkippedPassengerSamples(sb, skippedSamples, followUpSamples);
        }

        private static void AppendTopWorstStops(StringBuilder sb, TransitWaitStatusSystem.FamilySnapshot family)
        {
            if (family.TopWorstStops.Length == 0)
            {
                return;
            }

            sb.AppendLine();
            AppendSubHeader(sb, FormatReport(
                KeyReportTopWorstStopsHeader,
                "Top {0} worst stops by average wait",
                LocaleUtils.FormatN0(family.TopWorstStops.Length)).TrimEnd(':'));

            for (int i = 0; i < family.TopWorstStops.Length; i++)
            {
                TransitWaitStatusSystem.WorstStopSnapshot stop = family.TopWorstStops[i];
                sb.AppendLine(
                    $"{LocaleUtils.FormatN0(i + 1)}. {TextOrUnknown(stop.StopName)} | avg {FormatDuration(stop.AverageWaitSeconds)} | waiting {LocaleUtils.FormatN0(stop.WaitingPassengers)} | line hint {TextOrUnknown(stop.LineName)}");
                sb.AppendLine(
                    $"   stop {EntityText(stop.StopEntity)} | waypoint {EntityText(stop.WaypointEntity)} | line {EntityText(stop.LineEntity)}");
            }
        }

        private static void AppendSkippedPassengerSamples(
            StringBuilder sb,
            SkippedPassengerSampleRing skippedSamples,
            LateBoarderFollowUpSampleRing followUpSamples)
        {
            sb.AppendLine();
            AppendSubHeader(sb, "Skipped solo Late cim follow-up examples");
            sb.AppendLine("Legend: state=same vehicle/different vehicle means assigned; has path means repathing or walking; no path yet means unresolved.");
            sb.AppendLine("        next=stop/lane/waypoint/vehicle/target shows the cim's next path target.");

            if (followUpSamples.Count > 0)
            {
                // Prefer follow-up examples because they show what vanilla did after the skip.
                for (int i = 0; i < followUpSamples.Count; i++)
                {
                    LateBoarderFollowUpSample sample = followUpSamples.GetNewest(i);
                    sb.AppendLine(
                        $"{LocaleUtils.FormatN0(i + 1)}. {sample.TransportType} | outcome {sample.OutcomeLabel} | passenger {EntityText(sample.Passenger)} | missed vehicle {EntityText(sample.MissedVehicle)} | skipped {sample.SkippedLocalTime:HH:mm:ss} | follow-up {sample.FollowUpLocalTime:HH:mm:ss}");
                    sb.AppendLine(
                        $"   state {DescribeFollowUpState(sample)}");
                }

                return;
            }

            if (skippedSamples.Count == 0)
            {
                sb.AppendLine(Localize(KeyReportNone, "none"));
                return;
            }

            sb.AppendLine(GetFollowUpWaitingMessage());
            for (int i = 0; i < skippedSamples.Count; i++)
            {
                SkippedPassengerSample sample = skippedSamples.GetNewest(i);
                sb.AppendLine(
                    $"{LocaleUtils.FormatN0(i + 1)}. {sample.TransportType} | passenger {EntityText(sample.Passenger)} | missed vehicle {EntityText(sample.Vehicle)} | skipped {sample.LocalTime:HH:mm:ss}");
            }
        }

        private static SkippedPassengerSampleRing GetSkippedSampleRing(TransportType transportType)
        {
            switch (transportType)
            {
                case TransportType.Bus:
                    return s_BusSkippedSamples;
                case TransportType.Train:
                    return s_TrainSkippedSamples;
                case TransportType.Tram:
                    return s_TramSkippedSamples;
                case TransportType.Subway:
                    return s_SubwaySkippedSamples;
                case TransportType.Ship:
                    return s_ShipSkippedSamples;
                case TransportType.Ferry:
                    return s_FerrySkippedSamples;
                case TransportType.Airplane:
                    return s_AirSkippedSamples;
                default:
                    return s_BusSkippedSamples;
            }
        }

        private static LateBoarderFollowUpSampleRing GetFollowUpSampleRing(TransportType transportType)
        {
            switch (transportType)
            {
                case TransportType.Bus:
                    return s_BusFollowUpSamples;
                case TransportType.Train:
                    return s_TrainFollowUpSamples;
                case TransportType.Tram:
                    return s_TramFollowUpSamples;
                case TransportType.Subway:
                    return s_SubwayFollowUpSamples;
                case TransportType.Ship:
                    return s_ShipFollowUpSamples;
                case TransportType.Ferry:
                    return s_FerryFollowUpSamples;
                case TransportType.Airplane:
                    return s_AirFollowUpSamples;
                default:
                    return s_BusFollowUpSamples;
            }
        }

        private static FollowUpOutcomeCounts GetFollowUpOutcomeCounts(TransportType transportType)
        {
            switch (transportType)
            {
                case TransportType.Bus:
                    return s_BusFollowUpOutcomes;
                case TransportType.Train:
                    return s_TrainFollowUpOutcomes;
                case TransportType.Tram:
                    return s_TramFollowUpOutcomes;
                case TransportType.Subway:
                    return s_SubwayFollowUpOutcomes;
                case TransportType.Ship:
                    return s_ShipFollowUpOutcomes;
                case TransportType.Ferry:
                    return s_FerryFollowUpOutcomes;
                case TransportType.Airplane:
                    return s_AirFollowUpOutcomes;
                default:
                    return s_BusFollowUpOutcomes;
            }
        }

        private static void RecordFollowUpOutcome(
            TransportType transportType,
            TransitWaitStatusSystem.FollowUpOutcome outcome)
        {
            switch (transportType)
            {
                case TransportType.Bus:
                    s_BusFollowUpOutcomes.Record(outcome);
                    break;
                case TransportType.Train:
                    s_TrainFollowUpOutcomes.Record(outcome);
                    break;
                case TransportType.Tram:
                    s_TramFollowUpOutcomes.Record(outcome);
                    break;
                case TransportType.Subway:
                    s_SubwayFollowUpOutcomes.Record(outcome);
                    break;
                case TransportType.Ship:
                    s_ShipFollowUpOutcomes.Record(outcome);
                    break;
                case TransportType.Ferry:
                    s_FerryFollowUpOutcomes.Record(outcome);
                    break;
                case TransportType.Airplane:
                    s_AirFollowUpOutcomes.Record(outcome);
                    break;
            }
        }

        private static void AppendSectionHeader(StringBuilder sb, string title)
        {
            AppendDivider(sb);
            sb.AppendLine(title);
            AppendDivider(sb);
        }

        private static void AppendSubHeader(StringBuilder sb, string title)
        {
            sb.AppendLine("-- " + title + " --");
        }

        private static void AppendDivider(StringBuilder sb)
        {
            sb.AppendLine(new string('=', kReportHeaderWidth));
        }

        private static void AppendField(StringBuilder sb, string label, string value)
        {
            sb.Append(label);
            if (label.Length < kReportFieldWidth)
            {
                sb.Append('.', kReportFieldWidth - label.Length);
            }

            sb.Append(": ");
            sb.AppendLine(value);
        }

        private static string FormatFollowUpOutcomes(FollowUpOutcomeCounts counts)
        {
            if (counts.SameVehicle == 0 &&
                counts.DifferentVehicle == 0 &&
                counts.HasPathNotAssignedYet == 0 &&
                counts.Other == 0)
            {
                return IsVerboseFollowUpCollectionEnabled()
                    ? "none recorded yet"
                    : "verbose follow-up OFF";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(LocaleUtils.FormatN0(counts.SameVehicle)).Append(" same vehicle");
            sb.Append(" | ").Append(LocaleUtils.FormatN0(counts.DifferentVehicle)).Append(" different vehicle");
            sb.Append(" | ").Append(LocaleUtils.FormatN0(counts.HasPathNotAssignedYet)).Append(" has path, not assigned yet");

            if (counts.Other > 0)
            {
                sb.Append(" | ").Append(LocaleUtils.FormatN0(counts.Other)).Append(" other");
            }

            return sb.ToString();
        }

        private static string EntityText(Entity entity)
        {
            return entity == Entity.Null ? Localize(KeyReportNone, "none") : entity.ToString();
        }

        private static string TextOrUnknown(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? Localize(KeyReportUnknown, "(unknown)") : value;
        }

        private static string GetFollowUpWaitingMessage()
        {
            return IsVerboseFollowUpCollectionEnabled()
                ? "No verbose follow-up recorded yet. Leave verbose logging on a little longer after skips."
                : "Verbose follow-up collection is OFF. Enable [Verbose log] to record what skipped cims do next.";
        }

        private static bool IsVerboseFollowUpCollectionEnabled()
        {
#if DEBUG
            return true;
#else
            return BoardingRuntimeSettings.EnableVerboseLogging;
#endif
        }

        private static string DescribeFollowUpState(LateBoarderFollowUpSample sample)
        {
            if (sample.CurrentVehicle != Entity.Null)
            {
                string assignment = sample.Outcome == TransitWaitStatusSystem.FollowUpOutcome.SameVehicle
                    ? $"same vehicle {sample.CurrentVehicleText}"
                    : $"different vehicle {sample.CurrentVehicleText}";
                return AppendFollowUpTargetDetails(assignment, sample);
            }

            if (sample.Outcome == TransitWaitStatusSystem.FollowUpOutcome.HasPathNotAssignedYet)
            {
                return AppendFollowUpTargetDetails("has path", sample);
            }

            return "no path yet";
        }

        private static string AppendFollowUpTargetDetails(
            string summary,
            LateBoarderFollowUpSample sample)
        {
            string nextTarget = DescribeFollowUpTarget(sample);
            if (!string.IsNullOrWhiteSpace(nextTarget))
            {
                summary += $" | next {nextTarget}";
            }

            if (sample.NextTargetKind == TransitWaitStatusSystem.FollowUpTargetKind.Stop &&
                !string.IsNullOrWhiteSpace(sample.NextLineName))
            {
                summary += $" | line {sample.NextLineName}";
            }

            return summary;
        }

        private static string DescribeFollowUpTarget(LateBoarderFollowUpSample sample)
        {
            switch (sample.NextTargetKind)
            {
                case TransitWaitStatusSystem.FollowUpTargetKind.Stop:
                    return DescribeNamedEntity("stop", sample.NextStopName, sample.NextStopEntity);
                case TransitWaitStatusSystem.FollowUpTargetKind.Lane:
                    return DescribeNamedEntity("lane", sample.NextTargetName, Entity.Null);
                case TransitWaitStatusSystem.FollowUpTargetKind.Waypoint:
                    return DescribeNamedEntity("waypoint", sample.NextTargetName, sample.NextTargetEntity);
                case TransitWaitStatusSystem.FollowUpTargetKind.Vehicle:
                    return DescribeNamedEntity("vehicle", sample.NextTargetName, sample.NextTargetEntity);
                case TransitWaitStatusSystem.FollowUpTargetKind.Target:
                    return DescribeNamedEntity("target", sample.NextTargetName, sample.NextTargetEntity);
                default:
                    return string.Empty;
            }
        }

        private static string DescribeNamedEntity(string kind, string name, Entity entity)
        {
            string label = string.IsNullOrWhiteSpace(name)
                ? kind
                : $"{kind} {name}";
            return entity == Entity.Null
                ? label
                : $"{label} {EntityText(entity)}";
        }

        private static string FormatReport(string entryId, string fallbackFormat, params object[] args)
        {
            return LocaleUtils.SafeFormat(entryId, fallbackFormat, args);
        }

        private static string FormatDuration(int seconds)
        {
            seconds = Math.Max(0, seconds);
            if (seconds < 60)
            {
                return seconds + "s";
            }

            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;
            return remainingSeconds == 0
                ? minutes + "m"
                : minutes + "m" + remainingSeconds + "s";
        }

        private static void EnsureCounterDay(World world)
        {
            SimulationSystem? simulationSystem = world.GetExistingSystemManaged<SimulationSystem>();
            if (simulationSystem == null)
            {
                return;
            }

            if (s_LastSimulationFrame != uint.MaxValue && simulationSystem.frameIndex < s_LastSimulationFrame)
            {
                // Lower frame index means a different load/city; reset stale daily counters.
                s_CurrentDayKey = int.MinValue;
                ResetDailyCounters();
            }

            s_LastSimulationFrame = simulationSystem.frameIndex;

            TimeSystem? timeSystem = world.GetExistingSystemManaged<TimeSystem>() ??
                world.GetOrCreateSystemManaged<TimeSystem>();
            if (timeSystem == null)
            {
                return;
            }

            DateTime currentDate = timeSystem.GetCurrentDateTime();
            int dayKey = (currentDate.Year * 1000) + currentDate.DayOfYear;
            if (dayKey == s_CurrentDayKey)
            {
                return;
            }

            // "Skipped today" follows the in-game calendar day.
            s_CurrentDayKey = dayKey;
            ResetDailyCounters();
        }

        private static void ResetDailyCounters()
        {
            s_BusLateBoardersToday = 0;
            s_TrainLateBoardersToday = 0;
            s_TramLateBoardersToday = 0;
            s_SubwayLateBoardersToday = 0;
            s_ShipLateBoardersToday = 0;
            s_FerryLateBoardersToday = 0;
            s_AirLateBoardersToday = 0;
            s_BusRunSoonerToday = 0;
            s_TrainRunSoonerToday = 0;
            s_TramRunSoonerToday = 0;
            s_SubwayRunSoonerToday = 0;
            s_ShipRunSoonerToday = 0;
            s_FerryRunSoonerToday = 0;
            s_AirRunSoonerToday = 0;
            s_BusFollowUpOutcomes = default;
            s_TrainFollowUpOutcomes = default;
            s_TramFollowUpOutcomes = default;
            s_SubwayFollowUpOutcomes = default;
            s_ShipFollowUpOutcomes = default;
            s_FerryFollowUpOutcomes = default;
            s_AirFollowUpOutcomes = default;
            s_BusSkippedSamples.Clear();
            s_TrainSkippedSamples.Clear();
            s_TramSkippedSamples.Clear();
            s_SubwaySkippedSamples.Clear();
            s_ShipSkippedSamples.Clear();
            s_FerrySkippedSamples.Clear();
            s_AirSkippedSamples.Clear();
            s_BusFollowUpSamples.Clear();
            s_TrainFollowUpSamples.Clear();
            s_TramFollowUpSamples.Clear();
            s_SubwayFollowUpSamples.Clear();
            s_ShipFollowUpSamples.Clear();
            s_FerryFollowUpSamples.Clear();
            s_AirFollowUpSamples.Clear();
        }

        private static string Localize(string entryId, string fallback) => LocaleUtils.Localize(entryId, fallback);
    }
}
