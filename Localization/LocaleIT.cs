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

        public LocaleIT(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName + " (Imbarco migliorato)";

            const string ToggleName = "Salta passeggeri in ritardo";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Valori più alti rendono più rapidi salita e discesa a {transitName}.\n" +
                    "Riduce il normale tempo di fermata durante lo scambio passeggeri.\n" +
                    "<==========================>\n" +
                    "1x = 100% sosta vanilla\n" +
                    "2x = ~1/2 sosta prevista\n" +
                    "3x = ~1/3 sosta prevista (consigliato)\n" +
                    "5x = ~1/5 sosta prevista (max)";
            }

            string StatusDescription(string transitName)
            {
                return
                    $"<Stato attuale: {transitName}>\n" +
                    "**In attesa** = passeggeri in attesa adesso.\n" +
                    "**Media** = attesa media di quei passeggeri.\n" +
                    "**Peggiore** = attesa media più alta a una fermata.\n" +
                    "Se è alta, controlla traffico, fermate bloccate/buggate o pochi veicoli.\n" +
                    "<Stats nel log> mostra fermate, ID entità, passeggeri in ritardo saltati e altro.";
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
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Imbarco autobus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "fermate autobus",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Imbarco ferroviario" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "fermate treno, tram e metro",
                        "Vale per treni, tram e metro.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Imbarco aereo" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "terminal aerei",
                        "Vale per i terminal passeggeri degli aerei.\n")
                },

                // Boarding behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims corrono prima" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "I cims assegnati iniziano a <correre prima> per prendere il trasporto prima che parta.\n" +
                    "- Inizia fino a circa 3 minuti di gioco prima di vanilla.\n" +
                    "- Aiuto di movimento reale: i test mostrano che molti cim aiutati riescono poi a salire sullo stesso veicolo assegnato.\n" +
                    "- Aiuta sulle banchine lunghe e mentre si avvicinano alla fermata.\n" +
                    "- Vale solo per cims già assegnati a un veicolo che sta imbarcando.\n" +
                    "- Non teletrasporta i cims e non cambia gli orari di partenza."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Rete di sicurezza per i casi insoliti in cui il trasporto continua ad aspettare dopo la partenza un passeggero chiaramente lontano.\n" +
                    "Better Boarding interviene molto prima del limite vanilla di ~10 minuti per un veicolo bloccato.\n" +
                    "- Cims Run Sooner aiuta prima nei casi normali; questa opzione gestisce i ritardatari che restano ancora troppo lontani.\n" +
                    "- Chi è alla fermata/stazione o vicino resta assegnato e continua a salire.\n" +
                    "- Le lunghe banchine di treno/metro restano protette.\n" +
                    "- Gruppi/famiglie hanno protezione extra.\n" +
                    "- I cims saltati restano in città; vanilla può continuare o ricalcolare il viaggio, spesso su un veicolo successivo.\n" +
                    "- Navi, traghetti e aerei mantengono la gestione vanilla dei passeggeri in ritardo."
                },
                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Uso totale" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Uso mensile del trasporto pubblico dalla vista Trasporti del gioco.\n" +
                    "Aggiornato = momento in cui è stata acquisita questa istantanea."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims prima" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Conta gli **aiuti a correre prima** di oggi per tipo di trasporto.\n" +
                    "I test confermano che molti cim aiutati riescono poi a salire sullo stesso veicolo assegnato."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusGroupFamilyAssists)), "Aiuti a gruppi/famiglie" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusGroupFamilyAssists)),
                    "Conta gli aiuti di Better Boarding a gruppi/famiglie di oggi.\n" +
                    "Include aiutare i membri in ritardo a salire e gestire in sicurezza i gruppi idonei ancora molto lontani."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Autobus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("autobus") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Tram" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("tram") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Treno" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("treno") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Metro" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("metro") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Traghetto" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("traghetto") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Nave" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("nave") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Aereo" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("aereo") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats nel log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Scrive un rapporto dettagliato una tantum in **BetterBoarding.log**.\n" +
                    "Include totali in attesa, le 3 fermate peggiori, esempi di passeggeri in ritardo saltati, Entity ID e indicazioni di linea."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Apri log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Apre **BetterBoarding.log** se esiste.\n" +
                    "Se non esiste ancora, apre la cartella Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Nome visualizzato del mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Versione" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Versione attuale del mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Apre la pagina Paradox Mods dell’autore." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Log dettagliato" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**SOLO Debug / Test**\n" +
                    "Tieni DISATTIVATO durante il gioco normale.\n" +
                    "Aggiunge dettagli <live> a <Logs/BetterBoarding.log> mentre la città gira.\n" +
                    "Può ridurre le prestazioni e creare file di log grandi.\n" +
                    "Usalo per 10-15 min durante la diagnosi, poi DISATTIVALO di nuovo.\n" +
                    "<Stats nel log> è un’istantanea singola più leggera."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Stato non caricato." },
                { WaitStatus.KeyNoCityLoaded, "Nessuna città caricata." },
                { WaitStatus.KeyNoStopsFound, "Nessuna fermata trovata." },

                { WaitStatus.KeyStatusLine, "{0} attesa | med. {1} | peggio {2}" },

                { WaitStatus.KeyStatusOverviewLine, "{0} tur./mese | {1} citt./mese | agg. {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "corri prima OFF" },
                { WaitStatus.KeyStatusGroupFamilyAssistsLine, "{0} oggi" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Rapporto richiesto, ma non è caricata alcuna città." },
                { WaitStatus.KeyReportTitle, "Istantanea Stats nel log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Impostazioni: {0}" },
                { WaitStatus.KeyReportNote, "Il riferimento linea viene dal waypoint con l’attesa più alta a quella fermata." },
                { WaitStatus.KeyReportTesterHintsHeader, "Suggerimenti test" },
                { WaitStatus.KeyReportHintWorstStops, "Fermate peggiori: controllale prima nel gioco o con Scene Explorer (trova la posizione tramite ID entità). Verifica traffico, posizione o fermata buggata." },
                { WaitStatus.KeyReportHintSkippedCims, "Passeggeri in ritardo saltati: erano troppo lontani dopo la partenza, quindi Better Boarding ha impedito al veicolo di continuare ad aspettarli. Gli esempi mostrano cosa è successo dopo." },
                { WaitStatus.KeyReportHintLateGroups, "Imbarco gruppi: mostra famiglie/gruppi ancora incompleti quando viene creato il rapporto. Better Boarding concede più margine e aiuta i ritardatari a finire la salita." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Fermate servite: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Fermate con passeggeri in attesa: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Passeggeri in attesa: {0}" },
                { WaitStatus.KeyReportAverageWait, "Attesa media: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Passeggeri in ritardo saltati oggi: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Fermata peggiore: nessuna, al momento non c’è nessuno in attesa." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Attesa media fermata peggiore: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Nome fermata peggiore: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entità fermata peggiore: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Entità waypoint fermata peggiore: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Riferimento linea peggiore: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entità linea peggiore: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Media waypoint linea peggiore: {0} con {1} in attesa" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} fermate peggiori per attesa media:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | med. {2} | attesa {3} | fermata {4} | waypoint {5} | linea {6} | rif. {7}" },
                { WaitStatus.KeyReportLateGroups, "Imbarco gruppi incompleto: {0} passeggeri in {1} gruppi su {2} veicoli" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Esempi di passeggeri in ritardo saltati" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | passeggero {2} | veicolo perso {3} | ora {4} | adesso {5}" },
                { WaitStatus.KeyReportNone, "nessuno" },
                { WaitStatus.KeyReportUnknown, "(sconosciuto)" },
            };
        }

        public void Unload()
        {
        }
    }
}
