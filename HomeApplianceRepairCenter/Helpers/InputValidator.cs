using System;
using System.Text.RegularExpressions;

namespace HomeApplianceRepairCenter.Helpers;

/// <summary>
/// فئة ثابتة (static class): تجمع دوال التحقق من صحة البيانات المدخلة.
/// لا يمكن إنشاء كائن منها، وتُستدعى مباشرة مثل: InputValidator.IsValidSerialNumber("FR-100234")
/// </summary>
public static class InputValidator
{
    /// <summary>صيغة عرض التاريخ المعتمدة في النظام.</summary>
    public const string DateFormat = "yyyy-MM-dd";

    // الرقم التسلسلي: 2-3 أحرف إنجليزية كبيرة + شرطة + 4-8 أرقام (مثال: FR-100234)
    private static readonly Regex SerialNumberPattern =
        new(@"^[A-Z]{2,3}-[0-9]{4,8}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // الاسم: يبدأ بحرف (عربي أو إنجليزي) ثم أحرف أو مسافات أو . ' - ، بطول 2-50
    private static readonly Regex NamePattern =
        new(@"^\p{L}[\p{L} .'-]{1,49}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>هل الرقم التسلسلي بالصيغة الصحيحة؟</summary>
    public static bool IsValidSerialNumber(string? serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return false;
        }

        return SerialNumberPattern.IsMatch(serialNumber.Trim().ToUpperInvariant());
    }

    /// <summary>هل الاسم صالح (أحرف فقط وبطول مناسب)؟</summary>
    public static bool IsValidName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name) && NamePattern.IsMatch(name.Trim());
    }

    /// <summary>هل النص غير فارغ وطوله ضمن الحدود المسموحة؟</summary>
    public static bool IsValidText(string? text, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        int length = text.Trim().Length;
        return length >= minLength && length <= maxLength;
    }

    /// <summary>تاريخ الاستلام لا يجوز أن يكون في المستقبل.</summary>
    public static bool IsValidReceivedDate(DateTime receivedDate)
    {
        return receivedDate.Date <= DateTime.Today;
    }

    /// <summary>موعد التسليم المتوقع لا يجوز أن يسبق تاريخ الاستلام.</summary>
    public static bool IsValidDeliveryDate(DateTime receivedDate, DateTime expectedDeliveryDate)
    {
        return expectedDeliveryDate.Date >= receivedDate.Date;
    }

    /// <summary>هل القيمة الرقمية ضمن المجال [min, max]؟</summary>
    public static bool IsInRange(double value, double min, double max)
    {
        return value >= min && value <= max;
    }

    /// <summary>هل المبلغ المالي صفر أو أكبر؟</summary>
    public static bool IsNonNegative(decimal amount)
    {
        return amount >= 0m;
    }
}
