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

        /// <summary>
        /// Constructs the French locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleFR(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all French localization entries for this mod.
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

            const string ToggleName = "Ignorer passagers en retard";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Des valeurs plus élevées réduisent le temps de montée et de chargement à {transitName}.\n" +
                    $"3x est le réglage recommandé par défaut.\n" +
                    $"5x est le maximum.\n" +
                    $"Cela aide les files normales à se résorber plus vite, mais un passager en retard peut encore retarder le départ à cause du design vanilla.\n" +
                    $"Utilise [✓] <{ToggleName}> si tu veux que les cims en retard ratent le véhicule après l’heure de départ.\n" +
                    $"Les citoyens en retard ignorés ne sont pas supprimés ; le jeu les réachemine naturellement.\n" +
                    "<==========================>\n" +
                    "Valeur de chargement :\n" +
                    "1x = 100 % arrêt vanilla\n" +
                    "2x = ~1/2 de l’arrêt prévu\n" +
                    "3x = ~1/3 de l’arrêt prévu (recommandé)\n" +
                    "5x = ~1/5 de l’arrêt prévu (max)\n" +
                    $"Ce n’est pas la même chose que <{ToggleName}> ; cette case décide si les cims en retard peuvent rater le {shortName} après l’heure de départ.";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<État actuel {transitName}>\n" +
                    "**Attente** = total des passagers qui attendent maintenant.\n" +
                    "**Moy.** = temps d’attente moyen de ces passagers.\n" +
                    "**Pire** arrêt = attente moyenne la plus élevée à un arrêt.\n" +
                    "Les pires arrêts sont de bons endroits à vérifier pour les accidents, arrêts bloqués/buggés ou besoin de véhicules supplémentaires.\n" +
                    $"**En retard aujourd’hui** = passagers solo en retard ignorés aujourd’hui par <{ToggleName}>.\n" +
                    "Utilise <Stats vers log> pour un rapport détaillé : noms d’arrêts, ID d’entités et plus.";
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
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Statut" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Infos du mod" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Liens" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Debug" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Vitesse bus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "arrêt de bus",
                        "bus",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Vitesse rail" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "arrêt de train, tram et métro",
                        "véhicule",
                        "S’applique aux arrêts de train, tram et métro.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "Bateau + ferry" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "arrêt de bateau et ferry",
                        "véhicule",
                        "S’applique aux arrêts de bateau et ferry.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Vitesse avion" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "terminal d’avion",
                        "avion",
                        "S’applique aux terminaux d’avions de passagers.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Les passagers en retard qui sont encore <pas prêts> après l’heure de départ peuvent rater le véhicule.\n" +
                    "Note : seuls les citoyens solo en retard sont ignorés.\n" +
                    "Les groupes/familles qui voyagent ensemble et sont en retard ne sont <pas ignorés> et peuvent encore retarder le transport comme en vanilla.\n" +
                    "Les groupes représentent une petite partie de la foule ; la plupart du gain vient des cims solo en retard.\n" +
                    "Les citoyens en retard ignorés ne sont pas supprimés ; ils sont naturellement réaffectés par le jeu."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims courent plus tôt : bus + trams + trains" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Les citoyens <en retard> commencent à <courir plus tôt> pour essayer d’arriver **avant** l’heure de départ.\n" +
                    "Aide les bus, trams et trains à rester à l’heure, surtout sur les longs quais.\n" +
                    "N’affecte que les cims déjà assignés à un véhicule qui embarque actuellement.\n" +
                    "Vanilla ne fait courir les cims qu’à l’heure de départ, ce qui peut être trop tard.\n" +
                    $"Fonctionne bien avec <{ToggleName}> car cela peut réduire le nombre de cims qui ratent le véhicule et doivent être réaffectés.\n" +
                    "Ne modifie pas l’heure de départ du véhicule, ne force pas l’embarquement et ne téléporte pas les citoyens."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Utilisation totale" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Utilisation mensuelle des transports publics depuis l’infoview Transport du jeu.\n" +
                    "L’heure de mise à jour indique quand ce snapshot a été pris (généralement après l’ouverture du menu Options)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims courent plus tôt" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Si activé [x], compte tous les cims (aujourd’hui) qui ont commencé à **courir plus tôt** pour tenter d’attraper un bus, tram ou train avant le départ.\n" +
                    "Les cims courent 512 frames plus tôt que dans vanilla (~2-8 secondes plus tôt en temps réel, ~2 minutes en jeu)."
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
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Bateau" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("bateau") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Avion" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("avion") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats vers log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Écrit un rapport détaillé ponctuel dans **BetterBoarding.log**.\n" +
                    "Inclut les totaux d’attente, les 3 pires arrêts par mode, des exemples de cims ignorés, les ID d’entités et des indices de ligne."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Ouvrir log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Ouvre **BetterBoarding.log** s’il existe.\n" +
                    "Si le fichier n’est pas encore trouvé, ouvre le dossier Logs à la place."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Nom affiché de ce mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Version actuelle du mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Ouvre la page Paradox Mods de l’auteur." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Activer le log détaillé" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Debug / test uniquement**\n" +
                    "Ajoute des détails <live> dans <Logs/BetterBoarding.log> pendant que la ville tourne.\n" +
                    "**Ne pas activer pour une partie normale.**\n" +
                    "Le laisser activé peut réduire les performances et créer d’énormes fichiers log.\n" +
                    "Les anciens fichiers log peuvent être supprimés plus tard.\n" +
                    "Note : <Stats vers log> est un rapport instantané avec les compteurs de retards ignorés d’aujourd’hui ; c’est différent des logs détaillés.\n" +
                    "Lance le log détaillé pendant 15 à 20 min pour obtenir une chronologie de ce qui s’est passé.\n" +
                    "Ne pas oublier de le remettre **OFF** avant une partie normale."
                },

                // Runtime status strings
                { TransitWaitStatus.KeyStatusNotLoaded, "Statut non chargé." },
                { TransitWaitStatus.KeyNoCityLoaded, "Aucune ville chargée." },
                { TransitWaitStatus.KeyNoStopsFound, "Aucun arrêt trouvé." },

                { TransitWaitStatus.KeyStatusLine, "{0} attendent | moy. {1} | pire {2} | {3}" },
                { TransitWaitStatus.KeyStatusLateSkipped, "{0} retards aujourd’hui" },
                { TransitWaitStatus.KeyStatusSkipOff, "skip OFF" },

                { TransitWaitStatus.KeyStatusOverviewLine, "{0} touristes/mois | {1} citoyens/mois | màj {2}" },
                { TransitWaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { TransitWaitStatus.KeyStatusRunSoonerOff, "course OFF" },

                // Stats-to-log report strings
                { TransitWaitStatus.KeyReportNoCityLoaded, "[BBoard] Rapport demandé, mais aucune ville n’est chargée." },
                { TransitWaitStatus.KeyReportTitle, "Snapshot Stats vers log - Better Boarding" },
                { TransitWaitStatus.KeyReportSettings, "Réglages : {0}" },
                { TransitWaitStatus.KeyReportNote, "L’indice de ligne vient du waypoint avec la plus forte attente à cet arrêt." },
                { TransitWaitStatus.KeyReportTesterHintsHeader, "Astuces testeurs" },
                { TransitWaitStatus.KeyReportHintWorstStops, "Pires arrêts : inspecte-les d’abord en jeu ou avec le mod Scene Explorer (trouver les emplacements avec l’ID d’entité). Cherche trafic, mauvais emplacement d’arrêt ou arrêt buggué." },
                { TransitWaitStatus.KeyReportHintSkippedCims, "Cims solo ignorés : passagers en retard ignorés pour permettre au transport de partir. Leur état devrait généralement devenir 'has path' ou 'assigned'. S’il reste 'no path yet', inspecte cette entité cim après plus de temps." },
                { TransitWaitStatus.KeyReportHintLateGroups, "Groupes en retard (familles) : laissés exprès à vanilla pour rester ensemble et suivre le comportement vanilla ; ils sont peu nombreux comparés aux voyageurs seuls." },
                { TransitWaitStatus.KeyReportFamilyHeader, "{0}" },
                { TransitWaitStatus.KeyReportServedStops, "Arrêts desservis : {0}" },
                { TransitWaitStatus.KeyReportStopsWithWaiting, "Arrêts avec passagers en attente : {0}" },
                { TransitWaitStatus.KeyReportWaitingPassengers, "Passagers en attente : {0}" },
                { TransitWaitStatus.KeyReportAverageWait, "Attente moyenne : {0}" },
                { TransitWaitStatus.KeyReportLateBoardersSkipped, "Passagers en retard ignorés aujourd’hui : {0}" },
                { TransitWaitStatus.KeyReportWorstStopNone, "Pire arrêt : aucun, aucun passager n’attend pour le moment." },
                { TransitWaitStatus.KeyReportWorstStopAverageWait, "Attente moyenne du pire arrêt : {0}" },
                { TransitWaitStatus.KeyReportWorstStopName, "Nom du pire arrêt : {0}" },
                { TransitWaitStatus.KeyReportWorstStopEntity, "Entité du pire arrêt : {0}" },
                { TransitWaitStatus.KeyReportWorstWaypointEntity, "Entité waypoint du pire arrêt : {0}" },
                { TransitWaitStatus.KeyReportWorstLineHint, "Indice de ligne la pire : {0}" },
                { TransitWaitStatus.KeyReportWorstLineEntity, "Entité de la pire ligne : {0}" },
                { TransitWaitStatus.KeyReportWorstLineWaypointAverage, "Moyenne du waypoint de la pire ligne : {0} avec {1} en attente" },
                { TransitWaitStatus.KeyReportTopWorstStopsHeader, "Top {0} des pires arrêts par attente moyenne :" },
                { TransitWaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | moy. {2} | attente {3} | arrêt {4} | waypoint {5} | ligne {6} | indice {7}" },
                { TransitWaitStatus.KeyReportLateGroups, "Cims en retard voyageant en groupe laissés seuls : {0} passagers dans {1} groupes sur {2} véhicules" },
                { TransitWaitStatus.KeyReportLastSkippedSamplesHeader, "Exemples de cims solo en retard ignorés" },
                { TransitWaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | passager {2} | véhicule raté {3} | heure {4} | maintenant {5}" },
                { TransitWaitStatus.KeyReportNone, "aucun" },
                { TransitWaitStatus.KeyReportUnknown, "(inconnu)" },
            };
        }

        public void Unload()
        {
        }
    }
}
