// <copyright file="LateBoarderCancelSystem.Types.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/LateBoarderCancelSystem.Types.cs
// Purpose: Small private structs used by the late-boarder cancellation and diagnostics partials.

namespace BoardingNow
{
    using Game; // GameSystemBase
    using System; // DateTime
    using Unity.Entities; // Entity
    using TransportType = Game.Prefabs.TransportType; // bus/train/etc.

    public partial class LateBoarderCancelSystem : GameSystemBase
    {
        private readonly struct PassStats
        {
            public PassStats(int vehicles, int passengers, int candidates, int canceled, int runSoonerAssists)
            {
                Vehicles = vehicles;
                Passengers = passengers;
                Candidates = candidates;
                Canceled = canceled;
                RunSoonerAssists = runSoonerAssists;
            }

            public int Vehicles { get; }

            public int Passengers { get; }

            public int Candidates { get; }

            public int Canceled { get; }

            public int RunSoonerAssists { get; }
        }

        private enum FollowUpSampleKind
        {
            SkippedLatePassenger,
            RunSoonerPassenger,
        }

        private readonly struct CanceledPassengerSample
        {
            public CanceledPassengerSample(TransportType transportType, Entity vehicle, Entity passenger)
            {
                TransportType = transportType;
                Vehicle = vehicle;
                Passenger = passenger;
            }

            public TransportType TransportType { get; }

            public Entity Vehicle { get; }

            public Entity Passenger { get; }
        }

        private struct FollowUpSample
        {
            public FollowUpSample(
                FollowUpSampleKind kind,
                TransportType transportType,
                Entity vehicle,
                Entity passenger,
                uint frame,
                DateTime localTime)
            {
                Active = true;
                Logged = false;
                Kind = kind;
                TransportType = transportType;
                Vehicle = vehicle;
                Passenger = passenger;
                Frame = frame;
                LocalTime = localTime;
            }

            public bool Active;

            public bool Logged;

            public FollowUpSampleKind Kind;

            public TransportType TransportType;

            public Entity Vehicle;

            public Entity Passenger;

            public uint Frame;

            public DateTime LocalTime;
        }
    }
}
