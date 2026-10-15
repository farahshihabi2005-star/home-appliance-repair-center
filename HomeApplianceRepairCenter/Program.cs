using System;
using System.Globalization;
using System.IO;
using System.Text;
using HomeApplianceRepairCenter.Data;
using HomeApplianceRepairCenter.Services;
using HomeApplianceRepairCenter.UI;

namespace HomeApplianceRepairCenter;

/// <summary>
/// نقطة بداية البرنامج: نظام إدارة مركز صيانة وإصلاح الأجهزة المنزلية.
/// </summary>
public static class Program
{
    public static void Main()
    {
        ConfigureConsole();

        var center = new RepairCenter("Home Appliance Repair Center");
        SampleData.Load(center);

        var menu = new ConsoleMenu(center);

        try
        {
            menu.Run();
        }
        catch (EndOfStreamException)
        {
            // أُغلق الإدخال (مثلًا Ctrl+Z) فننهي البرنامج بهدوء
            Console.WriteLine();
            Console.WriteLine("Input closed. Goodbye!");
        }
    }

    private static void ConfigureConsole()
    {
        // تنسيق موحد للأرقام والتواريخ مهما كانت لغة الجهاز
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        try
        {
            var utf8 = new UTF8Encoding(false);
            Console.OutputEncoding = utf8;
            Console.InputEncoding = utf8;
        }
        catch (IOException)
        {
            // بعض البيئات لا تسمح بتغيير الترميز، ولا يؤثر ذلك على عمل البرنامج
        }
    }
}
