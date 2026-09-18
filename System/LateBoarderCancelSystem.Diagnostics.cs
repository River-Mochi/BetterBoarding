// <copyright file="LateBoarderCancelSystem.Diagnostics.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/LateBoarderCancelSystem.Diagnostics.cs
// Purpose: Verbose diagnostics and delayed follow-up samples.

namespace BetterBoarding
{
    using System;           // DateTime
    using CS2Shared.RiverMochi; // LogUtils
    using Game;             // GameSystemBase
    using Game.Common;      // Deleted, Destroyed
    using Game.Creatures;   // CurrentVehicle, CreatureVehicleFlags
    using Game.Vehicles;    // PublicTransport, Passenger
    using Unity.Entities;   // Entity
    using TransportType = Game.Prefabs.TransportType; // bus/train/etc.

    public partial class LateBoarderCancelSystem : GameSystemBase
    {
        // Verbose summary throttles. 4096 frames is about 22.5 in-game minutes.
        private const uint DiagnosticFrameInterval = 4096;

        private const int MaxSampledCimsPerModePerUpdate = 3;
        private const int MaxSampledCimsPerUpdate = MaxSampledCimsPerModePerUpdate * 7;
        private const int MaxRunSoonerFollowUpSamplesPerUpdate = 4;
        private const uint FollowUpDelayFrames = 2048; // ~11.25 in-game minutes.
        private const int MaxFollowUpSamples = 256;
        private const int MaxFollowUpLogsPerUpdate = 6;

        private uint m_LastDiagnosticFrame;

        private long m_TotalCanceled;
        private long m_TotalRunSoonerAssists;
        private static bool s_FollowUpLegendLogged;
        private static bool s_RunSoonerFollowUpLegendLogged;
        private int m_SkippedForTool;
        private bool m_LoggedActive;

        // Fixed-size storage for delayed follow-up checks. Reuse slots instead of allocating every update.
        private readonly FollowUpSample[] m_FollowUpSamples = new FollowUpSample[MaxFollowUpSamples];
        private int m_FollowUpCount;
        private int m_NextFollowUpSample;

        private static double FramesToGameMinutes(uint frames)
        {
            return frames * 1440.0 / 262144.0;
        }

        private void LogActiveOnce()
        {
            if (!ShouldLogDiagnostics())
            {
                return;
            }

            if (m_LoggedActive)
            {
                return;
            }

            m_LoggedActive = true;
            LogUtils.Info(
                Mod.s_Log,
                () => $"Boarding assist active: every {GetUpdateInterval(SystemUpdatePhase.GameSimulation)} frames, cap={MaxCancellationsPerUpdate} late solo cims/update, skipLateSoloCim={BoardingRuntimeSettings.CancelLateBoarders}, runSooner={BoardingRuntimeSettings.CimsRunSoonerToCatchBuses}");
        }

        private void LogPassSummary(uint frame, PassStats stats, string reason)
        {
            m_TotalCanceled += stats.Canceled;
            m_TotalRunSoonerAssists += stats.RunSoonerAssists;

            if (!ShouldLogDiagnostics())
            {
                return;
            }

            bool force = stats.Canceled >= MaxCancellationsPerUpdate;
            bool intervalElapsed = m_LastDiagnosticFrame == 0 ||
                frame < m_LastDiagnosticFrame ||
                frame - m_LastDiagnosticFrame >= DiagnosticFrameInterval;

            if (!force && !intervalElapsed && stats.Canceled == 0 && stats.RunSoonerAssists == 0)
            {
                return;
            }

            if (!force && !intervalElapsed)
            {
                return;
            }

            m_LastDiagnosticFrame = frame;
            string activeTool = m_ToolSystem?.activeTool?.GetType().Name ?? "none";
            if (reason == "paused-tool")
            {
                LogUtils.Info(
                    Mod.s_Log,
                    () => $"Boarding assist paused: activeTool={activeTool}, pauses={m_SkippedForTool}, totalSkipped={m_TotalCanceled}, totalRunSooner={m_TotalRunSoonerAssists}");
                return;
            }

            LogUtils.Info(
                Mod.s_Log,
                () => $"Boarding assist: vehicles={stats.Vehicles}, passengersScanned={stats.Passengers}, lateSolo={stats.Candidates}, skipped={stats.Canceled}, runSooner={stats.RunSoonerAssists}, totalSkipped={m_TotalCanceled}, totalRunSooner={m_TotalRunSoonerAssists}");
        }

