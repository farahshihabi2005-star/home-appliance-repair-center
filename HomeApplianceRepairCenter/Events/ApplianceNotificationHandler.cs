namespace HomeApplianceRepairCenter.Events;

/// <summary>
/// التفويض (Delegate): يحدد شكل الدالة التي يمكنها استقبال إشعارات الأجهزة
/// (تغيّر الحالة، اكتمال الإصلاح، تأخر التسليم).
/// أي دالة لها نفس هذا التوقيع يمكن ربطها بالأحداث.
/// </summary>
/// <param name="sender">الجهاز الذي أطلق الحدث.</param>
/// <param name="e">تفاصيل الحدث.</param>
public delegate void ApplianceNotificationHandler(object sender, ApplianceEventArgs e);
