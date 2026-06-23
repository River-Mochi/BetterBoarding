// <copyright file="LocaleKO.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License. You may not use this file except in compliance with this License.
// See LICENSE file in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleKO.cs
// Purpose: Korean ko-KR locale entries for Fast Boarding.

namespace FastBoarding
{
    using Colossal;
    using System.Collections.Generic;

    /// <summary>
    /// Korean localization source.
    /// </summary>
    public sealed class LocaleKO : IDictionarySource
    {
        private readonly Setting m_Setting;

        /// <summary>
        /// Constructs the Korean locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleKO(Setting setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all Korean localization entries for this mod.
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

            const string ToggleName = "늦은 승객 건너뛰기";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"값이 높을수록 {transitName}의 탑승 및 적재 시간이 줄어듭니다.\n" +
                    $"3x가 권장 기본값입니다.\n" +
                    $"5x가 최대값입니다.\n" +
                    $"일반 대기열은 더 빨리 줄어들지만, vanilla 설계상 늦은 승객이 여전히 출발을 지연시킬 수 있습니다.\n" +
                    $"출발 시간 이후 늦은 cim이 차량을 놓치게 하려면 [✓] <{ToggleName}>를 사용하세요.\n" +
                    $"건너뛴 늦은 시민은 삭제되지 않으며, 게임이 자연스럽게 경로를 다시 배정합니다.\n" +
                    "<==========================>\n" +
                    "적재 값:\n" +
                    "1x = 100% vanilla 정차\n" +
                    "2x = 계획 정차의 약 1/2\n" +
                    "3x = 계획 정차의 약 1/3 (권장)\n" +
                    "5x = 계획 정차의 약 1/5 (최대)\n" +
                    $"이것은 <{ToggleName}>와 같지 않습니다. 해당 체크박스는 출발 시간 이후 늦은 cim이 {shortName}을 놓칠 수 있는지를 결정합니다.";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<현재 {transitName} 상태>\n" +
                    "**대기 중** = 지금 대기 중인 총 승객 수.\n" +
                    "**평균** = 해당 승객들의 평균 대기 시간.\n" +
                    "**최악** 정류장 = 한 정류장의 가장 높은 평균 대기 시간.\n" +
                    "최악 정류장은 사고, 막힌/버그난 정류장, 배정 차량 부족을 확인하기 좋은 곳입니다.\n" +
                    $"**오늘 지각** = 오늘 <{ToggleName}>로 건너뛴 혼자 늦은 승객.\n" +
                    "<Stats를 로그로>를 사용하면 정류장 이름, 엔티티 ID 등 자세한 보고서를 기록합니다.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.ActionsTab), "작업" },
                { m_Setting.GetOptionTabLocaleID(Setting.AboutTab), "정보" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.SpeedGroup), "탑승 속도" },
                { m_Setting.GetOptionGroupLocaleID(Setting.BehaviorGroup), "동작" },
                { m_Setting.GetOptionGroupLocaleID(Setting.StatusGroup), "상태" },
                { m_Setting.GetOptionGroupLocaleID(Setting.AboutInfoGroup), "모드 정보" },
                { m_Setting.GetOptionGroupLocaleID(Setting.AboutLinksGroup), "링크" },
                { m_Setting.GetOptionGroupLocaleID(Setting.DebugGroup), "디버그" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.BusBoardingSpeedFactor)), "버스 탑승 속도" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "버스 정류장",
                        "버스",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RailBoardingSpeedFactor)), "철도 탑승 속도" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "기차, 트램, 지하철 정류장",
                        "차량",
                        "기차, 트램, 지하철 정류장에 적용됩니다.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.WaterBoardingSpeedFactor)), "선박 + 페리 속도" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "선박 및 페리 정류장",
                        "차량",
                        "선박 및 페리 정류장에 적용됩니다.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AirBoardingSpeedFactor)), "항공기 속도" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "항공기 터미널",
                        "항공기",
                        "여객기 터미널에 적용됩니다.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.CancelLateBoarders)),
                    "출발 시간 이후에도 <준비 안 됨> 상태인 늦은 승객은 차량을 놓칠 수 있습니다.\n" +
                    "참고: 혼자 늦은 시민만 건너뜁니다.\n" +
                    "함께 이동하는 그룹/가족이 늦은 경우에는 <건너뛰지 않으며>, vanilla처럼 대중교통 지연을 계속 일으킬 수 있습니다.\n" +
                    "그룹은 군중의 작은 부분입니다. 대부분의 효과는 늦게 뛰어오는 혼자 cim을 건너뛰는 데서 옵니다.\n" +
                    "건너뛴 늦은 시민은 삭제되지 않으며, 게임이 자연스럽게 다시 배정합니다."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.CimsRunSoonerToCatchBuses)), "더 일찍 달리기: 버스+트램" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.CimsRunSoonerToCatchBuses)),
                    "<늦은> 시민이 출발 시간 **전**에 도착해 보도록 <더 일찍 달리기> 시작합니다.\n" +
                    "버스/트램이 일정대로 움직이는 데 도움을 줍니다.\n" +
                    "현재 승차 중인 차량에 이미 배정된 cim에게만 적용됩니다.\n" +
                    "바닐라는 출발 시간이 되어야 cim이 달리기 시작해서 너무 늦을 수 있습니다.\n" +
                    $"<{ToggleName}>와 잘 맞으며, 차량을 놓쳐 다시 배정되어야 하는 cim 수를 줄일 수 있습니다.\n" +
                    "강제 승차나 시민 텔레포트는 하지 않습니다."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusOverview)), "전체 이용량" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusOverview)),
                    "게임의 교통 정보 보기에서 가져온 월간 대중교통 이용량입니다.\n" +
                    "업데이트 시간은 이 상태 스냅샷을 찍은 시각을 표시합니다(보통 옵션 메뉴에 들어간 뒤)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusCimsRunSooner)), "cim 더 일찍 달리기" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusCimsRunSooner)),
                    "켜져 있으면 [x], 오늘 출발 전에 버스/트램을 잡으려고 **더 일찍 달리기 시작한** cim을 셉니다.\n" +
                    "cim은 vanilla보다 512프레임 일찍 달립니다(실시간 약 2-8초 빠름, 게임 내 약 2분)."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusBus)), "버스" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusBus)), StatusDescription("버스") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusTram)), "트램" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusTram)), StatusDescription("트램") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusTrain)), "기차" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusTrain)), StatusDescription("기차") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusSubway)), "지하철" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusSubway)), StatusDescription("지하철") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusFerry)), "페리" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusFerry)), StatusDescription("페리") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusShip)), "선박" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusShip)), StatusDescription("선박") },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatusAir)), "항공기" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatusAir)), StatusDescription("항공기") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StatsToLog)), "Stats를 로그로" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.StatsToLog)),
                    "**FastBoarding.log**에 일회성 자세한 보고서를 기록합니다.\n" +
                    "대기 총합, 모드별 최악 정류장 Top 3, 건너뛴 cim 예시, 엔티티 ID, 노선 힌트를 포함합니다."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenLog)), "로그 열기" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenLog)),
                    "존재하면 **FastBoarding.log**를 엽니다.\n" +
                    "파일을 아직 찾을 수 없으면 Logs 폴더를 대신 엽니다."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AboutName)), "모드" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AboutName)), "이 모드의 표시 이름입니다." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AboutVersion)), "버전" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AboutVersion)), "현재 모드 버전입니다." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxMods)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadoxMods)), "제작자의 Paradox Mods 페이지를 엽니다." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableVerboseLogging)), "자세한 로그 사용" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableVerboseLogging)),
                    "**디버그 / 테스트 전용**\n" +
                    "도시가 실행되는 동안 <Logs/FastBoarding.log>에 <live> 세부 정보를 추가합니다.\n" +
                    "**일반 플레이에서는 켜지 마세요.**\n" +
                    "켜 둔 상태는 성능을 낮추고 거대한 로그 파일을 만들 수 있습니다.\n" +
                    "오래된 로그 파일은 나중에 삭제할 수 있습니다.\n" +
                    "참고: <Stats를 로그로>는 특정 시점 보고서와 오늘의 late-skip 카운터입니다.\n" +
                    "시간 흐름을 보고 싶으면 자세한 로그를 15-20분 동안 실행하세요.\n" +
                    "일반 플레이 전에 다시 **OFF**로 바꾸는 것을 잊지 마세요."
                },

                // Runtime status strings
                { TransitWaitStatus.KeyStatusNotLoaded, "상태를 불러오지 않았습니다." },
                { TransitWaitStatus.KeyNoCityLoaded, "불러온 도시가 없습니다." },
                { TransitWaitStatus.KeyNoStopsFound, "정류장을 찾지 못했습니다." },

                { TransitWaitStatus.KeyStatusLine, "{0} 대기 | 평균 {1} | 최악 {2} | {3}" },
                { TransitWaitStatus.KeyStatusLateSkipped, "{0} 오늘 지각" },
                { TransitWaitStatus.KeyStatusSkipOff, "스킵 OFF" },

                { TransitWaitStatus.KeyStatusOverviewLine, "관광객 {0}/월 | 시민 {1}/월 | 업데이트 {2}" },
                { TransitWaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { TransitWaitStatus.KeyStatusRunSoonerOff, "일찍 달리기 OFF" },

                // Stats-to-log report strings
                { TransitWaitStatus.KeyReportNoCityLoaded, "[FB] 통계 보고서가 요청되었지만 불러온 도시가 없습니다." },
                { TransitWaitStatus.KeyReportTitle, "Stats를 로그로 스냅샷 - Fast Boarding" },
                { TransitWaitStatus.KeyReportSettings, "설정: {0}" },
                { TransitWaitStatus.KeyReportNote, "노선 힌트는 해당 정류장에서 대기 시간이 가장 높은 waypoint에서 가져옵니다." },
                { TransitWaitStatus.KeyReportTesterHintsHeader, "테스터 힌트" },
                { TransitWaitStatus.KeyReportHintWorstStops, "최악 정류장: 먼저 게임 안이나 Scene Explorer 모드로 확인하세요(엔티티 ID로 위치 찾기). 교통, 나쁜 정류장 위치, 버그난 정류장을 확인하세요." },
                { TransitWaitStatus.KeyReportHintSkippedCims, "건너뛴 혼자 cim: 대중교통이 출발할 수 있도록 건너뛴 늦은 승객입니다. 이후 상태는 보통 'has path' 또는 'assigned'가 되어야 합니다. 'no path yet'에 머물면 시간이 더 지난 뒤 해당 cim 엔티티를 확인하세요." },
                { TransitWaitStatus.KeyReportHintLateGroups, "늦은 그룹(가족): 함께 있도록 의도적으로 바닐라에 맡겼습니다. 많은 단독 승객에 비해 적은 편입니다." },
                { TransitWaitStatus.KeyReportFamilyHeader, "{0}" },
                { TransitWaitStatus.KeyReportServedStops, "운행 정류장: {0}" },
                { TransitWaitStatus.KeyReportStopsWithWaiting, "대기 승객이 있는 정류장: {0}" },
                { TransitWaitStatus.KeyReportWaitingPassengers, "대기 승객: {0}" },
                { TransitWaitStatus.KeyReportAverageWait, "평균 대기: {0}" },
                { TransitWaitStatus.KeyReportLateBoardersSkipped, "오늘 건너뛴 늦은 승객: {0}" },
                { TransitWaitStatus.KeyReportWorstStopNone, "최악 정류장: 없음, 현재 대기 승객이 없습니다." },
                { TransitWaitStatus.KeyReportWorstStopAverageWait, "최악 정류장 평균 대기: {0}" },
                { TransitWaitStatus.KeyReportWorstStopName, "최악 정류장 이름: {0}" },
                { TransitWaitStatus.KeyReportWorstStopEntity, "최악 정류장 엔티티: {0}" },
                { TransitWaitStatus.KeyReportWorstWaypointEntity, "최악 waypoint 엔티티: {0}" },
                { TransitWaitStatus.KeyReportWorstLineHint, "최악 노선 힌트: {0}" },
                { TransitWaitStatus.KeyReportWorstLineEntity, "최악 노선 엔티티: {0}" },
                { TransitWaitStatus.KeyReportWorstLineWaypointAverage, "최악 노선 waypoint 평균: {0}, 대기 {1}" },
                { TransitWaitStatus.KeyReportTopWorstStopsHeader, "평균 대기 기준 최악 정류장 Top {0}:" },
                { TransitWaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | 평균 {2} | 대기 {3} | 정류장 {4} | waypoint {5} | 노선 {6} | 힌트 {7}" },
                { TransitWaitStatus.KeyReportLateGroups, "그룹으로 이동 중인 늦은 cim은 그대로 둠: 승객 {0}명, 그룹 {1}개, 차량 {2}대" },
                { TransitWaitStatus.KeyReportLastSkippedSamplesHeader, "건너뛴 혼자 늦은 cim 예시" },
                { TransitWaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | 승객 {2} | 놓친 차량 {3} | 시간 {4} | 현재 {5}" },
                { TransitWaitStatus.KeyReportNone, "없음" },
                { TransitWaitStatus.KeyReportUnknown, "(알 수 없음)" },
            };
        }

        public void Unload()
        {
        }
    }
}
