// <copyright file="LocaleIT.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleIT.cs
// Purpose: Italian it-IT locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Italian localization source.
    /// </summary>
    public sealed class LocaleIT : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        /// <summary>
        /// Constructs the Italian locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleIT(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all Italian localization entries for this mod.
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

            const string ToggleName = "Salta passeggeri in ritardo";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Valori più alti riducono il tempo di imbarco e carico alla fermata {transitName}.\n" +
                    $"3x è il valore predefinito consigliato.\n" +
                    $"5x è il massimo.\n" +
                    $"Aiuta a smaltire più rapidamente le code normali, ma un passeggero in ritardo può ancora ritardare la partenza per il design vanilla.\n" +
                    $"Usa [✓] <{ToggleName}> se vuoi che i cim in ritardo perdano il veicolo dopo l’orario di partenza.\n" +
                    $"I cittadini in ritardo saltati non vengono eliminati; il gioco li reindirizza naturalmente.\n" +
                    "<==========================>\n" +
                    "Valore di carico:\n" +
                    "1x = 100% sosta vanilla\n" +
                    "2x = ~1/2 sosta prevista\n" +
                    "3x = ~1/3 sosta prevista (consigliato)\n" +
                    "5x = ~1/5 sosta prevista (max)\n" +
                    $"Non è la stessa cosa di <{ToggleName}>; quella casella decide se i cim in ritardo possono perdere il {shortName} dopo l’orario di partenza.";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Stato attuale {transitName}>\n" +
                    "**In attesa** = totale passeggeri in attesa ora.\n" +
                    "**Media** = tempo medio di attesa di quei passeggeri.\n" +
                    "**Peggiore** fermata = attesa media più alta a una fermata.\n" +
                    "Le fermate peggiori sono buoni posti da controllare per incidenti, fermate bloccate/buggate o bisogno di più veicoli assegnati.\n" +
                    $"**In ritardo oggi** = passeggeri soli in ritardo saltati oggi da <{ToggleName}>.\n" +
                    "Usa <Stats nel log> per un report dettagliato: nomi fermate, ID entità e altro.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "Azioni" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "Info" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "Velocità imbarco" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "Comportamento" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Stato" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Info mod" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Link" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Debug" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Velocità bus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "fermata bus",
                        "bus",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Velocità ferrovia" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "fermata treno, tram e metropolitana",
                        "veicolo",
                        "Si applica alle fermate di treno, tram e metropolitana.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "Nave + traghetto" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "fermata nave e traghetto",
                        "veicolo",
                        "Si applica alle fermate di nave e traghetto.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Velocità aereo" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "terminal aereo",
                        "aereo",
                        "Si applica ai terminal degli aerei passeggeri.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "I passeggeri in ritardo che sono ancora <non pronti> dopo l’orario di partenza possono perdere il veicolo.\n" +
                    "Nota: vengono saltati solo cittadini soli in ritardo.\n" +
                    "Gruppi/famiglie che viaggiano insieme e sono in ritardo <non vengono saltati> e possono ancora causare ritardi come in vanilla.\n" +
                    "I gruppi sono una piccola parte della folla; il beneficio principale viene dal saltare i cim soli in ritardo.\n" +
                    "I cittadini in ritardo saltati non vengono eliminati; il gioco li riassegna naturalmente."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cim corrono prima: bus + tram + treni" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "I cittadini <in ritardo> iniziano a <correre prima> per provare ad arrivare **prima** dell’orario di partenza.\n" +
                    "Aiuta a tenere bus, tram e treni in orario, soprattutto sulle banchine ferroviarie lunghe.\n" +
                    "Riguarda solo i cim già assegnati a un veicolo che sta imbarcando.\n" +
                    "Vanilla fa iniziare a correre i cim solo all’orario di partenza, cosa che può essere troppo tardi.\n" +
                    $"Funziona bene con <{ToggleName}> perché può ridurre quanti cim perdono il veicolo e devono essere riassegnati.\n" +
                    "Non modifica l’orario di partenza del veicolo, non forza l’imbarco né teletrasporta i cittadini."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Uso totale" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Uso mensile del trasporto pubblico dall’infoview Trasporti del gioco.\n" +
                    "L’ora aggiornata mostra quando è stato preso questo snapshot (di solito dopo l’ingresso nel menu Opzioni)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cim corrono prima" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Se attivo [x], conta tutti i cim (oggi) che hanno iniziato a **correre prima** per provare a prendere un bus, tram o treno prima della partenza.\n" +
                    "I cim corrono 512 frame prima rispetto a vanilla (~2-8 secondi prima in tempo reale, ~2 minuti in gioco)."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Bus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("bus") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Tram" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("tram") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Treno" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("treno") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Metropolitana" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("metropolitana") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Traghetto" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("traghetto") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Nave" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("nave") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Aereo" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("aereo") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats nel log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Scrive un report dettagliato una tantum in **BetterBoarding.log**.\n" +
                    "Include totali in attesa, top 3 peggiori fermate per modalità, esempi di cim saltati, ID entità e indizi linea."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Apri log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Apre **BetterBoarding.log** se esiste.\n" +
                    "Se il file non viene ancora trovato, apre invece la cartella Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Nome visualizzato di questa mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Versione" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Versione attuale della mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Apre la pagina Paradox Mods dell’autore." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Abilita log dettagliato" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Solo debug / test**\n" +
                    "Aggiunge dettagli <live> a <Logs/BetterBoarding.log> mentre la città è in esecuzione.\n" +
                    "**Non abilitare durante il gioco normale.**\n" +
                    "Lasciarlo attivo può ridurre le prestazioni e creare file log enormi.\n" +
                    "Puoi eliminare i vecchi file log più tardi.\n" +
                    "Nota: <Stats nel log> è un report istantaneo con i contatori di oggi per i ritardi saltati; è diverso dai log dettagliati.\n" +
                    "Esegui il log dettagliato per 15-20 min se vuoi una cronologia di ciò che è successo.\n" +
                    "Non dimenticare di riportarlo **OFF** prima del gioco normale."
                },

                // Runtime status strings
                { TransitWaitStatus.KeyStatusNotLoaded, "Stato non caricato." },
                { TransitWaitStatus.KeyNoCityLoaded, "Nessuna città caricata." },
                { TransitWaitStatus.KeyNoStopsFound, "Nessuna fermata trovata." },

                { TransitWaitStatus.KeyStatusLine, "{0} in attesa | med. {1} | peggiore {2} | {3}" },
                { TransitWaitStatus.KeyStatusLateSkipped, "{0} in ritardo oggi" },
                { TransitWaitStatus.KeyStatusSkipOff, "skip OFF" },

                { TransitWaitStatus.KeyStatusOverviewLine, "{0} turisti/mese | {1} cittadini/mese | aggiornato {2}" },
                { TransitWaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { TransitWaitStatus.KeyStatusRunSoonerOff, "corsa OFF" },

                // Stats-to-log report strings
                { TransitWaitStatus.KeyReportNoCityLoaded, "[BBoard] Report statistiche richiesto, ma nessuna città è caricata." },
                { TransitWaitStatus.KeyReportTitle, "Snapshot Stats nel log - Better Boarding" },
                { TransitWaitStatus.KeyReportSettings, "Impostazioni: {0}" },
                { TransitWaitStatus.KeyReportNote, "L’indizio linea viene dal waypoint con attesa più alta a quella fermata." },
                { TransitWaitStatus.KeyReportTesterHintsHeader, "Suggerimenti tester" },
                { TransitWaitStatus.KeyReportHintWorstStops, "Peggiori fermate: ispezionale prima in gioco o con Scene Explorer mod (trova le posizioni con ID entità). Cerca traffico, cattiva posizione della fermata o una fermata buggata." },
                { TransitWaitStatus.KeyReportHintSkippedCims, "Cim soli saltati: passeggeri in ritardo saltati per permettere al trasporto di partire. Lo stato successivo di solito dovrebbe diventare 'has path' o 'assigned'. Se resta 'no path yet', ispeziona quell’entità cim dopo più tempo." },
                { TransitWaitStatus.KeyReportHintLateGroups, "Gruppi in ritardo (famiglie): lasciati intenzionalmente a vanilla per restare insieme e seguire il comportamento vanilla; sono pochi rispetto ai molti viaggiatori singoli." },
                { TransitWaitStatus.KeyReportFamilyHeader, "{0}" },
                { TransitWaitStatus.KeyReportServedStops, "Fermate servite: {0}" },
                { TransitWaitStatus.KeyReportStopsWithWaiting, "Fermate con passeggeri in attesa: {0}" },
                { TransitWaitStatus.KeyReportWaitingPassengers, "Passeggeri in attesa: {0}" },
                { TransitWaitStatus.KeyReportAverageWait, "Attesa media: {0}" },
                { TransitWaitStatus.KeyReportLateBoardersSkipped, "Passeggeri in ritardo saltati oggi: {0}" },
                { TransitWaitStatus.KeyReportWorstStopNone, "Fermata peggiore: nessuna, nessun passeggero in attesa al momento." },
                { TransitWaitStatus.KeyReportWorstStopAverageWait, "Attesa media fermata peggiore: {0}" },
                { TransitWaitStatus.KeyReportWorstStopName, "Nome fermata peggiore: {0}" },
                { TransitWaitStatus.KeyReportWorstStopEntity, "Entità fermata peggiore: {0}" },
                { TransitWaitStatus.KeyReportWorstWaypointEntity, "Entità waypoint fermata peggiore: {0}" },
                { TransitWaitStatus.KeyReportWorstLineHint, "Indizio linea peggiore: {0}" },
                { TransitWaitStatus.KeyReportWorstLineEntity, "Entità linea peggiore: {0}" },
                { TransitWaitStatus.KeyReportWorstLineWaypointAverage, "Media waypoint linea peggiore: {0} con {1} in attesa" },
                { TransitWaitStatus.KeyReportTopWorstStopsHeader, "Top {0} fermate peggiori per attesa media:" },
                { TransitWaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | med. {2} | attesa {3} | fermata {4} | waypoint {5} | linea {6} | indizio {7}" },
                { TransitWaitStatus.KeyReportLateGroups, "Cim in ritardo che viaggiano in gruppo lasciati soli: {0} passeggeri in {1} gruppi su {2} veicoli" },
                { TransitWaitStatus.KeyReportLastSkippedSamplesHeader, "Esempi di cim soli in ritardo saltati" },
                { TransitWaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | passeggero {2} | veicolo perso {3} | ora {4} | adesso {5}" },
                { TransitWaitStatus.KeyReportNone, "nessuno" },
                { TransitWaitStatus.KeyReportUnknown, "(sconosciuto)" },
            };
        }

        public void Unload()
        {
        }
    }
}
