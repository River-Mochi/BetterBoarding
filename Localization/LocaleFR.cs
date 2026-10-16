// <copyright file="LocaleFR.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleFR.cs
// Purpose: French fr-FR locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// French localization source.
    /// </summary>
    public sealed class LocaleFR : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        public LocaleFR(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName;

            if (!string.IsNullOrEmpty(Mod.ModVersion))
            {
                title = title + " (" + Mod.ModVersion + ")";
            }

            const string ToggleName = "Laisser les retardataires";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Des valeurs plus élevées accélèrent la montée/descente à {transitName}.\n" +
                    "Cela réduit le temps d’arrêt normal pendant l’échange de passagers.\n" +
                    "<==========================>\n" +
                    "Valeur de chargement :\n" +
                    "1x = 100% du temps vanilla\n" +
                    "2x = ~1/2 du temps prévu\n" +
                    "3x = ~1/3 du temps prévu (recommandé)\n" +
                    "5x = ~1/5 du temps prévu (max)";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<État actuel : {transitName}>\n" +
                    "**Attente** = passagers qui attendent maintenant.\n" +
                    "**Moy.** = attente moyenne de ces passagers.\n" +
                    "**Pire** = plus forte attente moyenne à un arrêt.\n" +
                    "Si elle est élevée, vérifiez trafic, arrêt bloqué/bugué ou manque de véhicules.\n" +
                    $"**Libérés aujourd’hui** = affectations en retard libérées par <{ToggleName}>.\n" +
                    "<Stats vers log> donne les arrêts, IDs d’entité et plus de détails.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "Actions" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "À propos" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "Vitesse d’embarquement" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "Comportement" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "État" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Infos du mod" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Liens" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Debug" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Embarquement bus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "arrêts de bus",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Embarquement rail" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "arrêts de train, tram et métro",
                        "Pour trains, trams et métros.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Embarquement avion" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "terminaux d’avion",
                        "Pour les terminaux passagers des avions.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Aide bus, trams, trains et métros à partir lorsqu’un passager affecté est clairement trop loin pour embarquer.\n" +
                    "- Les passagers à l’arrêt/gare ou à proximité restent affectés et continuent d’embarquer.\n" +
                    "- Les passagers sur les longs quais de train et métro restent protégés.\n" +
                    "- Les passagers clairement éloignés peuvent être libérés après l’heure de départ et un court délai.\n" +
                    "- Les groupes/familles sont mieux protégés pour laisser enfants et animaux finir d’embarquer.\n" +
                    "- Navires, ferries et avions gardent la gestion vanilla des retardataires.\n" +
                    "Les cims libérés restent en ville et poursuivent ou recalculent normalement leur trajet."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims courent plus tôt" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Les cims déjà affectés commencent à <courir plus tôt> pour attraper bus, tram, train ou métro avant son départ.\n" +
                    "- Utile sur les longs quais et à l’approche d’un arrêt.\n" +
                    "- Commence jusqu’à environ 3 minutes de jeu plus tôt que vanilla.\n" +
                    "- Ne concerne que les cims déjà affectés à un véhicule en embarquement.\n" +
                    "- Aucun téléport, embarquement forcé ni changement d’heure de départ."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Fréquentation" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Fréquentation mensuelle des transports publics depuis l’infovue Transport du jeu.\n" +
                    "L’heure de mise à jour indique quand cet état a été relevé (souvent en ouvrant Options)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims + tôt" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Si activé [x], compte les cims du jour que Better Boarding a fait **courir plus tôt** pour attraper bus, tram, train ou métro.\n" +
                    "Run Sooner commence jusqu’à environ 3 minutes de jeu avant vanilla."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Bus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("bus") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Tram" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("tram") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Train" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("train") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Métro" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("métro") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Ferry" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("ferry") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Navire" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("navire") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Avion" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("avion") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats vers log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Écrit un rapport détaillé ponctuel dans **BetterBoarding.log**.\n" +
                    "Inclut attentes, 3 pires arrêts par mode, exemples de passagers libérés, IDs d’entité et indices de ligne."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Ouvrir le log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Ouvre **BetterBoarding.log** s’il existe.\n" +
                    "Sinon ouvre le dossier Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Nom affiché du mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Version actuelle du mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Ouvre la page Paradox Mods de l’auteur." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Log détaillé" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Debug / tests uniquement**\n" +
                    "Laissez normalement **OFF**.\n" +
                    "Ajoute des détails <live> à <Logs/BetterBoarding.log> pendant la simulation.\n" +
                    "**Ne pas activer en jeu normal.**\n" +
                    "Peut réduire les performances et créer de très gros logs.\n" +
                    "Les anciens logs peuvent être supprimés plus tard.\n" +
                    "<Stats vers log> écrit un seul instantané et a peu d’impact face au log détaillé.\n" +
                    "Activez 10-15 min si vous avez besoin d’une chronologie ; cela produit beaucoup de données.\n" +
                    "Remettez **OFF** avant de jouer normalement."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "État non chargé." },
                { WaitStatus.KeyNoCityLoaded, "Aucune ville chargée." },
                { WaitStatus.KeyNoStopsFound, "Aucun arrêt trouvé." },

                { WaitStatus.KeyStatusLine, "{0} attente | moy. {1} | pire {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} libérés auj." },
                { WaitStatus.KeyStatusSkipOff, "retard OFF" },

                { WaitStatus.KeyStatusOverviewLine, "{0} tour./mois | {1} hab./mois | maj {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "course + tôt OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Rapport demandé, mais aucune ville n’est chargée." },
                { WaitStatus.KeyReportTitle, "Instantané Stats vers log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Réglages : {0}" },
                { WaitStatus.KeyReportNote, "L’indice de ligne vient du waypoint ayant la plus forte attente à cet arrêt." },
                { WaitStatus.KeyReportTesterHintsHeader, "Conseils de test" },
                { WaitStatus.KeyReportHintWorstStops, "Pires arrêts : inspectez-les d’abord en jeu ou avec Scene Explorer (localisez via l’ID d’entité). Vérifiez trafic, emplacement ou arrêt bugué." },
                { WaitStatus.KeyReportHintSkippedCims, "Affectations solo libérées : chaque compte correspond à une affectation de véhicule libérée après contrôle de distance. Les exemples de suivi montrent la suite." },
                { WaitStatus.KeyReportHintLateGroups, "Embarquement des groupes : familles/groupes encore incomplets au moment du rapport. Better Boarding leur donne un délai supplémentaire et aide les retardataires à finir d’embarquer." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Arrêts desservis : {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Arrêts avec passagers en attente : {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Passagers en attente : {0}" },
                { WaitStatus.KeyReportAverageWait, "Attente moyenne : {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Affectations solo libérées aujourd’hui : {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Pire arrêt : aucun, personne n’attend actuellement." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Attente moy. du pire arrêt : {0}" },
                { WaitStatus.KeyReportWorstStopName, "Nom du pire arrêt : {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entité du pire arrêt : {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Entité waypoint du pire arrêt : {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Indice de la pire ligne : {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entité de la pire ligne : {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Moy. waypoint de la pire ligne : {0} avec {1} en attente" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} des pires arrêts par attente moyenne :" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | moy. {2} | attente {3} | arrêt {4} | waypoint {5} | ligne {6} | indice {7}" },
                { WaitStatus.KeyReportLateGroups, "Embarquement groupe incomplet : {0} passagers dans {1} groupes sur {2} véhicules" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Exemples de passagers solo libérés" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | passager {2} | véhicule manqué {3} | heure {4} | maintenant {5}" },
                { WaitStatus.KeyReportNone, "aucun" },
                { WaitStatus.KeyReportUnknown, "(inconnu)" },
            };
        }

        public void Unload()
        {
        }
    }
}
