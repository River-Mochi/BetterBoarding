// <copyright file="TransitWaitStatusSystem.RailBoarding.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/Status/TransitWaitStatusSystem.RailBoarding.cs
// Purpose: Bounded rail boarding evidence collected only by Stats to Log.

namespace BetterBoarding
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Game.Common;
    using Game.Creatures;
    using Game.Prefabs;
    using Game.Routes;
    using Game.Vehicles;
    using Unity.Collections;
    using Unity.Entities;

    public sealed partial class TransitWaitStatusSystem
    {
        private const int kMaxReportedRailVehicles = 20;
        private const int kMaxReportedRidersPerVehicle = 5;

        private sealed class RailBoardingVehicleSnapshot
        {
            public Entity Vehicle;
            public Entity Line;
            public Entity Stop;
            public string LineName = string.Empty;
            public string StopName = string.Empty;
            public TransportType Type;
            public uint DepartureFrame;
            public uint FramesPastDeparture;
            public float MaxBoardingDistance;
            public float MinWaitingDistance;
            public int CarCount;
            public int Ready;
            public int Unready;
            public int Entering;
            public int ClearlyFar;
            public int ProtectedNear;
            public int ProtectedStationLane;
            public int ProtectedUnknown;
            public int MissingAssignment;
            public readonly List<string> RiderSamples = new List<string>(kMaxReportedRidersPerVehicle);
        }

        public void AppendRailBoardingReport(StringBuilder sb)
        {
            // This query is never enumerated during normal simulation; only the Options
            // button calls this method. No recurring position history or log writes exist.
            using NativeArray<Entity> controllers =
                m_RailControllerQuery.ToEntityArray(Allocator.Temp);
            List<RailBoardingVehicleSnapshot> snapshots = new List<RailBoardingVehicleSnapshot>();
            uint frame = m_SimulationSystem.frameIndex;

            for (int i = 0; i < controllers.Length; i++)
            {
                Entity controller = controllers[i];
                if (!EntityManager.Exists(controller) ||
                    !TryGetVehicleTransportType(controller, out TransportType type) ||
                    (type != TransportType.Train && type != TransportType.Subway) ||
                    (EntityManager.HasComponent<Controller>(controller) &&
                     EntityManager.GetComponentData<Controller>(controller).m_Controller != Entity.Null &&
                     EntityManager.GetComponentData<Controller>(controller).m_Controller != controller))
                {
                    continue;
                }

                Game.Vehicles.PublicTransport transport =
                    EntityManager.GetComponentData<Game.Vehicles.PublicTransport>(controller);
                if ((transport.m_State & PublicTransportFlags.Boarding) == 0)
                {
                    continue;
                }

                RailBoardingVehicleSnapshot snapshot = new RailBoardingVehicleSnapshot
                {
                    Vehicle = controller,
                    Type = type,
                    DepartureFrame = transport.m_DepartureFrame,
                    FramesPastDeparture = transport.m_DepartureFrame != 0 &&
                        frame >= transport.m_DepartureFrame
                            ? frame - transport.m_DepartureFrame
                            : 0,
                    MaxBoardingDistance = transport.m_MaxBoardingDistance,
                    MinWaitingDistance = transport.m_MinWaitingDistance,
                };

                if (EntityManager.HasComponent<CurrentRoute>(controller))
                {
                    snapshot.Line =
                        EntityManager.GetComponentData<CurrentRoute>(controller).m_Route;
                    if (snapshot.Line != Entity.Null && EntityManager.Exists(snapshot.Line))
                    {
                        snapshot.LineName = ResolveLineName(snapshot.Line);
                    }
                }

                if (EntityManager.HasComponent<Target>(controller))
                {
                    Entity waypoint = EntityManager.GetComponentData<Target>(controller).m_Target;
                    if (waypoint != Entity.Null &&
                        EntityManager.Exists(waypoint) &&
                        EntityManager.HasComponent<Connected>(waypoint))
                    {
                        snapshot.Stop =
                            EntityManager.GetComponentData<Connected>(waypoint).m_Connected;
                        if (snapshot.Stop != Entity.Null && EntityManager.Exists(snapshot.Stop))
                        {
                            snapshot.StopName = ResolveStopName(snapshot.Stop, type);
                        }
                    }
                }

                if (EntityManager.HasBuffer<LayoutElement>(controller))
                {
                    DynamicBuffer<LayoutElement> layout =
                        EntityManager.GetBuffer<LayoutElement>(controller, isReadOnly: true);
                    snapshot.CarCount = layout.Length == 0 ? 1 : layout.Length;
                    for (int carIndex = 0; carIndex < layout.Length; carIndex++)
                    {
                        CountRailCarPassengers(
                            controller,
                            layout[carIndex].m_Vehicle,
                            snapshot);
                    }

                    if (layout.Length == 0)
                    {
                        CountRailCarPassengers(controller, controller, snapshot);
                    }
                }
                else
                {
                    snapshot.CarCount = 1;
                    CountRailCarPassengers(controller, controller, snapshot);
                }

                snapshots.Add(snapshot);
            }

            snapshots.Sort((left, right) =>
            {
                int result = right.Unready.CompareTo(left.Unready);
                return result != 0
                    ? result
                    : right.FramesPastDeparture.CompareTo(left.FramesPastDeparture);
            });

            sb.AppendLine();
            sb.AppendLine("=== Rail boarding snapshot (Stats to Log only) ===");
            sb.AppendLine(
                $"Simulation frame {frame}; {snapshots.Count} train/subway vehicles boarding. " +
                "Distances are straight-line to the stop waypoint and nearest car center, " +
                "not walking distance or door distance.");

            int count = Math.Min(kMaxReportedRailVehicles, snapshots.Count);
            for (int i = 0; i < count; i++)
            {
                RailBoardingVehicleSnapshot snapshot = snapshots[i];
                string departure = snapshot.DepartureFrame == 0
                    ? "not scheduled"
                    : frame >= snapshot.DepartureFrame
                        ? $"{snapshot.FramesPastDeparture} frames past scheduled departure"
                        : $"{snapshot.DepartureFrame - frame} frames before scheduled departure";
                sb.AppendLine(
                    $"{i + 1}. {snapshot.Type} vehicle {snapshot.Vehicle} | " +
                    $"line {RailNameOrUnknown(snapshot.LineName)} ({snapshot.Line}) | " +
                    $"stop {RailNameOrUnknown(snapshot.StopName)} ({snapshot.Stop}) | " +
                    $"{departure} | cars {snapshot.CarCount} | " +
                    $"vanilla boarding distances max {snapshot.MaxBoardingDistance.ToString("G6", CultureInfo.InvariantCulture)}m, " +
                    $"nearest waiting {snapshot.MinWaitingDistance.ToString("G6", CultureInfo.InvariantCulture)}m | " +

                    $"boarding complete {snapshot.Ready}, " +
                    $"boarding incomplete {snapshot.Unready} " +
                    $"(entering transition {snapshot.Entering}), " +
                    $"clearly far {snapshot.ClearlyFar}, protected near {snapshot.ProtectedNear}, " +
                    $"protected station lane {snapshot.ProtectedStationLane}, " +
                    $"protected unknown {snapshot.ProtectedUnknown}, " +
                    $"missing assignment {snapshot.MissingAssignment}");

                for (int riderIndex = 0; riderIndex < snapshot.RiderSamples.Count; riderIndex++)
                {
                    sb.AppendLine("   " + snapshot.RiderSamples[riderIndex]);
                }
            }

            if (snapshots.Count > count)
            {
                sb.AppendLine($"... {snapshots.Count - count} additional boarding rail vehicles omitted.");
            }
        }

        private void CountRailCarPassengers(
            Entity controller,
            Entity car,
            RailBoardingVehicleSnapshot snapshot)
        {
            if (car == Entity.Null ||
                !EntityManager.Exists(car) ||
                !EntityManager.HasBuffer<Passenger>(car))
            {
                return;
            }

            DynamicBuffer<Passenger> passengers =
                EntityManager.GetBuffer<Passenger>(car, isReadOnly: true);
            for (int i = 0; i < passengers.Length; i++)
            {
                Entity rider = passengers[i].m_Passenger;
                if (!EntityManager.Exists(rider) ||
                    !EntityManager.HasComponent<CurrentVehicle>(rider))
                {
                    snapshot.MissingAssignment++;
                    continue;
                }

                CurrentVehicle assignment = EntityManager.GetComponentData<CurrentVehicle>(rider);
                if (assignment.m_Vehicle != car)
                {
                    snapshot.MissingAssignment++;
                    continue;
                }

                if ((assignment.m_Flags & CreatureVehicleFlags.Ready) != 0)
                {
                    snapshot.Ready++;
                    continue;
                }

                snapshot.Unready++;

                if ((assignment.m_Flags & CreatureVehicleFlags.Entering) != 0)
                {
                    snapshot.Entering++;
                }


                string proximity = "position or station topology unknown (protected)";
                bool measured = LatePassengerProximityPolicy.TryMeasure(
                        EntityManager,
                        controller,
                        rider,
                        out float stopDistance,
                        out float carDistance,
                        out bool onStationLane);
                bool clearlyFar = measured && LatePassengerProximityPolicy.IsClearlyFar(
                    stopDistance,
                    carDistance,
                    onStationLane);
                if (clearlyFar)
                {
                    snapshot.ClearlyFar++;
                }
                else if (!measured)
                {
                    snapshot.ProtectedUnknown++;
                }
                else if (onStationLane)
                {
                    snapshot.ProtectedStationLane++;
                }
                else
                {
                    snapshot.ProtectedNear++;
                }

                if (snapshot.RiderSamples.Count >= kMaxReportedRidersPerVehicle)
                {
                    continue;
                }

                if (measured)
                {
                    proximity =
                        $"stop {stopDistance.ToString("F1", CultureInfo.InvariantCulture)}m, " +
                        $"nearest car {carDistance.ToString("F1", CultureInfo.InvariantCulture)}m, " +
                        $"station-owned lane {onStationLane}";
                }

                bool group = EntityManager.HasComponent<GroupMember>(rider) ||
                    EntityManager.HasBuffer<GroupCreature>(rider);
                bool walking = EntityManager.HasComponent<HumanCurrentLane>(rider);
                string timer = EntityManager.HasComponent<Game.Creatures.Resident>(rider)
                    ? EntityManager.GetComponentData<Game.Creatures.Resident>(rider)
                        .m_Timer.ToString(CultureInfo.InvariantCulture)
                    : "n/a";
                snapshot.RiderSamples.Add(
                    $"rider {rider} | assigned car {car} | flags {assignment.m_Flags} | group {group} | " +
                    $"walking {walking} | resident timer {timer} | " +
                    $"clearly far {clearlyFar} | {proximity}");
            }
        }

        private static string RailNameOrUnknown(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? "unknown" : name;
        }
    }
}
