// <copyright file="LocaleZH_CN.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleZH_CN.cs
// Purpose: Simplified Chinese zh-HANS locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Simplified Chinese localization source.
    /// </summary>
    public sealed class LocaleZH_CN : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        public LocaleZH_CN(BBoardSettings setting)
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

            const string ToggleName = "不等待迟到乘客";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"数值越高，{transitName}的上下客越快。\n" +
                    "可缩短正常上下客时的停站时间。\n" +
                    "<==========================>\n" +
                    "1x = vanilla 100%停站\n" +
                    "2x = 约1/2计划停站\n" +
                    "3x = 约1/3（推荐）\n" +
                    "5x = 约1/5（最大）";
            }

            // One helper keeps the status tooltips consistent while explaining
            // which modes Better Boarding can actually skip late passengers for.
            string StatusDescription(string transitName, bool skipSupported)
            {
                string skippedText = skipSupported
                    ? "**跳过** = 今天跳过迟到乘客，避免车辆继续等待。\n" +
                      "Better Boarding 比 vanilla 对卡住车辆约10分钟的兜底处理更早介入。\n" +
                      $"由 <{ToggleName}> 控制。\n"
                    : "**跳过** = 此模式为0。迟到乘客继续由 vanilla 处理。\n";

                return
                    $"<{transitName}当前状态>\n" +
                    "**等待** = 当前等待乘客数。\n" +
                    "**平均** = 这些乘客的平均等待时间。\n" +
                    "**最差** = 单个站点最高平均等待时间。\n" +
                    "过高时请检查拥堵、站点卡住/异常或车辆不足。\n" +
                    skippedText +
                    "用<统计写入日志>查看站点名、Entity ID等详细信息。";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "操作" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "关于" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "上下客速度" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "行为" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "状态" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Mod信息" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "链接" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "调试" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "公交上下客速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "公交站",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "轨道上下客速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "火车、有轨电车和地铁站",
                        "适用于火车、有轨电车和地铁。\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "飞机上下客速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "机场航站楼",
                        "适用于客运飞机航站楼。\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "离站点太远的迟到乘客可以被跳过，避免车辆继续等待。\n" +
                    "Better Boarding 比 vanilla 对卡住车辆约10分钟的兜底处理更早介入。\n" +
                    "- 在站点/车站内或附近的乘客会保持分配并继续上车。\n" +
                    "- 长火车/地铁站台继续受到保护。\n" +
                    "- 群组/家庭有额外保护。\n" +
                    "- 被跳过的 Cim 仍留在城市中；vanilla 可以继续或重新规划行程，通常会乘坐之后的车辆。\n" +
                    "- 轮船、渡轮和飞机继续使用 vanilla 的迟到乘客处理。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cim 提前跑步" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "已分配的 Cim 会<提前开始跑步>，更容易在交通工具发车前赶上。\n" +
                    "- 最多比 vanilla 提前约3分钟游戏时间开始。\n" +
                    "- 对长站台和正在接近站点的 Cim 很有帮助。\n" +
                    "- 只影响已分配给正在上下客车辆的 Cim。\n" +
                    "- 不传送 Cim，也不改变发车时间。"
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "总使用量" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "来自游戏交通信息视图的每月公共交通使用量。\n" +
                    "更新时间 = 获取此状态快照的时间。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cim 提前跑" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "统计今天被 Better Boarding **提前跑步**的 Cim。\n" +
                    "最多比 vanilla 提前约3分钟游戏时间。"
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "公交" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("公交", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "有轨电车" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("有轨电车", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "火车" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("火车", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "地铁" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("地铁", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "渡轮" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("渡轮", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "轮船" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("轮船", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "飞机" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("飞机", skipSupported: false) },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "统计写入日志" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "向 **BetterBoarding.log** 写入一次详细报告。\n" +
                    "包括等待总数、最差的3个站点、被跳过的迟到乘客示例、Entity ID 和线路提示。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "打开日志" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "如果存在，则打开 **BetterBoarding.log**。\n" +
                    "如果还没有，则打开 Logs 文件夹。"
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Mod显示名称。" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "版本" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "当前Mod版本。" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "打开作者的 Paradox Mods 页面。" },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "启用详细日志" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**仅限 Debug / 测试**\n" +
                    "正常游玩时请保持关闭。\n" +
                    "城市运行时向 <Logs/BetterBoarding.log> 写入<实时>详情。\n" +
                    "可能降低性能并生成很大的日志文件。\n" +
                    "排查问题时使用10-15分钟，然后再次关闭。\n" +
                    "<统计写入日志> 是更轻量的一次性快照。"
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "状态未加载。" },
                { WaitStatus.KeyNoCityLoaded, "未加载城市。" },
                { WaitStatus.KeyNoStopsFound, "未找到站点。" },

                { WaitStatus.KeyStatusLine, "{0} 等待 | 平均 {1} | 最差 {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "跳过迟到乘客 {0}" },
                { WaitStatus.KeyStatusSkipOff, "不等待 OFF" },

                { WaitStatus.KeyStatusOverviewLine, "游客 {0}/月 | 市民 {1}/月 | 更新 {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "提前跑 OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] 请求统计报告，但当前未加载城市。" },
                { WaitStatus.KeyReportTitle, "统计日志快照 - Better Boarding" },
                { WaitStatus.KeyReportSettings, "设置：{0}" },
                { WaitStatus.KeyReportNote, "线路提示来自该站点等待时间最高的 waypoint。" },
                { WaitStatus.KeyReportTesterHintsHeader, "测试提示" },
                { WaitStatus.KeyReportHintWorstStops, "最差站点：先在游戏中或用 Scene Explorer 检查（通过 Entity ID 定位）。查看拥堵、位置不佳或站点异常。" },
                { WaitStatus.KeyReportHintSkippedCims, "被跳过的迟到乘客：发车后仍离得太远，因此 Better Boarding 不再让车辆继续等他们。后续示例会显示之后发生了什么。" },
                { WaitStatus.KeyReportHintLateGroups, "群组上车：显示生成报告时仍未完成上车的家庭/群组。Better Boarding 会提供额外宽限，并帮助落后的成员完成上车。" },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "服务站点：{0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "有乘客等待的站点：{0}" },
                { WaitStatus.KeyReportWaitingPassengers, "等待乘客：{0}" },
                { WaitStatus.KeyReportAverageWait, "平均等待：{0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "今日跳过迟到乘客：{0}" },
                { WaitStatus.KeyReportWorstStopNone, "最差站点：无；当前没有乘客等待。" },
                { WaitStatus.KeyReportWorstStopAverageWait, "最差站点平均等待：{0}" },
                { WaitStatus.KeyReportWorstStopName, "最差站点名称：{0}" },
                { WaitStatus.KeyReportWorstStopEntity, "最差站点 Entity：{0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "最差 waypoint Entity：{0}" },
                { WaitStatus.KeyReportWorstLineHint, "最差线路提示：{0}" },
                { WaitStatus.KeyReportWorstLineEntity, "最差线路 Entity：{0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "最差线路 waypoint 平均：{0}，等待 {1}" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "按平均等待排序的最差 {0} 个站点：" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | 平均 {2} | 等待 {3} | 站点 {4} | waypoint {5} | 线路 {6} | 提示 {7}" },
                { WaitStatus.KeyReportLateGroups, "群组上车未完成：{0} 名乘客 / {1} 个群组 / {2} 辆车" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "被跳过的迟到乘客示例" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | 乘客 {2} | 错过车辆 {3} | 时间 {4} | 当前 {5}" },
                { WaitStatus.KeyReportNone, "无" },
                { WaitStatus.KeyReportUnknown, "(未知)" },
            };
        }

        public void Unload()
        {
        }
    }
}