        private void TrackFollowUpSample(CanceledPassengerSample sample)
        {
            uint frame = m_SimulationSystem?.frameIndex ?? 0;
            int slot = FindFollowUpSampleSlot();
            if (slot < 0)
            {
                return;
            }

            m_FollowUpSamples[slot] =
                new FollowUpSample(
                    FollowUpSampleKind.SkippedLatePassenger,
                    sample.TransportType,
                    sample.Vehicle,
                    sample.Passenger,
                    frame,
                    DateTime.Now);

            if (m_FollowUpCount < m_FollowUpSamples.Length)
            {
                m_FollowUpCount++;
            }
        }

        private void TrackRunSoonerFollowUpSample(TransportType transportType, Entity vehicle, Entity passenger)
        {
            uint frame = m_SimulationSystem?.frameIndex ?? 0;
            int slot = FindFollowUpSampleSlot();
            if (slot < 0)
            {
                return;
            }

            m_FollowUpSamples[slot] =
                new FollowUpSample(
                    FollowUpSampleKind.RunSoonerPassenger,
                    transportType,
                    vehicle,
                    passenger,
                    frame,
                    DateTime.Now);

            if (m_FollowUpCount < m_FollowUpSamples.Length)
            {
                m_FollowUpCount++;
            }
        }

        private int FindFollowUpSampleSlot()
        {
            // Prefer finished/empty slots so high-volume skip passes do not overwrite samples before proof checks run.
            for (int attempt = 0; attempt < m_FollowUpSamples.Length; attempt++)
            {
                int index = (m_NextFollowUpSample + attempt) % m_FollowUpSamples.Length;
                FollowUpSample sample = m_FollowUpSamples[index];
                if (!sample.Active || sample.Logged)
                {
                    m_NextFollowUpSample = (index + 1) % m_FollowUpSamples.Length;
                    return index;
                }
            }

            return -1;
        }

        private void LogFollowUps(uint frame)
        {
            if (!ShouldLogDiagnostics())
            {
                return;
            }

            // Follow-up logs answer the "what did vanilla do next?" question after a cim is detached.
            TransitWaitStatusSystem followUpStatusSystem = World.GetOrCreateSystemManaged<TransitWaitStatusSystem>();
            int loggedThisUpdate = 0;
            for (int i = 0; i < m_FollowUpCount; i++)
            {
                if (loggedThisUpdate >= MaxFollowUpLogsPerUpdate)
                {
                    break;
                }

                FollowUpSample sample = m_FollowUpSamples[i];
                if (!sample.Active || sample.Logged)
                {
                    continue;
                }

                if ((frame >= sample.Frame
                        ? frame - sample.Frame
                        : uint.MaxValue) < FollowUpDelayFrames)
                {
                    continue;
                }

                sample.Logged = true;
                sample.Active = false;
                m_FollowUpSamples[i] = sample;
                loggedThisUpdate++;

                DateTime followUpLocalTime = DateTime.Now;
                TransitWaitStatusSystem.FollowUpSnapshot followUpSnapshot =
                    followUpStatusSystem.BuildLateBoarderFollowUpSnapshot(
                        sample.Passenger,
                        sample.TransportType,
                        sample.Vehicle);

                if (sample.Kind == FollowUpSampleKind.RunSoonerPassenger)
                {
                    LogRunSoonerFollowUpLegendOnce();
                    LogUtils.Info(
                        Mod.s_Log,
                        () => $"Run Sooner Follow-up: {sample.TransportType} | cim={sample.Passenger} | target={sample.Vehicle} | ran={sample.LocalTime:HH:mm:ss} | followUp={followUpLocalTime:HH:mm:ss} | result={DescribeRunSoonerFollowUpState(followUpSnapshot, sample.Vehicle)}");

                    continue;
                }

                LogFollowUpLegendOnce();
                TransitWaitStatus.RecordLateBoarderFollowUp(
                    World,
                    sample.TransportType,
                    sample.Vehicle,
                    sample.Passenger,
                    sample.LocalTime,
                    followUpLocalTime,
                    followUpSnapshot);

                LogUtils.Info(
                    Mod.s_Log,
                    () => $"Skipped Late Passenger: {sample.TransportType} | cim={sample.Passenger} | missed={sample.Vehicle} | skipped={sample.LocalTime:HH:mm:ss} | followUp={followUpLocalTime:HH:mm:ss} | state={DescribeFollowUpState(followUpSnapshot, sample.Vehicle, sample.Passenger, frame)}");
            }
        }

