// <copyright file="LocaleUK.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleUK.cs
// Purpose: Ukrainian uk-UA locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Ukrainian localization source.
    /// </summary>
    public sealed class LocaleUK : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        /// <summary>
        /// Constructs the Ukrainian locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleUK(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all Ukrainian localization entries for this mod.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName;

            const string ToggleName = "Пропускати пасажирів, що запізнилися";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Вищі значення зменшують час посадки й завантаження на {transitName}.\n" +
        
                    "Це допомагає звичайним чергам рухатися швидше, але пасажир, що запізнився, все ще може затримати відправлення через логіку vanilla.\n" +
                    $"Увімкніть [✓] <{ToggleName}>, якщо хочете, щоб cims, які запізнилися, могли пропустити транспорт після часу відправлення.\n" +
                    "Пропущені пасажири не видаляються; vanilla природно перебудує їхній маршрут.\n" +
                    "<==========================>\n" +
                    "Значення завантаження:\n" +
                    "1x = 100% часу стоянки vanilla\n" +
                    "2x = ~1/2 запланованого часу стоянки\n" +
                    "3x = ~1/3 запланованого часу стоянки (рекомендовано)\n" +
                    "5x = ~1/5 запланованого часу стоянки (макс.)\n" +
                    $"Це не те саме, що <{ToggleName}>; ця опція визначає, чи можуть cims, які запізнилися, пропустити {shortName} після часу відправлення.";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Поточний стан {transitName}>\n" +
                    "**Очікують** = загальна кількість пасажирів, що чекають зараз.\n" +
                    "**Сер.** = середній час очікування цих пасажирів.\n" +
                    "**Найгірша** зупинка = найвищий середній час очікування на одній зупинці.\n" +
                    "Найгірші зупинки варто перевірити на ДТП, затори, зламані зупинки або нестачу транспорту.\n" +
                    $"**Запізнилися сьогодні** = соло-пасажири, що запізнилися й були пропущені сьогодні через <{ToggleName}>.\n" +
                    "Використовуйте <Stats to Log> для детального звіту: назви зупинок, ID сутностей тощо.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "Дії" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "Про мод" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "Швидкість посадки" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "Поведінка" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Стан" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Інформація про мод" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Посилання" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Налагодження" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Швидкість посадки в автобус" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "автобусній зупинці",
                        "автобус",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Швидкість посадки на рейковий транспорт" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "зупинці потяга, трамвая й метро",
                        "транспорт",
                        "Застосовується до зупинок потяга, трамвая й метро.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "Швидкість корабля + порома" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "зупинці корабля й порома",
                        "транспорт",
                        "Застосовується до зупинок кораблів і поромів.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Швидкість літака" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "терміналі літака",
                        "літак",
                        "Застосовується до пасажирських авіатерміналів.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "<Пасажири, що запізнилися> і все ще <не готові> після <часу відправлення>, можуть пропустити транспорт.\n" +
                    "- Примітка: пропускаємо лише соло-пасажирів, які запізнилися.\n" +
                    "- Групи/сім'ї, що подорожують разом і запізнилися, <не пропускаються> та можуть і далі затримувати транспорт, як у vanilla.\n" +
                    "- Групи становлять невелику частину натовпу; найбільший ефект дає пропуск соло-cims, що запізнилися.\n" +
                    "- Пропущені пасажири не видаляються; гра природно призначить їм новий маршрут."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims біжать раніше: автобус + трамвай + потяг" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Громадяни, які <запізнюються>, починають <бігти раніше>, щоб встигнути **до** часу відправлення.\n" +
                    "- Працює для автобусів, трамваїв і потягів, особливо на довгих залізничних платформах.\n" +
                    "- Впливає лише на cims, уже призначених до транспорту, який зараз проводить посадку.\n" +
                    "- У vanilla cims починають бігти лише в момент відправлення, що іноді запізно.\n" +
                    $"- Добре працює разом із <{ToggleName}>, бо може зменшити кількість cims, які пропускають транспорт і потребують нового призначення.\n" +
                    "- Не змінює час відправлення, не змушує сідати й не телепортує громадян."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Загальне використання" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Місячне використання громадського транспорту з інфопанелі Transportation гри.\n" +
                    "Час оновлення показує, коли зроблено цей знімок стану (зазвичай після відкриття меню параметрів)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims біжать раніше" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Якщо ввімкнено [x], рахує всіх cims (сьогодні), які почали **бігти раніше**, щоб встигнути на автобус, трамвай або потяг до відправлення.\n" +
                    "Cims починають бігти на 512 кадрів раніше, ніж у vanilla (~2-8 секунд реального часу, ~2 хвилини в грі)."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Автобус" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("автобус") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Трамвай" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("трамвай") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Потяг" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("потяг") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Метро" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("метро") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Пором" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("пором") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Корабель" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("корабель") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Літак" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("літак") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Статистику в лог" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Записує одноразовий детальний звіт у **BetterBoarding.log**.\n" +
                    "Містить загальну кількість очікувань, 3 найгірші зупинки для кожного виду, приклади пропущених cims, ID сутностей і підказки ліній."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Відкрити лог" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Відкриває **BetterBoarding.log**, якщо він існує.\n" +
                    "Якщо файл ще не створено, відкриває папку Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Відображувана назва мода." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Версія" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Поточна версія мода." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Відкриває сторінку автора на Paradox Mods." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Увімкнути детальний лог" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Лише для налагодження / тестування**\n" +
                    "Додає <живі> подробиці до <Logs/BetterBoarding.log>, поки місто працює.\n" +
                    "**Не вмикайте для звичайної гри.**\n" +
                    "Якщо залишити це ввімкненим, продуктивність може знизитися, а файли логів сильно вирости.\n" +
                    "Старі файли логів можна видалити пізніше.\n" +
                    "Примітка: <Stats to Log> — це одноразовий звіт плюс сьогоднішні лічильники пропусків; він відрізняється від детального логу.\n" +
                    "Запустіть детальний лог на 15-20 хв, якщо хочете побачити хронологію подій.\n" +
                    "Не забудьте знову **ВИМКНУТИ** детальний лог перед звичайною грою."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Стан не завантажено." },
                { WaitStatus.KeyNoCityLoaded, "Місто не завантажено." },
                { WaitStatus.KeyNoStopsFound, "Зупинок не знайдено." },

                { WaitStatus.KeyStatusLine, "{0} очікують | сер. {1} | найгірша {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} запізнилися сьогодні" },
                { WaitStatus.KeyStatusSkipOff, "пропуск ВИМК." },

                { WaitStatus.KeyStatusOverviewLine, "{0} туристів/міс. | {1} громадян/міс. | оновлено {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "ранній біг ВИМК." },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Запитано звіт, але місто не завантажено." },
                { WaitStatus.KeyReportTitle, "Знімок Stats to Log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Налаштування: {0}" },
                { WaitStatus.KeyReportNote, "Підказка лінії береться з waypoint із найбільшим очікуванням на цій зупинці." },
                { WaitStatus.KeyReportTesterHintsHeader, "Підказки тестувальнику" },
                { WaitStatus.KeyReportHintWorstStops, "Найгірші зупинки: перевірте їх першими в грі або через мод Scene Explorer (знайдіть за entity ID). Шукайте затори, невдале розташування або зламану зупинку." },
                { WaitStatus.KeyReportHintSkippedCims, "Пропущені соло-cims: пасажири, що запізнилися, яких ми пропускаємо, щоб транспорт міг поїхати. Пізніше стан зазвичай має стати 'є маршрут' або 'призначено'. Якщо лишається 'маршруту ще немає', перевірте сутність пізніше." },
                { WaitStatus.KeyReportHintLateGroups, "Групи, що запізнилися (сім'ї): навмисно залишені vanilla, щоб не розділяти їх; їх небагато порівняно з соло-пасажирами." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Обслуговувані зупинки: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Зупинки з пасажирами, що чекають: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Пасажири в очікуванні: {0}" },
                { WaitStatus.KeyReportAverageWait, "Середнє очікування: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Пасажири, пропущені сьогодні через запізнення: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Найгірша зупинка: немає, зараз ніхто не чекає." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Сер. очікування на найгіршій зупинці: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Назва найгіршої зупинки: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entity найгіршої зупинки: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Entity найгіршого waypoint: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Підказка найгіршої лінії: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entity найгіршої лінії: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Сер. waypoint найгіршої лінії: {0}, очікують {1}" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Топ {0} найгірших зупинок за середнім очікуванням:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | сер. {2} | очікують {3} | stop entity {4} | waypoint entity {5} | line entity {6} | підказка лінії {7}" },
                { WaitStatus.KeyReportLateGroups, "Групові cims, що запізнилися, залишені без змін: {0} пасажирів у {1} групах на {2} транспортних засобах" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Приклади пропущених соло-cims, що запізнилися" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | пасажир {2} | пропущений транспорт {3} | час {4} | зараз {5}" },
                { WaitStatus.KeyReportNone, "немає" },
                { WaitStatus.KeyReportUnknown, "(невідомо)" },
            };
        }

        public void Unload()
        {
        }
    }
}
