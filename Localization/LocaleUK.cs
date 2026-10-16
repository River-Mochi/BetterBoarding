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

        public LocaleUK(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName + " (Покращена посадка)";

            const string ToggleName = "Не чекати запізнілих пасажирів";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Вищі значення пришвидшують посадку й висадку на {transitName}.\n" +
                    "Це скорочує звичайний час зупинки під час обміну пасажирами.\n" +
                    "<==========================>\n" +
                    "1x = 100% часу vanilla\n" +
                    "2x = ~1/2 планової стоянки\n" +
                    "3x = ~1/3 (рекомендовано)\n" +
                    "5x = ~1/5 (макс.)";
            }

            string StatusDescription(string transitName)
            {
                return
                    $"<Поточний стан: {transitName}>\n" +
                    "**Очікує** = пасажири, що чекають зараз.\n" +
                    "**Сер.** = середній час очікування.\n" +
                    "**Найгірше** = найбільше середнє очікування на одній зупинці.\n" +
                    "Якщо воно велике, перевірте затори, заблоковану/зламану зупинку або нестачу транспорту.\n" +
                    "<Stats у лог> показує зупинки, ID сутностей, пропуски запізнілих пасажирів та інше.";
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
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Інфо мода" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Посилання" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Налагодження" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Посадка в автобус" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "автобусних зупинках",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Посадка на рейковий транспорт" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "зупинках поїзда, трамвая й метро",
                        "Для поїздів, трамваїв і метро.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Посадка в літак" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "авіатерміналах",
                        "Для пасажирських авіатерміналів.\n")
                },

                // Boarding behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims біжать раніше" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Призначені cims починають <бігти раніше>, щоб встигнути на транспорт до відправлення.\n" +
                    "- Починається до ~3 ігрових хвилин раніше за vanilla.\n" +
                    "- Це реальна допомога руху: тести показують, що багато сімуів після допомоги сідають у той самий призначений транспорт.\n" +
                    "- Допомагає на довгих платформах і на підході до зупинки.\n" +
                    "- Впливає лише на cims, уже призначених транспорту, що здійснює посадку.\n" +
                    "- Не телепортує cims і не змінює час відправлення."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Страховка для рідкісних випадків, коли транспорт після часу відправлення все ще чекає пасажира, який явно далеко.\n" +
                    "Better Boarding втручається значно раніше за ~10-хвилинний механізм vanilla для застряглого транспорту.\n" +
                    "- Cims Run Sooner спочатку допомагає у звичайних випадках; ця опція ловить тих, хто все ще лишився надто далеко.\n" +
                    "- Пасажири на зупинці/станції або поруч залишаються призначеними й продовжують посадку.\n" +
                    "- Довгі платформи поїзда/метро залишаються захищеними.\n" +
                    "- Групи/сім’ї мають додатковий захист.\n" +
                    "- Пропущені cims залишаються в місті; vanilla може продовжити або перебудувати їхню поїздку, часто на пізніший транспорт.\n" +
                    "- Кораблі, пороми й літаки залишають vanilla-обробку запізнілих пасажирів."
                },
                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Загальне використання" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Місячне використання громадського транспорту з інфовікна Transportation.\n" +
                    "Оновлено = час, коли зроблено цей знімок стану."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims раніше" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Рахує сьогоднішні реальні **допомоги бігти раніше** за видом транспорту.\n" +
                    "Тести підтверджують, що багато сімуів після допомоги сідають у той самий призначений транспорт."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusGroupFamilyAssists)), "Допомога групам/сім’ям" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusGroupFamilyAssists)),
                    "Рахує сьогоднішні допомоги Better Boarding для груп/сімей.\n" +
                    "Включає допомогу відсталим учасникам завершити посадку та безпечну обробку придатних груп, що залишилися далеко."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Автобус" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("автобус") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Трамвай" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("трамвай") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Поїзд" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("поїзд") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Метро" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("метро") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Пором" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("пором") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Корабель" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("корабель") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Літак" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("літак") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Статистика в лог" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Записує одноразовий детальний звіт у **BetterBoarding.log**.\n" +
                    "Містить кількість очікувань, 3 найгірші зупинки, приклади пропущених запізнілих пасажирів, Entity ID та підказки ліній."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Відкрити лог" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Відкриває **BetterBoarding.log**, якщо він існує.\n" +
                    "Якщо ні — відкриває папку Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Мод" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Назва мода, що відображається." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Версія" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Поточна версія мода." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Відкриває сторінку автора в Paradox Mods." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Детальний лог" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**ЛИШЕ Debug / Тестування**\n" +
                    "Для звичайної гри залишайте ВИМКНЕНО.\n" +
                    "Додає <живі> деталі в <Logs/BetterBoarding.log>, поки місто працює.\n" +
                    "Може знижувати продуктивність і створювати великі лог-файли.\n" +
                    "Для діагностики увімкніть на 10-15 хв, потім знову ВИМКНІТЬ.\n" +
                    "<Статистика в лог> — легший одноразовий знімок."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Стан не завантажено." },
                { WaitStatus.KeyNoCityLoaded, "Місто не завантажено." },
                { WaitStatus.KeyNoStopsFound, "Зупинок не знайдено." },

                { WaitStatus.KeyStatusLine, "{0} чекає | сер. {1} | гірш. {2}" },

                { WaitStatus.KeyStatusOverviewLine, "{0} тур./міс. | {1} мешк./міс. | {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "біг раніше ВИМК." },
                { WaitStatus.KeyStatusGroupFamilyAssistsLine, "сьогодні {0}" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Запитано звіт, але місто не завантажено." },
                { WaitStatus.KeyReportTitle, "Знімок статистики в лог - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Налаштування: {0}" },
                { WaitStatus.KeyReportNote, "Підказка лінії береться з waypoint із найбільшим очікуванням на цій зупинці." },
                { WaitStatus.KeyReportTesterHintsHeader, "Підказки тестування" },
                { WaitStatus.KeyReportHintWorstStops, "Найгірші зупинки: спочатку перевірте в грі або через Scene Explorer (місце за Entity ID). Шукайте затори, невдале розташування або зламану зупинку." },
                { WaitStatus.KeyReportHintSkippedCims, "Пропущені запізнілі пасажири: після відправлення вони були надто далеко, тому Better Boarding не дав транспорту чекати на них далі. Приклади показують, що сталося потім." },
                { WaitStatus.KeyReportHintLateGroups, "Посадка груп: показує сім’ї/групи, що ще не завершили посадку на момент звіту. Better Boarding дає додатковий час і допомагає відсталим членам завершити посадку." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Обслуговувані зупинки: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Зупинки з пасажирами: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Пасажири очікують: {0}" },
                { WaitStatus.KeyReportAverageWait, "Середнє очікування: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Запізнілих пасажирів пропущено сьогодні: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Найгірша зупинка: немає, зараз ніхто не чекає." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Сер. очікування найгіршої зупинки: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Назва найгіршої зупинки: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entity найгіршої зупинки: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Waypoint Entity найгіршої зупинки: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Підказка найгіршої лінії: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entity найгіршої лінії: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Середнє waypoint найгіршої лінії: {0}, чекає {1}" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Топ {0} найгірших зупинок за середнім очікуванням:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | сер. {2} | чекає {3} | зупинка {4} | waypoint {5} | лінія {6} | підказка {7}" },
                { WaitStatus.KeyReportLateGroups, "Посадка груп не завершена: {0} пасажирів у {1} групах на {2} транспортних засобах" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Приклади пропущених запізнілих пасажирів" },
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
