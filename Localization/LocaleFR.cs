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
            string title = Mod.ModName + " (Embarquement amélioré)";

            const string ToggleName = "Laisser les retardataires";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Des valeurs plus élevées accélèrent la montée/descente à {transitName}.\n" +
                    "Cela réduit le temps d’arrêt normal pendant l’échange de passagers.\n" +
                    "<==========================>\n" +
                    "1x = 100% du temps vanilla\n" +
                    "2x = ~1/2 du temps prévu\n" +
                    "3x = ~1/3 du temps prévu (recommandé)\n" +
                    "5x = ~1/5 du temps prévu (max)";
            }

            string StatusDescription(string transitName)
            {
                return
                    $"<État actuel : {transitName}>\n" +
                    "**Attente** = passagers qui attendent maintenant.\n" +
                    "**Moy.** = attente moyenne de ces passagers.\n" +
                    "**Pire** = plus forte attente moyenne à un arrêt.\n" +
                    "Si elle est élevée, vérifiez trafic, arrêt bloqué/bugué ou manque de véhicules.\n" +
                    "<Stats vers log> affiche les arrêts, IDs d'entité, passagers retardataires ignorés et plus.";
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

                // Boarding behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims courent plus tôt" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Les cims affectés commencent à <courir plus tôt> pour attraper leur transport avant son départ.\n" +
                    "- Commence jusqu’à environ 3 minutes de jeu plus tôt que vanilla.\n" +
                    "- Aide de déplacement réelle : les tests montrent que beaucoup de cims aidés finissent par monter dans le véhicule qui leur était attribué.\n" +
                    "- Utile sur les longs quais et à l’approche d’un arrêt.\n" +
                    "- Ne concerne que les cims déjà affectés à un véhicule en embarquement.\n" +
                    "- Ne téléporte pas les cims et ne change pas les heures de départ."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Filet de sécurité pour les cas rares où un véhicule attend encore après le départ un passager clairement trop loin.\n" +
                    "Better Boarding agit bien avant le délai vanilla d’environ 10 minutes pour un véhicule bloqué.\n" +
                    "- Courir plus tôt aide d'abord dans les cas courants ; cette option rattrape les retardataires encore très loin.\n" +
                    "- Les passagers à l’arrêt/gare ou à proximité restent affectés et continuent d’embarquer.\n" +
                    "- Les longs quais de train/métro restent protégés.\n" +
                    "- Les groupes/familles ont une protection supplémentaire.\n" +
                    "- Les cims laissés restent en ville ; vanilla peut poursuivre ou recalculer leur trajet, souvent sur un véhicule plus tard.\n" +
                    "- Navires, ferries et avions gardent la gestion vanilla des retardataires."
                },
                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Fréquentation" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Fréquentation mensuelle des transports publics depuis l’infovue Transport du jeu.\n" +
                    "Mis à jour = moment de cette capture d’état."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims + tôt" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Compte les vraies **aides à courir plus tôt** d'aujourd'hui par type de transport.\n" +
                    "Les tests confirment que beaucoup de cims aidés finissent par monter dans le véhicule qui leur était attribué."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusGroupFamilyAssists)), "Aides groupes/familles" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusGroupFamilyAssists)),
                    "Compte les aides de Better Boarding aux groupes/familles aujourd'hui.\n" +
                    "Inclut l'aide aux membres en retard et le traitement sûr des groupes éligibles encore très loin."
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
                    "Inclut les totaux d’attente, les 3 pires arrêts, des exemples de retardataires laissés, les Entity IDs et des indices de ligne."
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
                    "**Debug / Tests UNIQUEMENT**\n" +
                    "Laisser DÉSACTIVÉ en jeu normal.\n" +
                    "Ajoute des détails <en direct> dans <Logs/BetterBoarding.log> pendant que la ville tourne.\n" +
                    "Peut réduire les performances et créer de gros fichiers log.\n" +
                    "Utiliser 10-15 min pour diagnostiquer, puis DÉSACTIVER à nouveau.\n" +
                    "<Stats vers log> est une capture unique plus légère."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "État non chargé." },
                { WaitStatus.KeyNoCityLoaded, "Aucune ville chargée." },
                { WaitStatus.KeyNoStopsFound, "Aucun arrêt trouvé." },

                { WaitStatus.KeyStatusLine, "{0} attente | moy. {1} | pire {2}" },

                { WaitStatus.KeyStatusOverviewLine, "{0} tour./mois | {1} hab./mois | maj {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "course + tôt OFF" },
                { WaitStatus.KeyStatusGroupFamilyAssistsLine, "{0} aujourd'hui" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Rapport demandé, mais aucune ville n’est chargée." },
                { WaitStatus.KeyReportTitle, "Instantané Stats vers log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Réglages : {0}" },
                { WaitStatus.KeyReportNote, "L’indice de ligne vient du waypoint ayant la plus forte attente à cet arrêt." },
                { WaitStatus.KeyReportTesterHintsHeader, "Conseils de test" },
                { WaitStatus.KeyReportHintWorstStops, "Pires arrêts : inspectez-les d’abord en jeu ou avec Scene Explorer (localisez via l’ID d’entité). Vérifiez trafic, emplacement ou arrêt bugué." },
                { WaitStatus.KeyReportHintSkippedCims, "Retardataires laissés : ils étaient trop loin après le départ, donc Better Boarding a empêché le véhicule de continuer à les attendre. Les exemples montrent ce qui s’est passé ensuite." },
                { WaitStatus.KeyReportHintLateGroups, "Embarquement des groupes : familles/groupes encore incomplets au moment du rapport. Better Boarding leur donne un délai supplémentaire et aide les retardataires à finir d’embarquer." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Arrêts desservis : {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Arrêts avec passagers en attente : {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Passagers en attente : {0}" },
                { WaitStatus.KeyReportAverageWait, "Attente moyenne : {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Retardataires laissés aujourd’hui : {0}" },
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
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Exemples de retardataires laissés" },
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
