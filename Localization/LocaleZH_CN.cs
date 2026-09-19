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

        /// <summary>
        /// Constructs the Simplified Chinese locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleZH_CN(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all Simplified Chinese localization entries for this mod.
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

            const string ToggleName = "跳过迟到乘客";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"较高的数值会减少{transitName}的上车和装载时间。\n" +
                    $"3x 是推荐默认值。\n" +
                    $"5x 是最大值。\n" +
                    $"这能帮助普通队列更快清空，但由于 vanilla 设计，迟到乘客仍可能拖延发车。\n" +
                    $"如果想让迟到 cim 在发车时间后错过车辆，请使用 [✓] <{ToggleName}>。\n" +
                    $"被跳过的迟到市民不会被删除；游戏会自然为他们重新规划路线。\n" +
                    "<==========================>\n" +
                    "装载值:\n" +
                    "1x = 100% vanilla 停靠\n" +
                    "2x = 约 1/2 计划停靠\n" +
                    "3x = 约 1/3 计划停靠（推荐）\n" +
                    "5x = 约 1/5 计划停靠（最大）\n" +
                    $"这不同于 <{ToggleName}>；该复选框决定迟到 cim 是否能在发车时间后错过{shortName}。";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<当前{transitName}状态>\n" +
                    "**等待中** = 当前正在等待的乘客总数。\n" +
                    "**平均** = 这些乘客的平均等待时间。\n" +
                    "**最差**站点 = 单个站点的最高平均等待时间。\n" +
                    "最差站点适合优先检查事故、堵塞/异常站点，或是否需要分配更多车辆。\n" +
                    $"**今日迟到** = 今天由 <{ToggleName}> 跳过的单独迟到乘客。\n" +
                    "使用 <统计写入日志> 可输出详细报告：站点名称、实体 ID 等。";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "操作" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "关于" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "上车速度" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "行为" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "状态" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "模组信息" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "链接" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "调试" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "公交上车速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "公交站",
                        "公交",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "轨道上车速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "火车、有轨电车和地铁站",
                        "车辆",
                        "适用于火车、有轨电车和地铁站。\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "船舶 + 渡轮速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "船舶和渡轮站",
                        "车辆",
                        "适用于船舶和渡轮站。\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "飞机速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "飞机航站楼",
                        "飞机",
                        "适用于客运飞机航站楼。\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "发车时间后仍然<未准备好>的迟到乘客可以错过车辆。\n" +
                    "注意：只跳过单独迟到的市民。\n" +
                    "一起出行的迟到团体/家庭<不会被跳过>，仍可能像 vanilla 一样造成公共交通延误。\n" +
                    "团体只占人群的一小部分；主要收益来自跳过迟到奔跑的单独 cim。\n" +
                    "被跳过的迟到市民不会被删除；游戏会自然重新分配他们。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "提前奔跑：公交+有轨电车+火车" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "<迟到>市民会<提前奔跑>，尝试在发车时间**之前**赶到。\n" +
                    "帮助公交、有轨电车和火车保持准点，尤其适合较长的火车站台。\n" +
                    "只影响已分配到当前正在上客车辆的市民。\n" +
                    "原版只会在发车时间才让市民开始奔跑，这可能已经太晚。\n" +
                    $"和 <{ToggleName}> 配合很好，因为它可能减少错过车辆并需要重新分配的市民数量。\n" +
                    "不会更改车辆发车时间、强制上车或传送市民。"
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "总使用量" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "来自游戏交通信息视图的每月公共交通使用量。\n" +
                    "更新时间显示此状态快照的获取时间（通常是在进入选项菜单后）。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "市民提前奔跑" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "启用 [x] 时，统计今天开始**提前奔跑**、尝试在发车前赶上公交、有轨电车或火车的市民。\n" +
                    "市民会比 vanilla 提前 512 帧奔跑（现实时间约提前 2-8 秒，游戏内约 2 分钟）。"
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "公交" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("公交") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "有轨电车" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("有轨电车") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "火车" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("火车") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "地铁" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("地铁") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "渡轮" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("渡轮") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "船舶" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("船舶") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "飞机" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("飞机") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "统计写入日志" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "向 **BetterBoarding.log** 写入一次性详细报告。\n" +
                    "包括等待总数、每种模式最差的 3 个站点、被跳过 cim 示例、实体 ID 和线路提示。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "打开日志" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "如果存在，则打开 **BetterBoarding.log**。\n" +
                    "如果尚未找到该文件，则改为打开 Logs 文件夹。"
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "模组" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "此模组的显示名称。" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "版本" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "当前模组版本。" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "打开作者的 Paradox Mods 页面。" },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "启用详细日志" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**仅用于调试 / 测试**\n" +
                    "城市运行时向 <Logs/BetterBoarding.log> 添加 <live> 详细信息。\n" +
                    "**不要在正常游玩时启用。**\n" +
                    "保持开启可能降低性能并生成巨大的日志文件。\n" +
                    "以后可以删除旧日志文件。\n" +
                    "注意：<统计写入日志> 是某一时刻的报告，加上今天的 late-skip 计数器。\n" +
                    "如需查看事件时间线，请运行详细日志 15-20 分钟。\n" +
                    "正常游玩前别忘了再次切回 **OFF**。"
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "状态未加载。" },
                { WaitStatus.KeyNoCityLoaded, "未加载城市。" },
                { WaitStatus.KeyNoStopsFound, "未找到站点。" },

                { WaitStatus.KeyStatusLine, "{0} 等待 | 平均 {1} | 最差 {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} 今日迟到" },
                { WaitStatus.KeyStatusSkipOff, "跳过 OFF" },

                { WaitStatus.KeyStatusOverviewLine, "{0} 游客/月 | {1} 市民/月 | 更新 {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "提前奔跑OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] 已请求统计报告，但未加载城市。" },
                { WaitStatus.KeyReportTitle, "统计写入日志快照 - Better Boarding" },
                { WaitStatus.KeyReportSettings, "设置: {0}" },
                { WaitStatus.KeyReportNote, "线路提示来自该站点等待最高的 waypoint。" },
                { WaitStatus.KeyReportTesterHintsHeader, "测试提示" },
                { WaitStatus.KeyReportHintWorstStops, "最差站点：先在游戏中或用 Scene Explorer 模组检查（可用实体 ID 找位置）。查找交通堵塞、站点位置不佳或异常站点。" },
                { WaitStatus.KeyReportHintSkippedCims, "被跳过的单独 cims：为让交通工具发车而跳过的迟到乘客。之后状态通常应变为 'has path' 或 'assigned'。如果仍是 'no path yet'，请过一段时间再检查该 cim 实体。" },
                { WaitStatus.KeyReportHintLateGroups, "迟到群组（家庭）：故意交给原版处理，让他们保持在一起并遵循原版行为；相比大量单独乘客，他们数量较少。" },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "服务站点: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "有等待乘客的站点: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "等待乘客: {0}" },
                { WaitStatus.KeyReportAverageWait, "平均等待: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "今天跳过的迟到乘客: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "最差站点：无，目前没有等待乘客。" },
                { WaitStatus.KeyReportWorstStopAverageWait, "最差站点平均等待: {0}" },
                { WaitStatus.KeyReportWorstStopName, "最差站点名称: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "最差站点实体: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "最差 waypoint 实体: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "最差线路提示: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "最差线路实体: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "最差线路 waypoint 平均: {0}，等待 {1}" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "按平均等待排名的最差站点前 {0} 名:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | 平均 {2} | 等待 {3} | 站点 {4} | waypoint {5} | 线路 {6} | 提示 {7}" },
                { WaitStatus.KeyReportLateGroups, "结伴出行的迟到市民保持原样：{0} 名乘客，{1} 个群组，涉及 {2} 辆车" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "被跳过的单独迟到 cim 示例" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | 乘客 {2} | 错过车辆 {3} | 时间 {4} | 现在 {5}" },
                { WaitStatus.KeyReportNone, "无" },
                { WaitStatus.KeyReportUnknown, "(未知)" },
            };
        }

        public void Unload()
        {
        }
    }
}
