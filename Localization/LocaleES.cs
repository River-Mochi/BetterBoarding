// <copyright file="LocaleES.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleES.cs
// Purpose: Spanish es-ES locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Spanish localization source.
    /// </summary>
    public sealed class LocaleES : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        public LocaleES(BBoardSettings setting)
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

            const string ToggleName = "Omitir pasajeros retrasados";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Los valores más altos aceleran la subida y bajada en {transitName}.\n" +
                    "Reduce el tiempo normal de parada mientras suben y bajan pasajeros.\n" +
                    "<==========================>\n" +
                    "Valor de carga:\n" +
                    "1x = 100% de la parada vanilla\n" +
                    "2x = ~1/2 de la parada prevista\n" +
                    "3x = ~1/3 de la parada prevista (recomendado)\n" +
                    "5x = ~1/5 de la parada prevista (máx.)";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Estado actual: {transitName}>\n" +
                    "**Esperando** = pasajeros que esperan ahora.\n" +
                    "**Prom.** = espera media de esos pasajeros.\n" +
                    "**Peor** = mayor espera media en una parada.\n" +
                    "Si es alta, revisa tráfico, paradas bloqueadas/con fallos o falta de vehículos.\n" +
                    $"**Liberados hoy** = asignaciones retrasadas liberadas por <{ToggleName}>.\n" +
                    "<Stats al log> muestra paradas, IDs de entidades y más detalles.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "Acciones" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "Acerca de" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "Velocidad de embarque" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "Comportamiento" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Estado" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Info del mod" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Enlaces" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Depuración" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Embarque de autobús" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "paradas de autobús",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Embarque ferroviario" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "paradas de tren, tranvía y metro",
                        "Se aplica a trenes, tranvías y metros.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Embarque de avión" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "terminales de avión",
                        "Se aplica a terminales de pasajeros.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Ayuda a que autobuses, tranvías, trenes y metros salgan cuando un pasajero asignado está claramente demasiado lejos para subir.\n" +
                    "- Quien esté en la parada/estación o cerca sigue asignado y puede subir.\n" +
                    "- Los pasajeros en andenes largos de tren y metro quedan protegidos.\n" +
                    "- Quien esté claramente lejos puede liberarse tras la hora de salida y un breve margen.\n" +
                    "- Los grupos/familias tienen protección extra para que niños y mascotas terminen de subir.\n" +
                    "- Barcos, ferris y aviones mantienen la gestión vanilla de pasajeros retrasados.\n" +
                    "Los cims liberados siguen en la ciudad y continúan o recalculan su viaje normalmente."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims corren antes" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Los cims ya asignados empiezan a <correr antes> para alcanzar un autobús, tranvía, tren o metro antes de que salga.\n" +
                    "- Ayuda en andenes largos y al acercarse a una parada.\n" +
                    "- Empieza hasta unos 3 minutos de juego antes que vanilla.\n" +
                    "- Solo afecta a cims ya asignados a un vehículo que está embarcando.\n" +
                    "- No teletransporta, fuerza el embarque ni cambia la hora de salida."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Uso total" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Uso mensual del transporte público según la vista de Transporte del juego.\n" +
                    "La hora de actualización indica cuándo se tomó este estado (normalmente al abrir Opciones)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims corren antes" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Si está activo [x], cuenta los cims de hoy que Better Boarding hizo **correr antes** para alcanzar bus, tranvía, tren o metro.\n" +
                    "Run Sooner empieza hasta unos 3 minutos de juego antes que vanilla."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Bus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("bus") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Tranvía" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("tranvía") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Tren" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("tren") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Metro" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("metro") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Ferri" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("ferri") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Barco" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("barco") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Avión" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("avión") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats al log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Escribe un informe detallado puntual en **BetterBoarding.log**.\n" +
                    "Incluye totales de espera, las 3 peores paradas por modo, ejemplos de pasajeros liberados, IDs de entidades y pistas de línea."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Abrir log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Abre **BetterBoarding.log** si existe.\n" +
                    "Si aún no existe, abre la carpeta Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Nombre visible del mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Versión" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Versión actual del mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Abre la página del autor en Paradox Mods." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Activar log detallado" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Solo depuración / pruebas**\n" +
                    "Déjalo normalmente en **OFF**.\n" +
                    "Añade detalles <live> a <Logs/BetterBoarding.log> mientras corre la ciudad.\n" +
                    "**No lo uses en juego normal.**\n" +
                    "Puede reducir el rendimiento y crear logs muy grandes.\n" +
                    "Puedes borrar logs antiguos después.\n" +
                    "<Stats al log> escribe solo una instantánea y tiene poco impacto comparado con el log detallado.\n" +
                    "Actívalo 10-15 min si necesitas una cronología; genera muchos datos.\n" +
                    "Vuelve a ponerlo en **OFF** antes de jugar normalmente."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Estado no cargado." },
                { WaitStatus.KeyNoCityLoaded, "No hay ciudad cargada." },
                { WaitStatus.KeyNoStopsFound, "No se encontraron paradas." },

                { WaitStatus.KeyStatusLine, "{0} esperan | prom. {1} | peor {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} liberados hoy" },
                { WaitStatus.KeyStatusSkipOff, "omitir OFF" },

                { WaitStatus.KeyStatusOverviewLine, "{0} tur./mes | {1} ciud./mes | act. {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "correr antes OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Se pidió el informe, pero no hay ninguna ciudad cargada." },
                { WaitStatus.KeyReportTitle, "Instantánea de Stats al log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Ajustes: {0}" },
                { WaitStatus.KeyReportNote, "La pista de línea viene del waypoint con mayor espera en esa parada." },
                { WaitStatus.KeyReportTesterHintsHeader, "Pistas para pruebas" },
                { WaitStatus.KeyReportHintWorstStops, "Peores paradas: revísalas primero en el juego o con Scene Explorer (busca la ubicación por ID de entidad). Comprueba tráfico, mala ubicación o una parada con fallos." },
                { WaitStatus.KeyReportHintSkippedCims, "Asignaciones individuales liberadas: cada cuenta es una asignación de vehículo liberada tras comprobar la distancia. Usa los ejemplos de seguimiento para ver qué ocurrió después." },
                { WaitStatus.KeyReportHintLateGroups, "Embarque de grupos: muestra familias/grupos que aún no habían terminado de subir al tomar el informe. Better Boarding les da margen extra y ayuda a los rezagados." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Paradas servidas: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Paradas con pasajeros esperando: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Pasajeros esperando: {0}" },
                { WaitStatus.KeyReportAverageWait, "Espera media: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Asignaciones individuales liberadas hoy: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Peor parada: ninguna; ahora no hay pasajeros esperando." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Espera media de la peor parada: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Nombre de la peor parada: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entidad de la peor parada: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Entidad waypoint de la peor parada: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Pista de la peor línea: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entidad de la peor línea: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Media del waypoint de la peor línea: {0} con {1} esperando" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} peores paradas por espera media:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | med. {2} | esperan {3} | parada {4} | waypoint {5} | línea {6} | pista {7}" },
                { WaitStatus.KeyReportLateGroups, "Embarque de grupos incompleto: {0} pasajeros en {1} grupos y {2} vehículos" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Ejemplos de pasajeros individuales liberados" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | pasajero {2} | vehículo perdido {3} | hora {4} | ahora {5}" },
                { WaitStatus.KeyReportNone, "ninguno" },
                { WaitStatus.KeyReportUnknown, "(desconocido)" },
            };
        }

        public void Unload()
        {
        }
    }
}
