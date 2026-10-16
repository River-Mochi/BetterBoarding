// <copyright file="LocaleKO.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleKO.cs
// Purpose: Korean ko-KR locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Korean localization source.
    /// </summary>
    public sealed class LocaleKO : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        public LocaleKO(BBoardSettings setting)
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

            const string ToggleName = "늦은 승객 기다리지 않기";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"값을 높이면 {transitName}의 승하차가 더 빨라집니다.\n" +
                    "승객이 타고 내릴 때의 일반 정차 시간을 줄입니다.\n" +
                    "<==========================>\n" +
                    "탑승 값:\n" +
                    "1x = vanilla 정차 100%\n" +
                    "2x = 예정 정차의 약 1/2\n" +
                    "3x = 약 1/3 (권장)\n" +
                    "5x = 약 1/5 (최대)";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<현재 {transitName} 상태>\n" +
                    "**대기** = 지금 기다리는 승객 수.\n" +
                    "**평균** = 해당 승객의 평균 대기 시간.\n" +
                    "**최악** = 한 정류장의 가장 긴 평균 대기 시간.\n" +
                    "길다면 교통 체증, 막힘/버그 정류장, 차량 부족을 확인하세요.\n" +
                    $"**오늘 해제** = <{ToggleName}>가 해제한 지연 승객 배정.\n" +
                    "<로그에 통계>에서 정류장 이름, Entity ID 등 자세한 내용을 볼 수 있습니다.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "동작" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "정보" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "승하차 속도" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "동작 방식" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "상태" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "모드 정보" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "링크" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "디버그" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "버스 승하차 속도" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "버스 정류장",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "철도 승하차 속도" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "기차·트램·지하철 정류장",
                        "기차, 트램, 지하철에 적용됩니다.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "항공기 승하차 속도" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "공항 터미널",
                        "여객기 터미널에 적용됩니다.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "배정된 승객이 탑승하기에 확실히 너무 멀리 있을 때 버스, 트램, 기차, 지하철이 출발하도록 돕습니다.\n" +
                    "- 정류장/역 안이나 근처의 승객은 배정을 유지하고 계속 탑승합니다.\n" +
                    "- 긴 기차·지하철 승강장에 있는 승객은 보호됩니다.\n" +
                    "- 확실히 멀리 있는 승객만 예정 출발 시각과 짧은 유예 후 배정을 해제할 수 있습니다.\n" +
                    "- 그룹/가족은 아이와 반려동물이 탑승을 마치도록 추가 보호됩니다.\n" +
                    "- 선박, 페리, 항공기는 늦은 승객을 vanilla 방식으로 처리합니다.\n" +
                    "해제된 cim은 도시에 남아 정상적으로 여행을 계속하거나 경로를 다시 찾습니다."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cim 더 일찍 달리기" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "이미 배정된 cim이 <더 일찍 달리기> 시작해 버스, 트램, 기차, 지하철을 놓치지 않도록 합니다.\n" +
                    "- 긴 승강장이나 정류장으로 이동 중일 때 도움이 됩니다.\n" +
                    "- vanilla보다 게임 시간 약 3분까지 일찍 시작합니다.\n" +
                    "- 현재 승하차 중인 차량에 이미 배정된 cim만 대상입니다.\n" +
                    "- 순간이동, 강제 탑승, 출발 시각 변경은 하지 않습니다."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "총 이용량" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "게임 교통 정보 보기의 월간 대중교통 이용량입니다.\n" +
                    "업데이트 시각은 이 상태를 읽은 시간입니다(보통 옵션을 열 때)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "일찍 달린 Cim" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "활성 [x] 시 오늘 Better Boarding이 **더 일찍 달리게 한** cim 수를 셉니다. 버스, 트램, 기차, 지하철 대상입니다.\n" +
                    "Run Sooner는 vanilla보다 게임 시간 약 3분까지 일찍 시작합니다."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "버스" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("버스") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "트램" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("트램") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "기차" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("기차") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "지하철" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("지하철") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "페리" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("페리") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "선박" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("선박") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "항공기" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("항공기") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "로그에 통계" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "**BetterBoarding.log**에 한 번 자세한 보고서를 씁니다.\n" +
                    "대기 수, 교통수단별 최악 정류장 3곳, 해제된 승객 예시, Entity ID, 노선 정보를 포함합니다."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "로그 열기" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "**BetterBoarding.log**가 있으면 엽니다.\n" +
                    "아직 없으면 Logs 폴더를 엽니다."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "모드" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "모드 표시 이름." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "버전" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "현재 모드 버전." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "제작자의 Paradox Mods 페이지를 엽니다." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "상세 로그 사용" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**디버그 / 테스트 전용**\n" +
                    "평소에는 **OFF**로 두세요.\n" +
                    "도시가 실행되는 동안 <Logs/BetterBoarding.log>에 <실시간> 세부 정보를 추가합니다.\n" +
                    "**일반 플레이에서는 사용하지 마세요.**\n" +
                    "성능이 떨어지고 로그 파일이 매우 커질 수 있습니다.\n" +
                    "오래된 로그는 나중에 삭제할 수 있습니다.\n" +
                    "<로그에 통계>는 한 번의 스냅샷만 기록해 상세 로그보다 부담이 적습니다.\n" +
                    "시간 흐름이 필요할 때만 10-15분 사용하세요. 많은 데이터가 생깁니다.\n" +
                    "일반 플레이 전 다시 **OFF**로 바꾸세요."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "상태 미로딩." },
                { WaitStatus.KeyNoCityLoaded, "도시가 로드되지 않았습니다." },
                { WaitStatus.KeyNoStopsFound, "정류장이 없습니다." },

                { WaitStatus.KeyStatusLine, "{0} 대기 | 평균 {1} | 최악 {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "오늘 {0} 해제" },
                { WaitStatus.KeyStatusSkipOff, "기다리지 않기 OFF" },

                { WaitStatus.KeyStatusOverviewLine, "관광객 {0}/월 | 시민 {1}/월 | 갱신 {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "일찍 달리기 OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] 통계 보고서를 요청했지만 로드된 도시가 없습니다." },
                { WaitStatus.KeyReportTitle, "로그 통계 스냅샷 - Better Boarding" },
                { WaitStatus.KeyReportSettings, "설정: {0}" },
                { WaitStatus.KeyReportNote, "노선 정보는 해당 정류장에서 대기 시간이 가장 긴 waypoint를 기준으로 합니다." },
                { WaitStatus.KeyReportTesterHintsHeader, "테스트 힌트" },
                { WaitStatus.KeyReportHintWorstStops, "최악 정류장: 게임 또는 Scene Explorer에서 먼저 확인하세요(Entity ID로 위치 찾기). 교통, 위치, 정류장 버그를 확인합니다." },
                { WaitStatus.KeyReportHintSkippedCims, "해제된 단독 배정: 각 수치는 거리 확인 후 해제된 차량 배정 1건입니다. 후속 예시에서 이후 상태를 확인하세요." },
                { WaitStatus.KeyReportHintLateGroups, "그룹 탑승: 보고서 시점에 아직 탑승이 끝나지 않은 가족/그룹입니다. Better Boarding은 추가 유예를 주고 늦은 구성원이 탑승을 마치도록 돕습니다." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "운행 정류장: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "대기 승객 있는 정류장: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "대기 승객: {0}" },
                { WaitStatus.KeyReportAverageWait, "평균 대기: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "오늘 해제한 단독 배정: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "최악 정류장: 없음. 현재 대기 승객이 없습니다." },
                { WaitStatus.KeyReportWorstStopAverageWait, "최악 정류장 평균 대기: {0}" },
                { WaitStatus.KeyReportWorstStopName, "최악 정류장 이름: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "최악 정류장 Entity: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "최악 waypoint Entity: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "최악 노선 정보: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "최악 노선 Entity: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "최악 노선 waypoint 평균: {0}, 대기 {1}" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "평균 대기 최악 {0}개 정류장:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | 평균 {2} | 대기 {3} | 정류장 {4} | waypoint {5} | 노선 {6} | 정보 {7}" },
                { WaitStatus.KeyReportLateGroups, "그룹 탑승 미완료: 승객 {0}명 / 그룹 {1}개 / 차량 {2}대" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "해제된 단독 승객 예시" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | 승객 {2} | 놓친 차량 {3} | 시간 {4} | 현재 {5}" },
                { WaitStatus.KeyReportNone, "없음" },
                { WaitStatus.KeyReportUnknown, "(알 수 없음)" },
            };
        }

        public void Unload()
        {
        }
    }
}