        private static void LogFollowUpLegendOnce()
        {
            if (s_FollowUpLegendLogged)
            {
                return;
            }

            s_FollowUpLegendLogged = true;
            LogUtils.Info(
                Mod.s_Log,
                () => "Skip follow-up legend: state=same vehicle/different vehicle means assigned; has path means repathing or walking; no path yet means unresolved. next=stop/lane/waypoint/vehicle/target shows the cim's next path target. same vehicle entries include missed vehicle proof.");
        }

        private static void LogRunSoonerFollowUpLegendOnce()
        {
            if (s_RunSoonerFollowUpLegendLogged)
            {
                return;
            }

            s_RunSoonerFollowUpLegendLogged = true;
            LogUtils.Info(
                Mod.s_Log,
                () => "Run sooner follow-up legend: result=made same vehicle means the sampled runner caught the original bus/tram/train; different vehicle/has path means vanilla reassigned or is still routing; no path yet means unresolved. These are sampled verbose diagnostics, not every runner.");
        }

        private static string EntityText(Entity entity)
        {
            return entity == Entity.Null ? "none" : entity.ToString();
        }

        private string DescribeFollowUpState(
            TransitWaitStatusSystem.FollowUpSnapshot snapshot,
            Entity missedVehicle,
            Entity passenger,
            uint frame)
        {
            if (snapshot.CurrentVehicle != Entity.Null)
            {
                string assignment = snapshot.Outcome == TransitWaitStatusSystem.FollowUpOutcome.SameVehicle
                    ? $"same vehicle {snapshot.CurrentVehicleText}"
                    : $"different vehicle {snapshot.CurrentVehicleText}";

                string details = AppendFollowUpTargetDetails(assignment, snapshot);

                if (snapshot.Outcome == TransitWaitStatusSystem.FollowUpOutcome.SameVehicle)
                {
                    details = AppendMissedVehicleProof(details, missedVehicle, passenger, frame);
                }

                return details;
            }

            if (snapshot.Outcome == TransitWaitStatusSystem.FollowUpOutcome.HasPathNotAssignedYet)
            {
                return AppendFollowUpTargetDetails("has path", snapshot);
            }

            return "no path yet";
        }

        private static string DescribeRunSoonerFollowUpState(
            TransitWaitStatusSystem.FollowUpSnapshot snapshot,
            Entity targetVehicle)
        {
            if (snapshot.CurrentVehicle != Entity.Null)
            {
                if (snapshot.CurrentVehicle == targetVehicle)
                {
                    bool ready = (snapshot.CurrentVehicleFlags & CreatureVehicleFlags.Ready) != 0;
                    string sameVehicleResult = ready
                        ? $"made same vehicle {snapshot.CurrentVehicleText}"
                        : $"same vehicle not ready {snapshot.CurrentVehicleText}";

                    return AppendFollowUpTargetDetails(sameVehicleResult, snapshot);
                }

                return AppendFollowUpTargetDetails(
                    $"different vehicle {snapshot.CurrentVehicleText}",
                    snapshot);
            }

            if (snapshot.Outcome == TransitWaitStatusSystem.FollowUpOutcome.HasPathNotAssignedYet)
            {
                return AppendFollowUpTargetDetails("has path", snapshot);
            }

            return "no path yet";
        }

        private string AppendMissedVehicleProof(string summary, Entity missedVehicle, Entity passenger, uint frame)
        {
            if (missedVehicle == Entity.Null)
            {
                return summary + " | missedVehicle=none";
            }

            if (!EntityManager.Exists(missedVehicle))
            {
                return summary + " | missedVehicle=gone";
            }

            if (EntityManager.HasComponent<Deleted>(missedVehicle) ||
                EntityManager.HasComponent<Destroyed>(missedVehicle))
            {
                return summary + " | missedVehicle=deleted/destroyed";
            }

            if (!EntityManager.HasComponent<Game.Vehicles.PublicTransport>(missedVehicle))
            {
                return summary + " | missedVehicle=noPublicTransport";
            }

            Game.Vehicles.PublicTransport publicTransport =
                EntityManager.GetComponentData<Game.Vehicles.PublicTransport>(missedVehicle);

            bool stillBoarding = (publicTransport.m_State & PublicTransportFlags.Boarding) != 0;
            string boardingText = stillBoarding ? "stillBoarding" : "notBoarding";

            string pastDepartureText = "n/a";
            if (publicTransport.m_DepartureFrame != 0)
            {
                uint framesPastDeparture = frame >= publicTransport.m_DepartureFrame
                    ? frame - publicTransport.m_DepartureFrame
                    : 0u;

                pastDepartureText = FramesToGameMinutes(framesPastDeparture).ToString("F1") + "m";
            }

            string passengerProof = BuildMissedVehiclePassengerProof(missedVehicle, passenger);

            return summary +
                $" | missedVehicle={boardingText}, pastDeparture={pastDepartureText}, {passengerProof}, state={publicTransport.m_State}";
        }

