using HomeApplianceRepairCenter.Enums;

namespace HomeApplianceRepairCenter.Interfaces;

/// <summary>
/// الواجهة (Interface): تعرّف العمليات الأساسية المشتركة بين كل الأجهزة
/// القابلة للصيانة، دون أن تحدد كيف تُنفَّذ.
/// </summary>
public interface IRepairable
{
    /// <summary>الرقم التسلسلي للجهاز (للقراءة فقط).</summary>
    string SerialNumber { get; }

    /// <summary>الحالة الحالية للجهاز (للقراءة فقط).</summary>
    RepairStatus Status { get; }

    /// <summary>بدء عملية الإصلاح.</summary>
    void StartRepair();

    /// <summary>إنهاء عملية الإصلاح.</summary>
    void CompleteRepair();

    /// <summary>تسليم الجهاز لصاحبه بعد الإصلاح.</summary>
    void DeliverToCustomer();

    /// <summary>حساب تكلفة الإصلاح (تختلف حسب نوع الجهاز).</summary>
    decimal CalculateRepairCost();

    /// <summary>نص يلخّص بيانات الجهاز وحالته.</summary>
    string GetDetails();
}
