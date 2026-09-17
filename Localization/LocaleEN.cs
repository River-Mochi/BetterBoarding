// <copyright file="LocaleEN.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleEN.cs
// Purpose: English en-US locale entries for Boarding Now.

namespace BoardingNow
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// English localization source.
    /// </summary>
    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;

        /// <summary>
        /// Constructs the English locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleEN(Setting setting)
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

            if (!string.IsNullOrEmpty(Mod.ModVersion))
            {
                title = title + " (" + Mod.ModVersion + ")";
            }

            const string ToggleName = "Skip Late Passengers";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Higher values reduce {transitName} boarding and loading time.\n" +
        
                    "This helps normal queues clear faster, but a late passenger can still delay departure because of vanilla design.\n" +
                    $"Use [✓] <{ToggleName}> if you want late cims to miss the vehicle after departure time.\n" +
                    "Skipped late citizens are not deleted; vanilla will naturally reroute them.\n" +
                    "<==========================>\n" +
                    "Loading value:\n" +
                    "1x = 100% vanilla dwell\n" +
                    "2x = ~1/2 planned dwell\n" +
                    "3x = ~1/3 planned dwell (recommended)\n" +
                    "5x = ~1/5 planned dwell (max)\n" +
                    $"This is not the same as <{ToggleName}>; that checkbox decides whether late cims can miss the {shortName} after departure time.";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Current {transitName} status>\n" +
                    "**Waiting** = total passengers waiting right now.\n" +
                    "**Avg** = average wait time for those passengers.\n" +
                    "**Worst** stop = highest average wait at one stop.\n" +
                    "Worst stops are good places to inspect for traffic accidents, blocked/bugged stops, or need more vehicles assigned.\n" +
                    $"**Late today** = solo late passengers skipped today by <{ToggleName}>.\n" +
                    "Use <Stats to Log> for detailed report: stop names, entity IDs, and more.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.ActionsTab), "Actions" },
                { m_Setting.GetOptionTabLocaleID(Setting.AboutTab), "About" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.SpeedGroup), "Boarding speed" },
                { m_Setting.GetOptionGroupLocaleID(Setting.BehaviorGroup), "Behavior" },
                { m_Setting.GetOptionGroupLocaleID(Setting.StatusGroup), "Status" },
                { m_Setting.GetOptionGroupLocaleID(Setting.AboutInfoGroup), "Mod info" },
                { m_Setting.GetOptionGroupLocaleID(Setting.AboutLinksGroup), "Links" },
                { m_Setting.GetOptionGroupLocaleID(Setting.DebugGroup), "Debug" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.BusBoardingSpeedFactor)), "Bus boarding speed" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "bus stop",
                        "bus",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RailBoardingSpeedFactor)), "Rail boarding speed" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "train, tram, and subway stop",
                        "vehicle",
                        "Applies to train, tram, and subway stops.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.WaterBoardingSpeedFactor)), "Ship + ferry speed" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "ship and ferry stop",
                        "vehicle",
                        "Applies to ship and ferry stops.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AirBoardingSpeedFactor)), "Airplane speed" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "airplane terminal",
                        "airplane",
                        "Applies to passenger airplane terminals.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.CancelLateBoarders)),
                    "<Late passengers> who are still <not ready> after <departure time> are allowed to miss the vehicle.\n" +
                    "- Note: we only skip solo late citizens.\n" +
                    "- Groups/families travelling together that are late are <not skipped> and may still cause delays to transit like in vanilla.\n" +
                    "- Group travelers are a small number; most benefits are from skipping solo cims who are running late.\n" +
                    "- Skipped late citizens are not deleted; they are naturally reassigned by the game."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.CimsRunSoonerToCatchBuses)), "Cims Run Sooner: Buses + Trams" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.CimsRunSoonerToCatchBuses)),
                    "Citizens who are <late> start <running sooner> to try to make it **before** departure time.\n" +
                    "- Helps keep buses/trams on schedule.\n" +
                    "- Only affects cims already assigned to a vehicle that is currently boarding.\n" +
                    "- Vanilla only starts cims running at departure time, which can be too late to help.\n" +
                    $"- Pairs well with <{ToggleName}> because it may reduce how many cims miss the vehicle and need to be reassigned.\n" +
                    "- Does not force boarding or teleport citizens."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusOverview)), "Total usage" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusOverview)),
                    "Monthly public transit usage from the game's Transportation infoview.\n" +
                    "Updated time shows when this status snapshot was taken (usually after entering Options menu)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusCimsRunSooner)), "Cims run earlier" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusCimsRunSooner)),
                    "If enabled [x], counts all cims (today) that started **running sooner** to try and catch a bus/tram before departure time.\n" +
                    "Cims run 512 frames earlier than they would in vanilla (~2-8 seconds sooner in real time, ~2 minutes in game)."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusBus)), "Bus" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusBus)), StatusDescription("bus") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusTram)), "Tram" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusTram)), StatusDescription("tram") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusTrain)), "Train" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusTrain)), StatusDescription("train") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusSubway)), "Subway" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusSubway)), StatusDescription("subway") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusFerry)), "Ferry" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusFerry)), StatusDescription("ferry") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusShip)), "Ship" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusShip)), StatusDescription("ship") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusAir)), "Airplane" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusAir)), StatusDescription("airplane") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatsToLog)), "Stats to Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatsToLog)),
                    "Writes a detailed one-time report to **BoardingNow.log**.\n" +
                    "Includes waiting totals, top 3 worst stops per mode, skipped cim examples, entity IDs, and line hints."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenLog)), "Open Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenLog)),
                    "Opens **BoardingNow.log** if it exists.\n" +
                    "If the file is not found yet, opens the Logs folder instead."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AboutName)), "Display name of mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AboutVersion)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AboutVersion)), "Current mod version." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxMods)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadoxMods)), "Opens the author's Paradox Mods page." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableVerboseLogging)), "Enable verbose logging" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableVerboseLogging)),
                    "**Debug / testing only**\n" +
                    "Adds <live> details to <Logs/BoardingNow.log> while the city runs.\n" +
                    "**Do not enable for normal gameplay.**\n" +
                    "Leaving this on can decrease performance and create huge log files.\n" +
                    "You can delete old log files later.\n" +
                    "Note: <Stats to Log> is a point-in-time report plus today's late-skip counters; it is different than what is seen with verbose logs.\n" +
                    "Run verbose logging for 15-20 min if you want a timeline of what happened over time.\n" +
                    "Just don't forget to turn **OFF** verbose again before normal gameplay."
                },

                // Runtime status strings
                { TransitWaitStatus.KeyStatusNotLoaded, "Status not loaded." },
                { TransitWaitStatus.KeyNoCityLoaded, "No city loaded." },
                { TransitWaitStatus.KeyNoStopsFound, "No stops found." },

                { TransitWaitStatus.KeyStatusLine, "{0} waiting | avg {1} | worst {2} | {3}" },
                { TransitWaitStatus.KeyStatusLateSkipped, "{0} late today" },
                { TransitWaitStatus.KeyStatusSkipOff, "skip OFF" },

                { TransitWaitStatus.KeyStatusOverviewLine, "{0} tourist/mo | {1} citizens/mo | updated {2}" },
                { TransitWaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { TransitWaitStatus.KeyStatusRunSoonerOff, "run sooner OFF" },

                // Stats-to-log report strings
                { TransitWaitStatus.KeyReportNoCityLoaded, "[BN] Stats report requested, but no city is loaded." },
                { TransitWaitStatus.KeyReportTitle, "Stats to Log snapshot - Boarding Now" },
                { TransitWaitStatus.KeyReportSettings, "Settings: {0}" },
                { TransitWaitStatus.KeyReportNote, "Line hint comes from the highest-wait waypoint at that stop." },
                { TransitWaitStatus.KeyReportTesterHintsHeader, "Tester hints" },
                { TransitWaitStatus.KeyReportHintWorstStops, "Worst stops: inspect these first in-game or with Scene Explorer mod (find locations with entity ID). Look for traffic, bad transit stop location, or a bugged stop." },
                { TransitWaitStatus.KeyReportHintSkippedCims, "Skipped solo cims: late passengers we skip to allow transit to leave. Later state should usually become 'has path' or 'assigned'. If it stays 'no path yet', inspect that cim entity after more time." },
                { TransitWaitStatus.KeyReportHintLateGroups, "Late groups (families): purposely left alone so they stay together and follow vanilla behavior; they are few compared to many single travelers." },
                { TransitWaitStatus.KeyReportFamilyHeader, "{0}" },
                { TransitWaitStatus.KeyReportServedStops, "Served stops: {0}" },
                { TransitWaitStatus.KeyReportStopsWithWaiting, "Stops with waiting passengers: {0}" },
                { TransitWaitStatus.KeyReportWaitingPassengers, "Waiting passengers: {0}" },
                { TransitWaitStatus.KeyReportAverageWait, "Average wait: {0}" },
                { TransitWaitStatus.KeyReportLateBoardersSkipped, "Late passengers skipped today: {0}" },
                { TransitWaitStatus.KeyReportWorstStopNone, "Worst stop: none, no waiting passengers right now." },
                { TransitWaitStatus.KeyReportWorstStopAverageWait, "Worst stop avg wait: {0}" },
                { TransitWaitStatus.KeyReportWorstStopName, "Worst stop name: {0}" },
                { TransitWaitStatus.KeyReportWorstStopEntity, "Worst stop entity: {0}" },
                { TransitWaitStatus.KeyReportWorstWaypointEntity, "Worst waypoint entity: {0}" },
                { TransitWaitStatus.KeyReportWorstLineHint, "Worst line hint: {0}" },
                { TransitWaitStatus.KeyReportWorstLineEntity, "Worst line entity: {0}" },
                { TransitWaitStatus.KeyReportWorstLineWaypointAverage, "Worst line waypoint avg: {0} with {1} waiting" },
                { TransitWaitStatus.KeyReportTopWorstStopsHeader, "Top {0} worst stops by average wait:" },
                { TransitWaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | avg {2} | waiting {3} | stop entity {4} | waypoint entity {5} | line entity {6} | line hint {7}" },
                { TransitWaitStatus.KeyReportLateGroups, "Late cims traveling as a group left alone: {0} passengers in {1} groups on {2} vehicles" },
                { TransitWaitStatus.KeyReportLastSkippedSamplesHeader, "Skipped solo late cim examples" },
                { TransitWaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | passenger {2} | missed vehicle {3} | time {4} | now {5}" },
                { TransitWaitStatus.KeyReportNone, "none" },
                { TransitWaitStatus.KeyReportUnknown, "(unknown)" },
            };
        }

        public void Unload()
        {
        }
    }
}
