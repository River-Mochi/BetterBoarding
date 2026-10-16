// <copyright file="LocaleDE.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleDE.cs
// Purpose: German de-DE locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// German localization source.
    /// </summary>
    public sealed class LocaleDE : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        public LocaleDE(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName + " (Besseres Einsteigen)";

            if (!string.IsNullOrEmpty(Mod.ModVersion))
            {
                title = title + " (" + Mod.ModVersion + ")";
            }

            const string ToggleName = "Verspätete Fahrgäste überspringen";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = Vanilla>\n" +
                    extraLine +
                    $"Höhere Werte beschleunigen Ein- und Aussteigen an {transitName}.\n" +
                    "Dadurch wird die normale Haltezeit beim Fahrgastwechsel kürzer.\n" +
                    "<==========================>\n" +
                    "1x = 100% Vanilla-Haltezeit\n" +
                    "2x = ~1/2 geplante Haltezeit\n" +
                    "3x = ~1/3 geplante Haltezeit (empfohlen)\n" +
                    "5x = ~1/5 geplante Haltezeit (max.)";
            }

            // One helper keeps the status tooltips consistent while explaining
            // which modes Better Boarding can actually skip late passengers for.
            string StatusDescription(string transitName, bool skipSupported)
            {
                string skippedText = skipSupported
                    ? "**Übersprungen** = heute übersprungene verspätete Fahrgäste, damit der Verkehr nicht weiter wartet.\n" +
                      "Better Boarding kann viel früher eingreifen als Vanillas ~10-Minuten-Notlösung für festhängende Fahrzeuge.\n" +
                      $"Gesteuert durch <{ToggleName}>.\n"
                    : "**Übersprungen** = 0 für dieses Verkehrsmittel. Verspätete Fahrgäste bleiben hier bei Vanilla.\n";

                return
                    $"<Aktueller {transitName}-Status>\n" +
                    "**Wartend** = Fahrgäste, die gerade warten.\n" +
                    "**Ø** = durchschnittliche Wartezeit.\n" +
                    "**Max** = höchste durchschnittliche Wartezeit an einer Haltestelle.\n" +
                    "Bei hohen Werten Verkehr, blockierte/fehlerhafte Haltestellen oder zu wenige Fahrzeuge prüfen.\n" +
                    skippedText +
                    "<Stats ins Log> zeigt Haltestellennamen, Entity-IDs und weitere Details.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "Aktionen" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "Über" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "Einstiegsgeschwindigkeit" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "Verhalten" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Status" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Mod-Info" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Links" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Debug" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Bus-Einstieg" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "Bushaltestellen",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Bahn-Einstieg" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "Zug-, Straßenbahn- und U-Bahn-Haltestellen",
                        "Gilt für Zug, Straßenbahn und U-Bahn.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Flugzeug-Einstieg" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "Flugzeugterminals",
                        "Gilt für Passagierflugzeug-Terminals.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Ein verspäteter Fahrgast, der zu weit von der Haltestelle entfernt ist, kann übersprungen werden, damit der Verkehr nicht weiter wartet.\n" +
                    "Better Boarding kann viel früher eingreifen als Vanillas ~10-Minuten-Notlösung für festhängende Fahrzeuge.\n" +
                    "- Fahrgäste an oder nahe Haltestelle/Bahnhof bleiben zugewiesen und steigen weiter ein.\n" +
                    "- Lange Zug-/U-Bahn-Bahnsteige bleiben geschützt.\n" +
                    "- Gruppen/Familien bekommen zusätzlichen Schutz.\n" +
                    "- Übersprungene Cims bleiben in der Stadt; Vanilla kann ihre Reise fortsetzen oder neu planen, oft mit einem späteren Fahrzeug.\n" +
                    "- Schiffe, Fähren und Flugzeuge behalten Vanillas Behandlung verspäteter Fahrgäste."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims laufen früher" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Zugewiesene Cims beginnen <früher zu laufen>, damit sie ihren ÖPNV noch erreichen.\n" +
                    "- Startet bis zu etwa 3 Spielminuten früher als Vanilla.\n" +
                    "- Hilft auf langen Bahnsteigen und beim Weg zur Haltestelle.\n" +
                    "- Betrifft nur Cims, die bereits einem Fahrzeug beim Fahrgastwechsel zugewiesen sind.\n" +
                    "- Teleportiert niemanden und ändert keine Abfahrtszeiten."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Gesamtnutzung" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Monatliche ÖPNV-Nutzung aus der Transport-Infoview des Spiels.\n" +
                    "Aktualisiert = Zeitpunkt dieses Status-Snapshots."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims früher" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Zählt die Cims von heute, die Better Boarding **früher laufen** ließ.\n" +
                    "Bis zu etwa 3 Spielminuten früher als Vanilla."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Bus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("Bus", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Tram" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("Tram", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Zug" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("Zug", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "U-Bahn" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("U-Bahn", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Fähre" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("Fähre", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Schiff" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("Schiff", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Flugzeug" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("Flugzeug", skipSupported: false) },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats ins Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Schreibt einmalig einen Detailbericht in **BetterBoarding.log**.\n" +
                    "Enthält Wartesummen, die 3 schlechtesten Haltestellen, Beispiele übersprungener verspäteter Fahrgäste, Entity-IDs und Linienhinweise."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Log öffnen" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Öffnet **BetterBoarding.log**, wenn die Datei existiert.\n" +
                    "Falls nicht, wird stattdessen der Logs-Ordner geöffnet."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Anzeigename des Mods." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Aktuelle Mod-Version." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Öffnet die Paradox-Mods-Seite des Autors." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Ausführliches Logging" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Nur Debug / Tests**\n" +
                    "Für normales Spielen AUS lassen.\n" +
                    "Schreibt <live>-Details in <Logs/BetterBoarding.log>, während die Stadt läuft.\n" +
                    "Kann Leistung kosten und große Logdateien erzeugen.\n" +
                    "Bei Problemen 10-15 Min. verwenden und danach wieder AUS schalten.\n" +
                    "<Stats ins Log> ist ein leichterer einmaliger Snapshot."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Status nicht geladen." },
                { WaitStatus.KeyNoCityLoaded, "Keine Stadt geladen." },
                { WaitStatus.KeyNoStopsFound, "Keine Haltestellen gefunden." },

                { WaitStatus.KeyStatusLine, "{0} wartend | Ø {1} | max {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} spät übersprungen" },
                { WaitStatus.KeyStatusSkipOff, "Skip AUS" },

                { WaitStatus.KeyStatusOverviewLine, "{0} Tour./Mon. | {1} Einw./Mon. | aktual. {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "früher laufen AUS" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Statistikbericht angefordert, aber keine Stadt ist geladen." },
                { WaitStatus.KeyReportTitle, "Stats-ins-Log-Snapshot - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Einstellungen: {0}" },
                { WaitStatus.KeyReportNote, "Der Linienhinweis stammt vom Waypoint mit der höchsten Wartezeit an dieser Haltestelle." },
                { WaitStatus.KeyReportTesterHintsHeader, "Testerhinweise" },
                { WaitStatus.KeyReportHintWorstStops, "Schlechteste Haltestellen: zuerst im Spiel oder mit Scene Explorer prüfen (Ort über Entity-ID finden). Auf Verkehr, ungünstige Lage oder fehlerhafte Haltestellen achten." },
                { WaitStatus.KeyReportHintSkippedCims, "Übersprungene verspätete Fahrgäste: Zu weit entfernte Fahrgäste wurden nach der Abfahrt übersprungen, damit das Fahrzeug nicht weiter auf sie wartet. Die Beispiele zeigen, was danach geschah." },
                { WaitStatus.KeyReportHintLateGroups, "Gruppeneinstieg: zeigt Familien/Gruppen, die beim Erstellen des Berichts noch nicht vollständig eingestiegen sind. Better Boarding gibt extra Zeit und hilft Nachzüglern beim Einsteigen." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Bediente Haltestellen: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Haltestellen mit Wartenden: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Wartende Fahrgäste: {0}" },
                { WaitStatus.KeyReportAverageWait, "Durchschnittliche Wartezeit: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Heute übersprungene verspätete Fahrgäste: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Schlechteste Haltestelle: keine, momentan wartet niemand." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Ø-Wartezeit schlechteste Haltestelle: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Name der schlechtesten Haltestelle: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entity der schlechtesten Haltestelle: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Waypoint-Entity der schlechtesten Haltestelle: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Hinweis schlechteste Linie: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entity der schlechtesten Linie: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Waypoint-Ø der schlechtesten Linie: {0} mit {1} Wartenden" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} schlechteste Haltestellen nach Ø-Wartezeit:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | Ø {2} | wartend {3} | Haltestelle {4} | Waypoint {5} | Linie {6} | Hinweis {7}" },
                { WaitStatus.KeyReportLateGroups, "Gruppeneinstieg unvollständig: {0} Fahrgäste in {1} Gruppen auf {2} Fahrzeugen" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Beispiele übersprungener verspäteter Fahrgäste" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | Fahrgast {2} | verpasstes Fahrzeug {3} | Zeit {4} | jetzt {5}" },
                { WaitStatus.KeyReportNone, "keine" },
                { WaitStatus.KeyReportUnknown, "(unbekannt)" },
            };
        }

        public void Unload()
        {
        }
    }
}
