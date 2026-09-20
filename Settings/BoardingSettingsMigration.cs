// <copyright file="BoardingSettingsMigration.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Settings/BoardingSettingsMigration.cs
// Purpose: Carry published FastBoarding settings into BetterBoarding.

namespace BetterBoarding
{
    using System;
    using System.IO;
    using Colossal.IO.AssetDatabase;
    using Colossal.PSI.Environment;
    using CS2Shared.RiverMochi;

    internal static class BoardingSettingsMigration
    {
        private const string LegacyModId = "FastBoarding";

        private static string LegacySettingsFilePath => Path.Combine(
            EnvPath.kUserDataPath,
            "ModsSettings",
            LegacyModId,
            $"{LegacyModId}.coc");

        private static string BetterBoardingSettingsFilePath => Path.Combine(
            EnvPath.kUserDataPath,
            "ModsSettings",
            Mod.ModId,
            $"{Mod.ModId}.coc");

        internal static bool BetterBoardingSettingsFileExists()
        {
            try
            {
                return File.Exists(BetterBoardingSettingsFilePath);
            }
            catch (Exception ex)
            {
                // If unsure, don't risk replacing current BetterBoarding settings.
                LogUtils.Warn(
                    () => $"Could not check BetterBoarding settings: {ex.GetType().Name}: {ex.Message}",
                    ex);

                return true;
            }
        }

        internal static void TryMigrateFromFastBoarding(
            BBoardSettings settings,
            bool betterBoardingSettingsExisted)
        {
            if (settings == null || settings.FastBoardingSettingsMigrationComplete)
            {
                return;
            }

            try
            {
                // Current BetterBoarding settings always win.
                if (betterBoardingSettingsExisted)
                {
                    CompleteMigration(settings);
                    return;
                }

                // Fresh install: remember that there was nothing to import.
                if (!File.Exists(LegacySettingsFilePath))
                {
                    CompleteMigration(settings);
                    return;
                }

                LegacyFastBoardingSettings? legacy = null;

                AssetDatabase.global.LoadSettings<LegacyFastBoardingSettings>(
                    LegacyModId,
                    (candidate, meta) =>
                    {
                        if (legacy == null && IsLegacySettingsFile(meta))
                        {
                            legacy = candidate;
                        }
                    });

                if (legacy == null)
                {
                    LogUtils.Warn(
                        "FastBoarding settings file exists, but compatible settings were not found.");
                    return;
                }

                settings.BusBoardingSpeedFactor = legacy.BusBoardingSpeedFactor;
                settings.RailBoardingSpeedFactor = legacy.RailBoardingSpeedFactor;
                settings.WaterBoardingSpeedFactor = legacy.WaterBoardingSpeedFactor;
                settings.AirBoardingSpeedFactor = legacy.AirBoardingSpeedFactor;
                settings.CancelLateBoarders = legacy.CancelLateBoarders;
                settings.CimsRunSoonerToCatchBuses = legacy.CimsRunSoonerToCatchBuses;
                settings.EnableVerboseLogging = legacy.EnableVerboseLogging;

                settings.RepairLoadedValues();
                CompleteMigration(settings);

                LogUtils.Info(
                    "Imported FastBoarding settings into BetterBoarding.");
            }
            catch (Exception ex)
            {
                // Migration failure should never stop the mod from loading.
                LogUtils.Warn(
                    () => $"FastBoarding settings migration failed: {ex.GetType().Name}: {ex.Message}",
                    ex);
            }
        }

        private static void CompleteMigration(BBoardSettings settings)
        {
            // Keep this marker even when every visible option is at its default.
            settings.FastBoardingSettingsMigrationComplete = true;
            settings.ApplyAndSave();
        }

        private static bool IsLegacySettingsFile(SourceMeta meta)
        {
            if (string.IsNullOrWhiteSpace(meta.path))
            {
                return false;
            }

            try
            {
                return string.Equals(
                    Path.GetFullPath(meta.path),
                    Path.GetFullPath(LegacySettingsFilePath),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
