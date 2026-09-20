// <copyright file="LocalePL.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocalePL.cs
// Purpose: Polish pl-PL locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Polish localization source.
    /// </summary>
    public sealed class LocalePL : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        /// <summary>
        /// Constructs the Polish locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocalePL(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all Polish localization entries for this mod.
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

            const string ToggleName = "Pomiń spóźnionych pasażerów";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Wyższe wartości skracają czas wsiadania i załadunku na {transitName}.\n" +
                    $"3x to zalecana wartość domyślna.\n" +
                    $"5x to maksimum.\n" +
                    $"Pomaga to szybciej rozładować zwykłe kolejki, ale spóźniony pasażer nadal może opóźnić odjazd przez mechanikę vanilla.\n" +
                    $"Użyj [✓] <{ToggleName}>, jeśli spóźnieni cims mają móc przegapić pojazd po czasie odjazdu.\n" +
                    $"Pominięci spóźnieni mieszkańcy nie są usuwani; gra naturalnie wyznaczy im nową trasę.\n" +
                    "<==========================>\n" +
                    "Wartość ładowania:\n" +
                    "1x = 100% postoju vanilla\n" +
                    "2x = ~1/2 planowanego postoju\n" +
                    "3x = ~1/3 planowanego postoju (zalecane)\n" +
                    "5x = ~1/5 planowanego postoju (maks.)\n" +
                    $"To nie to samo co <{ToggleName}>; ten checkbox decyduje, czy spóźnieni cims mogą przegapić {shortName} po czasie odjazdu.";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Aktualny status {transitName}>\n" +
                    "**Czeka** = łączna liczba pasażerów czekających teraz.\n" +
                    "**Śr.** = średni czas oczekiwania tych pasażerów.\n" +
                    "**Najgorszy** przystanek = najwyższy średni czas oczekiwania na jednym przystanku.\n" +
                    "Najgorsze przystanki warto sprawdzić pod kątem wypadków, zablokowanych/zbugowanych przystanków lub potrzeby przypisania większej liczby pojazdów.\n" +
                    $"**Spóźnieni dziś** = spóźnieni pasażerowie solo pominięci dziś przez <{ToggleName}>.\n" +
                    "Użyj <Statystyki do logu>, aby zapisać szczegółowy raport: nazwy przystanków, ID encji i więcej.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "Akcje" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "Informacje" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "Szybkość wsiadania" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "Zachowanie" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Status" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Info o modzie" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Linki" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Debug" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Szybkość autobusów" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "przystanku autobusowym",
                        "autobus",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Szybkość kolei" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "przystanku pociągu, tramwaju i metra",
                        "pojazd",
                        "Dotyczy przystanków pociągów, tramwajów i metra.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "Statek + prom" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "przystanku statku i promu",
                        "pojazd",
                        "Dotyczy przystanków statków i promów.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Szybkość samolotów" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "terminalu samolotów",
                        "samolot",
                        "Dotyczy terminali samolotów pasażerskich.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Spóźnieni pasażerowie, którzy po czasie odjazdu nadal są <niegotowi>, mogą przegapić pojazd.\n" +
                    "Uwaga: pomijani są tylko spóźnieni mieszkańcy podróżujący solo.\n" +
                    "Spóźnione grupy/rodziny podróżujące razem <nie są pomijane> i nadal mogą powodować opóźnienia jak w vanilla.\n" +
                    "Grupy to mała część tłumu; większość korzyści pochodzi z pomijania spóźnionych solo cims.\n" +
                    "Pominięci spóźnieni mieszkańcy nie są usuwani; gra naturalnie przydziela ich ponownie."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cimy biegną wcześniej: autobusy + tramwaje + pociągi" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Obywatele, którzy są <spóźnieni>, zaczynają <biec wcześniej>, aby zdążyć **przed** czasem odjazdu.\n" +
                    "Pomaga utrzymać autobusy, tramwaje i pociągi w rozkładzie, szczególnie na długich peronach kolejowych.\n" +
                    "Dotyczy tylko cimów już przypisanych do pojazdu, który aktualnie wpuszcza pasażerów.\n" +
                    "Vanilla każe cimom biec dopiero w chwili odjazdu, co może być za późno.\n" +
                    $"Dobrze działa z <{ToggleName}>, bo może zmniejszyć liczbę cimów, które przegapią pojazd i muszą zostać przypisane ponownie.\n" +
                    "Nie zmienia czasu odjazdu pojazdu, nie wymusza wejścia na pokład ani nie teleportuje obywateli."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Łączne użycie" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Miesięczne użycie transportu publicznego z widoku informacji Transport w grze.\n" +
                    "Czas aktualizacji pokazuje, kiedy wykonano ten snapshot statusu (zwykle po wejściu do menu Opcje)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cimy biegną wcześniej" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Gdy włączone [x], liczy wszystkie cimy (dzisiaj), które zaczęły **biec wcześniej**, aby spróbować złapać autobus, tramwaj lub pociąg przed odjazdem.\n" +
                    "Cimy biegną 512 klatek wcześniej niż w vanilla (~2-8 sekund wcześniej w czasie rzeczywistym, ~2 minuty w grze)."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Autobus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("autobus") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Tramwaj" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("tramwaj") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Pociąg" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("pociąg") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Metro" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("metro") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Prom" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("prom") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Statek" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("statek") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Samolot" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("samolot") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Statystyki do logu" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Zapisuje jednorazowy szczegółowy raport do **BetterBoarding.log**.\n" +
                    "Zawiera sumy oczekujących, 3 najgorsze przystanki dla każdego trybu, przykłady pominiętych cims, ID encji i wskazówki linii."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Otwórz log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Otwiera **BetterBoarding.log**, jeśli istnieje.\n" +
                    "Jeśli pliku jeszcze nie znaleziono, otwiera zamiast tego folder Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Wyświetlana nazwa tego moda." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Wersja" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Aktualna wersja moda." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Otwiera stronę autora w Paradox Mods." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Włącz szczegółowy log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Tylko debug / testy**\n" +
                    "Dodaje szczegóły <live> do <Logs/BetterBoarding.log>, gdy miasto działa.\n" +
                    "**Nie włączaj do normalnej gry.**\n" +
                    "Pozostawienie tego włączonego może obniżyć wydajność i utworzyć ogromne pliki logów.\n" +
                    "Stare pliki logów można później usunąć.\n" +
                    "Uwaga: <Statystyki do logu> to raport z chwili oraz dzisiejsze liczniki late-skip.\n" +
                    "Włącz szczegółowe logowanie na 15-20 min, jeśli potrzebna jest oś czasu zdarzeń.\n" +
                    "Nie zapomnij przełączyć z powrotem na **OFF** przed normalną grą."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Status niezaładowany." },
                { WaitStatus.KeyNoCityLoaded, "Brak załadowanego miasta." },
                { WaitStatus.KeyNoStopsFound, "Nie znaleziono przystanków." },

                { WaitStatus.KeyStatusLine, "{0} czeka | śr. {1} | najg. {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} spóźn. dziś" },
                { WaitStatus.KeyStatusSkipOff, "pomijanie OFF" },

                { WaitStatus.KeyStatusOverviewLine, "{0} turystów/mies. | {1} mieszkańców/mies. | aktual. {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "bieg wcześniej OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Zażądano raportu, ale żadne miasto nie jest załadowane." },
                { WaitStatus.KeyReportTitle, "Snapshot Statystyki do logu - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Ustawienia: {0}" },
                { WaitStatus.KeyReportNote, "Wskazówka linii pochodzi z waypointu o najwyższym oczekiwaniu na tym przystanku." },
                { WaitStatus.KeyReportTesterHintsHeader, "Wskazówki dla testerów" },
                { WaitStatus.KeyReportHintWorstStops, "Najgorsze przystanki: sprawdź je najpierw w grze albo modem Scene Explorer (lokalizacje po ID encji). Szukaj korków, złego położenia przystanku albo zbugowanego przystanku." },
                { WaitStatus.KeyReportHintSkippedCims, "Pominięci solo cims: spóźnieni pasażerowie pomijani, aby transport mógł odjechać. Późniejszy stan zwykle powinien stać się 'has path' albo 'assigned'. Jeśli zostaje 'no path yet', sprawdź tę encję cim po dłuższym czasie." },
                { WaitStatus.KeyReportHintLateGroups, "Spóźnione grupy (rodziny): celowo zostawione vanilla, aby trzymały się razem i działały jak w vanilla; jest ich mało w porównaniu z wieloma samotnymi podróżnymi." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Obsługiwane przystanki: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Przystanki z czekającymi pasażerami: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Czekający pasażerowie: {0}" },
                { WaitStatus.KeyReportAverageWait, "Średnie oczekiwanie: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Spóźnieni pasażerowie pominięci dziś: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Najgorszy przystanek: brak, teraz nikt nie czeka." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Śr. oczekiwanie najgorszego przystanku: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Nazwa najgorszego przystanku: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Encja najgorszego przystanku: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Encja waypoint najgorszego przystanku: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Wskazówka najgorszej linii: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Encja najgorszej linii: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Śr. waypoint najgorszej linii: {0} z {1} czekającymi" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} najgorszych przystanków wg średniego oczekiwania:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | śr. {2} | czeka {3} | przystanek {4} | waypoint {5} | linia {6} | wskazówka {7}" },
                { WaitStatus.KeyReportLateGroups, "Spóźnione cimy podróżujące w grupie zostawione w spokoju: {0} pasażerów w {1} grupach na {2} pojazdach" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Przykłady pominiętych spóźnionych solo cims" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | pasażer {2} | spóźniony pojazd {3} | czas {4} | teraz {5}" },
                { WaitStatus.KeyReportNone, "brak" },
                { WaitStatus.KeyReportUnknown, "(nieznane)" },
            };
        }

        public void Unload()
        {
        }
    }
}
