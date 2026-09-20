// <copyright file="BoardingRuntimeSettings.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Settings/BoardingRuntimeSettings.cs
// Purpose: Runtime settings snapshot shared by Options UI setters and ECS systems.

namespace BetterBoarding
{
    /// <summary>
    /// Runtime snapshot of the current mod settings for ECS systems.
    /// Mirrors applied options into simple static values
    /// and exposes separate revised counters so each system
    /// Only Wakes when its own inputs changed.
    /// </summary>
    public static class BoardingRuntimeSettings
    {
        public static int StopTuningRevision { get; private set; }
        public static int LateBoarderRevision { get; private set; }
        public static int BusBoardingSpeedFactor { get; private set; } = BBoardSettings.DefaultSpeedFactor;
        public static int RailBoardingSpeedFactor { get; private set; } = BBoardSettings.DefaultSpeedFactor;
        public static int WaterBoardingSpeedFactor { get; private set; } = BBoardSettings.DefaultSpeedFactor;
        public static int AirBoardingSpeedFactor { get; private set; } = BBoardSettings.DefaultSpeedFactor;
        public static bool CancelLateBoarders { get; private set; } = false;
        public static bool CimsRunSoonerToCatchBuses { get; private set; } = false;
        public static bool BoardingAssistEnabled => CancelLateBoarders || CimsRunSoonerToCatchBuses;
        public static bool EnableVerboseLogging { get; private set; } = false;
        public static void LoadFromSettings(BBoardSettings settings)
        {
            // Clamp loaded .coc values before systems see them.
            int bus = ClampSpeedFactor(settings.BusBoardingSpeedFactor);
            int rail = ClampSpeedFactor(settings.RailBoardingSpeedFactor);
            int water = ClampSpeedFactor(settings.WaterBoardingSpeedFactor);
            int air = ClampSpeedFactor(settings.AirBoardingSpeedFactor);
            int passengerRun = ClampSpeedFactor(settings.PassengerRunSpeedFactor);

            bool stopChanged = false;

            if (BusBoardingSpeedFactor != bus)
            {
                BusBoardingSpeedFactor = bus;
                stopChanged = true;
            }

            if (RailBoardingSpeedFactor != rail)
            {
                RailBoardingSpeedFactor = rail;
                stopChanged = true;
            }

            if (WaterBoardingSpeedFactor != water)
            {
                WaterBoardingSpeedFactor = water;
                stopChanged = true;
            }

            if (AirBoardingSpeedFactor != air)
            {
                AirBoardingSpeedFactor = air;
                stopChanged = true;
            }

            if (stopChanged)
            {
                StopTuningRevision++;
            }

            bool lateBoarderChanged = false;

            if (CancelLateBoarders != settings.CancelLateBoarders)
            {
                CancelLateBoarders = settings.CancelLateBoarders;
                lateBoarderChanged = true;
            }

            if (CimsRunSoonerToCatchBuses != settings.CimsRunSoonerToCatchBuses)
            {
                CimsRunSoonerToCatchBuses = settings.CimsRunSoonerToCatchBuses;
                lateBoarderChanged = true;
            }

            if (lateBoarderChanged)
            {
                LateBoarderRevision++;
            }
            PassengerRunSpeedFactor = passengerRun;
            EnableVerboseLogging = settings.EnableVerboseLogging;
        }
        public static bool SetBusBoardingSpeedFactor(int value)
        {
            value = ClampSpeedFactor(value);
            if (BusBoardingSpeedFactor == value)
            {
                return false;
            }

            BusBoardingSpeedFactor = value;
            // Any speed-factor change wakes the one-shot prefab tuning pass.
            StopTuningRevision++;
            return true;
        }

        public static bool SetRailBoardingSpeedFactor(int value)
        {
            value = ClampSpeedFactor(value);
            if (RailBoardingSpeedFactor == value)
            {
                return false;
            }

            RailBoardingSpeedFactor = value;
            StopTuningRevision++;
            return true;
        }

        public static bool SetWaterBoardingSpeedFactor(int value)
        {
            value = ClampSpeedFactor(value);
            if (WaterBoardingSpeedFactor == value)
            {
                return false;
            }

            WaterBoardingSpeedFactor = value;
            StopTuningRevision++;
            return true;
        }

        public static bool SetAirBoardingSpeedFactor(int value)
        {
            value = ClampSpeedFactor(value);
            if (AirBoardingSpeedFactor == value)
            {
                return false;
            }

            AirBoardingSpeedFactor = value;
            StopTuningRevision++;
            return true;
        }

        public static int PassengerRunSpeedFactor { get; private set; } =
            BBoardSettings.DefaultPassengerRunSpeedFactor;

        public static bool RunSoonerSpeedBoostEnabled =>
            CimsRunSoonerToCatchBuses &&
            PassengerRunSpeedFactor > BBoardSettings.VanillaSpeedFactor;

        public static bool SetCancelLateBoarders(bool value)
        {
            if (CancelLateBoarders == value)
            {
                return false;
            }

            CancelLateBoarders = value;
            LateBoarderRevision++;
            return true;
        }

        public static bool SetCimsRunSoonerToCatchBuses(bool value)
        {
            if (CimsRunSoonerToCatchBuses == value)
            {
                return false;
            }

            CimsRunSoonerToCatchBuses = value;
            LateBoarderRevision++;
            return true;
        }

        public static bool SetEnableVerboseLogging(bool value)
        {
            if (EnableVerboseLogging == value)
            {
                return false;
            }

            EnableVerboseLogging = value;
            return true;
        }

        public static bool SetPassengerRunSpeedFactor(int value)
        {
            value = ClampSpeedFactor(value);

            if (PassengerRunSpeedFactor == value)
            {
                return false;
            }

            PassengerRunSpeedFactor = value;
            return true;
        }

        public static string DescribeForLog()
        {
            // Keep this compact because it is reused in support logs and report headers.
            return
                $"bus={BusBoardingSpeedFactor}x, rail={RailBoardingSpeedFactor}x, " +
                $"ship+ferry={WaterBoardingSpeedFactor}x, air={AirBoardingSpeedFactor}x, " +

                $"skipLateSoloCim={CancelLateBoarders}, " +
                $"runSooner={CimsRunSoonerToCatchBuses}, " +
                $"passengerRun={PassengerRunSpeedFactor}x";
        }

        public static string DescribeVerboseForLog(bool enabled)
        {
            return $"Options Settings: Verbose log Enabled [x] {enabled.ToString().ToLowerInvariant()}";
        }

        private static int ClampSpeedFactor(int value)
        {
            if (value < BBoardSettings.MinSpeedFactor)
            {
                return BBoardSettings.MinSpeedFactor;
            }

            if (value > BBoardSettings.MaxSpeedFactor)
            {
                return BBoardSettings.MaxSpeedFactor;
            }

            return value;
        }
    }
}
