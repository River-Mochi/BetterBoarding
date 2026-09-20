// <copyright file="BBoardSettings.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Settings/BBoardSettings.cs
// Purpose: Options UI settings for Better Boarding.

namespace BetterBoarding
{
    using System;
    using Colossal.IO.AssetDatabase;
    using CS2Shared.RiverMochi;
    using Game;
    using Game.Modding;
    using Game.SceneFlow;
    using Game.Settings;
    using Unity.Entities;
    using UnityEngine;

    [FileLocation("ModsSettings/BetterBoarding/BetterBoarding")]
    [SettingsUITabOrder(ActionsTab, AboutTab)]
    [SettingsUIGroupOrder(SpeedGroup, BehaviorGroup, StatusGroup, AboutInfoGroup, AboutLinksGroup, DebugGroup)]
    [SettingsUIShowGroupName(SpeedGroup, BehaviorGroup, StatusGroup, AboutLinksGroup, DebugGroup)]
    public sealed class BBoardSettings : ModSetting
    {
        public const string ActionsTab = "Actions";
        public const string AboutTab = "About";

        public const string SpeedGroup = "BoardingSpeed";
        public const string BehaviorGroup = "Behavior";
        public const string StatusGroup = "Status";
        public const string StatusButtonsRow = "StatusButtonsRow";
        public const string AboutInfoGroup = "ModInfo";
        public const string AboutLinksGroup = "Links";
        public const string DebugGroup = "Debug";

        private const string kUrlParadox =
            "https://mods.paradoxplaza.com/authors/River-mochi/cities_skylines_2?games=cities_skylines_2&orderBy=desc&sortBy=best&time=alltime";

        // 1x is the real vanilla/no-mod baseline. DefaultSpeedFactor is the first-run preset.
        public const int VanillaSpeedFactor = 1;
        public const int DefaultSpeedFactor = 3;
        public const int MinSpeedFactor = VanillaSpeedFactor;
        public const int MaxSpeedFactor = 5;
        public const int MaxPassengerRunSpeedFactor = 4;
        public const int SpeedStepFactor = 1;
        public const int DefaultPassengerRunSpeedFactor = VanillaSpeedFactor;

        public BBoardSettings(IMod mod)
            : base(mod)
        {
            SetDefaults();
        }

        [SettingsUISlider(
            min = MinSpeedFactor,
            max = MaxSpeedFactor,
            step = SpeedStepFactor)]
        [SettingsUISection(ActionsTab, SpeedGroup)]
        [SettingsUISetter(typeof(BBoardSettings), nameof(SetBusBoardingSpeedFactorLive))]
        public int BusBoardingSpeedFactor { get; set; }

        [SettingsUISlider(
            min = MinSpeedFactor,
            max = MaxSpeedFactor,
            step = SpeedStepFactor)]
        [SettingsUISection(ActionsTab, SpeedGroup)]
        [SettingsUISetter(typeof(BBoardSettings), nameof(SetRailBoardingSpeedFactorLive))]
        public int RailBoardingSpeedFactor { get; set; }

        [SettingsUISlider(
            min = MinSpeedFactor,
            max = MaxSpeedFactor,
            step = SpeedStepFactor)]
        [SettingsUISection(ActionsTab, SpeedGroup)]
        [SettingsUISetter(typeof(BBoardSettings), nameof(SetWaterBoardingSpeedFactorLive))]
        public int WaterBoardingSpeedFactor { get; set; }

        [SettingsUISlider(
            min = MinSpeedFactor,
            max = MaxSpeedFactor,
            step = SpeedStepFactor)]
        [SettingsUISection(ActionsTab, SpeedGroup)]
        [SettingsUISetter(typeof(BBoardSettings), nameof(SetAirBoardingSpeedFactorLive))]
        public int AirBoardingSpeedFactor { get; set; }

        [SettingsUISection(ActionsTab, BehaviorGroup)]
        [SettingsUISetter(typeof(BBoardSettings), nameof(SetCancelLateBoardersLive))]
        public bool CancelLateBoarders { get; set; }

        // Historical property name retained for .coc compatibility; applies to bus, tram, train, and subway.
        [SettingsUISection(ActionsTab, BehaviorGroup)]
        [SettingsUISetter(typeof(BBoardSettings), nameof(SetCimsRunSoonerToCatchBusesLive))]
        public bool CimsRunSoonerToCatchBuses { get; set; }

