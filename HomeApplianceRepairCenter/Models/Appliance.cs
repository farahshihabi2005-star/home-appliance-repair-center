using System;
using System.Collections.Generic;
using System.Text;
using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Helpers;
using HomeApplianceRepairCenter.Interfaces;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// الفئة المجردة (Abstract Class) التي تمثل أي جهاز منزلي داخل مركز الصيانة.
/// تطبّق الواجهة IRepairable وتحتوي الخصائص المشتركة بين كل الأجهزة،
/// وتترك حساب التكلفة للفئات الفرعية لأن لكل جهاز طريقته الخاصة.
/// </summary>
public abstract class Appliance : IRepairable
{
    /// <summary>أقصى عدد ساعات عمل مسموح بتسجيله لجهاز واحد.</summary>
    public const double MaxLaborHours = 200;

    // ---------- أعضاء ثابتة (Static): عدّادات مشتركة بين كل الأجهزة ----------
    private static int _totalCount;
    private static readonly Dictionary<RepairStatus, int> _countByStatus = CreateStatusCounters();

    // ---------- الحقول الخاصة (Encapsulation) ----------
    // حقول لا تتغير بعد إنشاء الكائن
    private readonly string _serialNumber;
    private readonly string _brand;
    private readonly DateTime _receivedDate;
    private readonly bool _hasWarranty;

    // حقول يمكن تغييرها فقط عبر خصائص أو دوال تتحقق من القيمة
    private string _ownerName = string.Empty;
    private string _problemDescription = string.Empty;
    private DateTime _expectedDeliveryDate;
    private RepairStatus _status;
    private double _laborHours;
    private decimal _partsCost;
    private bool _delayNotified;

    // ---------- الأحداث (Events) المبنية على التفويض ApplianceNotificationHandler ----------

    /// <summary>يُطلَق كلما تغيّرت حالة الجهاز.</summary>
    public event ApplianceNotificationHandler? StatusChanged;

    /// <summary>يُطلَق عند اكتمال إصلاح الجهاز.</summary>
    public event ApplianceNotificationHandler? RepairCompleted;

    /// <summary>يُطلَق عند تأخر تسليم الجهاز عن الموعد المتوقع.</summary>
    public event ApplianceNotificationHandler? DeliveryDelayed;

    /// <summary>
    /// المُنشئ: يتحقق من كل القيم قبل إنشاء الجهاز، ويرمي ArgumentException إذا كانت غير صالحة.
    /// </summary>
    protected Appliance(
        string serialNumber,
        string brand,
        string ownerName,
        string problemDescription,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        bool hasWarranty)
    {
        if (!InputValidator.IsValidSerialNumber(serialNumber))
        {
            throw new ArgumentException("Invalid serial number. Use 2-3 letters, a dash, then 4-8 digits (e.g. FR-100234).");
        }

        if (!InputValidator.IsValidText(brand, 2, 30))
        {
            throw new ArgumentException("Brand must be between 2 and 30 characters.");
        }

        if (!InputValidator.IsValidReceivedDate(receivedDate))
        {
            throw new ArgumentException("Received date cannot be in the future.");
        }

        _serialNumber = serialNumber.Trim().ToUpperInvariant();
        _brand = brand.Trim();
        _receivedDate = receivedDate.Date;
        _hasWarranty = hasWarranty;
        _status = RepairStatus.Received;

        // نستخدم الخصائص هنا حتى تُطبَّق نفس قواعد التحقق ولا نكررها
        OwnerName = ownerName;
        ProblemDescription = problemDescription;
        ExpectedDeliveryDate = expectedDeliveryDate;

        _totalCount++;
        _countByStatus[RepairStatus.Received]++;
    }

    // ---------- خصائص للقراءة فقط بعد الإنشاء ----------

    /// <summary>الرقم التسلسلي: يُحدَّد مرة واحدة ولا يمكن تعديله.</summary>
    public string SerialNumber => _serialNumber;

    /// <summary>الماركة: لا يمكن تعديلها بعد الإنشاء.</summary>
    public string Brand => _brand;

    /// <summary>تاريخ استلام الجهاز: لا يمكن تعديله بعد الإنشاء.</summary>
    public DateTime ReceivedDate => _receivedDate;

    /// <summary>هل الجهاز تحت الكفالة؟ (الكفالة تغطي أجور اليد العاملة).</summary>
    public bool HasWarranty => _hasWarranty;

    /// <summary>الحالة الحالية: تتغير فقط عبر StartRepair و CompleteRepair و DeliverToCustomer.</summary>
    public RepairStatus Status => _status;

    // ---------- خصائص قابلة للتعديل مع تحقق من القيمة ----------

