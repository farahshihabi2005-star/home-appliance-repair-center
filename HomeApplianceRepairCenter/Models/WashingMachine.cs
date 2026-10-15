using System;
using HomeApplianceRepairCenter.Helpers;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// غسالة: فئة فرعية من Appliance.
/// تكلفتها = أجور + قطع + (استبدال الدرفيل عند الحاجة) + (رسوم الفك للغسالات الأمامية).
/// </summary>
public class WashingMachine : Appliance
{
    public const int MinLoadCapacityKg = 3;
    public const int MaxLoadCapacityKg = 30;

    private const decimal DrumReplacementFee = 120m;
    private const decimal FrontLoadFee = 15m;

    private readonly int _loadCapacityKg;
    private readonly bool _isFrontLoad;
    private bool _needsDrumReplacement;

    public WashingMachine(
        string serialNumber,
        string brand,
        string ownerName,
        string problemDescription,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        bool hasWarranty,
        int loadCapacityKg,
        bool isFrontLoad,
        bool needsDrumReplacement)
        : base(serialNumber, brand, ownerName, problemDescription, receivedDate, expectedDeliveryDate, hasWarranty)
    {
        if (!InputValidator.IsInRange(loadCapacityKg, MinLoadCapacityKg, MaxLoadCapacityKg))
        {
            throw new ArgumentException($"Washing machine load capacity must be between {MinLoadCapacityKg} and {MaxLoadCapacityKg} kg.");
        }

        _loadCapacityKg = loadCapacityKg;
        _isFrontLoad = isFrontLoad;
        _needsDrumReplacement = needsDrumReplacement;
    }

    /// <summary>سعة الغسالة بالكيلوغرام (للقراءة فقط).</summary>
    public int LoadCapacityKg => _loadCapacityKg;

    /// <summary>هل هي غسالة بتحميل أمامي؟ (للقراءة فقط).</summary>
    public bool IsFrontLoad => _isFrontLoad;

    /// <summary>هل تحتاج إلى استبدال الدرفيل؟ (تُقفَل بعد اكتمال الإصلاح).</summary>
    public bool NeedsDrumReplacement
    {
        get => _needsDrumReplacement;
        set
        {
            EnsureCostCanBeModified();
            _needsDrumReplacement = value;
        }
    }

    public override string ApplianceType => "Washing Machine";

    protected override decimal LaborRatePerHour => 10m;

    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost();

        if (NeedsDrumReplacement)
        {
            cost += DrumReplacementFee;
        }

        if (IsFrontLoad)
        {
            cost += FrontLoadFee;
        }

        return cost;
    }

    public override string GetDetails()
    {
        return base.GetDetails() + Environment.NewLine +
               $"  Load: {LoadCapacityKg} kg | Front load: {DisplayFormatter.YesNo(IsFrontLoad)} | Drum replacement needed: {DisplayFormatter.YesNo(NeedsDrumReplacement)}";
    }
}
