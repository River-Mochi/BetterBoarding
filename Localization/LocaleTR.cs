// <copyright file="LocaleTR.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleTR.cs
// Purpose: Turkish tr-TR locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Turkish localization source.
    /// </summary>
    public sealed class LocaleTR : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        public LocaleTR(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName + " (Daha İyi Biniş)";

            const string ToggleName = "Geç kalan yolcuyu bekleme";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Daha yüksek değerler {transitName} iniş-binişini hızlandırır.\n" +
                    "Yolcular inerken/binerken normal bekleme süresini kısaltır.\n" +
                    "<==========================>\n" +
                    "1x = vanilla bekleme %100\n" +
                    "2x = planlanan sürenin ~1/2’si\n" +
                    "3x = ~1/3 (önerilen)\n" +
                    "5x = ~1/5 (maks.)";
            }

            // One helper keeps the status tooltips consistent while explaining
            // which modes Better Boarding can actually skip late passengers for.
            string StatusDescription(string transitName, bool skipSupported)
            {
                string skippedText = skipSupported
                    ? "**Atlandı** = aracın beklememesi için bugün atlanan geç kalan yolcular.\n" +
                      "Better Boarding, vanilla'nın sıkışan araç için ~10 dakikalık çözümünden çok daha erken devreye girer.\n" +
                      $"<{ToggleName}> ile kontrol edilir.\n"
                    : "**Atlandı** = bu ulaşım türünde 0. Geç kalan yolcuları vanilla işler.\n";

                return
                    $"<Güncel {transitName} durumu>\n" +
                    "**Bekleyen** = şu an bekleyen yolcular.\n" +
                    "**Ort.** = bu yolcuların ortalama bekleme süresi.\n" +
                    "**En kötü** = tek duraktaki en yüksek ortalama bekleme.\n" +
                    "Yüksekse trafik, tıkalı/bozuk durak veya az araç olup olmadığını kontrol et.\n" +
                    skippedText +
                    "<Loga istatistik> durak adları, Entity ID ve daha fazla ayrıntı verir.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "Eylemler" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "Hakkında" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "Biniş hızı" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "Davranış" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Durum" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Mod bilgisi" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Bağlantılar" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Hata ayıklama" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Otobüs biniş hızı" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "otobüs durakları",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Raylı sistem binişi" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "tren, tramvay ve metro durakları",
                        "Tren, tramvay ve metro için geçerlidir.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Uçak biniş hızı" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "uçak terminalleri",
                        "Yolcu uçağı terminalleri için geçerlidir.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Duraktan fazla uzakta olan geç kalmış bir yolcu atlanabilir; böylece araç beklemeye devam etmez.\n" +
                    "Better Boarding, vanilla'nın sıkışan araç için ~10 dakikalık çözümünden çok daha erken devreye girer.\n" +
                    "- Durak/istasyonda veya yakınında olan yolcu atanmış kalır ve binmeye devam eder.\n" +
                    "- Uzun tren/metro peronları korunur.\n" +
                    "- Grup/aileler ek koruma alır.\n" +
                    "- Atlanan cim şehirde kalır; vanilla yolculuğunu sürdürebilir veya yeniden rota çizebilir, çoğu zaman daha sonraki bir araca binebilir.\n" +
                    "- Gemi, feribot ve uçaklar geç kalan yolcuları vanilla şekilde işler."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cimler daha erken koşsun" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Atanmış cimler ulaşım aracını kalkmadan yakalamak için <daha erken koşmaya> başlar.\n" +
                    "- vanilla'dan oyun içinde yaklaşık 3 dakikaya kadar erken başlar.\n" +
                    "- Uzun peronlarda ve durağa yaklaşırken yardımcı olur.\n" +
                    "- Yalnızca yolcu alan bir araca zaten atanmış cimleri etkiler.\n" +
                    "- Cimleri ışınlamaz veya kalkış saatini değiştirmez."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Toplam kullanım" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Oyunun Ulaşım bilgi görünümündeki aylık toplu taşıma kullanımı.\n" +
                    "Güncellendi = bu durum görüntüsünün alındığı zaman."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Erken koşan cim" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Better Boarding’ın bugün **daha erken koşturduğu** cimleri sayar.\n" +
                    "vanilla'dan oyun içinde yaklaşık 3 dakikaya kadar erken."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Otobüs" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("otobüs", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Tramvay" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("tramvay", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Tren" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("tren", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Metro" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("metro", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Feribot" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("feribot", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Gemi" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("gemi", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Uçak" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("uçak", skipSupported: false) },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Loga istatistik" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "**BetterBoarding.log** dosyasına tek seferlik ayrıntılı rapor yazar.\n" +
                    "Bekleme toplamları, en kötü 3 durak, atlanan geç yolcu örnekleri, Entity ID ve hat ipuçlarını içerir."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Logu aç" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Varsa **BetterBoarding.log** dosyasını açar.\n" +
                    "Henüz yoksa Logs klasörünü açar."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Modun görünen adı." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Sürüm" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Geçerli mod sürümü." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Yazarın Paradox Mods sayfasını açar." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Ayrıntılı log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**YALNIZCA Debug / Test**\n" +
                    "Normal oyunda KAPALI tut.\n" +
                    "Şehir çalışırken <Logs/BetterBoarding.log> dosyasına <canlı> ayrıntılar ekler.\n" +
                    "Performansı düşürebilir ve büyük log dosyaları oluşturabilir.\n" +
                    "Sorun ararken 10-15 dk kullan, sonra tekrar KAPAT.\n" +
                    "<Loga istatistik> daha hafif, tek seferlik bir görüntüdür."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Durum yüklenmedi." },
                { WaitStatus.KeyNoCityLoaded, "Şehir yüklenmedi." },
                { WaitStatus.KeyNoStopsFound, "Durak bulunamadı." },

                { WaitStatus.KeyStatusLine, "{0} bek. | ort. {1} | kötü {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} geç yolcu atlandı" },
                { WaitStatus.KeyStatusSkipOff, "bekleme KAPALI" },

                { WaitStatus.KeyStatusOverviewLine, "{0} tur./ay | {1} kişi/ay | {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "erken koşma KAPALI" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] İstatistik raporu istendi, ancak şehir yüklü değil." },
                { WaitStatus.KeyReportTitle, "Log istatistik görüntüsü - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Ayarlar: {0}" },
                { WaitStatus.KeyReportNote, "Hat ipucu, o duraktaki en yüksek beklemeye sahip waypoint’ten gelir." },
                { WaitStatus.KeyReportTesterHintsHeader, "Test ipuçları" },
                { WaitStatus.KeyReportHintWorstStops, "En kötü duraklar: önce oyunda veya Scene Explorer ile incele (Entity ID ile konumu bul). Trafik, kötü konum veya bozuk durak ara." },
                { WaitStatus.KeyReportHintSkippedCims, "Atlanan geç yolcular: kalkıştan sonra fazla uzaktaydılar, bu yüzden Better Boarding aracın onları beklemeye devam etmesini engelledi. Takip örnekleri sonrasında ne olduğunu gösterir." },
                { WaitStatus.KeyReportHintLateGroups, "Grup binişi: rapor alındığında hâlâ tamamlanmamış aile/grupları gösterir. Better Boarding ek süre verir ve geride kalan üyelerin binmesini tamamlamasına yardım eder." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Hizmet verilen duraklar: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Bekleyen yolcusu olan duraklar: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Bekleyen yolcular: {0}" },
                { WaitStatus.KeyReportAverageWait, "Ortalama bekleme: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Bugün atlanan geç yolcu: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "En kötü durak: yok; şu anda bekleyen yolcu yok." },
                { WaitStatus.KeyReportWorstStopAverageWait, "En kötü durağın ort. beklemesi: {0}" },
                { WaitStatus.KeyReportWorstStopName, "En kötü durak adı: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "En kötü durak Entity: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "En kötü waypoint Entity: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "En kötü hat ipucu: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "En kötü hat Entity: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "En kötü hat waypoint ort.: {0}, bekleyen {1}" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Ort. beklemeye göre en kötü {0} durak:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | ort. {2} | bekleyen {3} | durak {4} | waypoint {5} | hat {6} | ipucu {7}" },
                { WaitStatus.KeyReportLateGroups, "Grup binişi tamamlanmadı: {1} grupta {0} yolcu, {2} araç" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Atlanan geç yolcu örnekleri" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | yolcu {2} | kaçırılan araç {3} | saat {4} | şimdi {5}" },
                { WaitStatus.KeyReportNone, "yok" },
                { WaitStatus.KeyReportUnknown, "(bilinmiyor)" },
            };
        }

        public void Unload()
        {
        }
    }
}
