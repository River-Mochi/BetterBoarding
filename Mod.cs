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

#if DEBUG
        private const string kBuildType = "DEBUG";
#else
        private const string kBuildType = "RELEASE";
#endif

        // Release builds read as "Release" in the Options About tab; a DEBUG
        // build stays shouty so a tester can tell at a glance which one they have.
        public static string BuildDisplayName =>
            kBuildType == "RELEASE" ? "Release" : kBuildType;

        public static readonly string ModVersion =
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

        internal static readonly ILog s_Log =
            LogManager.GetLogger(ModId).SetShowsErrorsInUI(false);

        // Avoid duplicate load banners during mod reloads.
        private static bool s_BannerLogged;

        public static BBoardSettings? Settings;

        public void OnLoad(UpdateSystem updateSystem)
        {
            // Also sets the default logger used by LogUtils.
            ShellOpen.Configure(s_Log, ModId, ModTag);

            if (!s_BannerLogged)
            {
                s_BannerLogged = true;
                LogUtils.Info(
                    $"{ModName} {ModTag} v{ModVersion} [{kBuildType}] OnLoad");
            }

            BBoardSettings setting = new(this);
            Settings = setting;

            // Locales first so Options opens translated.
            LocalizationManager? localizationManager =
                GameManager.instance?.localizationManager;

            if (localizationManager == null)
            {
                LogUtils.Warn("LocalizationManager is null; locale sources were not registered.");
            }
            else
            {
                try
                {
                    localizationManager.AddSource("en-US", new LocaleEN(setting));
                    localizationManager.AddSource("fr-FR", new LocaleFR(setting));
                    localizationManager.AddSource("es-ES", new LocaleES(setting));
                    localizationManager.AddSource("de-DE", new LocaleDE(setting));
                    localizationManager.AddSource("it-IT", new LocaleIT(setting));
                    localizationManager.AddSource("ja-JP", new LocaleJA(setting));
                    localizationManager.AddSource("ko-KR", new LocaleKO(setting));
                    localizationManager.AddSource("pl-PL", new LocalePL(setting));
                    localizationManager.AddSource("pt-BR", new LocalePT_BR(setting));
                    localizationManager.AddSource("pt-PT", new LocalePT_PT(setting));
                    localizationManager.AddSource("th-TH", new LocaleTH(setting));
                    localizationManager.AddSource("tr-TR", new LocaleTR(setting));
                    localizationManager.AddSource("uk-UA", new LocaleUK(setting));
                    localizationManager.AddSource("vi-VN", new LocaleVI(setting));
                    localizationManager.AddSource("zh-HANS", new LocaleZH_CN(setting));
                    localizationManager.AddSource("zh-HANT", new LocaleZH_HANT(setting));
                }
                catch (Exception ex)
                {
                    LogUtils.Warn(
                        $"Localization registration failed: {ex.GetType().Name}: {ex.Message}",
                        ex);
                }
            }

            try
            {
                AssetDatabase.global.LoadSettings(
                    ModId, setting, new BBoardSettings(this));

                // Keep saved values inside the current slider range.
                setting.RepairLoadedValues();

                setting.RegisterInOptionsUI();
                BoardingRuntimeSettings.LoadFromSettings(setting);
            }
            catch (Exception ex)
            {
                LogUtils.Warn(
                    $"Settings/UI init failed: {ex.GetType().Name}: {ex.Message}",
                    ex);
            }

            try
            {
                // Stop data must be tuned before vanilla reads it.
                updateSystem.UpdateBefore<TransportStopTuningSystem, TransportStopSystem>(
                    SystemUpdatePhase.GameSimulation);

                // One registration is one executable update entry in CS2; registering against
                // every earlier system would run this full pass multiple times. HumanMoveSystem
                // already follows the transport AI systems and ResidentAISystem.Actions in
                // vanilla order, so this single anchor preserves the intended behavior window.
                updateSystem.UpdateBefore<LateBoarderCancelSystem, HumanMoveSystem>(
                    SystemUpdatePhase.GameSimulation);

                // Pace departures at one control stop per line after the transport AIs
                // have finalized boarding state. HumanMoveSystem follows all transit AIs.
                updateSystem.UpdateBefore<TransitSpacingAssistSystem, HumanMoveSystem>(
                    SystemUpdatePhase.GameSimulation);

                // Concurrent boarding needs one pass immediately before bus AI and one
                // immediately after it. Registration is deferred until every mod has loaded so
                // we can safely order around All Aboard and yield to Concurrent Bus Boarding.
                SecondBusBoardingCompatibilitySystem.Configure(updateSystem);
                updateSystem.UpdateAt<SecondBusBoardingCompatibilitySystem>(
                    SystemUpdatePhase.Modification1);
                updateSystem.UpdateAt<SecondBusBoardingProvisionSystem>(
                    SystemUpdatePhase.Modification1);
                updateSystem.UpdateAfter<SecondBusBoardingHoldSystem, CarNavigationSystem>(
                    SystemUpdatePhase.GameSimulation);

                // Group assistance runs after navigation but before pet/resident AI. This lets
                // vanilla consume adjusted approach/timeout state in the normal boarding path
                // and avoids conflicting with commands those systems defer to EndFrameBarrier.
                updateSystem.UpdateBefore<LateGroupBoardingSystem, PetAISystem>(
                    SystemUpdatePhase.GameSimulation);

#if DEBUG
                // Continuous read-only diagnostics are a Debug-only testing tool. Release
                // builds use the on-demand Stats to Log report without this recurring scan.
                updateSystem.UpdateAfter<TransitHeadwayDiagnosticSystem>(
                    SystemUpdatePhase.GameSimulation);
#endif

                // Retune once on load even if Options was never opened.
                updateSystem.World.GetOrCreateSystemManaged<TransportStopTuningSystem>().Enabled = true;

                // Start boarding assists in their saved ON/OFF state.
                updateSystem.World.GetOrCreateSystemManaged<LateBoarderCancelSystem>().Enabled =
                    BoardingRuntimeSettings.BoardingAssistEnabled;

                updateSystem.World.GetOrCreateSystemManaged<LateGroupBoardingSystem>().Enabled =
                    BoardingRuntimeSettings.CancelLateBoarders;

                updateSystem.World.GetOrCreateSystemManaged<TransitSpacingAssistSystem>().Enabled =
                    BoardingRuntimeSettings.SpacingAssist;
            }
            catch (Exception ex)
            {
                LogUtils.Warn(
                    $"System scheduling failed: {ex.GetType().Name}: {ex.Message}",
                    ex);
            }
        }

        public void OnDispose()
        {
            Settings?.UnregisterInOptionsUI();
            Settings = null;
            SecondBusBoardingCompatibility.Reset();
        }
    }
}
