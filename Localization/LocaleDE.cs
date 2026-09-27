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

        /// <summary>
        /// Constructs the German locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleDE(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all German localization entries for this mod.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName;

            const string ToggleName = "Späte Fahrgäste überspringen";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Höhere Werte verkürzen die Einstiegs- und Ladezeit an {transitName}.\n" +
                    "Normale Warteschlangen werden dadurch schneller abgebaut, aber ein verspäteter Fahrgast kann die Abfahrt durch das Vanilla-Design weiterhin verzögern.\n" +
                    $"Nutze [✓] <{ToggleName}>, wenn verspätete Cims das Fahrzeug nach der Abfahrtszeit verpassen dürfen.\n" +
                    "Übersprungene verspätete Bürger werden nicht gelöscht; Vanilla leitet sie bei Bedarf natürlich neu.\n" +
                    "<==========================>\n" +
                    "Ladewert:\n" +
                    "1x = 100% Vanilla-Aufenthalt\n" +
                    "2x = ~1/2 geplanter Aufenthalt\n" +
                    "3x = ~1/3 geplanter Aufenthalt (empfohlen)\n" +
                    "5x = ~1/5 geplanter Aufenthalt (max)\n" +
                    $"Das ist nicht dasselbe wie <{ToggleName}>; diese Checkbox entscheidet, ob verspätete Cims das {shortName} nach der Abfahrtszeit verpassen können.";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Aktueller {transitName}-Status>\n" +
                    "**Wartend** = alle Fahrgäste, die gerade warten.\n" +
                    "**Ø** = durchschnittliche Wartezeit dieser Fahrgäste.\n" +
                    "**Schlimmste** Haltestelle = höchste durchschnittliche Wartezeit an einer Haltestelle.\n" +
                    "Schlimmste Haltestellen sind gute Orte, um Unfälle, blockierte/fehlerhafte Haltestellen oder zu wenige zugewiesene Fahrzeuge zu prüfen.\n" +
                    $"**Spät heute** = heute von <{ToggleName}> übersprungene verspätete Solo-Fahrgäste.\n" +
                    "Nutze <Stats ins Log> für Details: Haltestellennamen, Entity-IDs und mehr.";
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
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Bus-Einstiegsgeschwindigkeit" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "Bushaltestellen",
                        "Bus",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Bahn-Einstiegsgeschwindigkeit" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "Zug-, Straßenbahn- und U-Bahn-Haltestellen",
                        "Fahrzeug",
                        "Gilt für Zug-, Straßenbahn- und U-Bahn-Haltestellen.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "Schiff + Fähre" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "Schiffs- und Fährhaltestellen",
                        "Fahrzeug",
                        "Gilt für Schiffs- und Fährhaltestellen.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Flugzeug-Geschwindigkeit" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "Flugzeugterminals",
                        "Flugzeug",
                        "Gilt für Passagierflugzeug-Terminals.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "<Verspätete Fahrgäste>, die nach der <Abfahrtszeit> noch <nicht bereit> sind, dürfen das Fahrzeug verpassen.\n" +
                    "- Verspätete Solo-Fahrgäste werden nach einer kurzen Schonfrist freigegeben, damit das Fahrzeug abfahren kann.\n" +
                    "- Gruppen/Familien bekommen etwas mehr Zeit. Ist der Gruppenleiter noch draußen, wird die ganze Gruppe über Vanillas Gruppenabbruch freigegeben.\n" +
                    "- Ist der Gruppenleiter bereits an Bord, lässt Better Boarding Vanilla nachhängende Kinder/Haustiere fertig einsteigen, damit ein einzelner Nachzügler das Fahrzeug nicht bis zum langen Vanilla-Timeout festhält.\n" +
                    "- Übersprungene verspätete Bürger werden nicht gelöscht; Vanilla kann ihre Reise normal fortsetzen oder sie neu routen."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims laufen früher: Bus + alle Bahnen" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Verspätete Bürger beginnen <früher zu laufen>, damit sie es **vor** der Abfahrtszeit schaffen.\n" +
                    "- Funktioniert für Busse, Straßenbahnen, Züge und U-Bahnen, besonders auf langen Bahnsteigen.\n" +
                    "- Betrifft nur Cims, die bereits einem Fahrzeug zugewiesen sind, das gerade einsteigen lässt.\n" +
                    "- Vanilla lässt Cims erst zur Abfahrtszeit laufen; dann kann es schon zu spät sein.\n" +
                    $"- Passt gut zu <{ToggleName}>, weil dadurch weniger Cims das Fahrzeug verpassen und neu zugewiesen werden müssen.\n" +
                    "- Ändert die Abfahrtszeit nicht, erzwingt kein Einsteigen und teleportiert keine Bürger."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Gesamtnutzung" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Monatliche ÖPNV-Nutzung aus der Transport-Infoview des Spiels.\n" +
                    "Die Aktualisierungszeit zeigt, wann dieser Status-Snapshot erstellt wurde (meist nach dem Öffnen des Optionenmenüs)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims laufen früher" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Wenn aktiviert [x], zählt alle Cims (heute), die **früher loslaufen**, um Bus, Straßenbahn, Zug oder U-Bahn vor der Abfahrt zu erreichen.\n" +
                    "Cims laufen 512 Frames früher als in Vanilla (~2-8 Sekunden früher in Echtzeit, ~2 Minuten im Spiel)."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Bus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("Bus") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Straßenbahn" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("Straßenbahn") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Zug" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("Zug") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "U-Bahn" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("U-Bahn") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Fähre" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("Fähre") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Schiff" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("Schiff") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Flugzeug" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("Flugzeug") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats ins Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Schreibt einen einmaligen Detailbericht in **BetterBoarding.log**.\n" +
                    "Enthält Wartesummen, die 3 schlechtesten Haltestellen je Verkehrsmittel, Beispiele übersprungener Cims, Entity-IDs und Linienhinweise."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Log öffnen" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Öffnet **BetterBoarding.log**, wenn die Datei existiert.\n" +
                    "Falls sie noch nicht existiert, wird stattdessen der Logs-Ordner geöffnet."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Anzeigename dieses Mods." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Aktuelle Mod-Version." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Öffnet die Paradox-Mods-Seite des Autors." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Ausführliches Logging aktivieren" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Nur Debug / Tests**\n" +
                    "Fügt <live>-Details zu <Logs/BetterBoarding.log> hinzu, während die Stadt läuft.\n" +
                    "**Nicht für normales Spielen aktivieren.**\n" +
                    "Aktiviert lassen kann die Leistung senken und riesige Logdateien erzeugen.\n" +
                    "Alte Logdateien können später gelöscht werden.\n" +
                    "Hinweis: <Stats ins Log> ist ein Zeitpunktbericht plus heutige Zähler für übersprungene verspätete Fahrgäste; das ist etwas anderes als das ausführliche Log.\n" +
                    "Für eine Zeitlinie der Ereignisse das ausführliche Logging 15-20 Min. laufen lassen.\n" +
                    "Vor normalem Spielen wieder **AUS** schalten."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Status nicht geladen." },
                { WaitStatus.KeyNoCityLoaded, "Keine Stadt geladen." },
                { WaitStatus.KeyNoStopsFound, "Keine Haltestellen gefunden." },

                { WaitStatus.KeyStatusLine, "{0} wartend | Ø {1} | schlimmste {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} spät heute" },
                { WaitStatus.KeyStatusSkipOff, "Skip AUS" },

                { WaitStatus.KeyStatusOverviewLine, "{0} Touristen/Monat | {1} Bürger/Monat | aktual. {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "früher laufen AUS" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Statistikbericht angefordert, aber keine Stadt ist geladen." },
                { WaitStatus.KeyReportTitle, "Stats-ins-Log-Snapshot - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Einstellungen: {0}" },
                { WaitStatus.KeyReportNote, "Der Linienhinweis stammt vom Waypoint mit der höchsten Wartezeit an dieser Haltestelle." },
                { WaitStatus.KeyReportTesterHintsHeader, "Testerhinweise" },
                { WaitStatus.KeyReportHintWorstStops, "Schlimmste Haltestellen: zuerst im Spiel oder mit Scene Explorer Mod prüfen (Ort über Entity-ID finden). Auf Verkehr, schlechte Haltestellenlage oder eine fehlerhafte Haltestelle achten." },
                { WaitStatus.KeyReportHintSkippedCims, "Übersprungene Solo-Cims: verspätete Fahrgäste, die freigegeben werden, damit das Fahrzeug abfahren kann. Später sollte der Status meist 'has path' oder 'assigned' sein. Bleibt er bei 'no path yet', diese Cim-Entity nach etwas Zeit erneut prüfen." },
                { WaitStatus.KeyReportHintLateGroups, "Späte Gruppen (Familien): Gruppen, die genau beim Erstellen des Berichts noch nicht aufgelöst sind. Better Boarding gibt ihnen kurz mehr Zeit; bleibt der Leiter draußen, wird die Gruppe freigegeben, ist der Leiter schon an Bord, hilft Vanilla den Nachzüglern beim Fertig-Einsteigen." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Bediente Haltestellen: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Haltestellen mit wartenden Fahrgästen: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Wartende Fahrgäste: {0}" },
                { WaitStatus.KeyReportAverageWait, "Durchschnittliche Wartezeit: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Heute übersprungene verspätete Fahrgäste: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Schlechteste Haltestelle: keine, momentan warten keine Fahrgäste." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Ø-Wartezeit schlechteste Haltestelle: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Name der schlechtesten Haltestelle: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entity der schlechtesten Haltestelle: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Waypoint-Entity der schlechtesten Haltestelle: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Hinweis schlechteste Linie: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entity der schlechtesten Linie: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Waypoint-Ø der schlechtesten Linie: {0} mit {1} wartend" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} schlechteste Haltestellen nach Ø-Wartezeit:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | Ø {2} | wartend {3} | Haltestelle {4} | Waypoint {5} | Linie {6} | Hinweis {7}" },
                { WaitStatus.KeyReportLateGroups, "Späte Gruppen-Cims jetzt noch ungelöst: {0} Fahrgäste in {1} Gruppen auf {2} Fahrzeugen" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Beispiele übersprungener verspäteter Solo-Cims" },
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
