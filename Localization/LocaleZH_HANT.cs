// <copyright file="LocaleZH_HANT.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleZH_HANT.cs
// Purpose: Traditional Chinese zh-HANT locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Traditional Chinese localization source.
    /// </summary>
    public sealed class LocaleZH_HANT : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        /// <summary>
        /// Constructs the Traditional Chinese locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleZH_HANT(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all Traditional Chinese localization entries for this mod.
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

            const string ToggleName = "跳過遲到乘客";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"較高的數值會減少{transitName}的上車與裝載時間。\n" +
                    $"3x 是建議預設值。\n" +
                    $"5x 是最大值。\n" +
                    $"這能幫助一般隊列更快清空，但由於 vanilla 設計，遲到乘客仍可能拖延發車。\n" +
                    $"如果想讓遲到 cim 在發車時間後錯過車輛，請使用 [✓] <{ToggleName}>。\n" +
                    $"被跳過的遲到市民不會被刪除；遊戲會自然為他們重新規劃路線。\n" +
                    "<==========================>\n" +
                    "裝載值:\n" +
                    "1x = 100% vanilla 停靠\n" +
                    "2x = 約 1/2 預定停靠\n" +
                    "3x = 約 1/3 預定停靠（建議）\n" +
                    "5x = 約 1/5 預定停靠（最大）\n" +
                    $"這不同於 <{ToggleName}>；該核取方塊決定遲到 cim 是否能在發車時間後錯過{shortName}。";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<目前{transitName}狀態>\n" +
                    "**等待中** = 目前正在等待的乘客總數。\n" +
                    "**平均** = 這些乘客的平均等待時間。\n" +
                    "**最差**站點 = 單一站點的最高平均等待時間。\n" +
                    "最差站點適合優先檢查事故、阻塞/異常站點，或是否需要分配更多車輛。\n" +
                    $"**今日遲到** = 今天由 <{ToggleName}> 跳過的單獨遲到乘客。\n" +
                    "使用 <統計寫入日誌> 可輸出詳細報告：站點名稱、實體 ID 等。";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "操作" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "關於" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "上車速度" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "行為" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "狀態" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "模組資訊" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "連結" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "偵錯" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "公車上車速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "公車站",
                        "公車",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "軌道上車速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "火車、電車和地鐵站",
                        "車輛",
                        "適用於火車、電車和地鐵站。\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "船舶 + 渡輪速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "船舶和渡輪站",
                        "車輛",
                        "適用於船舶和渡輪站。\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "飛機速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "飛機航廈",
                        "飛機",
                        "適用於客運飛機航廈。\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "發車時間後仍然<未準備好>的遲到乘客可以錯過車輛。\n" +
                    "注意：只跳過單獨遲到的市民。\n" +
                    "一起出行的遲到團體/家庭<不會被跳過>，仍可能像 vanilla 一樣造成公共交通延誤。\n" +
                    "團體只占人群的一小部分；主要收益來自跳過遲到奔跑的單獨 cim。\n" +
                    "被跳過的遲到市民不會被刪除；遊戲會自然重新分配他們。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "提早奔跑：公車+路面電車+火車" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "<遲到>市民會<提早奔跑>，嘗試在發車時間**之前**趕到。\n" +
                    "幫助公車、路面電車和火車保持準點，尤其適合較長的火車月台。\n" +
                    "只影響已分配到目前正在上客車輛的市民。\n" +
                    "原版只會在發車時間才讓市民開始奔跑，這可能已經太晚。\n" +
                    $"和 <{ToggleName}> 搭配很好，因為它可能減少錯過車輛並需要重新分配的市民數量。\n" +
                    "不會更改車輛發車時間、強制上車或傳送市民。"
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "總使用量" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "來自遊戲交通資訊視圖的每月公共交通使用量。\n" +
                    "更新時間顯示此狀態快照的取得時間（通常是在進入選項選單後）。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "市民提早奔跑" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "啟用 [x] 時，統計今天開始**提早奔跑**、嘗試在發車前趕上公車、路面電車或火車的市民。\n" +
                    "市民會比 vanilla 提早 512 幀奔跑（現實時間約提早 2-8 秒，遊戲內約 2 分鐘）。"
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "公車" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("公車") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "電車" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("電車") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "火車" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("火車") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "地鐵" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("地鐵") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "渡輪" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("渡輪") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "船舶" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("船舶") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "飛機" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("飛機") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "統計寫入日誌" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "向 **BetterBoarding.log** 寫入一次性詳細報告。\n" +
                    "包含等待總數、每種模式最差的 3 個站點、被跳過 cim 範例、實體 ID 和路線提示。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "開啟日誌" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "如果存在，則開啟 **BetterBoarding.log**。\n" +
                    "如果尚未找到該檔案，則改為開啟 Logs 資料夾。"
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "模組" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "此模組的顯示名稱。" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "版本" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "目前模組版本。" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "開啟作者的 Paradox Mods 頁面。" },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "啟用詳細日誌" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**僅供偵錯 / 測試**\n" +
                    "城市執行時向 <Logs/BetterBoarding.log> 加入 <live> 詳細資訊。\n" +
                    "**不要在正常遊玩時啟用。**\n" +
                    "保持啟用可能降低效能並產生巨大的日誌檔。\n" +
                    "之後可以刪除舊日誌檔。\n" +
                    "注意：<統計寫入日誌> 是某一時刻的報告，加上今天的 late-skip 計數器。\n" +
                    "如需查看事件時間軸，請執行詳細日誌 15-20 分鐘。\n" +
                    "正常遊玩前別忘了再次切回 **OFF**。"
                },

                // Runtime status strings
                { TransitWaitStatus.KeyStatusNotLoaded, "狀態未載入。" },
                { TransitWaitStatus.KeyNoCityLoaded, "未載入城市。" },
                { TransitWaitStatus.KeyNoStopsFound, "未找到站點。" },

                { TransitWaitStatus.KeyStatusLine, "{0} 等待 | 平均 {1} | 最差 {2} | {3}" },
                { TransitWaitStatus.KeyStatusLateSkipped, "{0} 今日遲到" },
                { TransitWaitStatus.KeyStatusSkipOff, "跳過 OFF" },

                { TransitWaitStatus.KeyStatusOverviewLine, "{0} 遊客/月 | {1} 市民/月 | 更新 {2}" },
                { TransitWaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { TransitWaitStatus.KeyStatusRunSoonerOff, "提早奔跑OFF" },

                // Stats-to-log report strings
                { TransitWaitStatus.KeyReportNoCityLoaded, "[BBoard] 已請求統計報告，但未載入城市。" },
                { TransitWaitStatus.KeyReportTitle, "統計寫入日誌快照 - Better Boarding" },
                { TransitWaitStatus.KeyReportSettings, "設定: {0}" },
                { TransitWaitStatus.KeyReportNote, "路線提示來自該站點等待最高的 waypoint。" },
                { TransitWaitStatus.KeyReportTesterHintsHeader, "測試提示" },
                { TransitWaitStatus.KeyReportHintWorstStops, "最差站點：先在遊戲中或用 Scene Explorer 模組檢查（可用實體 ID 找位置）。查看交通阻塞、站點位置不佳或異常站點。" },
                { TransitWaitStatus.KeyReportHintSkippedCims, "被跳過的單獨 cims：為讓交通工具發車而跳過的遲到乘客。之後狀態通常應變為 'has path' 或 'assigned'。如果仍是 'no path yet'，請過一段時間再檢查該 cim 實體。" },
                { TransitWaitStatus.KeyReportHintLateGroups, "遲到群組（家庭）：故意交給原版處理，讓他們保持在一起並遵循原版行為；相比大量單獨乘客，他們數量較少。" },
                { TransitWaitStatus.KeyReportFamilyHeader, "{0}" },
                { TransitWaitStatus.KeyReportServedStops, "服務站點: {0}" },
                { TransitWaitStatus.KeyReportStopsWithWaiting, "有等待乘客的站點: {0}" },
                { TransitWaitStatus.KeyReportWaitingPassengers, "等待乘客: {0}" },
                { TransitWaitStatus.KeyReportAverageWait, "平均等待: {0}" },
                { TransitWaitStatus.KeyReportLateBoardersSkipped, "今天跳過的遲到乘客: {0}" },
                { TransitWaitStatus.KeyReportWorstStopNone, "最差站點：無，目前沒有等待乘客。" },
                { TransitWaitStatus.KeyReportWorstStopAverageWait, "最差站點平均等待: {0}" },
                { TransitWaitStatus.KeyReportWorstStopName, "最差站點名稱: {0}" },
                { TransitWaitStatus.KeyReportWorstStopEntity, "最差站點實體: {0}" },
                { TransitWaitStatus.KeyReportWorstWaypointEntity, "最差 waypoint 實體: {0}" },
                { TransitWaitStatus.KeyReportWorstLineHint, "最差路線提示: {0}" },
                { TransitWaitStatus.KeyReportWorstLineEntity, "最差路線實體: {0}" },
                { TransitWaitStatus.KeyReportWorstLineWaypointAverage, "最差路線 waypoint 平均: {0}，等待 {1}" },
                { TransitWaitStatus.KeyReportTopWorstStopsHeader, "按平均等待排名的最差站點前 {0} 名:" },
                { TransitWaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | 平均 {2} | 等待 {3} | 站點 {4} | waypoint {5} | 路線 {6} | 提示 {7}" },
                { TransitWaitStatus.KeyReportLateGroups, "結伴出行的遲到市民保持原樣：{0} 名乘客，{1} 個群組，涉及 {2} 輛車" },
                { TransitWaitStatus.KeyReportLastSkippedSamplesHeader, "被跳過的單獨遲到 cim 範例" },
                { TransitWaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | 乘客 {2} | 錯過車輛 {3} | 時間 {4} | 現在 {5}" },
                { TransitWaitStatus.KeyReportNone, "無" },
                { TransitWaitStatus.KeyReportUnknown, "(未知)" },
            };
        }

        public void Unload()
        {
        }
    }
}
