// <copyright file="LocaleVI.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleVI.cs
// Purpose: Vietnamese vi-VN locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Vietnamese localization source.
    /// </summary>
    public sealed class LocaleVI : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        /// <summary>
        /// Constructs the Vietnamese locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleVI(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all Vietnamese localization entries for this mod.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName;

            const string ToggleName = "Bỏ qua hành khách đến trễ";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Giá trị cao hơn giúp giảm thời gian lên/xếp khách tại {transitName}.\n" +
                    "Giúp hàng chờ bình thường giải tỏa nhanh hơn, nhưng hành khách đến trễ vẫn có thể làm chậm giờ khởi hành do thiết kế vanilla.\n" +
                    $"Dùng [✓] <{ToggleName}> nếu muốn cim đến trễ có thể lỡ chuyến sau giờ khởi hành.\n" +
                    "Công dân đến trễ bị bỏ qua không bị xóa; vanilla sẽ tự chuyển tuyến cho họ.\n" +
                    "<==========================>\n" +
                    "Giá trị tải:\n" +
                    "1x = 100% thời gian dừng vanilla\n" +
                    "2x = ~1/2 thời gian dừng dự kiến\n" +
                    "3x = ~1/3 thời gian dừng dự kiến (khuyên dùng)\n" +
                    "5x = ~1/5 thời gian dừng dự kiến (tối đa)\n" +
                    $"Đây không giống <{ToggleName}>; tùy chọn này quyết định cim đến trễ có thể lỡ {shortName} sau giờ khởi hành hay không.";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Trạng thái hiện tại của {transitName}>\n" +
                    "**Đang chờ** = tổng số hành khách đang chờ ngay lúc này.\n" +
                    "**TB** = thời gian chờ trung bình của các hành khách đó.\n" +
                    "**Tệ nhất** = điểm dừng có thời gian chờ trung bình cao nhất.\n" +
                    "Các điểm dừng tệ nhất là nơi nên kiểm tra tai nạn, tắc đường, điểm dừng lỗi hoặc thiếu phương tiện.\n" +
                    $"**Trễ hôm nay** = hành khách đi một mình đến trễ bị bỏ qua hôm nay bởi <{ToggleName}>.\n" +
                    "Dùng <Ghi thống kê vào Log> để xem báo cáo chi tiết: tên điểm dừng, entity ID và nhiều hơn.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "Thao tác" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "Giới thiệu" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "Tốc độ lên xe" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "Hành vi" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Trạng thái" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Thông tin mod" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Liên kết" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Gỡ lỗi" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Tốc độ lên xe buýt" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "điểm dừng xe buýt",
                        "xe buýt",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Tốc độ lên phương tiện đường sắt" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "điểm dừng tàu hỏa, tàu điện và metro",
                        "phương tiện",
                        "Áp dụng cho điểm dừng tàu hỏa, tàu điện và metro.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Tốc độ máy bay" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "nhà ga máy bay",
                        "máy bay",
                        "Áp dụng cho nhà ga máy bay chở khách.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "<Hành khách đến trễ> vẫn <chưa sẵn sàng> sau <giờ khởi hành> có thể bị lỡ chuyến.\n" +
                    "- Hành khách đi một mình đến trễ được thả sau một khoảng chờ ngắn để phương tiện có thể rời đi.\n" +
                    "- Nhóm/gia đình được thêm một chút thời gian. Nếu người dẫn nhóm vẫn ở ngoài, cả nhóm được thả thông qua cơ chế hủy nhóm của vanilla.\n" +
                    "- Nếu người dẫn nhóm đã lên xe, Better Boarding để vanilla hoàn tất việc lên xe cho trẻ em/thú cưng còn chậm để một người không giữ phương tiện đến timeout dài của vanilla.\n" +
                    "- Công dân đến trễ bị bỏ qua không bị xóa; vanilla có thể tiếp tục hành trình hoặc tự chuyển tuyến cho họ."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cim chạy sớm hơn (xem chú thích, bảng thứ 3)" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Công dân <đến trễ> bắt đầu <chạy sớm hơn> để cố đến **trước** giờ khởi hành.\n" +
                    "- Hoạt động với xe buýt, tàu điện, tàu hỏa và metro, đặc biệt hữu ích ở sân ga dài.\n" +
                    "- Chỉ ảnh hưởng cim đã được gán cho phương tiện đang đón khách.\n" +
                    "- Vanilla chỉ cho cim bắt đầu chạy đúng giờ khởi hành, đôi khi quá muộn.\n" +
                    $"- Kết hợp tốt với <{ToggleName}> vì có thể giảm số cim lỡ chuyến và phải được phân công lại.\n" +
                    "- Không thay đổi giờ khởi hành, không ép lên xe và không dịch chuyển công dân."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.SpacingAssist)), "Hỗ trợ giãn cách" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.SpacingAssist)),
                    "**Giảm dồn chuyến:** giúp giãn cách xe buýt, tàu điện, tàu hỏa và metro thay vì để chúng chạy nối đuôi nhau.\n" +
                    "- Dùng điểm dừng hợp lệ đầu tiên của mỗi tuyến làm điểm điều tiết và đưa các lần khởi hành thực tế gần với khoảng cách mục tiêu của tuyến.\n" +
                    "- Chỉ thêm thời gian chờ có giới hạn tại đó; các điểm dừng khác giữ thời gian vanilla. Không dịch chuyển phương tiện hay bỏ qua điểm dừng được yêu cầu.\n" +
                    "- Xe buýt đầy mà không có ai xuống vẫn có thể đi qua điểm dừng nhờ cơ chế yêu cầu dừng bình thường của vanilla."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AllowSecondBusBoarding)), "Allow Second Bus Boarding" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AllowSecondBusBoarding)),
                    "When two buses are stopped close together for the same stop, lets the second bus exchange passengers too.\n" +
                    "- Bus only; trains, trams, and subways are never included.\n" +
                    "- Maximum two buses, using a fixed safe range with no extra slider or city panel.\n" +
                    "- The lead bus keeps its normal vanilla boarding lifecycle; the second bus returns to vanilla when the lead departs.\n" +
                    "- Automatically stays inactive if the separate Concurrent Bus Boarding mod is detected."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Tổng lượt sử dụng" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Lượt sử dụng giao thông công cộng hàng tháng từ bảng Transportation của game.\n" +
                    "Thời gian cập nhật cho biết lúc trạng thái này được ghi lại (thường khi mở menu Options)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cim chạy sớm hơn" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Nếu bật [x], đếm tất cả cim (hôm nay) đã bắt đầu **chạy sớm hơn** để cố bắt xe buýt, tàu điện, tàu hỏa hoặc metro trước giờ khởi hành.\n" +
                    "Cim chạy sớm hơn vanilla 512 frame (~2-8 giây ngoài đời, ~2 phút trong game)."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Xe buýt" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("xe buýt") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Tàu điện" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("tàu điện") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Tàu hỏa" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("tàu hỏa") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Metro" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("metro") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Phà" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("phà") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Tàu thủy" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("tàu thủy") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Máy bay" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("máy bay") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Ghi thống kê vào Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Ghi báo cáo chi tiết một lần vào **BetterBoarding.log**.\n" +
                    "Bao gồm tổng số đang chờ, 3 điểm dừng tệ nhất mỗi loại, ví dụ cim bị bỏ qua, entity ID và gợi ý tuyến."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Mở Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Mở **BetterBoarding.log** nếu tệp tồn tại.\n" +
                    "Nếu chưa có tệp, mở thư mục Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Tên hiển thị của mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Phiên bản" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Phiên bản mod hiện tại." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mở trang Paradox Mods của tác giả." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Bật ghi log chi tiết" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Chỉ dành cho gỡ lỗi / thử nghiệm**\n" +
                    "Nên luôn để **TẮT**, trừ khi bạn biết rõ tính năng này dùng để làm gì.\n" +
                    "Thêm chi tiết <trực tiếp> vào <Logs/BetterBoarding.log> khi thành phố đang chạy.\n" +
                    "**Không bật khi chơi bình thường.**\n" +
                    "Để bật có thể giảm hiệu năng và tạo tệp log rất lớn.\n" +
                    "Bạn có thể xóa các tệp log cũ sau.\n" +
                    "Lưu ý: <Ghi thống kê vào Log> là báo cáo trạng thái hiện tại kèm bộ đếm hành khách trễ bị bỏ qua hôm nay; không ảnh hưởng hiệu năng như verbose log.\n" +
                    "Chạy verbose log 10-15 phút nếu muốn xem dòng thời gian sự kiện, nhưng nó tạo ra rất nhiều dữ liệu.\n" +
                    "Đừng quên **TẮT** verbose log trước khi chơi bình thường."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Chưa tải trạng thái." },
                { WaitStatus.KeyNoCityLoaded, "Chưa tải thành phố." },
                { WaitStatus.KeyNoStopsFound, "Không tìm thấy điểm dừng." },

                { WaitStatus.KeyStatusLine, "{0} đang chờ | TB {1} | tệ nhất {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} trễ hôm nay" },
                { WaitStatus.KeyStatusSkipOff, "bỏ qua OFF" },

                { WaitStatus.KeyStatusOverviewLine, "{0} du khách/tháng | {1} công dân/tháng | cập nhật {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "chạy sớm OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Đã yêu cầu báo cáo nhưng chưa tải thành phố." },
                { WaitStatus.KeyReportTitle, "Ảnh chụp thống kê vào Log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Cài đặt: {0}" },
                { WaitStatus.KeyReportNote, "Gợi ý tuyến lấy từ waypoint có mức chờ cao nhất tại điểm dừng đó." },
                { WaitStatus.KeyReportTesterHintsHeader, "Gợi ý kiểm thử" },
                { WaitStatus.KeyReportHintWorstStops, "Điểm dừng tệ nhất: kiểm tra trước trong game hoặc bằng mod Scene Explorer (tìm theo entity ID). Xem tình trạng giao thông, vị trí điểm dừng kém hoặc điểm dừng bị lỗi." },
                { WaitStatus.KeyReportHintSkippedCims, "Cim đi một mình bị bỏ qua: hành khách đến trễ được thả để phương tiện có thể rời đi. Sau đó thường sẽ thành 'has path' hoặc 'assigned'. Nếu vẫn 'no path yet', hãy kiểm tra cim sau một lúc." },
                { WaitStatus.KeyReportHintLateGroups, "Nhóm đến trễ (gia đình): hiển thị các nhóm vẫn chưa giải quyết xong ngay lúc lấy báo cáo. Better Boarding cho thêm một khoảng chờ ngắn; nếu người dẫn nhóm vẫn ở ngoài thì thả cả nhóm, còn nếu đã lên xe thì giúp vanilla hoàn tất việc lên xe cho thành viên bị chậm." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Điểm dừng được phục vụ: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Điểm dừng có hành khách chờ: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Hành khách đang chờ: {0}" },
                { WaitStatus.KeyReportAverageWait, "Chờ trung bình: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Hành khách trễ bị bỏ qua hôm nay: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Điểm dừng tệ nhất: không có, hiện không có hành khách chờ." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Chờ TB tại điểm dừng tệ nhất: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Tên điểm dừng tệ nhất: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entity điểm dừng tệ nhất: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Entity waypoint tệ nhất: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Gợi ý tuyến tệ nhất: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entity tuyến tệ nhất: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "TB waypoint tuyến tệ nhất: {0} với {1} người chờ" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} điểm dừng tệ nhất theo chờ trung bình:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | TB {2} | chờ {3} | điểm dừng {4} | waypoint {5} | tuyến {6} | gợi ý {7}" },
                { WaitStatus.KeyReportLateGroups, "Cim trễ theo nhóm vẫn chưa được giải quyết: {0} hành khách trong {1} nhóm trên {2} phương tiện" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Ví dụ cim đi một mình đến trễ bị bỏ qua" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | hành khách {2} | phương tiện bị lỡ {3} | lúc {4} | hiện tại {5}" },
                { WaitStatus.KeyReportNone, "không có" },
                { WaitStatus.KeyReportUnknown, "(không rõ)" },
            };
        }

        public void Unload()
        {
        }
    }
}
