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
            string title = Mod.ModName;

            if (!string.IsNullOrEmpty(Mod.ModVersion))
            {
                title = title + " (" + Mod.ModVersion + ")";
            }

            const string ToggleName = "Geç kalan yolcuyu bekleme";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Daha yüksek değerler {transitName} iniş-binişini hızlandırır.\n" +
                    "Yolcular inerken/binerken normal bekleme süresini kısaltır.\n" +
                    "<==========================>\n" +
                    "Yükleme değeri:\n" +
                    "1x = vanilla bekleme %100\n" +
                    "2x = planlanan sürenin ~1/2’si\n" +
                    "3x = ~1/3 (önerilen)\n" +
                    "5x = ~1/5 (maks.)";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Güncel {transitName} durumu>\n" +
                    "**Bekleyen** = şu an bekleyen yolcular.\n" +
                    "**Ort.** = bu yolcuların ortalama bekleme süresi.\n" +
                    "**En kötü** = tek duraktaki en yüksek ortalama bekleme.\n" +
                    "Yüksekse trafik, tıkalı/bozuk durak veya az araç olup olmadığını kontrol et.\n" +
                    $"**Bugün bırakılan** = <{ToggleName}> tarafından bırakılan gecikmiş atamalar.\n" +
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
                    "Atanmış yolcu binemeyecek kadar uzaktaysa otobüs, tramvay, tren ve metronun kalkmasına yardımcı olur.\n" +
                    "- Durak/istasyonda veya yakınında olan yolcu atanmış kalır ve binmeye devam eder.\n" +
                    "- Uzun tren ve metro peronlarındaki yolcular korunur.\n" +
                    "- Açıkça uzaktaki yolcu, planlanan kalkış ve kısa ek süreden sonra bırakılabilir.\n" +
                    "- Grup/aileler çocuk ve evcil hayvanların binebilmesi için ek korunur.\n" +
                    "- Gemi, feribot ve uçaklar geciken yolcuları vanilla şekilde işler.\n" +
                    "Bırakılan cim şehirde kalır; yolculuğuna devam eder veya rotasını normal şekilde yeniler."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cimler daha erken koşsun" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Atanmış cimler otobüs, tramvay, tren veya metro kalkmadan yetişmek için <daha erken koşmaya> başlar.\n" +
                    "- Uzun peronlarda ve durağa yaklaşırken yardımcı olur.\n" +
                    "- vanilla’dan oyun içinde yaklaşık 3 dakikaya kadar erken başlar.\n" +
                    "- Yalnızca yolcu alan bir araca zaten atanmış cimleri etkiler.\n" +
                    "- Işınlama, zorla bindirme veya kalkış saati değişikliği yapmaz."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Toplam kullanım" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Oyunun Ulaşım bilgi görünümündeki aylık toplu taşıma kullanımı.\n" +
                    "Güncelleme saati bu durumun ne zaman alındığını gösterir (genelde Seçenekler açılınca)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Erken koşan cim" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Açık [x] ise Better Boarding’in bugün otobüs, tramvay, tren veya metro için **daha erken koşturduğu** cimleri sayar.\n" +
                    "Run Sooner vanilla’dan oyun içinde yaklaşık 3 dakikaya kadar erken başlar."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Otobüs" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("otobüs") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Tramvay" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("tramvay") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Tren" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("tren") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Metro" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("metro") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Feribot" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("feribot") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Gemi" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("gemi") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Uçak" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("uçak") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Loga istatistik" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "**BetterBoarding.log** dosyasına tek seferlik ayrıntılı rapor yazar.\n" +
                    "Bekleme toplamları, tür başına en kötü 3 durak, bırakılan yolcu örnekleri, Entity ID ve hat ipuçlarını içerir."
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
                    "**Yalnızca hata ayıklama / test**\n" +
                    "Normalde **KAPALI** tut.\n" +
                    "Şehir çalışırken <Logs/BetterBoarding.log> dosyasına <canlı> ayrıntılar ekler.\n" +
                    "**Normal oyunda açma.**\n" +
                    "Performansı düşürebilir ve çok büyük loglar oluşturabilir.\n" +
                    "Eski logları sonra silebilirsin.\n" +
                    "<Loga istatistik> yalnızca tek anlık görüntü yazar ve ayrıntılı logdan çok daha hafiftir.\n" +
                    "Zaman çizelgesi gerekiyorsa 10-15 dk aç; çok veri üretir.\n" +
                    "Normal oyundan önce tekrar **KAPALI** yap."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Durum yüklenmedi." },
                { WaitStatus.KeyNoCityLoaded, "Şehir yüklenmedi." },
                { WaitStatus.KeyNoStopsFound, "Durak bulunamadı." },

                { WaitStatus.KeyStatusLine, "{0} bek. | ort. {1} | kötü {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "bugün {0} bırakıldı" },
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
                { WaitStatus.KeyReportHintSkippedCims, "Bırakılan tekli atamalar: her sayı mesafe kontrolünden sonra bırakılan bir araç atamasıdır. Sonrasında ne olduğunu takip örneklerinden gör." },
                { WaitStatus.KeyReportHintLateGroups, "Grup binişi: rapor alındığında hâlâ tamamlanmamış aile/grupları gösterir. Better Boarding ek süre verir ve geride kalan üyelerin binmesini tamamlamasına yardım eder." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Hizmet verilen duraklar: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Bekleyen yolcusu olan duraklar: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Bekleyen yolcular: {0}" },
                { WaitStatus.KeyReportAverageWait, "Ortalama bekleme: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Bugün bırakılan tekli atamalar: {0}" },
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
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Bırakılan tekli yolcu örnekleri" },
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
