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

            const string ToggleName = "Omitir pasajeros atrasados";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Los valores más altos reducen el tiempo de embarque y carga en {transitName}.\n" +
                    "Esto ayuda a despejar las colas normales más rápido, pero un pasajero atrasado todavía puede demorar la salida por el mecanismo del juego.\n" +
                    $"Usa [✓] <{ToggleName}> si quieres que los cims que llegan tarde puedan perder el vehículo después de la hora de salida.\n" +
                    "Los ciudadanos omitidos por llegar tarde no se eliminan; el juego los redirige de forma natural.\n" +
                    "<==========================>\n" +
                    "Valor de carga:\n" +
                    "1x = 100% de la parada original\n" +
                    "2x = ~1/2 de la parada prevista\n" +
                    "3x = ~1/3 de la parada prevista (recomendado)\n" +
                    "5x = ~1/5 de la parada prevista (máx.)\n" +
                    $"No es lo mismo que <{ToggleName}>; esa casilla decide si los cims que llegan tarde pueden perder el {shortName} después de la hora de salida.";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Estado actual de {transitName}>\n" +
                    "**Esperando** = total de pasajeros esperando ahora.\n" +
                    "**Promedio** = tiempo promedio de espera de esos pasajeros.\n" +
                    "**Peor** parada = mayor espera promedio en una parada.\n" +
                    "Las peores paradas son buenos sitios para revisar accidentes, paradas bloqueadas/con errores o falta de vehículos asignados.\n" +
                    $"**Atrasados hoy** = pasajeros que viajaban solos y se omitieron hoy con <{ToggleName}>.\n" +
                    "Usa <Stats al log> para ver el informe detallado: nombres de paradas, IDs de entidades y más.";
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
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Velocidad de autobuses" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "paradas de autobuses",
                        "autobús",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Velocidad ferroviaria" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "paradas de tren, tranvía y metro",
                        "vehículo",
                        "Se aplica a paradas de tren, tranvía y metro.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Velocidad de los aviones" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "terminales de aviones",
                        "avión",
                        "Se aplica a terminales de aviones de pasajeros.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Los <pasajeros atrasados> que sigan <sin estar listos> después de la <hora de salida> pueden perder el vehículo.\n" +
                    "- Los pasajeros atrasados que viajan solos se liberan tras un breve margen para que el transporte pueda salir.\n" +
                    "- Los grupos/familias reciben un poco más de margen. Si el líder sigue fuera, el juego cancela el embarque del grupo y todos quedan liberados.\n" +
                    "- Si el líder ya está a bordo, Better Boarding deja que el juego termine de embarcar a niños o mascotas rezagados para que uno solo no retenga el vehículo hasta que venza el largo tiempo de espera del juego.\n" +
                    "- Los ciudadanos omitidos por llegar tarde no se eliminan; el juego puede dejar que continúen su viaje o redirigirlos de forma natural."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims corren antes (ver descripción, 3.er panel)" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Los ciudadanos que van <atrasados> empiezan a <correr antes> para intentar llegar **antes** de la hora de salida.\n" +
                    "- Funciona con autobuses, tranvías, trenes y metros, sobre todo en andenes largos.\n" +
                    "- Solo afecta a cims ya asignados a un vehículo que está embarcando.\n" +
                    "- El juego solo hace que los cims corran al llegar la hora de salida, y puede ser demasiado tarde.\n" +
                    $"- Combina bien con <{ToggleName}> porque puede reducir cuántos cims pierden el vehículo y necesitan reasignarse.\n" +
                    "- No cambia la hora de salida, no fuerza el embarque ni teletransporta ciudadanos."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Uso total" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Uso mensual del transporte público desde la vista de información de transporte del juego.\n" +
                    "La hora de actualización muestra cuándo se tomó este estado (normalmente al entrar en Opciones)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims corren antes" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Si está activado [x], cuenta todos los cims (hoy) que empezaron a **correr antes** para intentar alcanzar un autobús, tranvía, tren o metro antes de la salida.\n" +
                    "Los cims corren 512 frames antes que en el juego base (~2-8 segundos antes en tiempo real, ~2 minutos en el juego)."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Autobús" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("autobús") },
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
                    "Si el archivo todavía no existe, abre la carpeta Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Nombre visible de este mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Versión" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Versión actual del mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Abre la página del autor en Paradox Mods." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Activar log detallado" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Solo depuración / pruebas**\n" +
                    "Se recomienda dejarlo siempre en **OFF**, salvo que sepas para qué sirve.\n" +
                    "Añade detalles <live> a <Logs/BetterBoarding.log> mientras la ciudad está en marcha.\n" +
                    "**No lo actives para juego normal.**\n" +
                    "Dejarlo activado puede bajar el rendimiento y crear archivos log enormes.\n" +
                    "Puedes borrar los logs antiguos más tarde.\n" +
                    "Nota: <Stats al log> es un informe del momento con los contadores de pasajeros atrasados omitidos hoy; no tiene el impacto de rendimiento del log detallado.\n" +
                    "Usa el log detallado durante 10-15 min si quieres una cronología de lo ocurrido, pero genera muchos datos.\n" +
                    "No olvides volver a ponerlo en **OFF** antes de jugar normalmente."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Estado no cargado." },
                { WaitStatus.KeyNoCityLoaded, "No hay ciudad cargada." },
                { WaitStatus.KeyNoStopsFound, "No se encontraron paradas." },

                { WaitStatus.KeyStatusLine, "{0} esperando | prom. {1} | peor {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} atrasados hoy" },
                { WaitStatus.KeyStatusSkipOff, "omitir OFF" },

                { WaitStatus.KeyStatusOverviewLine, "{0} turistas/mes | {1} ciudadanos/mes | act. {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "correr antes OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Se pidió el informe, pero no hay ninguna ciudad cargada." },
                { WaitStatus.KeyReportTitle, "Snapshot de Stats al log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Ajustes: {0}" },
                { WaitStatus.KeyReportNote, "La pista de línea viene del waypoint con mayor espera en esa parada." },
                { WaitStatus.KeyReportTesterHintsHeader, "Pistas para testers" },
                { WaitStatus.KeyReportHintWorstStops, "Peores paradas: revísalas primero en el juego o con Scene Explorer (busca la ubicación por ID de entidad). Comprueba tráfico, mala ubicación o una parada con errores." },
                { WaitStatus.KeyReportHintSkippedCims, "Cims solos omitidos: pasajeros atrasados que liberamos para que el transporte pueda salir. Después, el estado normalmente debería ser 'has path' o 'assigned'. Si sigue en 'no path yet', revisa esa entidad cim tras más tiempo." },
                { WaitStatus.KeyReportHintLateGroups, "Grupos atrasados (familias): grupos que siguen sin resolverse justo cuando se toma el informe. Better Boarding les da un poco más de margen; si el líder sigue fuera, libera al grupo, y si ya está a bordo, ayuda al juego a terminar de subir a los rezagados." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Paradas servidas: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Paradas con pasajeros esperando: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Pasajeros esperando: {0}" },
                { WaitStatus.KeyReportAverageWait, "Espera media: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Pasajeros atrasados omitidos hoy: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Peor parada: ninguna, no hay pasajeros esperando ahora." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Espera media de la peor parada: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Nombre de la peor parada: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entidad de la peor parada: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Entidad waypoint de la peor parada: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Pista de la peor línea: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entidad de la peor línea: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Media del waypoint de la peor línea: {0} con {1} esperando" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} peores paradas por espera media:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | med. {2} | esperando {3} | parada {4} | waypoint {5} | línea {6} | pista {7}" },
                { WaitStatus.KeyReportLateGroups, "Cims de grupos atrasados aún sin resolver: {0} pasajeros en {1} grupos y {2} vehículos" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Ejemplos de cims solos atrasados omitidos" },
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
