// <copyright file="BoardingPerformanceStats.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/Status/BoardingPerformanceStats.cs
// Purpose: Low-overhead in-memory timing and workload counters for the on-demand stats report.

namespace BetterBoarding
{
    using System;
    using System.Diagnostics;

    internal static class BoardingPerformanceStats
    {
        private static readonly long s_OneMillisecondTicks =
            Math.Max(1L, Stopwatch.Frequency / 1000L);
        private static DateTime s_CollectionStartedLocalTime = DateTime.Now;
        private static TimingAccumulator s_BoardingAssistTiming;
        private static TimingAccumulator s_LateGroupTiming;
        private static long s_VehiclesScanned;
        private static long s_PassengersScanned;
        private static long s_LateSoloCandidates;
        private static long s_LateSoloCanceled;
        private static long s_RunSoonerAssists;
        private static long s_BoardingCommandBufferPlaybacks;
        private static long s_GroupControllersScanned;
        private static long s_GroupCandidates;
        private static long s_GroupsReleased;
        private static long s_GroupsAssisted;
        private static long s_GroupMembersPrompted;
        private static long s_GroupCommandBufferPlaybacks;

        internal static long BeginSample()
        {
            return Stopwatch.GetTimestamp();
        }

        internal static void RecordBoardingAssist(
            long startedTimestamp,
            int vehiclesScanned,
            int passengersScanned,
            int lateSoloCandidates,
            int lateSoloCanceled,
            int runSoonerAssists,
            bool playedCommandBuffer)
        {
            s_BoardingAssistTiming.Record(ElapsedTicks(startedTimestamp));
            s_VehiclesScanned += vehiclesScanned;
            s_PassengersScanned += passengersScanned;
            s_LateSoloCandidates += lateSoloCandidates;
            s_LateSoloCanceled += lateSoloCanceled;
            s_RunSoonerAssists += runSoonerAssists;

            if (playedCommandBuffer)
            {
                s_BoardingCommandBufferPlaybacks++;
            }
        }

        internal static void RecordLateGroups(
            long startedTimestamp,
            int controllersScanned,
            int candidates,
            int groupsReleased,
            int groupsAssisted,
            int membersPrompted,
            bool playedCommandBuffer)
        {
            s_LateGroupTiming.Record(ElapsedTicks(startedTimestamp));
            s_GroupControllersScanned += controllersScanned;
            s_GroupCandidates += candidates;
            s_GroupsReleased += groupsReleased;
            s_GroupsAssisted += groupsAssisted;
            s_GroupMembersPrompted += membersPrompted;

            if (playedCommandBuffer)
            {
                s_GroupCommandBufferPlaybacks++;
            }
        }

        internal static Snapshot GetSnapshot()
        {
            return new Snapshot(
                s_CollectionStartedLocalTime,
                s_BoardingAssistTiming.ToSnapshot(),
                s_LateGroupTiming.ToSnapshot(),
                s_VehiclesScanned,
                s_PassengersScanned,
                s_LateSoloCandidates,
                s_LateSoloCanceled,
                s_RunSoonerAssists,
                s_BoardingCommandBufferPlaybacks,
                s_GroupControllersScanned,
                s_GroupCandidates,
                s_GroupsReleased,
                s_GroupsAssisted,
                s_GroupMembersPrompted,
                s_GroupCommandBufferPlaybacks);
        }

        internal static void ResetForCityLoad()
        {
            s_CollectionStartedLocalTime = DateTime.Now;
            s_BoardingAssistTiming = default;
            s_LateGroupTiming = default;
            s_VehiclesScanned = 0;
            s_PassengersScanned = 0;
            s_LateSoloCandidates = 0;
            s_LateSoloCanceled = 0;
            s_RunSoonerAssists = 0;
            s_BoardingCommandBufferPlaybacks = 0;
            s_GroupControllersScanned = 0;
            s_GroupCandidates = 0;
            s_GroupsReleased = 0;
            s_GroupsAssisted = 0;
            s_GroupMembersPrompted = 0;
            s_GroupCommandBufferPlaybacks = 0;
        }

        private static long ElapsedTicks(long startedTimestamp)
        {
            long elapsed = Stopwatch.GetTimestamp() - startedTimestamp;
            return elapsed > 0 ? elapsed : 0;
        }

        private struct TimingAccumulator
        {
            public long Updates;
            public long TotalTicks;
            public long MaximumTicks;
            public long UpdatesOverOneMillisecond;
            public long UpdatesOverFiveMilliseconds;
            public long UpdatesOverTenMilliseconds;

