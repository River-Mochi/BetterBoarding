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

            if (!string.IsNullOrEmpty(Mod.ModVersion))
            {
                title = title + " (" + Mod.ModVersion + ")";
            }

            const string ToggleName = "Späte Fahrgäste überspringen";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Höhere Werte verkürzen die Einstiegs- und Ladezeit an {transitName}.\n" +
                    $"3x ist die empfohlene Standardeinstellung.\n" +
                    $"5x ist das Maximum.\n" +
                    $"Normale Warteschlangen werden schneller abgebaut, aber ein verspäteter Fahrgast kann die Abfahrt durch das Vanilla-Design weiterhin verzögern.\n" +
                    $"Nutze [✓] <{ToggleName}>, wenn verspätete Cims das Fahrzeug nach der Abfahrtszeit verpassen dürfen.\n" +
                    $"Übersprungene verspätete Bürger werden nicht gelöscht; Vanilla leitet sie natürlich um.\n" +
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
                    "**Wartend** = Gesamtzahl der gerade wartenden Fahrgäste.\n" +
                    "**Ø** = durchschnittliche Wartezeit dieser Fahrgäste.\n" +
                    "**Schlechteste** Haltestelle = höchste durchschnittliche Wartezeit an einer Haltestelle.\n" +
                    "Schlechteste Haltestellen sind gute Orte zur Prüfung auf Unfälle, blockierte/verbuggte Haltestellen oder zu wenige zugewiesene Fahrzeuge.\n" +
                    $"**Spät heute** = heute von <{ToggleName}> übersprungene verspätete Solo-Fahrgäste.\n" +
                    "Nutze <Stats ins Log> für einen Detailbericht: Haltestellennamen, Entity-IDs und mehr.";
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
                        "Bushaltestelle",
                        "Bus",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Bahn-Einstiegsgeschwindigkeit" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "Zug-, Straßenbahn- und U-Bahn-Haltestelle",
                        "Fahrzeug",
                        "Gilt für Zug-, Straßenbahn- und U-Bahn-Haltestellen.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "Schiff + Fähre" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "Schiff- und Fährhaltestelle",
                        "Fahrzeug",
                        "Gilt für Schiff- und Fährhaltestellen.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Flugzeug-Geschwindigkeit" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "Flugzeugterminal",
                        "Flugzeug",
                        "Gilt für Passagierflugzeug-Terminals.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Verspätete Fahrgäste, die nach der Abfahrtszeit noch <nicht bereit> sind, dürfen das Fahrzeug verpassen.\n" +
                    "Hinweis: Es werden nur verspätete Solo-Bürger übersprungen.\n" +
                    "Zusammen reisende Gruppen/Familien, die verspätet sind, werden <nicht übersprungen> und können wie in Vanilla weiterhin Verzögerungen verursachen.\n" +
                    "Gruppen sind nur ein kleiner Teil der Menge; der meiste Nutzen kommt vom Überspringen verspäteter Solo-Cims.\n" +
                    "Übersprungene verspätete Bürger werden nicht gelöscht; das Spiel weist sie natürlich neu zu."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims laufen früher: Busse + Trams + Züge" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Bürger, die <spät> sind, beginnen <früher zu laufen>, um es **vor** der Abfahrtszeit zu schaffen.\n" +
                    "Hilft Bussen, Trams und Zügen, im Zeitplan zu bleiben, besonders an langen Bahnsteigen.\n" +
                    "Betrifft nur Cims, die bereits einem Fahrzeug zugewiesen sind, das gerade einsteigen lässt.\n" +
                    "Vanilla lässt Cims erst zur Abfahrtszeit laufen, was zu spät sein kann.\n" +
                    $"Passt gut zu <{ToggleName}>, weil es reduzieren kann, wie viele Cims das Fahrzeug verpassen und neu zugewiesen werden müssen.\n" +
                    "Ändert die Abfahrtszeit des Fahrzeugs nicht, erzwingt kein Einsteigen und teleportiert keine Bürger."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Gesamtnutzung" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Monatliche ÖPNV-Nutzung aus der Transport-Infoview des Spiels.\n" +
                    "Die Aktualisierungszeit zeigt, wann dieser Status-Snapshot erstellt wurde (meist nach dem Öffnen des Optionenmenüs)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims laufen früher" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Wenn aktiviert [x], zählt alle Cims (heute), die **früher laufen**, um Bus, Tram oder Zug vor der Abfahrt zu erreichen.\n" +
                    "Cims laufen 512 Frames früher als in Vanilla (~2-8 Sekunden früher in Echtzeit, ~2 Minuten im Spiel)."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Bus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("bus") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Straßenbahn" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("straßenbahn") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Zug" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("zug") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "U-Bahn" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("u-bahn") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Fähre" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("fähre") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Schiff" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("schiff") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Flugzeug" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("flugzeug") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats ins Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Schreibt einen einmaligen Detailbericht in **BetterBoarding.log**.\n" +
                    "Enthält Wartesummen, die Top 3 schlechtesten Haltestellen je Modus, Beispiele übersprungener Cims, Entity-IDs und Linienhinweise."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Log öffnen" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Öffnet **BetterBoarding.log**, wenn die Datei existiert.\n" +
                    "Falls die Datei noch nicht gefunden wird, wird stattdessen der Logs-Ordner geöffnet."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Anzeigename dieses Mods." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Aktuelle Mod-Version." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Öffnet die Paradox-Mods-Seite des Autors." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Ausführliches Logging aktivieren" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Nur Debug / Tests**\n" +
                    "Fügt <live>-Details zu <Logs/BetterBoarding.log> hinzu, während die Stadt läuft.\n" +
                    "**Nicht für normales Spielen aktivieren.**\n" +
                    "Aktiviert lassen kann die Leistung senken und riesige Logdateien erzeugen.\n" +
                    "Alte Logdateien können später gelöscht werden.\n" +
                    "Hinweis: <Stats ins Log> ist ein Zeitpunktbericht plus heutige Zähler für späte Sprünge; das ist etwas anderes als ausführliche Logs.\n" +
                    "Ausführliches Logging 15-20 Min. laufen lassen, wenn eine Zeitlinie der Ereignisse gebraucht wird.\n" +
                    "Danach vor normalem Spielen wieder **OFF** schalten."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Status nicht geladen." },
                { WaitStatus.KeyNoCityLoaded, "Keine Stadt geladen." },
                { WaitStatus.KeyNoStopsFound, "Keine Haltestellen gefunden." },

                { WaitStatus.KeyStatusLine, "{0} wartend | Ø {1} | schlimmste {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} spät heute" },
                { WaitStatus.KeyStatusSkipOff, "Skip AUS" },

                { WaitStatus.KeyStatusOverviewLine, "{0} Touristen/Monat | {1} Bürger/Monat | aktualisiert {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "früher laufen OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Statistikbericht angefordert, aber keine Stadt ist geladen." },
                { WaitStatus.KeyReportTitle, "Stats-ins-Log-Snapshot - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Einstellungen: {0}" },
                { WaitStatus.KeyReportNote, "Der Linienhinweis stammt vom Waypoint mit der höchsten Wartezeit an dieser Haltestelle." },
                { WaitStatus.KeyReportTesterHintsHeader, "Testerhinweise" },
                { WaitStatus.KeyReportHintWorstStops, "Schlimmste Halte: zuerst im Spiel oder mit Scene Explorer Mod prüfen (Orte über Entity-ID finden). Auf Verkehr, schlechte Haltestellenlage oder fehlerhafte Halte achten." },
                { WaitStatus.KeyReportHintSkippedCims, "Übersprungene Solo-Cims: verspätete Fahrgäste, die übersprungen werden, damit der Verkehr abfahren kann. Später sollte der Status meist 'has path' oder 'assigned' werden. Bleibt er bei 'no path yet', diese Cim-Entity nach mehr Zeit prüfen." },
                { WaitStatus.KeyReportHintLateGroups, "Späte Gruppen (Familien): absichtlich vanilla überlassen, damit sie zusammen bleiben und vanilla Verhalten folgen; sie sind wenige im Vergleich zu vielen Einzelreisenden." },
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
                { WaitStatus.KeyReportLateGroups, "Späte Cims in Gruppenreise in Ruhe gelassen: {0} Passagiere in {1} Gruppen auf {2} Fahrzeugen" },
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
