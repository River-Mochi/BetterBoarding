// <copyright file="LocaleEN.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleEN.cs
// Purpose: English en-US locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;
   
    /// <summary>
    /// English localization source.
    /// </summary>
    public sealed class LocaleEN : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        /// <summary>
        /// Constructs the English locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleEN(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all English localization entries for this mod.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName;

            const string ToggleName = "Skip Late Passengers";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Higher values make {transitName} boarding/loading faster.\n" +
                    "This shortens normal time spent getting passengers on/off.\n" +
                    "<==========================>\n" +
                    "1x = 100% vanilla dwell\n" +
                    "2x = ~1/2 planned dwell\n" +
                    "3x = ~1/3 planned dwell (recommended)\n" +
                    "5x = ~1/5 planned dwell (max)";
            }


            string StatusDescription(string transitName, bool skipSupported)
            {
                string skippedText = skipSupported
                    ? "**Skipped** = late passengers skipped today so transit didn't keep waiting for them.\n" +
                      "Better Boarding can do this much sooner than vanilla's ~10-minute stuck-vehicle fallback.\n" +
                      $"Controlled by <{ToggleName}>.\n"
                    : "**Skipped** = 0 for this mode. It keeps vanilla late-passenger handling.\n";

                return
                    $"<Current {transitName} status>\n" +
                    "**Waiting** = passengers waiting now.\n" +
                    "**Avg** = average wait.\n" +
                    "**Worst** = highest average wait at one stop.\n" +
                    "Check worst stops for traffic, blocked/bugged stops, or too few vehicles.\n" +
                    skippedText +
                    "Use <Stats to Log> for stop names, entity IDs, and more.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "Actions" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "About" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "Boarding speed" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "Behavior" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Status" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Mod info" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Links" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Debug" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Bus boarding speed" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "bus stop",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Rail boarding speed" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "train, tram, and subway stop",
                        "Applies to train, tram, and subway stops.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Airplane boarding speed" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "airplane terminal",
                        "Applies to passenger airplane terminals.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "A late passenger who is too far from the stop can be skipped, so transit doesn't keep waiting.\n" +
                    "Better Boarding can step in much sooner than vanilla's ~10-minute stuck-vehicle fallback.\n" +
                    "- Riders at or near the stop/station stay assigned and keep boarding.\n" +
                    "- Long train/subway platforms stay protected.\n" +
                    "- Groups/families get extra protection.\n" +
                    "- Skipped cims stay in the city; vanilla can continue or reroute their trip, often onto a later vehicle.\n" +
                    "- Ships, ferries, and airplanes keep vanilla late-passenger handling."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims Run Sooner" },

                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Assigned cims start <running sooner> to help catch transit before it leaves.\n" +
                    "- Starts up to about 3 in-game minutes earlier than vanilla.\n" +
                    "- Real movement assists: testing shows assisted cims often are able to board the same assigned vehicle.\n" +
                    "- Helps on long platforms and while approaching a stop.\n" +
                    "- Only affects cims already assigned to a boarding vehicle.\n" +
                    "- Does not change departure times."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Total usage" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Monthly public transit usage from the game's Transportation infoview.\n" +
                    "Updated = when this snapshot was taken."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims run earlier" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Counts cims today that Better Boarding started **running sooner**.\n" +
                    "Up to about 3 in-game minutes earlier than vanilla."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Bus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("bus", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Tram" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("tram", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Train" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("train", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Subway" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("subway", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Ferry" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("ferry", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Ship" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("ship", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Airplane" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("airplane", skipSupported: false) },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats to Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Writes a detailed one-time report to **BetterBoarding.log**.\n" +
                    "Includes waiting totals, top 3 worst stops, skipped late passenger examples, entity IDs, and line hints."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Open Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Opens **BetterBoarding.log** if it exists.\n" +
                    "If the file is not found yet, opens the Logs folder instead."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Display name of mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Current mod version." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Opens the author's Paradox Mods page." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Enable verbose logging" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Debug / Testing ONLY**\n" +
                    "Keep OFF for normal gameplay.\n" +
                    "Adds <live> details to <Logs/BetterBoarding.log> while the city runs.\n" +
                    "Can reduce performance and create large log files.\n" +
                    "Use for 10-15 min when troubleshooting, then turn it OFF again.\n" +
                    "<Stats to Log> is a lighter one-time snapshot."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Status not loaded." },
                { WaitStatus.KeyNoCityLoaded, "No city loaded." },
                { WaitStatus.KeyNoStopsFound, "No stops found." },

                { WaitStatus.KeyStatusLine, "{0} waiting | avg {1} | worst {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} late skipped" },
                { WaitStatus.KeyStatusSkipOff, "skip OFF" },

                { WaitStatus.KeyStatusOverviewLine, "{0} tourist/mo | {1} citizens/mo | updated {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "run sooner OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Stats report requested, but no city is loaded." },
                { WaitStatus.KeyReportTitle, "Stats to Log snapshot - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Settings: {0}" },
                { WaitStatus.KeyReportNote, "Line hint comes from the highest-wait waypoint at that stop." },
                { WaitStatus.KeyReportTesterHintsHeader, "Tester hints" },
                { WaitStatus.KeyReportHintWorstStops, "Worst stops: inspect these first in-game or with Scene Explorer mod (find locations with entity ID). Look for traffic, bad transit stop location, or a bugged stop." },
                { WaitStatus.KeyReportHintSkippedCims, "Skipped late passengers: riders too far away after departure, so Better Boarding stopped that vehicle from waiting for them. Follow-up examples show what happened next." },
                { WaitStatus.KeyReportHintLateGroups, "Group boarding: shows families/groups still incomplete when this report is taken. Better Boarding gives them extra grace and helps lagging members finish boarding." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Served stops: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Stops with waiting passengers: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Waiting passengers: {0}" },
                { WaitStatus.KeyReportAverageWait, "Average wait: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Late passengers skipped today: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Worst stop: none, no waiting passengers right now." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Worst stop avg wait: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Worst stop name: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Worst stop entity: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Worst waypoint entity: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Worst line hint: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Worst line entity: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Worst line waypoint avg: {0} with {1} waiting" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} worst stops by average wait:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | avg {2} | waiting {3} | stop entity {4} | waypoint entity {5} | line entity {6} | line hint {7}" },
                { WaitStatus.KeyReportLateGroups, "Group boarding incomplete now: {0} passengers in {1} groups on {2} vehicles" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Skipped late passenger examples" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | passenger {2} | missed vehicle {3} | time {4} | now {5}" },
                { WaitStatus.KeyReportNone, "none" },
                { WaitStatus.KeyReportUnknown, "(unknown)" },
            };
        }

        public void Unload()
        {
        }
    }
}
