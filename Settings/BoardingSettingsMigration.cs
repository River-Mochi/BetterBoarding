// <copyright file="BoardingSettingsMigration.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Settings/BoardingSettingsMigration.cs
// Purpose: One-time carry-over from the published FastBoarding settings identity.

namespace BetterBoarding
{
    using System;
    using System.IO;
    using Colossal.IO.AssetDatabase;
    using CS2Shared.RiverMochi;
    using UnityEngine;

    internal static class BoardingSettingsMigration
    {
        private const string LegacyModId = "FastBoarding";
        private const string LegacySettingsRelativePath =
            "ModsSettings/FastBoarding/FastBoarding.coc";
        private const string CurrentSettingsRelativePath =
            "ModsSettings/BetterBoarding/BetterBoarding.coc";

        internal static bool BetterBoardingSettingsFileExists()
        {
            try
            {
                return File.Exists(GetUserDataPath(CurrentSettingsRelativePath));
            }
            catch (Exception ex)
            {
                // When existence cannot be established, preserving current settings is safer than importing.
                LogUtils.Warn(
                    Mod.s_Log,
                    () => $"Could not check BetterBoarding settings before migration: {ex.GetType().Name}: {ex.Message}",
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
                if (betterBoardingSettingsExisted)
                {
                    // A settings file from an earlier BetterBoarding run always wins over legacy data.
                    CompleteMigration(settings);
                    return;
                }

                if (!File.Exists(GetUserDataPath(LegacySettingsRelativePath)))
                {
                    // Mark clean installs so a stale legacy file cannot be imported on a later launch.
                    CompleteMigration(settings);
                    return;
                }

                LegacyFastBoardingSettings? legacy = null;
                AssetDatabase.global.LoadSettings<LegacyFastBoardingSettings>(
                    LegacyModId,
                    (candidate, meta) =>
                    {
                        if (legacy == null && IsExpectedLegacySource(meta))
                        {
                            legacy = candidate;
                        }
                    });

                if (legacy == null)
                {
                    LogUtils.Warn(
                        Mod.s_Log,
                        () => "FastBoarding settings file exists, but no compatible FastBoarding settings block was found.");
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
                    Mod.s_Log,
                    () => "Imported legacy FastBoarding settings and queued them for saving as BetterBoarding settings.");
            }
            catch (Exception ex)
            {
                // Migration is optional compatibility work and must never prevent the mod from loading.
                LogUtils.Warn(
                    Mod.s_Log,
                    () => $"FastBoarding settings migration failed: {ex.GetType().Name}: {ex.Message}",
                    ex);
            }
        }

        private static void CompleteMigration(BBoardSettings settings)
        {
            // Force-saving this hidden marker keeps the one-time gate even when every visible
            // setting equals its default and CS2 would otherwise delete the empty .coc file.
            settings.FastBoardingSettingsMigrationComplete = true;
            settings.ApplyAndSave();
        }

        private static bool IsExpectedLegacySource(SourceMeta meta)
        {
            if (string.IsNullOrWhiteSpace(meta.path))
            {
                return false;
            }

            string normalizedPath = meta.path.Replace('\\', '/').TrimEnd('/');
            string normalizedUserPath = GetUserDataPath(LegacySettingsRelativePath)
                .Replace('\\', '/')
                .TrimEnd('/');

            return string.Equals(
                    normalizedPath,
                    normalizedUserPath,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    normalizedPath.TrimStart('/'),
                    LegacySettingsRelativePath,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static string GetUserDataPath(string relativePath)
        {
            string platformPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.persistentDataPath, platformPath);
        }
    }
}
