namespace HomeApplianceRepairCenter.Enums;

/// <summary>
/// حالات الجهاز أثناء رحلته داخل مركز الصيانة.
/// الترتيب: Received -> UnderRepair -> Completed -> Delivered
/// </summary>
public enum RepairStatus
{
    /// <summary>تم استلام الجهاز وبانتظار بدء الإصلاح.</summary>
    Received,

    /// <summary>الجهاز قيد الصيانة حاليًا.</summary>
    UnderRepair,

    /// <summary>تم إصلاح الجهاز وهو جاهز للتسليم.</summary>
    Completed,

    /// <summary>تم تسليم الجهاز لصاحبه.</summary>
    Delivered
}
