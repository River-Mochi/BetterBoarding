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
                        "bus",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Rail boarding speed" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "train, tram, and subway stop",
                        "vehicle",
                        "Applies to train, tram, and subway stops.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "Ship + ferry speed" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "ship and ferry stop",
                        "vehicle",
                        "Applies to ship and ferry stops.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Airplane speed" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "airplane terminal",
                        "airplane",
                        "Applies to passenger airplane terminals.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "<Late passengers> who are still <not ready> after <departure time> are allowed to miss the vehicle.\n" +
                    "- Note: we only skip solo late citizens.\n" +
                    "- Groups/families travelling together that are late are <not skipped> and may still cause delays to transit like in vanilla.\n" +
                    "- Group travelers are a small number; most benefits are from skipping solo cims who are running late.\n" +
                    "- Skipped late citizens are not deleted; they are naturally reassigned by the game."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims Run Sooner: Bus + Tram + Train" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Citizens who are <late> start <running sooner> to try to make it **before** departure time.\n" +
                    "- Works for buses, trams, and trains, especially on long train platforms.\n" +
                    "- Only affects cims already assigned to a vehicle that is currently boarding.\n" +
                    "- Vanilla only starts cims running at departure time, which can be too late to help.\n" +
                    $"- Pairs well with <{ToggleName}> because it may reduce how many cims miss the vehicle and need to be reassigned.\n" +
                    "- Does not change the vehicle's departure time, force boarding, or teleport citizens."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.PassengerRunSpeedFactor)), "Passenger run speed"
                },
                { m_Setting.GetOptionDescLocaleID( nameof(BBoardSettings.PassengerRunSpeedFactor)),
                    "<1x = vanilla running speed>\n" +
                    "Makes BetterBoarding passengers run faster while hurrying to their assigned bus, tram, or train.\n" +
                    "Only affects passengers currently trying to board those vehicles. Other citizens are unchanged.\n" +
                    "Higher values can look unrealistic; 5x is intentionally extreme."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Total usage" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Monthly public transit usage from the game's Transportation infoview.\n" +
                    "Updated time shows when this status snapshot was taken (usually after entering Options menu)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims run earlier" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "If enabled [x], counts all cims (today) that started **running sooner** to try and catch a bus, tram, or train before departure time.\n" +
                    "Cims run 512 frames earlier than they would in vanilla (~2-8 seconds sooner in real time, ~2 minutes in game)."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Bus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("bus") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Tram" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("tram") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Train" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("train") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Subway" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("subway") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Ferry" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("ferry") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Ship" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("ship") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Airplane" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("airplane") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats to Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Writes a detailed one-time report to **BetterBoarding.log**.\n" +
                    "Includes waiting totals, top 3 worst stops per mode, skipped cim examples, entity IDs, and line hints."
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
                    "**Debug / testing only**\n" +
                    "Adds <live> details to <Logs/BetterBoarding.log> while the city runs.\n" +
                    "**Do not enable for normal gameplay.**\n" +
                    "Leaving this on can decrease performance and create huge log files.\n" +
                    "You can delete old log files later.\n" +
                    "Note: <Stats to Log> is a point-in-time report plus today's late-skip counters; it is different than what is seen with verbose logs.\n" +
                    "Run verbose logging for 15-20 min if you want a timeline of what happened over time.\n" +
                    "Just don't forget to turn **OFF** verbose again before normal gameplay."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Status not loaded." },
                { WaitStatus.KeyNoCityLoaded, "No city loaded." },
                { WaitStatus.KeyNoStopsFound, "No stops found." },

                { WaitStatus.KeyStatusLine, "{0} waiting | avg {1} | worst {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} late today" },
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
                { WaitStatus.KeyReportHintSkippedCims, "Skipped solo cims: late passengers we skip to allow transit to leave. Later state should usually become 'has path' or 'assigned'. If it stays 'no path yet', inspect that cim entity after more time." },
                { WaitStatus.KeyReportHintLateGroups, "Late groups (families): purposely left alone so they stay together and follow vanilla behavior; they are few compared to many single travelers." },
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
                { WaitStatus.KeyReportLateGroups, "Late cims traveling as a group left alone: {0} passengers in {1} groups on {2} vehicles" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Skipped solo late cim examples" },
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