    /// <summary>اسم صاحب الجهاز.</summary>
    public string OwnerName
    {
        get => _ownerName;
        set
        {
            if (!InputValidator.IsValidName(value))
            {
                throw new ArgumentException("Owner name must be 2-50 letters (spaces, . ' - are allowed).");
            }

            _ownerName = value.Trim();
        }
    }

    /// <summary>وصف العطل.</summary>
    public string ProblemDescription
    {
        get => _problemDescription;
        set
        {
            if (!InputValidator.IsValidText(value, 3, 200))
            {
                throw new ArgumentException("Problem description must be between 3 and 200 characters.");
            }

            _problemDescription = value.Trim();
        }
    }

    /// <summary>موعد التسليم المتوقع (يمكن تمديده لكن لا يسبق تاريخ الاستلام).</summary>
    public DateTime ExpectedDeliveryDate
    {
        get => _expectedDeliveryDate;
        set
        {
            if (!InputValidator.IsValidDeliveryDate(_receivedDate, value))
            {
                throw new ArgumentException("Expected delivery date cannot be before the received date.");
            }

            _expectedDeliveryDate = value.Date;

            // إذا تغيّر الموعد نسمح بإشعار تأخير جديد
            _delayNotified = false;
        }
    }

    /// <summary>عدد ساعات العمل الفعلية (تُقفَل بعد اكتمال الإصلاح).</summary>
    public double LaborHours
    {
        get => _laborHours;
        set
        {
            EnsureCostCanBeModified();

            if (!InputValidator.IsInRange(value, 0, MaxLaborHours))
            {
                throw new ArgumentException($"Labor hours must be between 0 and {MaxLaborHours}.");
            }

            _laborHours = value;
        }
    }

    /// <summary>تكلفة قطع الغيار (تُقفَل بعد اكتمال الإصلاح).</summary>
    public decimal PartsCost
    {
        get => _partsCost;
        set
        {
            EnsureCostCanBeModified();

            if (!InputValidator.IsNonNegative(value))
            {
                throw new ArgumentException("Parts cost cannot be negative.");
            }

            _partsCost = value;
        }
    }

    // ---------- الأعضاء المجردة: كل فئة فرعية يجب أن تعرّفها بطريقتها ----------

    /// <summary>اسم نوع الجهاز للعرض (مثل Refrigerator).</summary>
    public abstract string ApplianceType { get; }

    /// <summary>أجرة ساعة العمل: تختلف من جهاز لآخر.</summary>
    protected abstract decimal LaborRatePerHour { get; }

    /// <summary>
    /// دالة مجردة: تحسب تكلفة الإصلاح الكلية.
    /// هي تنفيذ للدالة المعرّفة في الواجهة IRepairable، وتتركها للفئات الفرعية.
    /// </summary>
    public abstract decimal CalculateRepairCost();

    // ---------- العمليات المشتركة (تنتقل بالجهاز بين الحالات) ----------

    public void StartRepair()
    {
        EnsureStatus(RepairStatus.Received, "start repair");
        ChangeStatus(RepairStatus.UnderRepair, $"Repair of your {ApplianceType} ({SerialNumber}) has started.");
    }

    public void CompleteRepair()
    {
        EnsureStatus(RepairStatus.UnderRepair, "complete repair");
        ChangeStatus(RepairStatus.Completed, $"Repair of your {ApplianceType} ({SerialNumber}) is finished.");

        string message = $"Your {ApplianceType} ({SerialNumber}) is ready for pickup. Total cost: {DisplayFormatter.Money(CalculateRepairCost())}.";
        OnRepairCompleted(new ApplianceEventArgs(this, RepairStatus.UnderRepair, RepairStatus.Completed, message));
    }

    public void DeliverToCustomer()
    {
        EnsureStatus(RepairStatus.Completed, "deliver appliance");
        ChangeStatus(RepairStatus.Delivered, $"Your {ApplianceType} ({SerialNumber}) was delivered. Thank you!");
    }

    // ---------- التأخير ----------

    /// <summary>هل تأخر الجهاز عن موعد التسليم في التاريخ المعطى؟</summary>
    public bool IsDelayed(DateTime today)
    {
        bool stillInCenter = _status != RepairStatus.Completed && _status != RepairStatus.Delivered;
        return stillInCenter && today.Date > _expectedDeliveryDate;
    }

