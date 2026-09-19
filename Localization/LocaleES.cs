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

        /// <summary>
        /// Constructs the Spanish locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleES(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all Spanish localization entries for this mod.
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

            const string ToggleName = "Omitir pasajeros tarde";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Los valores más altos reducen el tiempo de abordaje y carga en {transitName}.\n" +
                    $"3x es el valor predeterminado recomendado.\n" +
                    $"5x es el máximo.\n" +
                    $"Esto ayuda a despejar las colas normales más rápido, pero un pasajero tarde aún puede retrasar la salida por el diseño vanilla.\n" +
                    $"Usa [✓] <{ToggleName}> si quieres que los cims tarde pierdan el vehículo después de la hora de salida.\n" +
                    $"Los ciudadanos tarde omitidos no se eliminan; el juego los redirige de forma natural.\n" +
                    "<==========================>\n" +
                    "Valor de carga:\n" +
                    "1x = 100% parada vanilla\n" +
                    "2x = ~1/2 de la parada prevista\n" +
                    "3x = ~1/3 de la parada prevista (recomendado)\n" +
                    "5x = ~1/5 de la parada prevista (máx.)\n" +
                    $"Esto no es lo mismo que <{ToggleName}>; esa casilla decide si los cims tarde pueden perder el {shortName} después de la hora de salida.";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Estado actual de {transitName}>\n" +
                    "**Esperando** = total de pasajeros esperando ahora mismo.\n" +
                    "**Media** = tiempo medio de espera de esos pasajeros.\n" +
                    "**Peor** parada = mayor espera media en una parada.\n" +
                    "Las peores paradas son buenos sitios para revisar accidentes, paradas bloqueadas/bugueadas o falta de vehículos asignados.\n" +
                    $"**Tarde hoy** = pasajeros solitarios tarde omitidos hoy por <{ToggleName}>.\n" +
                    "Usa <Stats al log> para un informe detallado: nombres de paradas, IDs de entidades y más.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "Acciones" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "Acerca de" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "Velocidad de abordaje" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "Comportamiento" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Estado" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Info del mod" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Enlaces" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Depuración" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Velocidad de bus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "parada de bus",
                        "bus",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Velocidad de tren" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "parada de tren, tranvía y metro",
                        "vehículo",
                        "Se aplica a paradas de tren, tranvía y metro.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "Barco + ferry" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "parada de barco y ferry",
                        "vehículo",
                        "Se aplica a paradas de barco y ferry.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Velocidad de avión" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "terminal de avión",
                        "avión",
                        "Se aplica a terminales de aviones de pasajeros.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Los pasajeros tarde que sigan <no listos> después de la hora de salida pueden perder el vehículo.\n" +
                    "Nota: solo omitimos ciudadanos solo que llegan tarde.\n" +
                    "Los grupos/familias que viajan juntos y llegan tarde <no se omiten> y aún pueden causar retrasos como en vanilla.\n" +
                    "Los grupos son una parte pequeña de la multitud; la mayor parte del beneficio viene de omitir cims solo que llegan tarde.\n" +
                    "Los ciudadanos tarde omitidos no se eliminan; el juego los reasigna de forma natural."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims corren antes: buses + tranvías + trenes" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Los ciudadanos <tarde> empiezan a <correr antes> para intentar llegar **antes** de la hora de salida.\n" +
                    "Ayuda a mantener buses, tranvías y trenes a horario, especialmente en andenes largos.\n" +
                    "Solo afecta a cims ya asignados a un vehículo que está embarcando.\n" +
                    "Vanilla solo hace que los cims corran en la hora de salida, lo que puede ser demasiado tarde.\n" +
                    $"Funciona bien con <{ToggleName}> porque puede reducir cuántos cims pierden el vehículo y necesitan reasignarse.\n" +
                    "No cambia la hora de salida del vehículo, no fuerza el embarque ni teletransporta ciudadanos."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Uso total" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Uso mensual del transporte público desde la infovista Transporte del juego.\n" +
                    "La hora de actualización muestra cuándo se tomó este snapshot (normalmente al entrar en el menú Opciones)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims corren antes" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Si está activado [x], cuenta todos los cims (hoy) que empezaron a **correr antes** para intentar alcanzar un bus, tranvía o tren antes de la salida.\n" +
                    "Los cims corren 512 frames antes que en vanilla (~2-8 segundos antes en tiempo real, ~2 minutos en el juego)."
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
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Ferry" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("ferry") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Barco" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("barco") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Avión" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("avión") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats al log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Escribe un informe detallado puntual en **BetterBoarding.log**.\n" +
                    "Incluye totales de espera, las 3 peores paradas por modo, ejemplos de cims omitidos, IDs de entidades y pistas de línea."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Abrir log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Abre **BetterBoarding.log** si existe.\n" +
                    "Si el archivo aún no existe, abre la carpeta Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Nombre visible de este mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Versión" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Versión actual del mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Abre la página del autor en Paradox Mods." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Activar log detallado" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Solo depuración / pruebas**\n" +
                    "Añade detalles <live> a <Logs/BetterBoarding.log> mientras la ciudad está en marcha.\n" +
                    "**No lo actives para juego normal.**\n" +
                    "Dejarlo activado puede bajar el rendimiento y crear archivos log enormes.\n" +
                    "Puedes borrar los archivos log antiguos más tarde.\n" +
                    "Nota: <Stats al log> es un informe puntual con los contadores de saltos tardíos de hoy; es distinto de los logs detallados.\n" +
                    "Ejecuta el log detallado durante 15-20 min si quieres una cronología de lo ocurrido.\n" +
                    "No olvides volver a ponerlo en **OFF** antes de jugar normal."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Estado no cargado." },
                { WaitStatus.KeyNoCityLoaded, "No hay ciudad cargada." },
                { WaitStatus.KeyNoStopsFound, "No se encontraron paradas." },

                { WaitStatus.KeyStatusLine, "{0} esperando | med. {1} | peor {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} tarde hoy" },
                { WaitStatus.KeyStatusSkipOff, "skip OFF" },

                { WaitStatus.KeyStatusOverviewLine, "{0} turistas/mes | {1} ciudadanos/mes | actualizado {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "correr OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Se pidió el informe, pero no hay ninguna ciudad cargada." },
                { WaitStatus.KeyReportTitle, "Snapshot de Stats al log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Ajustes: {0}" },
                { WaitStatus.KeyReportNote, "La pista de línea viene del waypoint con mayor espera en esa parada." },
                { WaitStatus.KeyReportTesterHintsHeader, "Pistas para testers" },
                { WaitStatus.KeyReportHintWorstStops, "Peores paradas: inspecciónalas primero en el juego o con Scene Explorer mod (encuentra ubicaciones con ID de entidad). Busca tráfico, mala ubicación de parada o una parada bugueada." },
                { WaitStatus.KeyReportHintSkippedCims, "Cims solo omitidos: pasajeros tarde que omitimos para permitir que el transporte salga. Después, el estado normalmente debería ser 'has path' o 'assigned'. Si sigue en 'no path yet', inspecciona esa entidad cim tras más tiempo." },
                { WaitStatus.KeyReportHintLateGroups, "Grupos tarde (familias): dejados intencionadamente a vanilla para que permanezcan juntos y sigan el comportamiento vanilla; son pocos comparados con muchos viajeros solos." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Paradas servidas: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Paradas con pasajeros esperando: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Pasajeros esperando: {0}" },
                { WaitStatus.KeyReportAverageWait, "Espera media: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Pasajeros tarde omitidos hoy: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Peor parada: ninguna, no hay pasajeros esperando ahora." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Espera media de peor parada: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Nombre de peor parada: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entidad de peor parada: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Entidad waypoint de peor parada: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Pista de peor línea: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entidad de peor línea: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Media del waypoint de peor línea: {0} con {1} esperando" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} peores paradas por espera media:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | med. {2} | esperando {3} | parada {4} | waypoint {5} | línea {6} | pista {7}" },
                { WaitStatus.KeyReportLateGroups, "Cims tarde viajando en grupo dejados solos: {0} pasajeros en {1} grupos en {2} vehículos" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Ejemplos de cims solo tarde omitidos" },
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
