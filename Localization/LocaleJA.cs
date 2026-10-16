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
                    "乗降倍率:\n" +
                    "1x = vanilla の100%\n" +
                    "2x = 予定停車時間の約1/2\n" +
                    "3x = 約1/3（推奨）\n" +
                    "5x = 約1/5（最大）";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<{transitName}の現在状況>\n" +
                    "**待ち** = 現在待っている乗客数。\n" +
                    "**平均** = その乗客の平均待ち時間。\n" +
                    "**最悪** = 1停留所で最も長い平均待ち時間。\n" +
                    "長い場合は渋滞、停留所の詰まり/不具合、車両不足を確認してください。\n" +
                    $"**本日解放** = <{ToggleName}>で解除した遅延乗客の割り当て。\n" +
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
                    "割り当て済みの乗客が明らかに遠すぎる場合、バス・路面電車・鉄道・地下鉄が出発できるようにします。\n" +
                    "- 停留所や駅の近くにいる乗客は割り当てを維持し、そのまま乗車します。\n" +
                    "- 長い鉄道・地下鉄ホーム上の乗客は保護されます。\n" +
                    "- 明らかに遠い乗客だけ、予定出発時刻と短い猶予の後に割り当て解除できます。\n" +
                    "- グループ/家族は追加保護され、子どもやペットが乗り終えられます。\n" +
                    "- 船、フェリー、航空機は遅延乗客を vanilla のまま処理します。\n" +
                    "解除されたCimは街に残り、通常どおり移動を続けるか経路を再計算します。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cimを早めに走らせる" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "割り当て済みCimが<早めに走り始め>、バス・路面電車・鉄道・地下鉄の出発に間に合いやすくします。\n" +
                    "- 長いホームや停留所へ向かう途中で役立ちます。\n" +
                    "- vanilla より最大でゲーム内約3分早く開始します。\n" +
                    "- 乗降中の車両にすでに割り当てられたCimだけが対象です。\n" +
                    "- テレポート、強制乗車、出発時刻の変更はしません。"
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "総利用数" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "ゲームの交通情報ビューにある月間公共交通利用数です。\n" +
                    "更新時刻はこの状況を取得した時刻です（通常はオプションを開いた時）。"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "早走りCim" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "有効 [x] の場合、Better Boarding が今日**早めに走らせた**Cimを数えます。対象はバス・路面電車・鉄道・地下鉄です。\n" +
                    "Run Sooner は vanilla より最大でゲーム内約3分早く開始します。"
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "バス" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("バス") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "路面電車" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("路面電車") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "鉄道" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("鉄道") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "地下鉄" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("地下鉄") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "フェリー" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("フェリー") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "船" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("船") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "航空機" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("航空機") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "ログに統計" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "**BetterBoarding.log** に1回だけ詳細レポートを書き込みます。\n" +
                    "待ち人数、各交通のワースト3停留所、解放された乗客例、Entity ID、路線情報を含みます。"
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
                    "通常は必ず **OFF** にしてください。\n" +
                    "街の実行中、<Logs/BetterBoarding.log> に<ライブ>詳細を書き込みます。\n" +
                    "**通常プレイでは有効にしないでください。**\n" +
                    "性能低下や巨大なログの原因になります。\n" +
                    "古いログは後で削除できます。\n" +
                    "<ログに統計>は1回のスナップショットだけで、詳細ログより負荷が小さいです。\n" +
                    "経過確認が必要なときだけ10～15分有効にしてください。大量のデータが出ます。\n" +
                    "通常プレイ前に必ず **OFF** に戻してください。"
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "状況未読込" },
                { WaitStatus.KeyNoCityLoaded, "都市が読み込まれていません。" },
                { WaitStatus.KeyNoStopsFound, "停留所がありません。" },

                { WaitStatus.KeyStatusLine, "{0}人待ち | 平均 {1} | 最悪 {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "本日 {0}件解放" },
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
                { WaitStatus.KeyReportHintSkippedCims, "解放した単独割り当て: 各カウントは距離確認後に解除した車両割り当て1件です。追跡例でその後を確認できます。" },
                { WaitStatus.KeyReportHintLateGroups, "グループ乗車: レポート取得時にまだ乗車完了していない家族/グループです。Better Boarding は追加猶予を与え、遅れたメンバーの乗車完了を助けます。" },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "運行停留所: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "待ち客がいる停留所: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "待ち乗客: {0}" },
                { WaitStatus.KeyReportAverageWait, "平均待ち時間: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "本日解放した単独割り当て: {0}" },
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
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "解放した単独乗客の例" },
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
