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

        /// <summary>
        /// Constructs the Turkish locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleTR(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all Turkish localization entries for this mod.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName;

            const string ToggleName = "Geç kalan yolcuları atla";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Daha yüksek değerler {transitName} için biniş ve yükleme süresini azaltır.\n" +
        
                    "Bu, normal kuyrukların daha hızlı boşalmasına yardımcı olur; ancak vanilla tasarımı nedeniyle geç kalan bir yolcu yine kalkışı geciktirebilir.\n" +
                    $"Geç kalan cimlerin kalkıştan sonra aracı kaçırabilmesi için [✓] <{ToggleName}> seçeneğini kullanın.\n" +
                    "Atlanan geç yolcular silinmez; vanilla onları doğal olarak yeniden yönlendirir.\n" +
                    "<==========================>\n" +
                    "Yükleme değeri:\n" +
                    "1x = %100 vanilla bekleme süresi\n" +
                    "2x = ~1/2 planlanan bekleme süresi\n" +
                    "3x = ~1/3 planlanan bekleme süresi (önerilen)\n" +
                    "5x = ~1/5 planlanan bekleme süresi (maks.)\n" +
                    $"Bu, <{ToggleName}> ile aynı değildir; bu seçenek geç kalan cimlerin kalkıştan sonra {shortName} kaçırıp kaçıramayacağını belirler.";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Güncel {transitName} durumu>\n" +
                    "**Bekleyen** = şu anda bekleyen toplam yolcu.\n" +
                    "**Ort.** = bu yolcuların ortalama bekleme süresi.\n" +
                    "**En kötü** durak = tek bir duraktaki en yüksek ortalama bekleme.\n" +
                    "En kötü duraklar; trafik kazaları, tıkanmış/hatalı duraklar veya yetersiz araç sayısını kontrol etmek için iyi yerlerdir.\n" +
                    $"**Bugün geç kalan** = bugün <{ToggleName}> tarafından atlanan geç kalan solo yolcular.\n" +
                    "Ayrıntılı rapor için <Stats to Log> kullanın: durak adları, entity ID'leri ve daha fazlası.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "İşlemler" },
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
                        "otobüs durağı",
                        "otobüsü",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Raylı sistem biniş hızı" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "tren, tramvay ve metro durağı",
                        "aracı",
                        "Tren, tramvay ve metro duraklarına uygulanır.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "Gemi + feribot hızı" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "gemi ve feribot durağı",
                        "aracı",
                        "Gemi ve feribot duraklarına uygulanır.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Uçak hızı" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "uçak terminali",
                        "uçağı",
                        "Yolcu uçağı terminallerine uygulanır.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "<Geç kalan yolcular> <kalkış saatinden> sonra hâlâ <hazır değilse> aracı kaçırabilir.\n" +
                    "- Not: yalnızca geç kalan solo yolcuları atlıyoruz.\n" +
                    "- Birlikte seyahat eden geç kalmış gruplar/aileler <atlanmaz> ve vanilla gibi ulaşımı geciktirebilir.\n" +
                    "- Grup yolcuları kalabalığın küçük bir kısmıdır; asıl fayda geç kalan solo cimleri atlamaktan gelir.\n" +
                    "- Atlanan geç yolcular silinmez; oyun onları doğal olarak yeniden atar."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cimler daha erken koşsun: Otobüs + tramvay + tren" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "<Geç kalan> vatandaşlar kalkış saatinden **önce** yetişmeye çalışmak için <daha erken koşmaya> başlar.\n" +
                    "- Otobüs, tramvay ve trenlerde çalışır; özellikle uzun tren peronlarında faydalıdır.\n" +
                    "- Yalnızca şu anda yolcu alan bir araca zaten atanmış cimleri etkiler.\n" +
                    "- Vanilla cimleri ancak kalkış saatinde koşturmaya başlatır; bu bazen çok geç olabilir.\n" +
                    $"- <{ToggleName}> ile iyi çalışır; aracı kaçırıp yeniden atanması gereken cim sayısını azaltabilir.\n" +
                    "- Aracın kalkış saatini değiştirmez, zorla bindirmez ve vatandaşları ışınlamaz."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Toplam kullanım" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Oyunun Ulaşım bilgi görünümündeki aylık toplu taşıma kullanımı.\n" +
                    "Güncelleme saati bu durum görüntüsünün ne zaman alındığını gösterir (genellikle Seçenekler menüsüne girince)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cimler daha erken koşuyor" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Etkinse [x], bugün otobüs, tramvay veya trene kalkıştan önce yetişmek için **daha erken koşmaya başlayan** tüm cimleri sayar.\n" +
                    "Cimler vanilla'dan 512 kare daha erken koşar (gerçek zamanda ~2-8 saniye, oyun içinde ~2 dakika)."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Otobüs" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("otobüsü") },
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
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("uçağı") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "İstatistikleri günlüğe yaz" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "**BetterBoarding.log** dosyasına tek seferlik ayrıntılı rapor yazar.\n" +
                    "Bekleyen toplamları, mod başına en kötü 3 durağı, atlanan cim örneklerini, entity ID'lerini ve hat ipuçlarını içerir."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Günlüğü aç" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Varsa **BetterBoarding.log** dosyasını açar.\n" +
                    "Dosya henüz yoksa Logs klasörünü açar."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Modun görünen adı." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Sürüm" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Geçerli mod sürümü." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Yazarın Paradox Mods sayfasını açar." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Ayrıntılı günlüğü etkinleştir" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Yalnızca hata ayıklama / test için**\n" +
                    "Şehir çalışırken <Logs/BetterBoarding.log> dosyasına <canlı> ayrıntılar ekler.\n" +
                    "**Normal oyunda açık bırakmayın.**\n" +
                    "Açık bırakmak performansı düşürebilir ve çok büyük günlük dosyaları oluşturabilir.\n" +
                    "Eski günlük dosyalarını daha sonra silebilirsiniz.\n" +
                    "Not: <Stats to Log>, o anlık bir rapor ve bugünkü geç-atlama sayaçlarıdır; ayrıntılı günlükten farklıdır.\n" +
                    "Zaman içinde ne olduğunu görmek istiyorsanız ayrıntılı günlüğü 15-20 dakika çalıştırın.\n" +
                    "Normal oyuna dönmeden önce ayrıntılı günlüğü tekrar **KAPATMAYI** unutmayın."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Durum yüklenmedi." },
                { WaitStatus.KeyNoCityLoaded, "Şehir yüklenmedi." },
                { WaitStatus.KeyNoStopsFound, "Durak bulunamadı." },

                { WaitStatus.KeyStatusLine, "{0} bekliyor | ort. {1} | en kötü {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "bugün {0} geç" },
                { WaitStatus.KeyStatusSkipOff, "atlama KAPALI" },

                { WaitStatus.KeyStatusOverviewLine, "{0} turist/ay | {1} vatandaş/ay | güncelleme {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "erken koşma KAPALI" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] İstatistik raporu istendi ama şehir yüklü değil." },
                { WaitStatus.KeyReportTitle, "Stats to Log anlık görüntüsü - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Ayarlar: {0}" },
                { WaitStatus.KeyReportNote, "Hat ipucu, o duraktaki en yüksek beklemeye sahip waypoint'ten gelir." },
                { WaitStatus.KeyReportTesterHintsHeader, "Test ipuçları" },
                { WaitStatus.KeyReportHintWorstStops, "En kötü duraklar: önce oyunda veya Scene Explorer moduyla inceleyin (entity ID ile bulun). Trafik, kötü durak konumu veya hatalı durak arayın." },
                { WaitStatus.KeyReportHintSkippedCims, "Atlanan solo cimler: toplu taşımanın ayrılabilmesi için atladığımız geç yolcular. Sonraki durum genelde 'yolu var' veya 'atandı' olmalıdır. 'Henüz yol yok' kalırsa cimi biraz sonra tekrar inceleyin." },
                { WaitStatus.KeyReportHintLateGroups, "Geç kalan gruplar (aileler): birlikte kalmaları için kasıtlı olarak vanilla'ya bırakılır; solo yolculara göre azdır." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Hizmet verilen duraklar: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Bekleyen yolcusu olan duraklar: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Bekleyen yolcular: {0}" },
                { WaitStatus.KeyReportAverageWait, "Ortalama bekleme: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Bugün atlanan geç yolcular: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "En kötü durak: yok, şu anda bekleyen yolcu yok." },
                { WaitStatus.KeyReportWorstStopAverageWait, "En kötü durak ort. bekleme: {0}" },
                { WaitStatus.KeyReportWorstStopName, "En kötü durak adı: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "En kötü durak entity: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "En kötü waypoint entity: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "En kötü hat ipucu: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "En kötü hat entity: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "En kötü hat waypoint ort.: {0}, bekleyen {1}" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Ortalama beklemeye göre en kötü {0} durak:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | ort. {2} | bekleyen {3} | durak entity {4} | waypoint entity {5} | hat entity {6} | hat ipucu {7}" },
                { WaitStatus.KeyReportLateGroups, "Grup halinde seyahat eden geç cimlere dokunulmadı: {0} yolcu, {1} grup, {2} araç" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Atlanan geç solo cim örnekleri" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | yolcu {2} | kaçırdığı araç {3} | saat {4} | şimdi {5}" },
                { WaitStatus.KeyReportNone, "yok" },
                { WaitStatus.KeyReportUnknown, "(bilinmiyor)" },
            };
        }

        public void Unload()
        {
        }
    }
}
