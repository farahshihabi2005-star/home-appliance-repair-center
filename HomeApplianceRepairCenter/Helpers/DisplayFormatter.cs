using System;
using System.Globalization;

namespace HomeApplianceRepairCenter.Helpers;

/// <summary>
/// فئة ثابتة لتنسيق القيم عند عرضها، حتى يكون الشكل موحدًا في كل النظام
/// وحتى لا نكرر نفس سطور التنسيق في أكثر من مكان.
/// </summary>
public static class DisplayFormatter
{
    private const string CurrencyCode = "USD";

    /// <summary>مثال: 1,250.50 USD</summary>
    public static string Money(decimal amount)
    {
        return amount.ToString("N2", CultureInfo.InvariantCulture) + " " + CurrencyCode;
    }

    /// <summary>مثال: 2026-11-12</summary>
    public static string Date(DateTime date)
    {
        return date.ToString(InputValidator.DateFormat, CultureInfo.InvariantCulture);
    }

    /// <summary>يحوّل القيمة المنطقية إلى Yes / No.</summary>
    public static string YesNo(bool value)
    {
        return value ? "Yes" : "No";
    }
}
