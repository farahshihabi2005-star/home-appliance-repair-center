using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Helpers;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Services;

/// <summary>
/// الفئة الرئيسية: تحتفظ بقائمة من نوع Appliance (الفئة المجردة) تحوي أجهزة مختلفة
/// (ثلاجات وغسالات ومكيفات). عند التعامل مع القائمة نستدعي CalculateRepairCost و GetDetails
/// دون أن نعرف نوع كل جهاز، وكل جهاز ينفّذ الدالة بطريقته (تعدد الأشكال).
/// </summary>
public class RepairCenter
{
    private readonly string _name;
    private readonly List<Appliance> _appliances = new();
    private readonly CustomerNotifier _notifier = new();

    public RepairCenter(string name)
    {
        if (!InputValidator.IsValidText(name, 2, 60))
        {
            throw new ArgumentException("Repair center name must be between 2 and 60 characters.");
        }

        _name = name.Trim();
    }

    public string Name => _name;

    /// <summary>عرض للقراءة فقط: لا يمكن إضافة أجهزة إلا عبر AddAppliance.</summary>
    public IReadOnlyList<Appliance> Appliances => _appliances;

    public CustomerNotifier Notifier => _notifier;

    /// <summary>يضيف جهازًا جديدًا ويربطه بمرسل الإشعارات.</summary>
    public void AddAppliance(Appliance appliance)
    {
        ArgumentNullException.ThrowIfNull(appliance);

        if (FindBySerial(appliance.SerialNumber) != null)
        {
            throw new InvalidOperationException($"An appliance with serial number {appliance.SerialNumber} is already registered.");
        }

        _appliances.Add(appliance);
        _notifier.Subscribe(appliance);
    }

    /// <summary>يبحث عن جهاز بالرقم التسلسلي، ويعيد null إذا لم يجده.</summary>
    public Appliance? FindBySerial(string serialNumber)
    {
        string key = serialNumber.Trim().ToUpperInvariant();
        return _appliances.FirstOrDefault(appliance => appliance.SerialNumber == key);
    }

    public List<Appliance> GetByStatus(RepairStatus status)
    {
        return _appliances.Where(appliance => appliance.Status == status).ToList();
    }

    /// <summary>الأجهزة التي ما زالت قيد الصيانة.</summary>
    public List<Appliance> GetUnderRepair()
    {
        return GetByStatus(RepairStatus.UnderRepair);
    }

    /// <summary>الأجهزة التي تم إصلاحها (جاهزة أو مُسلَّمة).</summary>
    public List<Appliance> GetRepaired()
    {
        return _appliances
            .Where(appliance => appliance.Status == RepairStatus.Completed || appliance.Status == RepairStatus.Delivered)
            .ToList();
    }

    /// <summary>الأجهزة المتأخرة عن موعد التسليم في التاريخ المعطى.</summary>
    public List<Appliance> GetDelayed(DateTime today)
    {
        return _appliances.Where(appliance => appliance.IsDelayed(today)).ToList();
    }

    /// <summary>يفحص كل الأجهزة، فتُطلق المتأخرة منها حدث DeliveryDelayed.</summary>
    public void CheckDelays(DateTime today)
    {
        foreach (Appliance appliance in _appliances)
        {
            appliance.CheckDelay(today);
        }
    }

    /// <summary>مجموع تكلفة إصلاح كل الأجهزة المسجلة.</summary>
    public decimal CalculateTotalCost()
    {
        return SumCosts(_appliances);
    }

    /// <summary>مجموع تكلفة الأجهزة التي تم إصلاحها فقط.</summary>
    public decimal CalculateRepairedCost()
    {
        return SumCosts(GetRepaired());
    }

    /// <summary>يبني تقريرًا نصيًا لمجموعة أجهزة (تعدد الأشكال: كل جهاز يعرض تفاصيله الخاصة).</summary>
    public static string BuildReport(IEnumerable<Appliance> appliances)
    {
        var report = new StringBuilder();

        foreach (Appliance appliance in appliances)
        {
            report.AppendLine(appliance.GetDetails());
            report.AppendLine();
        }

        return report.ToString().TrimEnd();
    }

    // تعدد الأشكال: نفس السطر يستدعي دالة مختلفة حسب النوع الحقيقي لكل جهاز
    private static decimal SumCosts(IEnumerable<Appliance> appliances)
    {
        decimal total = 0m;

        foreach (Appliance appliance in appliances)
        {
            total += appliance.CalculateRepairCost();
        }

        return total;
    }
}
