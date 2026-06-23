// <copyright file="LocaleJA.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License. You may not use this file except in compliance with this License.
// See LICENSE file in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleJA.cs
// Purpose: Japanese ja-JP locale entries for Fast Boarding.

namespace FastBoarding
{
    using Colossal;
    using System.Collections.Generic;

    /// <summary>
    /// Japanese localization source.
    /// </summary>
    public sealed class LocaleJA : IDictionarySource
    {
        private readonly Setting m_Setting;

        /// <summary>
        /// Constructs the Japanese locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleJA(Setting setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all Japanese localization entries for this mod.
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

            const string ToggleName = "遅れた乗客をスキップ";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"値を上げると、{transitName}での乗車・積み込み時間が短くなります。\n" +
                    $"3x が推奨デフォルトです。\n" +
                    $"5x が最大です。\n" +
                    $"通常の行列は早く解消されますが、vanilla の仕様により、遅れた乗客が出発を遅らせることはあります。\n" +
                    $"出発時刻後に遅れた cim が車両を逃してよい場合は [✓] <{ToggleName}> を使ってください。\n" +
                    $"スキップされた遅れた市民は削除されません。ゲームが自然に経路を再割り当てします。\n" +
                    "<==========================>\n" +
                    "読み込み値:\n" +
                    "1x = 100% vanilla 停車\n" +
                    "2x = 予定停車の約 1/2\n" +
                    "3x = 予定停車の約 1/3（推奨）\n" +
                    "5x = 予定停車の約 1/5（最大）\n" +
                    $"これは <{ToggleName}> とは別です。このチェックボックスは、出発時刻後に遅れた cim が {shortName} を逃せるかどうかを決めます。";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<現在の{transitName}ステータス>\n" +
                    "**待機中** = 現在待っている乗客の合計。\n" +
                    "**平均** = その乗客たちの平均待ち時間。\n" +
                    "**最悪**停留所 = 1つの停留所で最も高い平均待ち時間。\n" +
                    "最悪の停留所は、事故、詰まり/バグった停留所、割り当て車両不足を調べるのに向いています。\n" +
                    $"**今日の遅れ** = <{ToggleName}> により今日スキップされた単独の遅れた乗客。\n" +
                    "<Statsをログへ> で、停留所名、エンティティIDなどを含む詳細レポートを出力します。";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.ActionsTab), "操作" },
                { m_Setting.GetOptionTabLocaleID(Setting.AboutTab), "情報" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.SpeedGroup), "乗車速度" },
                { m_Setting.GetOptionGroupLocaleID(Setting.BehaviorGroup), "動作" },
                { m_Setting.GetOptionGroupLocaleID(Setting.StatusGroup), "ステータス" },
                { m_Setting.GetOptionGroupLocaleID(Setting.AboutInfoGroup), "MOD情報" },
                { m_Setting.GetOptionGroupLocaleID(Setting.AboutLinksGroup), "リンク" },
                { m_Setting.GetOptionGroupLocaleID(Setting.DebugGroup), "デバッグ" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.BusBoardingSpeedFactor)), "バス乗車速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "バス停",
                        "バス",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RailBoardingSpeedFactor)), "鉄道乗車速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "列車・トラム・地下鉄の停車場",
                        "車両",
                        "列車、トラム、地下鉄の停車場に適用されます。\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.WaterBoardingSpeedFactor)), "船＋フェリー速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "船・フェリー停留所",
                        "車両",
                        "船とフェリーの停留所に適用されます。\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AirBoardingSpeedFactor)), "飛行機速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "飛行機ターミナル",
                        "飛行機",
                        "旅客機ターミナルに適用されます。\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.CancelLateBoarders)),
                    "出発時刻後も <準備未完了> の遅れた乗客は、車両を逃すことがあります。\n" +
                    "注: スキップするのは単独の遅れた市民のみです。\n" +
                    "一緒に移動するグループ/家族が遅れている場合は <スキップされません>。vanilla と同じように交通機関を遅らせることがあります。\n" +
                    "グループは群衆の一部にすぎません。主な効果は、遅れて走っている単独 cim のスキップから得られます。\n" +
                    "スキップされた遅れた市民は削除されません。ゲームにより自然に再割り当てされます。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.CimsRunSoonerToCatchBuses)), "早めに走る: バス+トラム" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.CimsRunSoonerToCatchBuses)),
                    "<遅れている>市民が出発時刻**前**に間に合うよう、<早めに走り始め>ます。\n" +
                    "バス/トラムの定時運行を助けます。\n" +
                    "現在乗車中の車両にすでに割り当てられている cim だけに影響します。\n" +
                    "vanilla では出発時刻になってから cim が走り始めるため、遅すぎる場合があります。\n" +
                    $"<{ToggleName}> と相性がよく、車両を逃して再割り当てが必要になる cim を減らせる場合があります。\n" +
                    "強制乗車や市民のテレポートは行いません。"
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusOverview)), "総利用状況" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusOverview)),
                    "ゲームの交通インフォビューから取得した月間公共交通利用数。\n" +
                    "更新時刻は、このステータス snapshot が取得された時刻です（通常はオプションメニューを開いた後）。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusCimsRunSooner)), "早めに走る cim" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusCimsRunSooner)),
                    "有効 [x] の場合、今日、出発前にバス/トラムへ間に合うよう **早めに走り始めた** cim を数えます。\n" +
                    "cim は vanilla より 512 フレーム早く走ります（実時間で約 2～8 秒早く、ゲーム内で約 2 分）。"
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusBus)), "バス" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusBus)), StatusDescription("バス") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusTram)), "トラム" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusTram)), StatusDescription("トラム") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusTrain)), "列車" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusTrain)), StatusDescription("列車") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusSubway)), "地下鉄" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusSubway)), StatusDescription("地下鉄") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusFerry)), "フェリー" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusFerry)), StatusDescription("フェリー") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusShip)), "船" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusShip)), StatusDescription("船") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusAir)), "飛行機" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusAir)), StatusDescription("飛行機") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatsToLog)), "Statsをログへ" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatsToLog)),
                    "**FastBoarding.log** に1回限りの詳細レポートを書き込みます。\n" +
                    "待機合計、モードごとのワースト停留所上位3件、スキップされた cim 例、エンティティID、路線ヒントを含みます。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenLog)), "ログを開く" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenLog)),
                    "存在する場合は **FastBoarding.log** を開きます。\n" +
                    "ファイルがまだ見つからない場合は、代わりに Logs フォルダーを開きます。"
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AboutName)), "このMODの表示名。" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AboutVersion)), "バージョン" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AboutVersion)), "現在のMODバージョン。" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxMods)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadoxMods)), "作者の Paradox Mods ページを開きます。" },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableVerboseLogging)), "詳細ログを有効化" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableVerboseLogging)),
                    "**デバッグ / テスト専用**\n" +
                    "都市実行中に <Logs/FastBoarding.log> へ <live> 詳細を追加します。\n" +
                    "**通常プレイでは有効にしないでください。**\n" +
                    "有効のままにすると、性能が下がり、巨大なログファイルが作成されることがあります。\n" +
                    "古いログファイルは後で削除できます。\n" +
                    "注: <Statsをログへ> は、その時点のレポートと今日の late-skip カウンターです。詳細ログとは異なります。\n" +
                    "時間経過の流れを見たい場合は、詳細ログを15～20分実行してください。\n" +
                    "通常プレイ前に **OFF** に戻すのを忘れないでください。"
                },

                // Runtime status strings
                { TransitWaitStatus.KeyStatusNotLoaded, "ステータス未読み込み。" },
                { TransitWaitStatus.KeyNoCityLoaded, "都市が読み込まれていません。" },
                { TransitWaitStatus.KeyNoStopsFound, "停留所が見つかりません。" },

                { TransitWaitStatus.KeyStatusLine, "{0} 待機 | 平均 {1} | 最悪 {2} | {3}" },
                { TransitWaitStatus.KeyStatusLateSkipped, "{0} 本日遅れ" },
                { TransitWaitStatus.KeyStatusSkipOff, "スキップ OFF" },

                { TransitWaitStatus.KeyStatusOverviewLine, "{0} 観光客/月 | {1} 市民/月 | 更新 {2}" },
                { TransitWaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { TransitWaitStatus.KeyStatusRunSoonerOff, "早走りOFF" },

                // Stats-to-log report strings
                { TransitWaitStatus.KeyReportNoCityLoaded, "[FB] 統計レポートが要求されましたが、都市が読み込まれていません。" },
                { TransitWaitStatus.KeyReportTitle, "Statsをログへ snapshot - Fast Boarding" },
                { TransitWaitStatus.KeyReportSettings, "設定: {0}" },
                { TransitWaitStatus.KeyReportNote, "路線ヒントは、その停留所で最も待ち時間が高い waypoint から取得されます。" },
                { TransitWaitStatus.KeyReportTesterHintsHeader, "テスター向けヒント" },
                { TransitWaitStatus.KeyReportHintWorstStops, "最悪の停留所: まずゲーム内または Scene Explorer mod で確認してください（エンティティIDで場所を探せます）。交通、悪い停留所配置、バグった停留所を探します。" },
                { TransitWaitStatus.KeyReportHintSkippedCims, "スキップされた単独 cim: 交通機関を出発させるためにスキップした遅れた乗客です。後の状態は通常 'has path' または 'assigned' になります。'no path yet' のままなら、時間を置いてその cim エンティティを確認してください。" },
                { TransitWaitStatus.KeyReportHintLateGroups, "遅れたグループ（家族）: 一緒にいられるよう、意図的に vanilla に任せています。単独旅行者が多い中では少数です。" },
                { TransitWaitStatus.KeyReportFamilyHeader, "{0}" },
                { TransitWaitStatus.KeyReportServedStops, "提供中の停留所: {0}" },
                { TransitWaitStatus.KeyReportStopsWithWaiting, "待機乗客がいる停留所: {0}" },
                { TransitWaitStatus.KeyReportWaitingPassengers, "待機乗客: {0}" },
                { TransitWaitStatus.KeyReportAverageWait, "平均待ち時間: {0}" },
                { TransitWaitStatus.KeyReportLateBoardersSkipped, "今日スキップされた遅れた乗客: {0}" },
                { TransitWaitStatus.KeyReportWorstStopNone, "最悪の停留所: なし。現在待機乗客はいません。" },
                { TransitWaitStatus.KeyReportWorstStopAverageWait, "最悪停留所の平均待ち時間: {0}" },
                { TransitWaitStatus.KeyReportWorstStopName, "最悪停留所名: {0}" },
                { TransitWaitStatus.KeyReportWorstStopEntity, "最悪停留所エンティティ: {0}" },
                { TransitWaitStatus.KeyReportWorstWaypointEntity, "最悪 waypoint エンティティ: {0}" },
                { TransitWaitStatus.KeyReportWorstLineHint, "最悪路線ヒント: {0}" },
                { TransitWaitStatus.KeyReportWorstLineEntity, "最悪路線エンティティ: {0}" },
                { TransitWaitStatus.KeyReportWorstLineWaypointAverage, "最悪路線 waypoint 平均: {0}、待機 {1}" },
                { TransitWaitStatus.KeyReportTopWorstStopsHeader, "平均待ち時間による最悪停留所 Top {0}:" },
                { TransitWaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | 平均 {2} | 待機 {3} | 停留所 {4} | waypoint {5} | 路線 {6} | ヒント {7}" },
                { TransitWaitStatus.KeyReportLateGroups, "グループで移動中の遅れた cim はそのまま: {0} 人、{1} グループ、{2} 台の車両" },
                { TransitWaitStatus.KeyReportLastSkippedSamplesHeader, "スキップされた遅れた単独 cim の例" },
                { TransitWaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | 乗客 {2} | 逃した車両 {3} | 時刻 {4} | 現在 {5}" },
                { TransitWaitStatus.KeyReportNone, "なし" },
                { TransitWaitStatus.KeyReportUnknown, "(不明)" },
            };
        }

        public void Unload()
        {
        }
    }
}
