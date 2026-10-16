// <copyright file="LocaleJA.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleJA.cs
// Purpose: Japanese ja-JP locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Japanese localization source.
    /// </summary>
    public sealed class LocaleJA : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        public LocaleJA(BBoardSettings setting)
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

            const string ToggleName = "遅れた乗客を待たない";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"値を上げると{transitName}での乗降が速くなります。\n" +
                    "通常の乗降による停車時間を短縮します。\n" +
                    "<==========================>\n" +
                    "1x = vanilla の100%\n" +
                    "2x = 予定停車時間の約1/2\n" +
                    "3x = 約1/3（推奨）\n" +
                    "5x = 約1/5（最大）";
            }

            // One helper keeps the status tooltips consistent while explaining
            // which modes Better Boarding can actually skip late passengers for.
            string StatusDescription(string transitName, bool skipSupported)
            {
                string skippedText = skipSupported
                    ? "**スキップ** = 今日、交通が待ち続けないようスキップした遅れた乗客数。\n" +
                      "Better Boarding は、vanilla の約10分の車両詰まり対策よりずっと早く対応します。\n" +
                      $"<{ToggleName}> で制御します。\n"
                    : "**スキップ** = この交通では0。遅れた乗客は vanilla の処理を使います。\n";

                return
                    $"<{transitName}の現在状況>\n" +
                    "**待ち** = 現在待っている乗客数。\n" +
                    "**平均** = その乗客の平均待ち時間。\n" +
                    "**最悪** = 1停留所で最も長い平均待ち時間。\n" +
                    "長い場合は渋滞、停留所の詰まり/不具合、車両不足を確認してください。\n" +
                    skippedText +
                    "<ログに統計>で停留所名、Entity IDなどの詳細を確認できます。";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "操作" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "情報" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "乗降速度" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "動作" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "状況" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Mod情報" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "リンク" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "デバッグ" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "バス乗降速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "バス停",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "鉄道乗降速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "鉄道・路面電車・地下鉄の停留所",
                        "鉄道、路面電車、地下鉄に適用します。\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "航空機乗降速度" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "空港ターミナル",
                        "旅客機ターミナルに適用します。\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "停留所から遠すぎる遅れた乗客をスキップし、交通が待ち続けないようにします。\n" +
                    "Better Boarding は、vanilla の約10分の車両詰まり対策よりずっと早く対応します。\n" +
                    "- 停留所/駅の近くにいる乗客は割り当てを維持し、そのまま乗車します。\n" +
                    "- 長い鉄道/地下鉄ホームも保護されます。\n" +
                    "- グループ/家族は追加保護されます。\n" +
                    "- スキップされたCimは街に残り、vanilla が移動を続行・再経路化し、後の車両に乗ることもあります。\n" +
                    "- 船、フェリー、航空機は遅れた乗客を vanilla のまま処理します。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cimを早めに走らせる" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "割り当て済みのCimは、交通に間に合うよう<早めに走り始めます>。\n" +
                    "- vanilla よりゲーム内で最大約3分早く開始します。\n" +
                    "- 長いホームや停留所へ向かう途中で役立ちます。\n" +
                    "- 乗客を乗せている車両にすでに割り当てられたCimだけが対象です。\n" +
                    "- Cimをテレポートさせたり、出発時刻を変えたりしません。"
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "総利用数" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "ゲームの交通情報ビューにある月間公共交通利用数です。\n" +
                    "更新 = このステータスを取得した時刻。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "早走りCim" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Better Boarding が今日**早めに走らせた**Cim数。\n" +
                    "vanilla よりゲーム内で最大約3分早く開始します。"
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "バス" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("バス", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "路面電車" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("路面電車", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "鉄道" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("鉄道", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "地下鉄" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("地下鉄", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "フェリー" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("フェリー", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "船" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("船", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "航空機" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("航空機", skipSupported: false) },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "ログに統計" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "**BetterBoarding.log** に1回だけ詳細レポートを書き込みます。\n" +
                    "待ち人数、ワースト3停留所、遅れてスキップされた乗客例、Entity ID、路線ヒントを含みます。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "ログを開く" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "**BetterBoarding.log** があれば開きます。\n" +
                    "まだ無い場合は Logs フォルダーを開きます。"
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Modの表示名。" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "バージョン" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "現在のModバージョン。" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "作者の Paradox Mods ページを開きます。" },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "詳細ログを有効化" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**デバッグ / テスト専用**\n" +
                    "通常プレイではオフにしてください。\n" +
                    "都市実行中に <Logs/BetterBoarding.log> へ<リアルタイム>詳細を書きます。\n" +
                    "性能低下や大きなログファイルの原因になります。\n" +
                    "問題調査時に10～15分使い、その後オフにしてください。\n" +
                    "<ログに統計> は軽い1回限りのスナップショットです。"
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "状況未読込" },
                { WaitStatus.KeyNoCityLoaded, "都市が読み込まれていません。" },
                { WaitStatus.KeyNoStopsFound, "停留所がありません。" },

                { WaitStatus.KeyStatusLine, "{0}人待ち | 平均 {1} | 最悪 {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} 遅れスキップ" },
                { WaitStatus.KeyStatusSkipOff, "待たない OFF" },

                { WaitStatus.KeyStatusOverviewLine, "観光 {0}/月 | 市民 {1}/月 | 更新 {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "早走り OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] 統計レポートを要求しましたが、都市が読み込まれていません。" },
                { WaitStatus.KeyReportTitle, "ログ統計スナップショット - Better Boarding" },
                { WaitStatus.KeyReportSettings, "設定: {0}" },
                { WaitStatus.KeyReportNote, "路線ヒントは、その停留所で待ち時間が最も長い waypoint から取得します。" },
                { WaitStatus.KeyReportTesterHintsHeader, "テスト用ヒント" },
                { WaitStatus.KeyReportHintWorstStops, "ワースト停留所: まずゲーム内または Scene Explorer で確認してください（Entity IDで場所を特定）。渋滞、位置、不具合を確認します。" },
                { WaitStatus.KeyReportHintSkippedCims, "遅れてスキップした乗客: 出発後も遠すぎた乗客をスキップし、車両が待ち続けないようにしました。後続例でその後を確認できます。" },
                { WaitStatus.KeyReportHintLateGroups, "グループ乗車: レポート取得時にまだ乗車完了していない家族/グループです。Better Boarding は追加猶予を与え、遅れたメンバーの乗車完了を助けます。" },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "運行停留所: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "待ち客がいる停留所: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "待ち乗客: {0}" },
                { WaitStatus.KeyReportAverageWait, "平均待ち時間: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "本日スキップした遅れ乗客: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "ワースト停留所: なし。現在待ち客はいません。" },
                { WaitStatus.KeyReportWorstStopAverageWait, "ワースト停留所の平均待ち: {0}" },
                { WaitStatus.KeyReportWorstStopName, "ワースト停留所名: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "ワースト停留所Entity: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "ワースト waypoint Entity: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "ワースト路線ヒント: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "ワースト路線Entity: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "ワースト路線 waypoint 平均: {0} / 待ち {1}" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "平均待ち時間ワースト{0}停留所:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | 平均 {2} | 待ち {3} | 停留所 {4} | waypoint {5} | 路線 {6} | ヒント {7}" },
                { WaitStatus.KeyReportLateGroups, "グループ乗車未完了: {0}人 / {1}グループ / {2}台" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "遅れてスキップした乗客の例" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | 乗客 {2} | 乗り遅れ車両 {3} | 時刻 {4} | 現在 {5}" },
                { WaitStatus.KeyReportNone, "なし" },
                { WaitStatus.KeyReportUnknown, "(不明)" },
            };
        }

        public void Unload()
        {
        }
    }
}
