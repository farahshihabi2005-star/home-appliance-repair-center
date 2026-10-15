using System;
using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Events;

/// <summary>
/// البيانات التي تُرسَل مع كل حدث: أي جهاز، وما الحالة القديمة والجديدة، ونص الرسالة.
/// جميع خصائصها للقراءة فقط حتى لا يغيّرها مستقبِل الحدث.
/// </summary>
public class ApplianceEventArgs : EventArgs
{
    public Appliance Appliance { get; }
    public RepairStatus OldStatus { get; }
    public RepairStatus NewStatus { get; }
    public string Message { get; }
    public DateTime RaisedAt { get; }

    public ApplianceEventArgs(Appliance appliance, RepairStatus oldStatus, RepairStatus newStatus, string message)
    {
        Appliance = appliance;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Message = message;
        RaisedAt = DateTime.Now;
    }
}