        private string BuildMissedVehiclePassengerProof(Entity vehicleEntity, Entity followedPassenger)
        {
            if (!EntityManager.HasBuffer<Passenger>(vehicleEntity))
            {
                return "passengerBuffer=none";
            }

            DynamicBuffer<Passenger> passengers = EntityManager.GetBuffer<Passenger>(vehicleEntity);
            int readyCount = 0;
            int notReadyCount = 0;
            bool containsFollowedPassenger = false;

            for (int i = 0; i < passengers.Length; i++)
            {
                Entity passenger = passengers[i].m_Passenger;
                if (passenger == followedPassenger)
                {
                    containsFollowedPassenger = true;
                }

                if (!EntityManager.Exists(passenger) ||
                    !EntityManager.HasComponent<CurrentVehicle>(passenger))
                {
                    continue;
                }

                CurrentVehicle currentVehicle = EntityManager.GetComponentData<CurrentVehicle>(passenger);
                if (currentVehicle.m_Vehicle != vehicleEntity)
                {
                    continue;
                }

                if ((currentVehicle.m_Flags & CreatureVehicleFlags.Ready) != 0)
                {
                    readyCount++;
                }
                else
                {
                    notReadyCount++;
                }
            }

            return
                $"passengerBuffer={passengers.Length}, ready={readyCount}, notReady={notReadyCount}, containsCim={containsFollowedPassenger}";
        }

        private static string AppendFollowUpTargetDetails(
            string summary,
            TransitWaitStatusSystem.FollowUpSnapshot snapshot)
        {
            string nextTarget = DescribeFollowUpTarget(snapshot);
            if (!string.IsNullOrWhiteSpace(nextTarget))
            {
                summary += $" | next={nextTarget}";
            }

            if (snapshot.NextTargetKind == TransitWaitStatusSystem.FollowUpTargetKind.Stop &&
                !string.IsNullOrWhiteSpace(snapshot.NextLineName))
            {
                summary += $" | line={snapshot.NextLineName}";
            }

            return summary;
        }

        private static string DescribeFollowUpTarget(TransitWaitStatusSystem.FollowUpSnapshot snapshot)
        {
            switch (snapshot.NextTargetKind)
            {
                case TransitWaitStatusSystem.FollowUpTargetKind.Stop:
                    return DescribeNamedEntity("stop", snapshot.NextStopName, snapshot.NextStopEntity);
                case TransitWaitStatusSystem.FollowUpTargetKind.Lane:
                    return DescribeNamedEntity("lane", snapshot.NextTargetName, Entity.Null);
                case TransitWaitStatusSystem.FollowUpTargetKind.Waypoint:
                    return DescribeNamedEntity("waypoint", snapshot.NextTargetName, snapshot.NextTargetEntity);
                case TransitWaitStatusSystem.FollowUpTargetKind.Vehicle:
                    return DescribeNamedEntity("vehicle", snapshot.NextTargetName, snapshot.NextTargetEntity);
                case TransitWaitStatusSystem.FollowUpTargetKind.Target:
                    return DescribeNamedEntity("target", snapshot.NextTargetName, snapshot.NextTargetEntity);
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

        private static bool ShouldLogDiagnostics()
        {
#if DEBUG
            return true;
#else
            return BoardingRuntimeSettings.EnableVerboseLogging;
#endif
        }

        private void ResetDiagnosticsForCityLoad()
        {
            m_LastDiagnosticFrame = 0;
            m_TotalCanceled = 0;
            m_TotalRunSoonerAssists = 0;
            m_SkippedForTool = 0;
            m_LoggedActive = false;
            m_FollowUpCount = 0;
            m_NextFollowUpSample = 0;
        }
    }
}
