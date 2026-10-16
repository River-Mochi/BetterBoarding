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

        public LocaleZH_HANT(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName + " (更順暢上下車)";

            const string ToggleName = "不等待遲到乘客";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"數值越高，{transitName}的上下客越快。\n" +
                    "可縮短正常上下客時的停站時間。\n" +
                    "<==========================>\n" +
                    "1x = vanilla 100%停站\n" +
                    "2x = 約1/2計畫停站\n" +
                    "3x = 約1/3（建議）\n" +
                    "5x = 約1/5（最大）";
            }

            string StatusDescription(string transitName)
            {
                return
                    $"<{transitName}目前狀態>\n" +
                    "**等待** = 目前等待乘客數。\n" +
                    "**平均** = 這些乘客的平均等待時間。\n" +
                    "**最差** = 單一站點最高平均等待時間。\n" +
                    "過高時請檢查壅塞、站點卡住/異常或車輛不足。\n" +
                    "<Stats to Log> 可查看站點、實體 ID、遲到乘客略過記錄等。";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "操作" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "關於" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "上下客速度" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "行為" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "狀態" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Mod資訊" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "連結" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "偵錯" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "公車上下客速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "公車站",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "軌道上下客速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "火車、電車與地鐵站",
                        "適用於火車、電車與地鐵。\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "飛機上下客速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "機場航廈",
                        "適用於客運飛機航廈。\n")
                },

                // Boarding behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cim 提早跑步" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "已指派的 Cim 會<提早開始跑步>，更容易在交通工具發車前趕上。\n" +
                    "- 最多比 vanilla 提早約3分鐘遊戲時間開始。\n" +
                    "- 這是真實的移動輔助：測試中，許多獲得幫助的市民最後成功搭上原本分配的車輛。\n" +
                    "- 對長月台與正在接近站點的 Cim 很有幫助。\n" +
                    "- 只影響已指派給正在上下客車輛的 Cim。\n" +
                    "- 不傳送 Cim，也不改變發車時間。"
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "用於少數異常情況：車輛已過發車時間，卻還在等待明顯離站點很遠的乘客。\n" +
                    "Better Boarding 比 vanilla 對卡住車輛約10分鐘的保底處理更早介入。\n" +
                    "- 「市民提前跑」先處理常見情況；這個選項再處理仍然離得很遠的遲到乘客。\n" +
                    "- 在站點/車站內或附近的乘客會保留指派並繼續上車。\n" +
                    "- 長火車/地鐵月台繼續受到保護。\n" +
                    "- 群組/家庭有額外保護。\n" +
                    "- 被跳過的 Cim 仍留在城市中；vanilla 可以繼續或重新規劃行程，通常會搭乘之後的車輛。\n" +
                    "- 船舶、渡輪與飛機繼續使用 vanilla 的遲到乘客處理。"
                },
                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "總使用量" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "來自遊戲交通資訊檢視的每月大眾運輸使用量。\n" +
                    "更新時間 = 取得此狀態快照的時間。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cim 提早跑" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "按交通類型統計今天實際觸發的**提前跑輔助**。\n" +
                    "測試確認，許多獲得幫助的市民最後搭上了原本分配的車輛。"
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusGroupFamilyAssists)), "團體/家庭協助" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusGroupFamilyAssists)),
                    "統計今天 Better Boarding 對團體/家庭乘車的協助次數。\n" +
                    "包括幫助落後成員完成上車，以及安全處理符合條件的遠距離團體。"
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
                    "向 **BetterBoarding.log** 寫入一次詳細報告。\n" +
                    "包含等待總數、最差的3個站點、被跳過的遲到乘客範例、Entity ID 與路線提示。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "開啟日誌" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "若存在則開啟 **BetterBoarding.log**。\n" +
                    "若尚未建立，則開啟 Logs 資料夾。"
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Mod顯示名稱。" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "版本" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "目前Mod版本。" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "開啟作者的 Paradox Mods 頁面。" },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "啟用詳細日誌" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**僅限 Debug / 測試**\n" +
                    "正常遊玩時請保持關閉。\n" +
                    "城市運行時向 <Logs/BetterBoarding.log> 寫入<即時>詳細資訊。\n" +
                    "可能降低效能並產生很大的日誌檔案。\n" +
                    "排查問題時使用10-15分鐘，之後再次關閉。\n" +
                    "<統計寫入日誌> 是更輕量的一次性快照。"
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "狀態未載入。" },
                { WaitStatus.KeyNoCityLoaded, "未載入城市。" },
                { WaitStatus.KeyNoStopsFound, "找不到站點。" },

                { WaitStatus.KeyStatusLine, "{0} 等待 | 平均 {1} | 最差 {2}" },

                { WaitStatus.KeyStatusOverviewLine, "遊客 {0}/月 | 市民 {1}/月 | 更新 {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "提早跑 OFF" },
                { WaitStatus.KeyStatusGroupFamilyAssistsLine, "今天 {0}" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] 要求統計報告，但目前未載入城市。" },
                { WaitStatus.KeyReportTitle, "統計日誌快照 - Better Boarding" },
                { WaitStatus.KeyReportSettings, "設定：{0}" },
                { WaitStatus.KeyReportNote, "路線提示來自該站點等待時間最高的 waypoint。" },
                { WaitStatus.KeyReportTesterHintsHeader, "測試提示" },
                { WaitStatus.KeyReportHintWorstStops, "最差站點：先在遊戲中或用 Scene Explorer 檢查（透過 Entity ID 定位）。查看壅塞、位置不佳或站點異常。" },
                { WaitStatus.KeyReportHintSkippedCims, "被跳過的遲到乘客：發車後仍離得太遠，因此 Better Boarding 不再讓車輛繼續等他們。後續範例會顯示之後發生了什麼。" },
                { WaitStatus.KeyReportHintLateGroups, "群組上車：顯示產生報告時仍未完成上車的家庭/群組。Better Boarding 會提供額外寬限，並協助落後成員完成上車。" },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "服務站點：{0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "有乘客等待的站點：{0}" },
                { WaitStatus.KeyReportWaitingPassengers, "等待乘客：{0}" },
                { WaitStatus.KeyReportAverageWait, "平均等待：{0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "今日跳過遲到乘客：{0}" },
                { WaitStatus.KeyReportWorstStopNone, "最差站點：無；目前沒有乘客等待。" },
                { WaitStatus.KeyReportWorstStopAverageWait, "最差站點平均等待：{0}" },
                { WaitStatus.KeyReportWorstStopName, "最差站點名稱：{0}" },
                { WaitStatus.KeyReportWorstStopEntity, "最差站點 Entity：{0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "最差 waypoint Entity：{0}" },
                { WaitStatus.KeyReportWorstLineHint, "最差路線提示：{0}" },
                { WaitStatus.KeyReportWorstLineEntity, "最差路線 Entity：{0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "最差路線 waypoint 平均：{0}，等待 {1}" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "依平均等待排序的最差 {0} 個站點：" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | 平均 {2} | 等待 {3} | 站點 {4} | waypoint {5} | 路線 {6} | 提示 {7}" },
                { WaitStatus.KeyReportLateGroups, "群組上車未完成：{0} 名乘客 / {1} 個群組 / {2} 輛車" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "被跳過的遲到乘客範例" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | 乘客 {2} | 錯過車輛 {3} | 時間 {4} | 目前 {5}" },
                { WaitStatus.KeyReportNone, "無" },
                { WaitStatus.KeyReportUnknown, "(未知)" },
            };
        }

        public void Unload()
        {
        }
    }
}