            public void Record(long elapsedTicks)
            {
                Updates++;
                TotalTicks += elapsedTicks;

                if (elapsedTicks > MaximumTicks)
                {
                    MaximumTicks = elapsedTicks;
                }

                if (elapsedTicks >= s_OneMillisecondTicks)
                {
                    UpdatesOverOneMillisecond++;
                }

                if (elapsedTicks >= s_OneMillisecondTicks * 5L)
                {
                    UpdatesOverFiveMilliseconds++;
                }

                if (elapsedTicks >= s_OneMillisecondTicks * 10L)
                {
                    UpdatesOverTenMilliseconds++;
                }
            }

            public TimingSnapshot ToSnapshot()
            {
                return new TimingSnapshot(
                    Updates,
                    TotalTicks,
                    MaximumTicks,
                    UpdatesOverOneMillisecond,
                    UpdatesOverFiveMilliseconds,
                    UpdatesOverTenMilliseconds);
            }
        }

        internal readonly struct TimingSnapshot
        {
            public TimingSnapshot(
                long updates,
                long totalTicks,
                long maximumTicks,
                long updatesOverOneMillisecond,
                long updatesOverFiveMilliseconds,
                long updatesOverTenMilliseconds)
            {
                Updates = updates;
                TotalTicks = totalTicks;
                MaximumTicks = maximumTicks;
                UpdatesOverOneMillisecond = updatesOverOneMillisecond;
                UpdatesOverFiveMilliseconds = updatesOverFiveMilliseconds;
                UpdatesOverTenMilliseconds = updatesOverTenMilliseconds;
            }

            public long Updates { get; }

            public long TotalTicks { get; }

            public long MaximumTicks { get; }

            public long UpdatesOverOneMillisecond { get; }

            public long UpdatesOverFiveMilliseconds { get; }

            public long UpdatesOverTenMilliseconds { get; }

            public double TotalMilliseconds => TicksToMilliseconds(TotalTicks);

            public double AverageMilliseconds => Updates > 0
                ? TotalMilliseconds / Updates
                : 0d;

            public double MaximumMilliseconds => TicksToMilliseconds(MaximumTicks);

            private static double TicksToMilliseconds(long ticks)
            {
                return ticks * 1000d / Stopwatch.Frequency;
            }
        }

        internal readonly struct Snapshot
        {
            public Snapshot(
                DateTime collectionStartedLocalTime,
                TimingSnapshot boardingAssist,
                TimingSnapshot lateGroups,
                long vehiclesScanned,
                long passengersScanned,
                long lateSoloCandidates,
                long lateSoloCanceled,
                long runSoonerAssists,
                long boardingCommandBufferPlaybacks,
                long groupControllersScanned,
                long groupCandidates,
                long groupsReleased,
                long groupsAssisted,
                long groupMembersPrompted,
                long groupCommandBufferPlaybacks)
            {
                CollectionStartedLocalTime = collectionStartedLocalTime;
                BoardingAssist = boardingAssist;
                LateGroups = lateGroups;
                VehiclesScanned = vehiclesScanned;
                PassengersScanned = passengersScanned;
                LateSoloCandidates = lateSoloCandidates;
                LateSoloCanceled = lateSoloCanceled;
                RunSoonerAssists = runSoonerAssists;
                BoardingCommandBufferPlaybacks = boardingCommandBufferPlaybacks;
                GroupControllersScanned = groupControllersScanned;
                GroupCandidates = groupCandidates;
                GroupsReleased = groupsReleased;
                GroupsAssisted = groupsAssisted;
                GroupMembersPrompted = groupMembersPrompted;
                GroupCommandBufferPlaybacks = groupCommandBufferPlaybacks;
            }

            public DateTime CollectionStartedLocalTime { get; }

            public TimingSnapshot BoardingAssist { get; }

            public TimingSnapshot LateGroups { get; }

            public long VehiclesScanned { get; }

            public long PassengersScanned { get; }

            public long LateSoloCandidates { get; }

            public long LateSoloCanceled { get; }

            public long RunSoonerAssists { get; }

            public long BoardingCommandBufferPlaybacks { get; }

            public long GroupControllersScanned { get; }

            public long GroupCandidates { get; }

            public long GroupsReleased { get; }

            public long GroupsAssisted { get; }

            public long GroupMembersPrompted { get; }

            public long GroupCommandBufferPlaybacks { get; }
        }
    }
}
