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
    using Game.Citizens;
    using Game.Common;
    using Game.Objects;
    using UnityEngine;

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

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\\n" +
                    extraLine +
                    $"Higher values make {transitName} boarding and loading faster.\\n" +
                    "This shortens normal boarding time when passengers are getting on/off.\\n" +
                    "<==========================>\\n" +
                    "Loading value:\\n" +
                    "1x = 100% vanilla dwell\\n" +
                    "2x = ~1/2 planned dwell\\n" +
                    "3x = ~1/3 planned dwell (recommended)\\n" +
                    "5x = ~1/5 planned dwell (max)";
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
                    $"**Released today** = solo rider assignments removed today by <{ToggleName}> after the distance check; it is not a count of unique people or failed trips.\n" +
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
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Rail boarding speed" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "train, tram, and subway stop",
                        "Applies to train, tram, and subway stops.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Airplane speed" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "airplane terminal",
                        "Applies to passenger airplane terminals.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Only assigned riders confirmed clearly far away after the <scheduled departure time> and a short grace may be released. <Not ready> alone does not mean far away.\n" +
                    "- Bus and tram riders at or near the stop keep boarding; riders clearly far away may be released after a short grace.\n" +
                    "- Train and subway riders already in or near the station keep boarding; only riders clearly far from both the stop and every car may be released early.\n" +
                    "- Groups/families get a little extra grace. A bus, tram, train, or subway leader at the stop stays assigned; a clearly distant leader may be released with the group.\n" +
                    "- If the leader is already aboard, lagging child/pet members receive a vanilla boarding nudge so one straggler is less likely to hold the vehicle.\n" +
                    "- Ships and airplanes are not skipped early. Released citizens are not deleted; vanilla may continue or reroute their trips."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims Run Sooner (see tooltip, 3rd panel)" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Assigned cims who are walking start <running sooner> to help catch their bus, tram, train, or subway before departure.\\n" +
                    "- Better Boarding turns on the game's Run behavior up to 512 frames earlier than vanilla.\\n" +
                    "- Helps cims farther down a platform or still approaching the stop reach the vehicle sooner.\\n" +
                    "- Only affects cims already assigned to a vehicle that is currently boarding.\\n" +
                    "- Does not teleport citizens, force boarding, or change the scheduled departure time."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Total usage" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Monthly public transit usage from the game's Transportation infoview.\n" +
                    "Updated time shows when this status snapshot was taken (usually after entering Options menu)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims run earlier" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "If enabled [x], counts all cims (today) that started **running sooner** to try and catch a bus, tram, train, or subway before departure time.\n" +
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
                    "**Debug / Testing ONLY**\n" +
                    "Recommended to KEEP OFF at all times unless you know what this is for.\n" +
                    "Adds <live> details to <Logs/BetterBoarding.log> while the city runs.\n" +
                    "**Do Not enable for normal gameplay.**\n" +
                    "Leaving this on can decrease performance and create huge log files.\n" +
                    "You can delete old log files later.\n" +
                    "Note: <Stats to Log> is right-now report with today's late-skip counters; it does not have the performance impact of Verbose logging.\n" +
                    "Run verbose logging for 10-15 min if you want a timeline of what happened over time, but it's a lot of data.\n" +
                    "Important: Don't forget to turn **OFF** verbose again before normal gameplay."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Status not loaded." },
                { WaitStatus.KeyNoCityLoaded, "No city loaded." },
                { WaitStatus.KeyNoStopsFound, "No stops found." },

                { WaitStatus.KeyStatusLine, "{0} waiting | avg {1} | worst {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} released today" },
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
                { WaitStatus.KeyReportHintSkippedCims, "Released solo assignments: each count is one assignment removal after the distance check, not proof of a completed or failed trip. A later 'assigned' state does not prove that the rider boarded. Inspect a sampled cim after more time." },
                { WaitStatus.KeyReportHintLateGroups, "Late groups (families): shows groups that are still unresolved at the instant this report is taken. BetterBoarding gives groups a short extra grace, then releases an outside group or nudges vanilla to finish lagging members when the leader is already aboard." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Served stops: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Stops with waiting passengers: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Waiting passengers: {0}" },
                { WaitStatus.KeyReportAverageWait, "Average wait: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Solo assignments released today: {0}" },
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
                { WaitStatus.KeyReportLateGroups, "Late group cims still unresolved right now: {0} passengers in {1} groups on {2} vehicles" },
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
