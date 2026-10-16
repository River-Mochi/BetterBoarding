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

        public LocaleVI(BBoardSettings setting)
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

            const string ToggleName = "Không chờ khách đến muộn";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Giá trị cao hơn giúp lên/xuống nhanh hơn tại {transitName}.\n" +
                    "Giảm thời gian dừng bình thường khi hành khách lên xuống.\n" +
                    "<==========================>\n" +
                    "Giá trị lên xe:\n" +
                    "1x = 100% thời gian vanilla\n" +
                    "2x = ~1/2 thời gian dự kiến\n" +
                    "3x = ~1/3 (khuyên dùng)\n" +
                    "5x = ~1/5 (tối đa)";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Trạng thái {transitName} hiện tại>\n" +
                    "**Đang chờ** = số hành khách đang chờ.\n" +
                    "**TB** = thời gian chờ trung bình.\n" +
                    "**Tệ nhất** = thời gian chờ TB cao nhất tại một điểm dừng.\n" +
                    "Nếu cao, hãy kiểm tra kẹt xe, điểm dừng bị kẹt/lỗi hoặc thiếu xe.\n" +
                    $"**Đã thả hôm nay** = lượt gán hành khách trễ được <{ToggleName}> thả.\n" +
                    "<Ghi thống kê> cho biết tên điểm dừng, Entity ID và chi tiết khác.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "Thao tác" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "Thông tin" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "Tốc độ lên xuống" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "Hành vi" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Trạng thái" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Thông tin mod" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Liên kết" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Gỡ lỗi" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Lên xuống xe buýt" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "điểm dừng xe buýt",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Lên xuống đường sắt" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "điểm dừng tàu hỏa, tram và metro",
                        "Áp dụng cho tàu, tram và metro.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Lên xuống máy bay" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "nhà ga sân bay",
                        "Áp dụng cho nhà ga máy bay chở khách.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Giúp xe buýt, tram, tàu và metro rời đi khi hành khách đã được gán rõ ràng ở quá xa để lên xe.\n" +
                    "- Hành khách ở hoặc gần điểm dừng/ga vẫn giữ lượt gán và tiếp tục lên xe.\n" +
                    "- Hành khách trên sân ga tàu/metro dài được bảo vệ.\n" +
                    "- Chỉ người rõ ràng ở xa mới có thể được thả sau giờ khởi hành dự kiến và một khoảng chờ ngắn.\n" +
                    "- Nhóm/gia đình được bảo vệ thêm để trẻ em và thú cưng kịp lên xe.\n" +
                    "- Tàu thủy, phà và máy bay vẫn dùng cách xử lý hành khách trễ của vanilla.\n" +
                    "Cim được thả vẫn ở trong thành phố và tiếp tục hoặc tìm lại tuyến đường bình thường."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cim chạy sớm hơn" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Cim đã được gán bắt đầu <chạy sớm hơn> để kịp xe buýt, tram, tàu hoặc metro trước khi rời đi.\n" +
                    "- Hữu ích trên sân ga dài và khi đang tiến tới điểm dừng.\n" +
                    "- Bắt đầu sớm hơn vanilla tối đa khoảng 3 phút trong game.\n" +
                    "- Chỉ ảnh hưởng cim đã được gán cho xe đang đón khách.\n" +
                    "- Không dịch chuyển tức thời, ép lên xe hay đổi giờ khởi hành."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Tổng lượt dùng" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Lượt dùng giao thông công cộng hằng tháng từ màn hình Transportation của game.\n" +
                    "Giờ cập nhật cho biết lúc trạng thái này được lấy (thường khi mở Options)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cim chạy sớm" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Nếu bật [x], đếm cim hôm nay được Better Boarding cho **chạy sớm hơn** để kịp xe buýt, tram, tàu hoặc metro.\n" +
                    "Run Sooner có thể bắt đầu sớm hơn vanilla khoảng 3 phút trong game."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Xe buýt" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("xe buýt") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Tram" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("tram") },
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
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Ghi thống kê" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Ghi báo cáo chi tiết một lần vào **BetterBoarding.log**.\n" +
                    "Gồm tổng số chờ, 3 điểm dừng tệ nhất mỗi loại, ví dụ hành khách được thả, Entity ID và gợi ý tuyến."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Mở Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Mở **BetterBoarding.log** nếu có.\n" +
                    "Nếu chưa có, mở thư mục Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Tên hiển thị của mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Phiên bản" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Phiên bản mod hiện tại." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mở trang Paradox Mods của tác giả." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Bật log chi tiết" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Chỉ gỡ lỗi / thử nghiệm**\n" +
                    "Bình thường hãy để **OFF**.\n" +
                    "Thêm chi tiết <live> vào <Logs/BetterBoarding.log> khi thành phố đang chạy.\n" +
                    "**Không bật khi chơi bình thường.**\n" +
                    "Có thể giảm hiệu năng và tạo file log rất lớn.\n" +
                    "Có thể xóa log cũ sau.\n" +
                    "<Ghi thống kê> chỉ ghi một ảnh chụp trạng thái nên nhẹ hơn nhiều so với log chi tiết.\n" +
                    "Chỉ bật 10-15 phút nếu cần dòng thời gian; sẽ tạo nhiều dữ liệu.\n" +
                    "Nhớ **OFF** lại trước khi chơi bình thường."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Chưa tải trạng thái." },
                { WaitStatus.KeyNoCityLoaded, "Chưa tải thành phố." },
                { WaitStatus.KeyNoStopsFound, "Không tìm thấy điểm dừng." },

                { WaitStatus.KeyStatusLine, "{0} chờ | TB {1} | tệ {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} thả hôm nay" },
                { WaitStatus.KeyStatusSkipOff, "không chờ OFF" },

                { WaitStatus.KeyStatusOverviewLine, "{0} khách/th | {1} dân/th | {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "chạy sớm OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Đã yêu cầu báo cáo nhưng chưa tải thành phố." },
                { WaitStatus.KeyReportTitle, "Ảnh chụp Ghi thống kê - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Cài đặt: {0}" },
                { WaitStatus.KeyReportNote, "Gợi ý tuyến lấy từ waypoint có thời gian chờ cao nhất tại điểm dừng đó." },
                { WaitStatus.KeyReportTesterHintsHeader, "Gợi ý thử nghiệm" },
                { WaitStatus.KeyReportHintWorstStops, "Điểm dừng tệ nhất: kiểm tra trước trong game hoặc Scene Explorer (tìm vị trí bằng Entity ID). Kiểm tra kẹt xe, vị trí xấu hoặc điểm dừng lỗi." },
                { WaitStatus.KeyReportHintSkippedCims, "Lượt gán đơn đã thả: mỗi số là một lượt gán xe được thả sau khi kiểm tra khoảng cách. Dùng ví dụ theo dõi để xem chuyện gì xảy ra sau đó." },
                { WaitStatus.KeyReportHintLateGroups, "Lên xe theo nhóm: hiển thị gia đình/nhóm chưa lên xong khi báo cáo được tạo. Better Boarding cho thêm thời gian và giúp thành viên chậm lên xe hoàn tất." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Điểm dừng được phục vụ: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Điểm dừng có hành khách chờ: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Hành khách đang chờ: {0}" },
                { WaitStatus.KeyReportAverageWait, "Thời gian chờ TB: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Lượt gán đơn đã thả hôm nay: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Điểm dừng tệ nhất: không có; hiện không ai chờ." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Chờ TB tại điểm dừng tệ nhất: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Tên điểm dừng tệ nhất: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entity điểm dừng tệ nhất: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Waypoint Entity tệ nhất: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Gợi ý tuyến tệ nhất: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entity tuyến tệ nhất: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "TB waypoint tuyến tệ nhất: {0} với {1} người chờ" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} điểm dừng tệ nhất theo chờ TB:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | TB {2} | chờ {3} | điểm {4} | waypoint {5} | tuyến {6} | gợi ý {7}" },
                { WaitStatus.KeyReportLateGroups, "Lên xe theo nhóm chưa xong: {0} hành khách trong {1} nhóm trên {2} xe" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Ví dụ hành khách đơn đã thả" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | hành khách {2} | xe bị lỡ {3} | lúc {4} | hiện tại {5}" },
                { WaitStatus.KeyReportNone, "không có" },
                { WaitStatus.KeyReportUnknown, "(không rõ)" },
            };
        }

        public void Unload()
        {
        }
    }
}