    /// <summary>يفحص التأخير ويُطلق الحدث DeliveryDelayed مرة واحدة فقط لكل موعد.</summary>
    public void CheckDelay(DateTime today)
    {
        if (_delayNotified || !IsDelayed(today))
        {
            return;
        }

        _delayNotified = true;
        int daysLate = (today.Date - _expectedDeliveryDate).Days;
        string message = $"Your {ApplianceType} ({SerialNumber}) is {daysLate} day(s) late. Expected delivery was {DisplayFormatter.Date(_expectedDeliveryDate)}.";
        OnDeliveryDelayed(new ApplianceEventArgs(this, _status, _status, message));
    }

    // ---------- العرض ----------

    /// <summary>
    /// دالة virtual: تعرض البيانات المشتركة، والفئات الفرعية تضيف عليها تفاصيلها الخاصة.
    /// </summary>
    public virtual string GetDetails()
    {
        var details = new StringBuilder();
        details.AppendLine($"[{ApplianceType}] Serial: {SerialNumber} | Brand: {Brand} | Status: {Status}");
        details.AppendLine($"  Owner: {OwnerName} | Problem: {ProblemDescription}");
        details.AppendLine($"  Received: {DisplayFormatter.Date(ReceivedDate)} | Expected delivery: {DisplayFormatter.Date(ExpectedDeliveryDate)} | Warranty: {DisplayFormatter.YesNo(HasWarranty)}");
        details.Append($"  Labor hours: {LaborHours:0.##} | Parts: {DisplayFormatter.Money(PartsCost)} | Repair cost: {DisplayFormatter.Money(CalculateRepairCost())}");
        return details.ToString();
    }

    public override string ToString()
    {
        return $"{ApplianceType} {SerialNumber} ({Status})";
    }

    // ---------- الأعضاء الثابتة (Static) لإحصاء الأجهزة حسب الحالة ----------

    /// <summary>عدد كل الأجهزة التي سُجّلت في النظام.</summary>
    public static int TotalCount => _totalCount;

    /// <summary>عدد الأجهزة المستلمة وبانتظار بدء الإصلاح.</summary>
    public static int ReceivedCount => _countByStatus[RepairStatus.Received];

    /// <summary>عدد الأجهزة قيد الصيانة حاليًا.</summary>
    public static int UnderRepairCount => _countByStatus[RepairStatus.UnderRepair];

    /// <summary>عدد الأجهزة التي تم إصلاحها وبانتظار التسليم.</summary>
    public static int CompletedCount => _countByStatus[RepairStatus.Completed];

    /// <summary>عدد الأجهزة التي سُلّمت لأصحابها.</summary>
    public static int DeliveredCount => _countByStatus[RepairStatus.Delivered];

    // ---------- دوال مساعدة داخلية ----------

    /// <summary>
    /// دالة محمية: تحسب (أجور العمل + قطع الغيار) المشتركة بين كل الأجهزة.
    /// الأجور مجانية إذا كان الجهاز تحت الكفالة.
    /// </summary>
    protected decimal CalculateBaseCost()
    {
        decimal laborCost = HasWarranty ? 0m : (decimal)LaborHours * LaborRatePerHour;
        return Math.Round(laborCost + PartsCost, 2);
    }

    /// <summary>تمنع تعديل بيانات التكلفة بعد اكتمال الإصلاح.</summary>
    protected void EnsureCostCanBeModified()
    {
        if (_status == RepairStatus.Completed || _status == RepairStatus.Delivered)
        {
            throw new InvalidOperationException($"Cost details of {SerialNumber} are locked because the repair is already {_status}.");
        }
    }

    protected virtual void OnStatusChanged(ApplianceEventArgs e)
    {
        StatusChanged?.Invoke(this, e);
    }

    protected virtual void OnRepairCompleted(ApplianceEventArgs e)
    {
        RepairCompleted?.Invoke(this, e);
    }

    protected virtual void OnDeliveryDelayed(ApplianceEventArgs e)
    {
        DeliveryDelayed?.Invoke(this, e);
    }

    private void EnsureStatus(RepairStatus required, string action)
    {
        if (_status != required)
        {
            throw new InvalidOperationException($"Cannot {action}: appliance {SerialNumber} is '{_status}' but must be '{required}'.");
        }
    }

    private void ChangeStatus(RepairStatus newStatus, string message)
    {
        RepairStatus oldStatus = _status;
        _status = newStatus;

        // تحديث العدّادات الثابتة
        _countByStatus[oldStatus]--;
        _countByStatus[newStatus]++;

        OnStatusChanged(new ApplianceEventArgs(this, oldStatus, newStatus, message));
    }

    private static Dictionary<RepairStatus, int> CreateStatusCounters()
    {
        var counters = new Dictionary<RepairStatus, int>();

        foreach (RepairStatus status in Enum.GetValues<RepairStatus>())
        {
            counters[status] = 0;
        }

        return counters;
    }
}
