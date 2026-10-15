using System;
using System.Collections.Generic;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Services;

/// <summary>
/// مستقبِل الأحداث: يشترك في أحداث الأجهزة، وعند وقوع حدث يطبع إشعارًا للزبون
/// ويحفظه في سجل الإشعارات.
/// الدوال الثلاث الخاصة هنا تطابق توقيع التفويض ApplianceNotificationHandler.
/// </summary>
public class CustomerNotifier
{
    private readonly List<string> _history = new();

    /// <summary>سجل كل الإشعارات المُرسلة (للقراءة فقط).</summary>
    public IReadOnlyList<string> History => _history;

    /// <summary>يربط دوال هذه الفئة بأحداث الجهاز (الاشتراك في الأحداث).</summary>
    public void Subscribe(Appliance appliance)
    {
        appliance.StatusChanged += OnStatusChanged;
        appliance.RepairCompleted += OnRepairCompleted;
        appliance.DeliveryDelayed += OnDeliveryDelayed;
    }

    private void OnStatusChanged(object sender, ApplianceEventArgs e)
    {
        Notify("STATUS", e, ConsoleColor.Cyan);
    }

    private void OnRepairCompleted(object sender, ApplianceEventArgs e)
    {
        Notify("READY", e, ConsoleColor.Green);
    }

    private void OnDeliveryDelayed(object sender, ApplianceEventArgs e)
    {
        Notify("DELAY", e, ConsoleColor.Yellow);
    }

    private void Notify(string tag, ApplianceEventArgs e, ConsoleColor color)
    {
        string line = $"[{e.RaisedAt:yyyy-MM-dd HH:mm}] [{tag}] To {e.Appliance.OwnerName}: {e.Message}";
        _history.Add(line);

        Console.ForegroundColor = color;
        Console.WriteLine($"  >> {line}");
        Console.ResetColor();
    }
}
