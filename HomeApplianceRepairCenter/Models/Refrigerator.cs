using System;
using HomeApplianceRepairCenter.Helpers;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// ثلاجة: فئة فرعية من Appliance.
/// تكلفتها = أجور + قطع + (تعبئة غاز عند الحاجة) + (رسوم إضافية للثلاجات الكبيرة).
/// </summary>
public class Refrigerator : Appliance
{
    public const int MinCapacityLiters = 50;
    public const int MaxCapacityLiters = 1000;

    private const int LargeCapacityLiters = 500;
    private const decimal GasRefillFee = 40m;
    private const decimal LargeSizeFee = 25m;

    private readonly int _capacityLiters;
    private bool _needsGasRefill;

    public Refrigerator(
        string serialNumber,
        string brand,
        string ownerName,
        string problemDescription,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        bool hasWarranty,
        int capacityLiters,
        bool needsGasRefill)
        : base(serialNumber, brand, ownerName, problemDescription, receivedDate, expectedDeliveryDate, hasWarranty)
    {
        if (!InputValidator.IsInRange(capacityLiters, MinCapacityLiters, MaxCapacityLiters))
        {
            throw new ArgumentException($"Refrigerator capacity must be between {MinCapacityLiters} and {MaxCapacityLiters} liters.");
        }

        _capacityLiters = capacityLiters;
        _needsGasRefill = needsGasRefill;
    }

    /// <summary>سعة الثلاجة باللتر (للقراءة فقط بعد الإنشاء).</summary>
    public int CapacityLiters => _capacityLiters;

    /// <summary>هل تحتاج الثلاجة إلى تعبئة غاز؟ (تُقفَل بعد اكتمال الإصلاح).</summary>
    public bool NeedsGasRefill
    {
        get => _needsGasRefill;
        set
        {
            EnsureCostCanBeModified();
            _needsGasRefill = value;
        }
    }

    public override string ApplianceType => "Refrigerator";

    protected override decimal LaborRatePerHour => 12m;

    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost();

        if (NeedsGasRefill)
        {
            cost += GasRefillFee;
        }

        if (CapacityLiters > LargeCapacityLiters)
        {
            cost += LargeSizeFee;
        }

        return cost;
    }

    public override string GetDetails()
    {
        return base.GetDetails() + Environment.NewLine +
               $"  Capacity: {CapacityLiters} L | Gas refill needed: {DisplayFormatter.YesNo(NeedsGasRefill)}";
    }
}
