// <copyright file="LocaleTH.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocaleTH.cs
// Purpose: Thai th-TH locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Thai localization source.
    /// </summary>
    public sealed class LocaleTH : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        /// <summary>
        /// Constructs the Thai locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocaleTH(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all Thai localization entries for this mod.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName;

            const string ToggleName = "ข้ามผู้โดยสารที่มาสาย";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"ค่าที่สูงขึ้นช่วยลดเวลาในการขึ้นและโหลดที่ {transitName}\n" +
        
                    "ช่วยให้คิวปกติเดินเร็วขึ้น แต่ผู้โดยสารที่มาสายยังอาจทำให้รถออกช้าได้ตามระบบของเกม\n" +
                    $"ใช้ [✓] <{ToggleName}> ถ้าต้องการให้ cim ที่มาสายพลาดรถได้หลังเวลาออก\n" +
                    "พลเมืองที่มาสายและถูกข้ามจะไม่ถูกลบ เกมจะจัดเส้นทางใหม่ให้ตามปกติ\n" +
                    "<==========================>\n" +
                    "ค่าการโหลด:\n" +
                    "1x = 100% เวลาจอดแบบ vanilla\n" +
                    "2x = ~1/2 เวลาจอดตามแผน\n" +
                    "3x = ~1/3 เวลาจอดตามแผน (แนะนำ)\n" +
                    "5x = ~1/5 เวลาจอดตามแผน (สูงสุด)\n" +
                    $"นี่ไม่ใช่อย่างเดียวกับ <{ToggleName}>; ตัวเลือกนี้กำหนดว่า cim ที่มาสายจะพลาด {shortName} หลังเวลาออกได้หรือไม่";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<สถานะปัจจุบันของ {transitName}>\n" +
                    "**รอ** = จำนวนผู้โดยสารที่กำลังรออยู่ตอนนี้\n" +
                    "**เฉลี่ย** = เวลารอเฉลี่ยของผู้โดยสารเหล่านั้น\n" +
                    "**แย่สุด** = ป้ายที่มีเวลารอเฉลี่ยสูงสุด\n" +
                    "ป้ายที่แย่ที่สุดเหมาะสำหรับตรวจอุบัติเหตุ การจราจรติด ป้ายมีปัญหา หรือจำนวนรถไม่พอ\n" +
                    $"**มาสายวันนี้** = ผู้โดยสารเดี่ยวที่มาสายและถูกข้ามวันนี้โดย <{ToggleName}>\n" +
                    "ใช้ <Stats to Log> เพื่อดูรายงานละเอียด: ชื่อป้าย Entity ID และข้อมูลอื่น ๆ";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "การทำงาน" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "เกี่ยวกับ" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "ความเร็วการขึ้นรถ" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "พฤติกรรม" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "สถานะ" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "ข้อมูลม็อด" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "ลิงก์" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "ดีบัก" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "ความเร็วการขึ้นรถบัส" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "ป้ายรถบัส",
                        "รถบัส",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "ความเร็วการขึ้นระบบราง" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "ป้ายรถไฟ รถราง และรถไฟใต้ดิน",
                        "รถ",
                        "ใช้กับป้ายรถไฟ รถราง และรถไฟใต้ดิน\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "ความเร็วเรือ + เฟอร์รี" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "ท่าเรือและเฟอร์รี",
                        "รถ",
                        "ใช้กับท่าเรือและเฟอร์รี\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "ความเร็วเครื่องบิน" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "อาคารผู้โดยสารเครื่องบิน",
                        "เครื่องบิน",
                        "ใช้กับอาคารผู้โดยสารเครื่องบินโดยสาร\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "<ผู้โดยสารที่มาสาย> ซึ่งยัง <ไม่พร้อม> หลัง <เวลาออก> สามารถพลาดรถคันนั้นได้\n" +
                    "- หมายเหตุ: ข้ามเฉพาะพลเมืองเดี่ยวที่มาสาย\n" +
                    "- กลุ่ม/ครอบครัวที่เดินทางด้วยกันและมาสายจะ <ไม่ถูกข้าม> และยังอาจทำให้การขนส่งล่าช้าเหมือน vanilla\n" +
                    "- ผู้เดินทางแบบกลุ่มมีจำนวนน้อย ประโยชน์หลักมาจากการข้าม cim เดี่ยวที่มาสาย\n" +
                    "- พลเมืองที่ถูกข้ามจะไม่ถูกลบ เกมจะจัดให้ใหม่ตามธรรมชาติ"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cim วิ่งเร็วขึ้น: รถบัส + รถราง + รถไฟ" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "พลเมืองที่ <มาสาย> จะเริ่ม <วิ่งเร็วขึ้น> เพื่อพยายามไปถึง **ก่อน** เวลาออก\n" +
                    "- ใช้กับรถบัส รถราง และรถไฟ โดยเฉพาะชานชาลารถไฟยาว ๆ\n" +
                    "- มีผลเฉพาะ cim ที่ถูกกำหนดให้ขึ้นรถที่กำลังรับผู้โดยสารอยู่แล้ว\n" +
                    "- vanilla เริ่มให้ cim วิ่งเมื่อถึงเวลาออก ซึ่งอาจสายเกินไป\n" +
                    $"- ใช้คู่กับ <{ToggleName}> ได้ดี เพราะช่วยลดจำนวน cim ที่พลาดรถและต้องถูกจัดใหม่\n" +
                    "- ไม่เปลี่ยนเวลาออก ไม่บังคับขึ้นรถ และไม่เทเลพอร์ตพลเมือง"
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "การใช้งานรวม" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "จำนวนการใช้ขนส่งสาธารณะรายเดือนจากหน้า Transportation ของเกม\n" +
                    "เวลาอัปเดตแสดงเวลาที่บันทึกสถานะนี้ (ปกติเมื่อเปิดเมนู Options)"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cim วิ่งเร็วขึ้น" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "ถ้าเปิด [x] จะนับ cim ทั้งหมด (วันนี้) ที่เริ่ม **วิ่งเร็วขึ้น** เพื่อพยายามขึ้นรถบัส รถราง หรือรถไฟก่อนเวลาออก\n" +
                    "cim จะวิ่งเร็วกว่า vanilla 512 เฟรม (~2-8 วินาทีจริง หรือ ~2 นาทีในเกม)"
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "รถบัส" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("รถบัส") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "รถราง" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("รถราง") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "รถไฟ" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("รถไฟ") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "รถไฟใต้ดิน" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("รถไฟใต้ดิน") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "เฟอร์รี" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("เฟอร์รี") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "เรือ" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("เรือ") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "เครื่องบิน" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("เครื่องบิน") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "บันทึกสถิติลง Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "เขียนรายงานแบบครั้งเดียวอย่างละเอียดลง **BetterBoarding.log**\n" +
                    "รวมจำนวนที่รอ ป้ายแย่สุด 3 อันดับของแต่ละโหมด ตัวอย่าง cim ที่ถูกข้าม Entity ID และคำใบ้สาย"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "เปิด Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "เปิด **BetterBoarding.log** ถ้ามีไฟล์อยู่\n" +
                    "ถ้ายังไม่มีไฟล์ จะเปิดโฟลเดอร์ Logs แทน"
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "ชื่อที่แสดงของม็อด" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "เวอร์ชัน" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "เวอร์ชันปัจจุบันของม็อด" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "เปิดหน้า Paradox Mods ของผู้สร้าง" },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "เปิด Log แบบละเอียด" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**สำหรับดีบัก / ทดสอบเท่านั้น**\n" +
                    "เพิ่มรายละเอียด <สด> ลง <Logs/BetterBoarding.log> ขณะเมืองทำงาน\n" +
                    "**อย่าเปิดไว้ตอนเล่นปกติ**\n" +
                    "การเปิดทิ้งไว้อาจลดประสิทธิภาพและทำให้ไฟล์ log ใหญ่มาก\n" +
                    "ลบไฟล์ log เก่าได้ภายหลัง\n" +
                    "หมายเหตุ: <Stats to Log> เป็นรายงาน ณ เวลานั้นพร้อมตัวนับการข้ามผู้โดยสารวันนี้ ซึ่งต่างจาก verbose log\n" +
                    "เปิด verbose log 15-20 นาทีถ้าต้องการดูไทม์ไลน์ว่าเกิดอะไรขึ้น\n" +
                    "อย่าลืมปิด verbose log ก่อนกลับไปเล่นปกติ"
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "ยังไม่ได้โหลดสถานะ" },
                { WaitStatus.KeyNoCityLoaded, "ยังไม่ได้โหลดเมือง" },
                { WaitStatus.KeyNoStopsFound, "ไม่พบป้าย" },

                { WaitStatus.KeyStatusLine, "รอ {0} | เฉลี่ย {1} | แย่สุด {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "มาสายวันนี้ {0}" },
                { WaitStatus.KeyStatusSkipOff, "ข้าม OFF" },

                { WaitStatus.KeyStatusOverviewLine, "นักท่องเที่ยว {0}/เดือน | พลเมือง {1}/เดือน | อัปเดต {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "วิ่งเร็วขึ้น OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] ขอรายงานสถิติ แต่ยังไม่ได้โหลดเมือง" },
                { WaitStatus.KeyReportTitle, "ภาพรวม Stats to Log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "การตั้งค่า: {0}" },
                { WaitStatus.KeyReportNote, "คำใบ้สายมาจาก waypoint ที่มีเวลารอสูงสุดของป้ายนั้น" },
                { WaitStatus.KeyReportTesterHintsHeader, "คำแนะนำสำหรับผู้ทดสอบ" },
                { WaitStatus.KeyReportHintWorstStops, "ป้ายแย่สุด: ตรวจในเกมหรือใช้ม็อด Scene Explorer ก่อน (ค้นหาตำแหน่งด้วย Entity ID) ดูการจราจร ตำแหน่งป้ายไม่ดี หรือป้ายมีปัญหา" },
                { WaitStatus.KeyReportHintSkippedCims, "cim เดี่ยวที่ถูกข้าม: ผู้โดยสารมาสายที่เราข้ามเพื่อให้รถออกได้ หลังจากนั้นสถานะควรเป็น 'มีเส้นทาง' หรือ 'ถูกกำหนดแล้ว' ถ้ายัง 'ไม่มีเส้นทาง' ให้ตรวจ entity นั้นอีกครั้งภายหลัง" },
                { WaitStatus.KeyReportHintLateGroups, "กลุ่มที่มาสาย (ครอบครัว): ปล่อยให้ vanilla จัดการเพื่อให้อยู่ด้วยกัน มีจำนวนน้อยเมื่อเทียบกับผู้เดินทางเดี่ยว" },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "ป้ายที่ให้บริการ: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "ป้ายที่มีผู้โดยสารรอ: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "ผู้โดยสารที่รอ: {0}" },
                { WaitStatus.KeyReportAverageWait, "เวลารอเฉลี่ย: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "ผู้โดยสารมาสายที่ถูกข้ามวันนี้: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "ป้ายแย่สุด: ไม่มี ตอนนี้ไม่มีผู้โดยสารรอ" },
                { WaitStatus.KeyReportWorstStopAverageWait, "เวลารอเฉลี่ยของป้ายแย่สุด: {0}" },
                { WaitStatus.KeyReportWorstStopName, "ชื่อป้ายแย่สุด: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entity ป้ายแย่สุด: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Entity waypoint แย่สุด: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "คำใบ้สายแย่สุด: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entity สายแย่สุด: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "ค่าเฉลี่ย waypoint ของสายแย่สุด: {0} มี {1} คนรอ" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "ป้ายแย่สุด {0} อันดับตามเวลารอเฉลี่ย:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | เฉลี่ย {2} | รอ {3} | stop entity {4} | waypoint entity {5} | line entity {6} | คำใบ้สาย {7}" },
                { WaitStatus.KeyReportLateGroups, "cim ที่มาสายเป็นกลุ่มและปล่อยไว้: {0} คนใน {1} กลุ่ม บน {2} คัน" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "ตัวอย่าง cim เดี่ยวที่มาสายและถูกข้าม" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | ผู้โดยสาร {2} | รถที่พลาด {3} | เวลา {4} | ตอนนี้ {5}" },
                { WaitStatus.KeyReportNone, "ไม่มี" },
                { WaitStatus.KeyReportUnknown, "(ไม่ทราบ)" },
            };
        }

        public void Unload()
        {
        }
    }
}
