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
    using System.Collections.Generic; // HashSet
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
        private const uint kDiagnosticFrameInterval = 4096;

        private const int kMaxSampledCimsPerModePerUpdate = 3;
        private const int kMaxSampledCimsPerUpdate = kMaxSampledCimsPerModePerUpdate * 7;
        private const int kMaxRunSoonerSoloFollowUpSamplesPerUpdate = 2;
        private const int kMaxRunSoonerGroupFollowUpSamplesPerUpdate = 2;
        private const uint kSkippedPassengerFollowUpDelayFrames = 2048; // ~11.25 in-game minutes after cancellation.
        private const uint kRunSoonerDepartureGraceFrames = 128; // One assist interval after scheduled departure.
        private const uint kRunSoonerPreVanillaTimeoutFrames = 1536; // ~8.44 in-game minutes after departure, before vanilla's 1800-frame cutoff.
        private const int kMaxFollowUpSamples = 256;
        private const int kMaxFollowUpLogsPerUpdate = 6;

        private uint m_LastDiagnosticFrame;

        private long m_TotalCanceled;
        private long m_TotalRunSoonerAssists;
        private static bool s_FollowUpLegendLogged;
        private static bool s_RunSoonerFollowUpLegendLogged;
        private bool m_LoggedActive;

        // Fixed-size storage for delayed follow-up checks. Reuse slots instead of allocating every update.
        private readonly FollowUpSample[] m_FollowUpSamples = new FollowUpSample[kMaxFollowUpSamples];
        private readonly HashSet<Entity> m_LoggedRunSoonerSpeedPrefabs = new HashSet<Entity>();
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
                () => $"Boarding assist active: every {GetUpdateInterval(SystemUpdatePhase.GameSimulation)} frames, cap={kMaxCancellationsPerUpdate} solo cancellations/update, skipLatePassengers={BoardingRuntimeSettings.CancelLateBoarders}, runSooner={BoardingRuntimeSettings.CimsRunSoonerToCatchBuses}");
        }

        private void LogPassSummary(uint frame, PassStats stats)
        {
            m_TotalCanceled += stats.Canceled;
            m_TotalRunSoonerAssists += stats.RunSoonerAssists;

            if (!ShouldLogDiagnostics())
            {
                return;
            }

            bool force = stats.Canceled >= kMaxCancellationsPerUpdate;
            bool intervalElapsed = m_LastDiagnosticFrame == 0 ||
                frame < m_LastDiagnosticFrame ||
                frame - m_LastDiagnosticFrame >= kDiagnosticFrameInterval;

            if (!force && !intervalElapsed && stats.Canceled == 0 && stats.RunSoonerAssists == 0)
            {
                return;
            }

            if (!force && !intervalElapsed)
            {
                return;
            }

            m_LastDiagnosticFrame = frame;
            LogUtils.Info(
                Mod.s_Log,
                () => $"Boarding assist: vehicles={stats.Vehicles}, passengersScanned={stats.Passengers}, lateSolo={stats.Candidates}, skipped={stats.Canceled}, runFlagsSetByBB={stats.RunSoonerAssists}, totalSkipped={m_TotalCanceled}, totalRunSooner={m_TotalRunSoonerAssists}");
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

        private void TrackRunSoonerFollowUpSample(
            TransportType transportType,
            Entity controllerVehicle,
            Entity vehicle,
            Entity passenger,
            uint departureFrame,
            Entity groupLeader,
            int groupSize,
            float maxStraightDistanceAtRun)
        {
            uint frame = m_SimulationSystem?.frameIndex ?? 0;
            int slot = FindFollowUpSampleSlot();
            if (slot < 0)
            {
                return;
            }

            LogRunSoonerSpeedSample(transportType, passenger);

            FollowUpSample sample =
                new FollowUpSample(
                    FollowUpSampleKind.RunSoonerPassenger,
                    transportType,
                    vehicle,
                    passenger,
                    frame,
                    DateTime.Now)
                {
                    ControllerVehicle = controllerVehicle,
                    DepartureFrame = departureFrame,
                    GroupLeader = groupLeader,
                    GroupSizeAtRun = groupSize,
                    MaxStraightDistanceAtRun = maxStraightDistanceAtRun
                };

            m_FollowUpSamples[slot] = sample;

            if (m_FollowUpCount < m_FollowUpSamples.Length)
            {
                m_FollowUpCount++;
            }
        }

        private void LogRunSoonerSpeedSample(
            TransportType transportType,
            Entity passenger)
        {
            if (!EntityManager.Exists(passenger) ||
                !EntityManager.HasComponent<Game.Prefabs.PrefabRef>(passenger))
            {
                return;
            }

            Game.Prefabs.PrefabRef prefabRef =
                EntityManager.GetComponentData<Game.Prefabs.PrefabRef>(passenger);

            Entity humanPrefab = prefabRef.m_Prefab;
            if (humanPrefab == Entity.Null ||
                !EntityManager.Exists(humanPrefab) ||
                !EntityManager.HasComponent<Game.Prefabs.HumanData>(humanPrefab))
            {
                return;
            }

            // Log each loaded human prefab only once per city.
            if (!m_LoggedRunSoonerSpeedPrefabs.Add(humanPrefab))
            {
                return;
            }

            Game.Prefabs.HumanData humanData =
                EntityManager.GetComponentData<Game.Prefabs.HumanData>(humanPrefab);

            float navigationMaxSpeed = -1f;
            if (EntityManager.HasComponent<HumanNavigation>(passenger))
            {
                navigationMaxSpeed =
                    EntityManager.GetComponentData<HumanNavigation>(passenger).m_MaxSpeed;
            }

            float movingSpeed = -1f;
            if (EntityManager.HasComponent<Game.Objects.Moving>(passenger))
            {
                Game.Objects.Moving moving =
                    EntityManager.GetComponentData<Game.Objects.Moving>(passenger);

                movingSpeed = Unity.Mathematics.math.length(moving.m_Velocity);
            }

            LogUtils.Info(
                Mod.s_Log,
                () =>
                    $"Run Sooner Runtime Speed: {transportType} | " +
                    $"cim={passenger} | " +
                    $"humanPrefab={humanPrefab} | " +
                    $"runtimeWalk={FormatRuntimeSpeed(humanData.m_WalkSpeed)} | " +
                    $"runtimeRun={FormatRuntimeSpeed(humanData.m_RunSpeed)} | " +
                    $"acceleration={humanData.m_Acceleration:F2}m/s2 | " +
                    $"navMaxBeforeBBRun={FormatRuntimeSpeed(navigationMaxSpeed)} | " +
                    $"movingAtBBRun={FormatRuntimeSpeed(movingSpeed)}");
        }

        private static string FormatRuntimeSpeed(float metersPerSecond)
        {
            if (metersPerSecond < 0f)
            {
                return "n/a";
            }

            return $"{metersPerSecond:F3}m/s ({metersPerSecond * 3.6f:F1}km/h)";
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
            if (!ShouldLogDiagnostics() || m_FollowUpCount == 0)
            {
                return;
            }

            // Follow-up checks read the same live ECS data and wait for earlier jobs too.
            CompleteBoardingAssistDependencies();

            TransitWaitStatusSystem followUpStatusSystem = World.GetOrCreateSystemManaged<TransitWaitStatusSystem>();
            int loggedThisUpdate = 0;
            for (int i = 0; i < m_FollowUpCount; i++)
            {
                if (loggedThisUpdate >= kMaxFollowUpLogsPerUpdate)
                {
                    break;
                }

                FollowUpSample sample = m_FollowUpSamples[i];
                if (!sample.Active || sample.Logged)
                {
                    continue;
                }

                if (sample.Kind == FollowUpSampleKind.RunSoonerPassenger)
                {
                    bool departureCheckpoint =
                        sample.RunSoonerCheckpoint == RunSoonerFollowUpCheckpoint.DepartureGrace;
                    bool stillBoarding = IsRunSoonerVehicleStillBoarding(sample.ControllerVehicle);
                    bool boardingEndedCheckpoint = !departureCheckpoint && !stillBoarding;
                    if (!IsRunSoonerFollowUpDue(sample, frame) && !boardingEndedCheckpoint)
                    {
                        continue;
                    }

                    DateTime followUpLocalTime = DateTime.Now;
                    TransitWaitStatusSystem.FollowUpSnapshot followUpSnapshot =
                        followUpStatusSystem.BuildLateBoarderFollowUpSnapshot(
                            sample.Passenger,
                            sample.TransportType,
                            sample.Vehicle);

                    uint leadFrames = sample.DepartureFrame >= sample.Frame
                        ? sample.DepartureFrame - sample.Frame
                        : 0u;
                    uint framesPastDeparture = frame >= sample.DepartureFrame
                        ? frame - sample.DepartureFrame
                        : 0u;
                    string checkpoint = departureCheckpoint
                        ? "departure-grace"
                        : boardingEndedCheckpoint
                            ? "boarding-ended"
                            : "pre-vanilla-timeout";
                    string passengerState =
                        DescribeRunSoonerFollowUpState(followUpSnapshot, sample.Vehicle);
                    string vehicleState =
                        DescribeRunSoonerVehicleState(sample.ControllerVehicle, sample.DepartureFrame, frame);
                    string groupState = DescribeRunSoonerGroupState(sample);
                    string distanceAtRun = sample.MaxStraightDistanceAtRun >= 0f
                        ? sample.MaxStraightDistanceAtRun.ToString("F1") + "m"
                        : "n/a";

                    LogRunSoonerFollowUpLegendOnce();
                    LogUtils.Info(
                        Mod.s_Log,
                        () => $"Run Sooner Checkpoint: {sample.TransportType} | checkpoint={checkpoint} | cim={sample.Passenger} | target={sample.Vehicle} | controller={sample.ControllerVehicle} | lead={leadFrames}f/{FramesToGameMinutes(leadFrames):F2}m | maxStraightDistanceAtRun={distanceAtRun} | checked={framesPastDeparture}f/{FramesToGameMinutes(framesPastDeparture):F2}m after departure | bbSetRunFlag={sample.LocalTime:HH:mm:ss} | followUp={followUpLocalTime:HH:mm:ss} | result={passengerState} | vehicle={vehicleState} | {groupState}");

                    loggedThisUpdate++;
                    bool madeOriginalVehicle =
                        followUpSnapshot.CurrentVehicle == sample.Vehicle &&
                        (followUpSnapshot.CurrentVehicleFlags & CreatureVehicleFlags.Ready) != 0;

                    if (departureCheckpoint && stillBoarding && !madeOriginalVehicle)
                    {
                        sample.RunSoonerCheckpoint = RunSoonerFollowUpCheckpoint.PreVanillaTimeout;
                        m_FollowUpSamples[i] = sample;
                    }
                    else
                    {
                        sample.Logged = true;
                        sample.Active = false;
                        m_FollowUpSamples[i] = sample;
                    }

                    continue;
                }

                if ((frame >= sample.Frame
                        ? frame - sample.Frame
                        : uint.MaxValue) < kSkippedPassengerFollowUpDelayFrames)
                {
                    continue;
                }

                sample.Logged = true;
                sample.Active = false;
                m_FollowUpSamples[i] = sample;
                loggedThisUpdate++;

                DateTime canceledFollowUpLocalTime = DateTime.Now;
                TransitWaitStatusSystem.FollowUpSnapshot canceledFollowUpSnapshot =
                    followUpStatusSystem.BuildLateBoarderFollowUpSnapshot(
                        sample.Passenger,
                        sample.TransportType,
                        sample.Vehicle);

                LogFollowUpLegendOnce();
                WaitStatus.RecordLateBoarderFollowUp(
                    World,
                    sample.TransportType,
                    sample.Vehicle,
                    sample.Passenger,
                    sample.LocalTime,
                    canceledFollowUpLocalTime,
                    canceledFollowUpSnapshot);

                LogUtils.Info(
                    Mod.s_Log,
                    () => $"Skipped Late Passenger: {sample.TransportType} | cim={sample.Passenger} | missed={sample.Vehicle} | skipped={sample.LocalTime:HH:mm:ss} | followUp={canceledFollowUpLocalTime:HH:mm:ss} | state={DescribeFollowUpState(canceledFollowUpSnapshot, sample.Vehicle, sample.Passenger, frame)}");
            }
        }

        private static bool IsRunSoonerFollowUpDue(FollowUpSample sample, uint frame)
        {
            if (sample.DepartureFrame == 0 || frame < sample.DepartureFrame)
            {
                return false;
            }

            uint checkpointFrames =
                sample.RunSoonerCheckpoint == RunSoonerFollowUpCheckpoint.DepartureGrace
                    ? kRunSoonerDepartureGraceFrames
                    : kRunSoonerPreVanillaTimeoutFrames;

            return frame - sample.DepartureFrame >= checkpointFrames;
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
                () => "Run sooner checkpoint legend: departure-grace checks one assist interval after scheduled departure. Unresolved passengers still holding a boarding vehicle are logged again when boarding ends or shortly before vanilla's 1800-frame timeout. maxStraightDistanceAtRun is a position estimate, not path distance. Group counts show exact target-carriage readiness; these are sampled verbose diagnostics, not every runner.");
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

        private bool IsRunSoonerVehicleStillBoarding(Entity controllerVehicle)
        {
            return EntityManager.Exists(controllerVehicle) &&
                !EntityManager.HasComponent<Deleted>(controllerVehicle) &&
                !EntityManager.HasComponent<Destroyed>(controllerVehicle) &&
                EntityManager.HasComponent<Game.Vehicles.PublicTransport>(controllerVehicle) &&
                (EntityManager.GetComponentData<Game.Vehicles.PublicTransport>(controllerVehicle).m_State &
                    PublicTransportFlags.Boarding) != 0;
        }

        private string DescribeRunSoonerVehicleState(
            Entity controllerVehicle,
            uint departureFrame,
            uint frame)
        {
            if (!EntityManager.Exists(controllerVehicle))
            {
                return "gone";
            }

            if (EntityManager.HasComponent<Deleted>(controllerVehicle) ||
                EntityManager.HasComponent<Destroyed>(controllerVehicle))
            {
                return "deleted/destroyed";
            }

            if (!EntityManager.HasComponent<Game.Vehicles.PublicTransport>(controllerVehicle))
            {
                return "noPublicTransport";
            }

            Game.Vehicles.PublicTransport publicTransport =
                EntityManager.GetComponentData<Game.Vehicles.PublicTransport>(controllerVehicle);
            string boarding = (publicTransport.m_State & PublicTransportFlags.Boarding) != 0
                ? "stillBoarding"
                : "notBoarding";
            uint framesPastDeparture = frame >= departureFrame
                ? frame - departureFrame
                : 0u;

            return $"{boarding}, pastDeparture={framesPastDeparture}f/{FramesToGameMinutes(framesPastDeparture):F2}m, state={publicTransport.m_State}";
        }

        private string DescribeRunSoonerGroupState(FollowUpSample sample)
        {
            if (sample.GroupLeader == Entity.Null)
            {
                return "group=solo";
            }

            if (!EntityManager.Exists(sample.GroupLeader))
            {
                return $"group=leader {sample.GroupLeader}, sizeAtRun={sample.GroupSizeAtRun}, leader=gone";
            }

            int sizeNow = 1;
            int existing = 0;
            int missingEntities = 0;
            int targetReady = 0;
            int targetNotReady = 0;
            int otherVehicle = 0;
            int noVehicle = 0;
            int running = 0;

            CountRunSoonerGroupEntity(
                sample.GroupLeader,
                sample.Vehicle,
                ref existing,
                ref missingEntities,
                ref targetReady,
                ref targetNotReady,
                ref otherVehicle,
                ref noVehicle,
                ref running);

            if (EntityManager.HasBuffer<GroupCreature>(sample.GroupLeader))
            {
                DynamicBuffer<GroupCreature> group =
                    EntityManager.GetBuffer<GroupCreature>(sample.GroupLeader);
                sizeNow += group.Length;
                for (int i = 0; i < group.Length; i++)
                {
                    CountRunSoonerGroupEntity(
                        group[i].m_Creature,
                        sample.Vehicle,
                        ref existing,
                        ref missingEntities,
                        ref targetReady,
                        ref targetNotReady,
                        ref otherVehicle,
                        ref noVehicle,
                        ref running);
                }
            }

            return
                $"group=leader {sample.GroupLeader}, sizeAtRun={sample.GroupSizeAtRun}, sizeNow={sizeNow}, existing={existing}, missing={missingEntities}, targetReady={targetReady}, targetNotReady={targetNotReady}, otherVehicle={otherVehicle}, noVehicle={noVehicle}, running={running}";
        }

        private void CountRunSoonerGroupEntity(
            Entity passenger,
            Entity targetVehicle,
            ref int existing,
            ref int missingEntities,
            ref int targetReady,
            ref int targetNotReady,
            ref int otherVehicle,
            ref int noVehicle,
            ref int running)
        {
            if (!EntityManager.Exists(passenger) ||
                EntityManager.HasComponent<Deleted>(passenger) ||
                EntityManager.HasComponent<Destroyed>(passenger))
            {
                missingEntities++;
                return;
            }

            existing++;
            if (EntityManager.HasComponent<Human>(passenger) &&
                (EntityManager.GetComponentData<Human>(passenger).m_Flags & HumanFlags.Run) != 0)
            {
                running++;
            }

            if (!EntityManager.HasComponent<CurrentVehicle>(passenger))
            {
                noVehicle++;
                return;
            }

            CurrentVehicle currentVehicle = EntityManager.GetComponentData<CurrentVehicle>(passenger);
            if (currentVehicle.m_Vehicle != targetVehicle)
            {
                otherVehicle++;
            }
            else if ((currentVehicle.m_Flags & CreatureVehicleFlags.Ready) != 0)
            {
                targetReady++;
            }
            else
            {
                targetNotReady++;
            }
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
            m_LoggedActive = false;
            m_FollowUpCount = 0;
            m_NextFollowUpSample = 0;
            m_LoggedRunSoonerSpeedPrefabs.Clear();
        }
    }
}
