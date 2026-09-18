// <copyright file="Mod.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Mod.cs
// Purpose: Entry point for Better Boarding.

namespace BetterBoarding
{
    using System;
    using System.Reflection;
    using Colossal;
    using Colossal.IO.AssetDatabase;
    using Colossal.Localization;
    using Colossal.Logging;
    using CS2Shared.RiverMochi;
    using Game;
    using Game.Modding;
    using Game.SceneFlow;
    using Game.Simulation;

    public sealed class Mod : IMod
    {
        public const string ModName = "Better Boarding";
        public const string ModId = "BetterBoarding";
        public const string ModTag = "[BBoard]";

        public static readonly string ModVersion =
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

        // Register a dedicated BetterBoarding.log, then use LogUtils for popup-safe writes.
        public static readonly ILog s_Log =
            LogManager.GetLogger(ModId).SetShowsErrorsInUI(false);

        // OnLoad may run more than once during mod reload tests; keep the banner once per process.
        private static bool s_BannerLogged;

        public static BBoardSettings? Settings;

        public void OnLoad(UpdateSystem updateSystem)
        {
            ShellOpen.Configure(s_Log, ModId, ModTag);

            if (!s_BannerLogged)
            {
                s_BannerLogged = true;
                LogUtils.Info(s_Log, () => $"{ModName} v{ModVersion} OnLoad");
            }

            BBoardSettings setting = new(this);
            Settings = setting;
          

            // Register languages here so future locale files only need one new AddLocaleSource line.
            AddLocaleSource("en-US", new LocaleEN(setting));
            AddLocaleSource("fr-FR", new LocaleFR(setting));
            AddLocaleSource("es-ES", new LocaleES(setting));
            AddLocaleSource("de-DE", new LocaleDE(setting));
            AddLocaleSource("it-IT", new LocaleIT(setting));
            AddLocaleSource("ja-JP", new LocaleJA(setting));
            AddLocaleSource("ko-KR", new LocaleKO(setting));
            AddLocaleSource("pl-PL", new LocalePL(setting));
            AddLocaleSource("pt-BR", new LocalePT_BR(setting));
            AddLocaleSource("zh-HANS", new LocaleZH_CN(setting));    // Simplified Chinese
            AddLocaleSource("zh-HANT", new LocaleZH_HANT(setting));  // Traditional Chinese

            try
            {
                // CS2 persists ModSetting values in the mod .coc file.
                // Locales + load settings before register in OptionsUI so it shows localized+saved settings.
                bool betterBoardingSettingsExisted =
                    BoardingSettingsMigration.BetterBoardingSettingsFileExists();
                AssetDatabase.global.LoadSettings(ModId, setting, new BBoardSettings(this));

                // Clamp old saved values before Options UI sees them.
                // Example: helps legacy 6x-10x values become new 5x max after this update.
                setting.RepairLoadedValues();
                BoardingSettingsMigration.TryMigrateFromFastBoarding(
                    setting,
                    betterBoardingSettingsExisted);
                setting.RegisterInOptionsUI();
                BoardingRuntimeSettings.Apply(setting);
            }
            catch (Exception ex)
            {
                LogUtils.Warn(s_Log, () => $"Settings/UI init failed: {ex.GetType().Name}: {ex.Message}", ex);
            }

            try
            {
                // Stop tuning is one-shot; boarding assist runs only while a behavior option is enabled.
                // Run after vanilla transport AI has decided whether the vehicle is still boarding, then
                // before cim movement consumes the passenger/path edits.
                updateSystem.UpdateBefore<TransportStopTuningSystem, TransportStopSystem>(
                    SystemUpdatePhase.GameSimulation);
                updateSystem.UpdateAfter<LateBoarderCancelSystem, TransportCarAISystem>(
                    SystemUpdatePhase.GameSimulation);
                updateSystem.UpdateAfter<LateBoarderCancelSystem, TransportTrainAISystem>(
                    SystemUpdatePhase.GameSimulation);
                updateSystem.UpdateAfter<LateBoarderCancelSystem, TransportWatercraftAISystem>(
                    SystemUpdatePhase.GameSimulation);
                updateSystem.UpdateAfter<LateBoarderCancelSystem, TransportAircraftAISystem>(
                    SystemUpdatePhase.GameSimulation);
                updateSystem.UpdateAfter<LateBoarderCancelSystem, ResidentAISystem.Actions>(
                    SystemUpdatePhase.GameSimulation);
                updateSystem.UpdateBefore<LateBoarderCancelSystem, HumanMoveSystem>(
                    SystemUpdatePhase.GameSimulation);

                // Ensure the one-shot prefab retune pass runs on startup/load without needing the player
                // to open Options first.
                updateSystem.World.GetOrCreateSystemManaged<TransportStopTuningSystem>().Enabled = true;
                // Start the boarding-assist system in the same ON/OFF state saved in the mod settings file.
                updateSystem.World.GetOrCreateSystemManaged<LateBoarderCancelSystem>().Enabled =
                    BoardingRuntimeSettings.BoardingAssistEnabled;
            }
            catch (Exception ex)
            {
                LogUtils.Warn(s_Log, () => $"System scheduling failed: {ex.GetType().Name}: {ex.Message}", ex);
            }
        }

        public void OnDispose()
        {
            LogUtils.Info(s_Log, () => nameof(OnDispose));

            if (Settings != null)
            {
                try
                {
                    // Keep the Options UI clean if the mod is unloaded/reloaded.
                    Settings.UnregisterInOptionsUI();
                }
                catch (Exception ex)
                {
                    LogUtils.Warn(s_Log, () => $"UnregisterInOptionsUI failed: {ex.GetType().Name}: {ex.Message}", ex);
                }

                Settings = null;
            }
        }

        internal static void WarnOnce(string key, Func<string> messageFactory)
        {
            LogUtils.WarnOnce(s_Log, key, messageFactory);
        }

        private static void AddLocaleSource(string localeId, IDictionarySource source)
        {
            if (string.IsNullOrEmpty(localeId))
            {
                return;
            }

            LocalizationManager? localizationManager = GameManager.instance.localizationManager;
            if (localizationManager == null)
            {
                LogUtils.Warn(s_Log, () => $"AddLocaleSource: No LocalizationManager; cannot add source for '{localeId}'.");
                return;
            }

            try
            {
                localizationManager.AddSource(localeId, source);
            }
            catch (Exception ex)
            {
                LogUtils.Warn(s_Log, () => $"AddLocaleSource: AddSource for '{localeId}' failed: {ex.GetType().Name}: {ex.Message}", ex);
            }
        }
    }
}
