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

        public LocaleTH(BBoardSettings setting)
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

            const string ToggleName = "ไม่รอผู้โดยสารที่มาช้า";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"ค่าสูงขึ้นทำให้การขึ้นลงที่ {transitName} เร็วขึ้น\n" +
                    "ช่วยลดเวลาจอดปกติระหว่างผู้โดยสารขึ้นลง\n" +
                    "<==========================>\n" +
                    "ค่าการขึ้นลง:\n" +
                    "1x = เวลาจอด vanilla 100%\n" +
                    "2x = ~1/2 ของเวลาที่กำหนด\n" +
                    "3x = ~1/3 (แนะนำ)\n" +
                    "5x = ~1/5 (สูงสุด)";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<สถานะ {transitName} ตอนนี้>\n" +
                    "**รอ** = จำนวนผู้โดยสารที่กำลังรอ\n" +
                    "**เฉลี่ย** = เวลารอเฉลี่ย\n" +
                    "**แย่สุด** = เวลารอเฉลี่ยสูงสุดของหนึ่งป้าย\n" +
                    "ถ้าสูง ให้ตรวจรถติด ป้ายติด/บั๊ก หรือจำนวนรถไม่พอ\n" +
                    $"**ปล่อยวันนี้** = งานโดยสารที่มาช้าซึ่ง <{ToggleName}> ปล่อยแล้ว\n" +
                    "ใช้ <สถิติลง Log> เพื่อดูชื่อป้าย Entity ID และรายละเอียดเพิ่ม";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "การทำงาน" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "เกี่ยวกับ" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "ความเร็วขึ้นลง" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "พฤติกรรม" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "สถานะ" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "ข้อมูลม็อด" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "ลิงก์" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "ดีบัก" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "ขึ้นลงรถบัส" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "ป้ายรถบัส",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "ขึ้นลงระบบราง" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "ป้ายรถไฟ รถราง และรถไฟใต้ดิน",
                        "ใช้กับรถไฟ รถราง และรถไฟใต้ดิน\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "ขึ้นลงเครื่องบิน" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "อาคารผู้โดยสารสนามบิน",
                        "ใช้กับอาคารผู้โดยสารเครื่องบิน\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "ช่วยให้รถบัส รถราง รถไฟ และรถไฟใต้ดินออกได้ เมื่อผู้โดยสารที่ถูกกำหนดอยู่ไกลเกินกว่าจะขึ้นรถอย่างชัดเจน\n" +
                    "- ผู้โดยสารที่อยู่ในหรือใกล้ป้าย/สถานียังถูกกำหนดไว้และขึ้นรถต่อ\n" +
                    "- ผู้โดยสารบนชานชาลารถไฟและรถไฟใต้ดินยาว ๆ จะไม่ถูกปล่อย\n" +
                    "- ผู้โดยสารที่อยู่ไกลอย่างชัดเจนอาจถูกปล่อยหลังเวลาออกตามตารางและช่วงผ่อนผันสั้น ๆ\n" +
                    "- กลุ่ม/ครอบครัวได้รับการป้องกันเพิ่ม เพื่อให้เด็กและสัตว์เลี้ยงขึ้นรถได้ครบ\n" +
                    "- เรือ เฟอร์รี และเครื่องบินยังใช้การจัดการผู้โดยสารมาช้าของ vanilla\n" +
                    "Cim ที่ถูกปล่อยยังอยู่ในเมืองและเดินทางต่อหรือหาเส้นทางใหม่ตามปกติ"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "ให้ Cim วิ่งเร็วขึ้น" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Cim ที่ถูกกำหนดแล้วจะเริ่ม <วิ่งเร็วขึ้น> เพื่อให้ทันรถบัส รถราง รถไฟ หรือรถไฟใต้ดินก่อนออก\n" +
                    "- ช่วยบนชานชาลายาวและตอนกำลังเข้าหาป้าย\n" +
                    "- เริ่มได้เร็วกว่า vanilla สูงสุดประมาณ 3 นาทีในเกม\n" +
                    "- มีผลเฉพาะ Cim ที่ถูกกำหนดให้รถที่กำลังรับผู้โดยสารแล้ว\n" +
                    "- ไม่วาร์ป ไม่บังคับขึ้นรถ และไม่เปลี่ยนเวลาออก"
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "การใช้งานรวม" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "จำนวนใช้ขนส่งสาธารณะต่อเดือนจากมุมมอง Transportation ของเกม\n" +
                    "เวลาอัปเดตคือเวลาที่เก็บสถานะนี้ (ปกติเมื่อเปิด Options)"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cim วิ่งเร็วขึ้น" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "เมื่อเปิด [x] จะนับ Cim วันนี้ที่ Better Boarding ให้ **เริ่มวิ่งเร็วขึ้น** เพื่อให้ทันรถบัส รถราง รถไฟ หรือรถไฟใต้ดิน\n" +
                    "Run Sooner เริ่มได้เร็วกว่า vanilla สูงสุดประมาณ 3 นาทีในเกม"
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
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "สถิติลง Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "เขียนรายงานละเอียดหนึ่งครั้งลง **BetterBoarding.log**\n" +
                    "มีจำนวนรอ 3 ป้ายแย่สุดต่อประเภท ตัวอย่างผู้โดยสารที่ถูกปล่อย Entity ID และข้อมูลสาย"
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "เปิด Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "เปิด **BetterBoarding.log** ถ้ามีไฟล์\n" +
                    "ถ้ายังไม่มี จะเปิดโฟลเดอร์ Logs แทน"
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "ม็อด" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "ชื่อม็อดที่แสดง" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "เวอร์ชัน" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "เวอร์ชันม็อดปัจจุบัน" },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "เปิดหน้า Paradox Mods ของผู้สร้าง" },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "เปิดบันทึกละเอียด" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**สำหรับดีบัก / ทดสอบเท่านั้น**\n" +
                    "ปกติให้ **ปิด** ไว้\n" +
                    "เพิ่มรายละเอียด <live> ลง <Logs/BetterBoarding.log> ขณะเมืองทำงาน\n" +
                    "**อย่าเปิดระหว่างเล่นปกติ**\n" +
                    "อาจลดประสิทธิภาพและสร้างไฟล์ log ใหญ่มาก\n" +
                    "ลบ log เก่าได้ภายหลัง\n" +
                    "<สถิติลง Log> เขียน snapshot ครั้งเดียวและกระทบประสิทธิภาพน้อยกว่าบันทึกละเอียด\n" +
                    "เปิด 10-15 นาทีเมื่อจำเป็นต้องดูเหตุการณ์ตามเวลา เพราะจะสร้างข้อมูลจำนวนมาก\n" +
                    "ก่อนเล่นปกติให้ **ปิด** อีกครั้ง"
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "ยังไม่โหลดสถานะ" },
                { WaitStatus.KeyNoCityLoaded, "ยังไม่ได้โหลดเมือง" },
                { WaitStatus.KeyNoStopsFound, "ไม่พบป้าย" },

                { WaitStatus.KeyStatusLine, "รอ {0} | เฉลี่ย {1} | แย่ {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "ปล่อยวันนี้ {0}" },
                { WaitStatus.KeyStatusSkipOff, "ไม่รอ OFF" },

                { WaitStatus.KeyStatusOverviewLine, "นักท่องเที่ยว {0}/ด. | พลเมือง {1}/ด. | {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "วิ่งเร็วขึ้น OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] ขอรายงานสถิติ แต่ยังไม่ได้โหลดเมือง" },
                { WaitStatus.KeyReportTitle, "Snapshot สถิติลง Log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "การตั้งค่า: {0}" },
                { WaitStatus.KeyReportNote, "ข้อมูลสายมาจาก waypoint ที่มีเวลารอสูงสุดของป้ายนั้น" },
                { WaitStatus.KeyReportTesterHintsHeader, "คำแนะนำทดสอบ" },
                { WaitStatus.KeyReportHintWorstStops, "ป้ายแย่สุด: ตรวจในเกมหรือ Scene Explorer ก่อน (หาตำแหน่งจาก Entity ID) ตรวจรถติด ตำแหน่งป้าย หรือป้ายที่มีบั๊ก" },
                { WaitStatus.KeyReportHintSkippedCims, "งานโดยสารเดี่ยวที่ปล่อย: ทุกจำนวนคือการปล่อยการกำหนดรถ 1 ครั้งหลังตรวจระยะทาง ดูตัวอย่างติดตามเพื่อดูว่าเกิดอะไรต่อ" },
                { WaitStatus.KeyReportHintLateGroups, "การขึ้นรถของกลุ่ม: แสดงครอบครัว/กลุ่มที่ยังขึ้นไม่ครบตอนสร้างรายงาน Better Boarding ให้เวลาพิเศษและช่วยสมาชิกที่ช้าขึ้นรถให้เสร็จ" },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "ป้ายที่ให้บริการ: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "ป้ายที่มีผู้โดยสารรอ: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "ผู้โดยสารรอ: {0}" },
                { WaitStatus.KeyReportAverageWait, "เวลารอเฉลี่ย: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "งานโดยสารเดี่ยวที่ปล่อยวันนี้: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "ป้ายแย่สุด: ไม่มี ตอนนี้ไม่มีผู้โดยสารรอ" },
                { WaitStatus.KeyReportWorstStopAverageWait, "เวลารอเฉลี่ยป้ายแย่สุด: {0}" },
                { WaitStatus.KeyReportWorstStopName, "ชื่อป้ายแย่สุด: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entity ป้ายแย่สุด: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Waypoint Entity ป้ายแย่สุด: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "ข้อมูลสายแย่สุด: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entity สายแย่สุด: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "ค่าเฉลี่ย waypoint สายแย่สุด: {0} มี {1} รอ" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "{0} ป้ายแย่สุดตามเวลารอเฉลี่ย:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | เฉลี่ย {2} | รอ {3} | ป้าย {4} | waypoint {5} | สาย {6} | ข้อมูล {7}" },
                { WaitStatus.KeyReportLateGroups, "กลุ่มขึ้นรถยังไม่ครบ: ผู้โดยสาร {0} ใน {1} กลุ่ม บน {2} คัน" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "ตัวอย่างผู้โดยสารเดี่ยวที่ถูกปล่อย" },
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
