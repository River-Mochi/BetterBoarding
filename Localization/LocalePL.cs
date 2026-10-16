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

        public LocalePL(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName + " (Lepsze wsiadanie)";

            const string ToggleName = "Pomiń spóźnionych pasażerów";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Wyższe wartości przyspieszają wsiadanie i wysiadanie na {transitName}.\n" +
                    "Skraca to normalny czas postoju podczas wymiany pasażerów.\n" +
                    "<==========================>\n" +
                    "1x = 100% czasu vanilla\n" +
                    "2x = ~1/2 planowanego postoju\n" +
                    "3x = ~1/3 planowanego postoju (zalecane)\n" +
                    "5x = ~1/5 planowanego postoju (maks.)";
            }

            // One helper keeps the status tooltips consistent while explaining
            // which modes Better Boarding can actually skip late passengers for.
            string StatusDescription(string transitName, bool skipSupported)
            {
                string skippedText = skipSupported
                    ? "**Pominięto** = spóźnieni pasażerowie pominięci dziś, aby transport nie czekał dalej.\n" +
                      "Better Boarding działa znacznie wcześniej niż ~10-minutowy awaryjny limit vanilla dla zablokowanego pojazdu.\n" +
                      $"Sterowane przez <{ToggleName}>.\n"
                    : "**Pominięto** = 0 dla tego środka transportu. Tu zostaje obsługa spóźnionych pasażerów z vanilla.\n";

                return
                    $"<Bieżący stan: {transitName}>\n" +
                    "**Czeka** = liczba pasażerów czekających teraz.\n" +
                    "**Śr.** = średni czas oczekiwania.\n" +
                    "**Najg.** = najwyższa średnia na jednym przystanku.\n" +
                    "Przy wysokiej wartości sprawdź korki, zablokowany/błędny przystanek lub za mało pojazdów.\n" +
                    skippedText +
                    "<Statystyki do logu> pokazują nazwy przystanków, ID encji i więcej szczegółów.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "Akcje" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "O modzie" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "Szybkość wsiadania" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "Zachowanie" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Stan" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Info o modzie" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Linki" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Debug" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Wsiadanie do autobusu" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "przystankach autobusowych",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Wsiadanie kolejowe" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "przystankach pociągu, tramwaju i metra",
                        "Dotyczy pociągów, tramwajów i metra.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Wsiadanie do samolotu" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "terminalach lotniczych",
                        "Dotyczy terminali pasażerskich samolotów.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Spóźnionego pasażera zbyt daleko od przystanku można pominąć, aby transport nie czekał dalej.\n" +
                    "Better Boarding działa znacznie wcześniej niż ~10-minutowy awaryjny limit vanilla dla zablokowanego pojazdu.\n" +
                    "- Pasażerowie na przystanku/stacji lub w pobliżu zostają przypisani i dalej wsiadają.\n" +
                    "- Długie perony pociągu/metra pozostają chronione.\n" +
                    "- Grupy/rodziny dostają dodatkową ochronę.\n" +
                    "- Pominięte cimy zostają w mieście; vanilla może kontynuować lub przeliczyć ich podróż, często na późniejszy pojazd.\n" +
                    "- Statki, promy i samoloty zachowują obsługę spóźnionych pasażerów z vanilla."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cimy biegną wcześniej" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Przypisane cimy zaczynają <biec wcześniej>, żeby zdążyć na transport przed odjazdem.\n" +
                    "- Zaczyna do około 3 minut w grze wcześniej niż vanilla.\n" +
                    "- Pomaga na długich peronach i przy dojściu do przystanku.\n" +
                    "- Dotyczy tylko cimów już przypisanych do pojazdu przyjmującego pasażerów.\n" +
                    "- Nie teleportuje cimów ani nie zmienia czasu odjazdu."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Łączne użycie" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Miesięczne użycie transportu publicznego z widoku Transport w grze.\n" +
                    "Zaktualizowano = czas wykonania tego zrzutu stanu."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cimy wcześniej" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Liczy cimy, które Better Boarding kazał dziś **biec wcześniej**.\n" +
                    "Do około 3 minut w grze wcześniej niż vanilla."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Autobus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("autobus", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Tramwaj" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("tramwaj", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Pociąg" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("pociąg", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Metro" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("metro", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Prom" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("prom", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Statek" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("statek", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Samolot" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("samolot", skipSupported: false) },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Statystyki do logu" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Zapisuje jednorazowy szczegółowy raport do **BetterBoarding.log**.\n" +
                    "Zawiera sumy oczekujących, 3 najgorsze przystanki, przykłady pominiętych spóźnionych pasażerów, Entity ID i wskazówki linii."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Otwórz log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Otwiera **BetterBoarding.log**, jeśli istnieje.\n" +
                    "Jeśli nie, otwiera folder Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Wyświetlana nazwa moda." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Wersja" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Bieżąca wersja moda." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Otwiera stronę autora w Paradox Mods." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Szczegółowe logowanie" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**TYLKO Debug / Testy**\n" +
                    "Podczas normalnej gry zostaw WYŁĄCZONE.\n" +
                    "Dodaje szczegóły <na żywo> do <Logs/BetterBoarding.log> podczas działania miasta.\n" +
                    "Może obniżyć wydajność i tworzyć duże pliki logów.\n" +
                    "Przy diagnozie używaj 10-15 min, potem znów WYŁĄCZ.\n" +
                    "<Statystyki do logu> to lżejszy jednorazowy zrzut."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Stan niezaładowany." },
                { WaitStatus.KeyNoCityLoaded, "Brak wczytanego miasta." },
                { WaitStatus.KeyNoStopsFound, "Nie znaleziono przystanków." },

                { WaitStatus.KeyStatusLine, "{0} czeka | śr. {1} | najg. {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} spóźn. pominiętych" },
                { WaitStatus.KeyStatusSkipOff, "pomijanie WYŁ." },

                { WaitStatus.KeyStatusOverviewLine, "{0} tur./mies. | {1} mieszk./mies. | akt. {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "bieg wcześniej WYŁ." },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Zażądano raportu, ale nie wczytano miasta." },
                { WaitStatus.KeyReportTitle, "Migawka Statystyki do logu - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Ustawienia: {0}" },
                { WaitStatus.KeyReportNote, "Wskazówka linii pochodzi z waypointu o najwyższym czasie oczekiwania na tym przystanku." },
                { WaitStatus.KeyReportTesterHintsHeader, "Wskazówki testowe" },
                { WaitStatus.KeyReportHintWorstStops, "Najgorsze przystanki: najpierw sprawdź je w grze lub Scene Explorerem (lokalizacja przez ID encji). Szukaj korków, złego położenia lub błędnego przystanku." },
                { WaitStatus.KeyReportHintSkippedCims, "Pominięci spóźnieni pasażerowie: po odjeździe byli zbyt daleko, więc Better Boarding nie pozwolił pojazdowi dalej na nich czekać. Przykłady pokazują, co stało się później." },
                { WaitStatus.KeyReportHintLateGroups, "Wsiadanie grup: pokazuje rodziny/grupy nadal niekompletne w chwili raportu. Better Boarding daje dodatkowy zapas i pomaga spóźnionym członkom dokończyć wsiadanie." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Obsługiwane przystanki: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Przystanki z oczekującymi: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Oczekujący pasażerowie: {0}" },
                { WaitStatus.KeyReportAverageWait, "Średni czas oczekiwania: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Spóźnieni pasażerowie pominięci dziś: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Najgorszy przystanek: brak, teraz nikt nie czeka." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Śr. oczekiwanie najgorszego przystanku: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Nazwa najgorszego przystanku: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Encja najgorszego przystanku: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Encja waypointu najgorszego przystanku: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Wskazówka najgorszej linii: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Encja najgorszej linii: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Śr. waypointu najgorszej linii: {0}, oczekuje {1}" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} najgorszych przystanków wg średniego oczekiwania:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | śr. {2} | czeka {3} | przystanek {4} | waypoint {5} | linia {6} | wsk. {7}" },
                { WaitStatus.KeyReportLateGroups, "Wsiadanie grup nieukończone: {0} pasażerów w {1} grupach na {2} pojazdach" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Przykłady pominiętych spóźnionych pasażerów" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | pasażer {2} | pominięty pojazd {3} | czas {4} | teraz {5}" },
                { WaitStatus.KeyReportNone, "brak" },
                { WaitStatus.KeyReportUnknown, "(nieznane)" },
            };
        }

        public void Unload()
        {
        }
    }
}