        [SettingsUISlider(
            min = MinSpeedFactor,
            max = MaxPassengerRunSpeedFactor,
            step = SpeedStepFactor)]
        [SettingsUISection(ActionsTab, BehaviorGroup)]
        [SettingsUISetter(typeof(BBoardSettings), nameof(SetPassengerRunSpeedFactorLive))]
        public int PassengerRunSpeedFactor { get; set; }

        [SettingsUISection(ActionsTab, StatusGroup)]
        public string StatusOverview
        {
            get
            {
                try { WaitStatus.RefreshIfNeeded(); } catch { }
                return WaitStatus.OverviewSummary ?? string.Empty;
            }
        }

        [SettingsUISection(ActionsTab, StatusGroup)]
        public string StatusCimsRunSooner
        {
            get
            {
                try { WaitStatus.RefreshIfNeeded(); } catch { }
                return WaitStatus.CimsRunSoonerSummary ?? string.Empty;
            }
        }

        [SettingsUISection(ActionsTab, StatusGroup)]
        public string StatusBus
        {
            get
            {
                // Options UI polls status rows separately; the cache prevents duplicate work.
                try { WaitStatus.RefreshIfNeeded(); } catch { }
                return WaitStatus.BusSummary ?? string.Empty;
            }
        }

        [SettingsUISection(ActionsTab, StatusGroup)]
        public string StatusTram
        {
            get
            {
                try { WaitStatus.RefreshIfNeeded(); } catch { }
                return WaitStatus.TramSummary ?? string.Empty;
            }
        }

        [SettingsUISection(ActionsTab, StatusGroup)]
        public string StatusTrain
        {
            get
            {
                try { WaitStatus.RefreshIfNeeded(); } catch { }
                return WaitStatus.TrainSummary ?? string.Empty;
            }
        }

        [SettingsUISection(ActionsTab, StatusGroup)]
        public string StatusSubway
        {
            get
            {
                try { WaitStatus.RefreshIfNeeded(); } catch { }
                return WaitStatus.SubwaySummary ?? string.Empty;
            }
        }

        [SettingsUISection(ActionsTab, StatusGroup)]
        public string StatusFerry
        {
            get
            {
                try { WaitStatus.RefreshIfNeeded(); } catch { }
                return WaitStatus.FerrySummary ?? string.Empty;
            }
        }

        [SettingsUISection(ActionsTab, StatusGroup)]
        public string StatusShip
        {
            get
            {
                try { WaitStatus.RefreshIfNeeded(); } catch { }
                return WaitStatus.ShipSummary ?? string.Empty;
            }
        }

        [SettingsUISection(ActionsTab, StatusGroup)]
        public string StatusAir
        {
            get
            {
                try { WaitStatus.RefreshIfNeeded(); } catch { }
                return WaitStatus.AirSummary ?? string.Empty;
            }
        }

        [SettingsUISection(ActionsTab, StatusGroup)]
        [SettingsUIButtonGroup(StatusButtonsRow)]
        [SettingsUIButton]
        public bool StatsToLog
        {
            set
            {
                if (!value)
                {
                    return;
                }

                // Detailed report belongs in the log so the UI rows can stay compact.
                WaitStatus.LogDetailedReport();
            }
        }

        [SettingsUISection(ActionsTab, StatusGroup)]
        [SettingsUIButtonGroup(StatusButtonsRow)]
        [SettingsUIButton]
        public bool OpenLog
        {
            set
            {
                if (!value)
                {
                    return;
                }

                // Open the exact mod log when possible; otherwise open the Logs folder.
                ShellOpen.OpenModLogOrLogsFolder();
            }
        }

        [SettingsUISection(AboutTab, AboutInfoGroup)]
        public string AboutName => Mod.ModName;

        [SettingsUISection(AboutTab, AboutInfoGroup)]
        public string AboutVersion => Mod.ModVersion;

