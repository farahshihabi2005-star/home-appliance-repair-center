using System;
using HomeApplianceRepairCenter.Models;
using HomeApplianceRepairCenter.Services;

namespace HomeApplianceRepairCenter.Data;

/// <summary>
/// فئة ثابتة تحمّل بيانات تجريبية (ستة أجهزة بحالات مختلفة) لتجربة النظام فورًا.
/// نغيّر حالات الأجهزة قبل إضافتها للمركز حتى لا تظهر إشعارات أثناء التحميل.
/// </summary>
public static class SampleData
{
    public static void Load(RepairCenter center)
    {
        DateTime today = DateTime.Today;

        // 1) ثلاجة قيد الصيانة ومتأخرة عن الموعد (ستُطلق حدث التأخير)
        var fridgeLate = new Refrigerator("FR-100001", "Samsung", "Ahmad Khaled", "Not cooling",
            today.AddDays(-12), today.AddDays(-3), false, 450, false);
        fridgeLate.StartRepair();
        fridgeLate.NeedsGasRefill = true;   // اكتُشف تسرب غاز أثناء الفحص
        fridgeLate.LaborHours = 3;
        fridgeLate.PartsCost = 25m;

        // 2) ثلاجة تحت الكفالة، تم إصلاحها وجاهزة للتسليم
        var fridgeDone = new Refrigerator("FR-100002", "Bosch", "Lina Youssef", "Door seal is broken",
            today.AddDays(-15), today.AddDays(-8), true, 600, false);
        fridgeDone.StartRepair();
        fridgeDone.LaborHours = 2;
        fridgeDone.PartsCost = 15m;
        fridgeDone.CompleteRepair();

        // 3) غسالة أمامية قيد الصيانة
        var washerActive = new WashingMachine("WM-200001", "LG", "Sara Ali", "Drum makes a loud noise",
            today.AddDays(-8), today.AddDays(2), false, 8, true, false);
        washerActive.StartRepair();
        washerActive.NeedsDrumReplacement = true;
        washerActive.LaborHours = 4;
        washerActive.PartsCost = 30m;

        // 4) غسالة تم إصلاحها وتسليمها
        var washerDelivered = new WashingMachine("WM-200002", "Samsung", "Khaled Nasser", "Does not spin",
            today.AddDays(-20), today.AddDays(-10), false, 7, false, false);
        washerDelivered.StartRepair();
        washerDelivered.LaborHours = 1.5;
        washerDelivered.PartsCost = 18m;
        washerDelivered.CompleteRepair();
        washerDelivered.DeliverToCustomer();

        // 5) مكيّف مستلم وبانتظار بدء الإصلاح
        var acWaiting = new AirConditioner("AC-300001", "Gree", "Omar Hasan", "Leaking water",
            today.AddDays(-5), today.AddDays(5), false, 18000, false, false);

        // 6) مكيّف إنفرتر كبير قيد الصيانة ويتطلب زيارة منزلية
        var acActive = new AirConditioner("AC-300002", "Midea", "Rania Saeed", "Not cooling",
            today.AddDays(-3), today.AddDays(6), false, 24000, true, true);
        acActive.StartRepair();
        acActive.LaborHours = 2.5;
        acActive.PartsCost = 45m;

        center.AddAppliance(fridgeLate);
        center.AddAppliance(fridgeDone);
        center.AddAppliance(washerActive);
        center.AddAppliance(washerDelivered);
        center.AddAppliance(acWaiting);
        center.AddAppliance(acActive);
    }
}
