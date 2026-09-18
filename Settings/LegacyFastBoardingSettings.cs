// <copyright file="LegacyFastBoardingSettings.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Settings/LegacyFastBoardingSettings.cs
// Purpose: Data-only shape for importing settings from the published FastBoarding identity.

namespace BetterBoarding
{
    internal sealed class LegacyFastBoardingSettings
    {
        // AssetDatabase.LoadSettings<T> requires a public parameterless constructor.
        public LegacyFastBoardingSettings()
        {
        }

        public int BusBoardingSpeedFactor { get; set; } = BBoardSettings.DefaultSpeedFactor;

        public int RailBoardingSpeedFactor { get; set; } = BBoardSettings.DefaultSpeedFactor;

        public int WaterBoardingSpeedFactor { get; set; } = BBoardSettings.DefaultSpeedFactor;

        public int AirBoardingSpeedFactor { get; set; } = BBoardSettings.DefaultSpeedFactor;

        public bool CancelLateBoarders { get; set; } = true;

        // Historical serialized name retained because this reads the published FastBoarding file.
        public bool CimsRunSoonerToCatchBuses { get; set; } = true;

        public bool EnableVerboseLogging { get; set; }
    }
}