        [SettingsUISection(AboutTab, AboutLinksGroup)]
        [SettingsUIButtonGroup(AboutLinksGroup)]
        [SettingsUIButton]
        public bool OpenParadoxMods
        {
            set
            {
                if (!value)
                {
                    return;
                }

                try
                {
                    // External links use Unity's URL opener; no filesystem fallback needed here.
                    Application.OpenURL(kUrlParadox);
                }
                catch (Exception ex)
                {
                    LogUtils.Info(Mod.s_Log, () => $"{Mod.ModTag} OpenParadoxMods failed: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        [SettingsUISection(AboutTab, DebugGroup)]
        [SettingsUISetter(typeof(BBoardSettings), nameof(SetEnableVerboseLoggingLive))]
        public bool EnableVerboseLogging { get; set; }

        private void SetBusBoardingSpeedFactorLive(int value)
        {
            if (BoardingRuntimeSettings.SetBusBoardingSpeedFactor(ClampSpeedFactor(value)))
            {
                // Live setters let the relevant system react immediately instead of waking every system.
                LogSpeedChange();
                TryEnableStopTuningSystem();
            }
        }

        // Use the same locale info for both Open Log buttons.
        [SettingsUISection(AboutTab, DebugGroup)]
        [SettingsUIDisplayName("BetterBoarding.BetterBoarding.Mod.BBoardSettings.OpenLog")]
        [SettingsUIDescription("BetterBoarding.BetterBoarding.Mod.BBoardSettings.OpenLog")]
        [SettingsUIButton]
        public bool OpenLogAbout
        {
            set
            {
                if (!value)
                {
                    return;
                }

                ShellOpen.OpenModLogOrLogsFolder();
            }
        }

        private void SetRailBoardingSpeedFactorLive(int value)
        {
            if (BoardingRuntimeSettings.SetRailBoardingSpeedFactor(ClampSpeedFactor(value)))
            {
                LogSpeedChange();
                TryEnableStopTuningSystem();
            }
        }

        private void SetWaterBoardingSpeedFactorLive(int value)
        {
            if (BoardingRuntimeSettings.SetWaterBoardingSpeedFactor(ClampSpeedFactor(value)))
            {
                LogSpeedChange();
                TryEnableStopTuningSystem();
            }
        }

        private void SetAirBoardingSpeedFactorLive(int value)
        {
            if (BoardingRuntimeSettings.SetAirBoardingSpeedFactor(ClampSpeedFactor(value)))
            {
                LogSpeedChange();
                TryEnableStopTuningSystem();
            }
        }

        private void SetCancelLateBoardersLive(bool value)
        {
            if (BoardingRuntimeSettings.SetCancelLateBoarders(value))
            {
                // Apply this toggle immediately without waking unrelated systems.
                LogUtils.Info(
                    Mod.s_Log,
                    () => DescribeBehaviorForLog(value, BoardingRuntimeSettings.CimsRunSoonerToCatchBuses));
                TrySetLateBoarderSystemEnabled(BoardingRuntimeSettings.BoardingAssistEnabled);
            }
        }

        private void SetCimsRunSoonerToCatchBusesLive(bool value)
        {
            if (BoardingRuntimeSettings.SetCimsRunSoonerToCatchBuses(value))
            {
                // This only sets vanilla's Run flag a little before bus/tram/train/subway departure.
                LogUtils.Info(
                    Mod.s_Log,
                    () => DescribeBehaviorForLog(BoardingRuntimeSettings.CancelLateBoarders, value));
                TrySetLateBoarderSystemEnabled(BoardingRuntimeSettings.BoardingAssistEnabled);
                TrySetRunSoonerSpeedSystemEnabled(
                    BoardingRuntimeSettings.RunSoonerSpeedBoostEnabled);
            }
        }

        private void SetPassengerRunSpeedFactorLive(int value)
        {
            if (BoardingRuntimeSettings.SetPassengerRunSpeedFactor(
                    ClampPassengerRunSpeedFactor(value)))
            {
                LogSpeedChange();

                TrySetRunSoonerSpeedSystemEnabled(
                    BoardingRuntimeSettings.RunSoonerSpeedBoostEnabled);
            }
        }

        private void SetEnableVerboseLoggingLive(bool value)
        {
            if (BoardingRuntimeSettings.SetEnableVerboseLogging(value))
            {
                LogUtils.Info(Mod.s_Log, () => BoardingRuntimeSettings.DescribeVerboseForLog(value));
            }
        }

        private static void LogSpeedChange()
        {
            // Keep slider logs short because players may drag several sliders in one session.
            LogUtils.Info(Mod.s_Log, () => $"Speed changed: {BoardingRuntimeSettings.DescribeForLog()}");
        }

        private static string DescribeBehaviorForLog(
            bool skipLateSoloCim,
            bool runSooner)
        {
            return $"Options Settings: skipLateSoloCim={skipLateSoloCim}, runSooner={runSooner}";
        }

        public void RepairLoadedValues()
        {
            RepairAndClamp();
        }

        private void RepairAndClamp()
        {
            BusBoardingSpeedFactor = ClampSpeedFactor(BusBoardingSpeedFactor);
            RailBoardingSpeedFactor = ClampSpeedFactor(RailBoardingSpeedFactor);
            WaterBoardingSpeedFactor = ClampSpeedFactor(WaterBoardingSpeedFactor);
            AirBoardingSpeedFactor = ClampSpeedFactor(AirBoardingSpeedFactor);
            PassengerRunSpeedFactor = ClampPassengerRunSpeedFactor(PassengerRunSpeedFactor);
        }

        private static int ClampSpeedFactor(int value)
        {
            if (value < MinSpeedFactor)
            {
                return MinSpeedFactor;
            }

            if (value > MaxSpeedFactor)
            {
                return MaxSpeedFactor;
            }

            return value;
        }

        private static int ClampPassengerRunSpeedFactor(int value)
        {
            if (value < MinSpeedFactor)
            {
                return MinSpeedFactor;
            }

            if (value > MaxPassengerRunSpeedFactor)
            {
                return MaxPassengerRunSpeedFactor;
            }

            return value;
        }

        private static void TryEnableStopTuningSystem()
        {
            if (!TryGetLoadedWorld(out World? world))
            {
                return;
            }

            try
            {
                // SettingsUISetter can fire while in-game, so wake only the relevant one-shot system.
                TransportStopTuningSystem system =
                    world.GetExistingSystemManaged<TransportStopTuningSystem>() ??
                    world.GetOrCreateSystemManaged<TransportStopTuningSystem>();

                // The stop tuning system does one pass, then disables itself again.
                system.Enabled = true;
            }
            catch (Exception ex)
            {
                LogUtils.Warn(Mod.s_Log, () => $"Failed enabling TransportStopTuningSystem: {ex.GetType().Name}: {ex.Message}", ex);
            }
        }

        private static void TrySetLateBoarderSystemEnabled(bool enabled)
        {
            if (!TryGetLoadedWorld(out World? world))
            {
                return;
            }

            try
            {
                // The live system stays disabled unless at least one boarding behavior is on.
                LateBoarderCancelSystem system =
                    world.GetExistingSystemManaged<LateBoarderCancelSystem>() ??
                    world.GetOrCreateSystemManaged<LateBoarderCancelSystem>();

                system.Enabled = enabled;
            }
            catch (Exception ex)
            {
                LogUtils.Warn(Mod.s_Log, () => $"Failed updating LateBoarderCancelSystem state: {ex.GetType().Name}: {ex.Message}", ex);
            }
        }

        private static void TrySetRunSoonerSpeedSystemEnabled(bool enabled)
        {
            if (!TryGetLoadedWorld(out World? world))
            {
                return;
            }

            try
            {
                RunSoonerSpeedSystem system =
                    world.GetExistingSystemManaged<RunSoonerSpeedSystem>() ??
                    world.GetOrCreateSystemManaged<RunSoonerSpeedSystem>();

                system.Enabled = enabled;
            }
            catch (Exception ex)
            {
                LogUtils.Warn(
                    Mod.s_Log,
                    () =>
                        $"Failed updating RunSoonerSpeedSystem state: " +
                        $"{ex.GetType().Name}: {ex.Message}",
                    ex);
            }
        }

        private static bool TryGetLoadedWorld(out World world)
        {
            world = World.DefaultGameObjectInjectionWorld;

            GameManager gameManager = GameManager.instance;
            // UI setters can fire from the main menu too, so guard against a missing game world.
            if (!gameManager.gameMode.IsGame() || world == null)
            {
                return false;
            }

            return true;
        }

        public override void SetDefaults()
        {
            // New installs start at a noticeable but not extreme middle value.
            BusBoardingSpeedFactor = DefaultSpeedFactor;
            RailBoardingSpeedFactor = DefaultSpeedFactor;
            WaterBoardingSpeedFactor = DefaultSpeedFactor;
            AirBoardingSpeedFactor = DefaultSpeedFactor;
            CancelLateBoarders = true;
            CimsRunSoonerToCatchBuses = true;
            EnableVerboseLogging = false;
            PassengerRunSpeedFactor = DefaultPassengerRunSpeedFactor;
        }
    }
}
