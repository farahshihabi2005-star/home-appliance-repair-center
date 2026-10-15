using System;
using HomeApplianceRepairCenter.Helpers;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// مكيّف: فئة فرعية من Appliance.
/// تكلفته = أجور + قطع + (زيارة منزلية) + (رسوم الإنفرتر) + (رسوم الوحدات الكبيرة).
/// </summary>
public class AirConditioner : Appliance
{
    public const int MinCapacityBtu = 9000;
    public const int MaxCapacityBtu = 60000;

    private const int LargeCapacityBtu = 24000;
    private const decimal OnSiteVisitFee = 30m;
    private const decimal InverterDiagnosticFee = 20m;
    private const decimal LargeUnitFee = 20m;

    private readonly int _capacityBtu;
    private readonly bool _isInverter;
    private bool _requiresOnSiteVisit;

    public AirConditioner(
        string serialNumber,
        string brand,
        string ownerName,
        string problemDescription,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        bool hasWarranty,
        int capacityBtu,
        bool isInverter,
        bool requiresOnSiteVisit)
        : base(serialNumber, brand, ownerName, problemDescription, receivedDate, expectedDeliveryDate, hasWarranty)
    {
        if (!InputValidator.IsInRange(capacityBtu, MinCapacityBtu, MaxCapacityBtu))
        {
            throw new ArgumentException($"Air conditioner capacity must be between {MinCapacityBtu} and {MaxCapacityBtu} BTU.");
        }

        _capacityBtu = capacityBtu;
        _isInverter = isInverter;
        _requiresOnSiteVisit = requiresOnSiteVisit;
    }

    /// <summary>القدرة التبريدية بوحدة BTU (للقراءة فقط).</summary>
    public int CapacityBtu => _capacityBtu;

    /// <summary>هل المكيّف من نوع إنفرتر؟ (للقراءة فقط).</summary>
    public bool IsInverter => _isInverter;

    /// <summary>هل يتطلب الإصلاح زيارة منزلية؟ (تُقفَل بعد اكتمال الإصلاح).</summary>
    public bool RequiresOnSiteVisit
    {
        get => _requiresOnSiteVisit;
        set
        {
            EnsureCostCanBeModified();
            _requiresOnSiteVisit = value;
        }
    }

    public override string ApplianceType => "Air Conditioner";

    protected override decimal LaborRatePerHour => 15m;

    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost();

        if (RequiresOnSiteVisit)
        {
            cost += OnSiteVisitFee;
        }

        if (IsInverter)
        {
            cost += InverterDiagnosticFee;
        }

        if (CapacityBtu >= LargeCapacityBtu)
        {
            cost += LargeUnitFee;
        }

        return cost;
    }

    public override string GetDetails()
    {
        return base.GetDetails() + Environment.NewLine +
               $"  Capacity: {CapacityBtu} BTU | Inverter: {DisplayFormatter.YesNo(IsInverter)} | On-site visit: {DisplayFormatter.YesNo(RequiresOnSiteVisit)}";
    }
}
