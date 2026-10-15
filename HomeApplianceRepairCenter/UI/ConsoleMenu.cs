using System;
using System.Collections.Generic;
using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Helpers;
using HomeApplianceRepairCenter.Models;
using HomeApplianceRepairCenter.Services;

namespace HomeApplianceRepairCenter.UI;

/// <summary>
/// واجهة المستخدم النصية: تعرض القائمة وتنفّذ اختيار المستخدم عبر RepairCenter.
/// لا تحتوي أي منطق صيانة، فقط قراءة مدخلات وعرض نتائج.
/// </summary>
public class ConsoleMenu
{
    private readonly RepairCenter _center;

    public ConsoleMenu(RepairCenter center)
    {
        _center = center ?? throw new ArgumentNullException(nameof(center));
    }

    /// <summary>الحلقة الرئيسية: تُظهر القائمة وتنفّذ الخيار حتى يختار المستخدم الخروج.</summary>
    public void Run()
    {
        bool running = true;

        while (running)
        {
            ShowMenu();
            string choice = ConsoleHelper.ReadLine("Select an option: ");
            Console.WriteLine();

            try
            {
                running = Execute(choice);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                // أخطاء التحقق وانتقال الحالات تُعرض كرسالة مفهومة بدل إيقاف البرنامج
                ConsoleHelper.WriteError(ex.Message);
            }

            Console.WriteLine();
        }

        Console.WriteLine("Goodbye!");
    }

    private void ShowMenu()
    {
        Console.WriteLine("=============================================");
        Console.WriteLine($"  {_center.Name}");
        Console.WriteLine("=============================================");
        Console.WriteLine("  1. Show all appliances");
        Console.WriteLine("  2. Register a new appliance");
        Console.WriteLine("  3. Start repair");
        Console.WriteLine("  4. Complete repair");
        Console.WriteLine("  5. Deliver appliance to customer");
        Console.WriteLine("  6. Show appliances under repair");
        Console.WriteLine("  7. Show repaired appliances");
        Console.WriteLine("  8. Search by serial number");
        Console.WriteLine("  9. Check delayed deliveries");
        Console.WriteLine(" 10. Show statistics");
        Console.WriteLine(" 11. Show customer notifications");
        Console.WriteLine("  0. Exit");
        Console.WriteLine("---------------------------------------------");
    }

    /// <summary>ينفّذ الخيار المختار. يعيد false عند اختيار الخروج.</summary>
    private bool Execute(string choice)
    {
        switch (choice)
        {
            case "1":
                PrintAppliances("All appliances", _center.Appliances);
                break;
            case "2":
                RegisterAppliance();
                break;
            case "3":
                AskForAppliance()?.StartRepair();
                break;
            case "4":
                CompleteRepair();
                break;
            case "5":
                AskForAppliance()?.DeliverToCustomer();
                break;
            case "6":
                PrintAppliances("Appliances under repair", _center.GetUnderRepair());
                break;
            case "7":
                PrintAppliances("Repaired appliances (ready or delivered)", _center.GetRepaired());
                break;
            case "8":
                SearchBySerial();
                break;
            case "9":
                CheckDelays();
                break;
            case "10":
                ShowStatistics();
                break;
            case "11":
                ShowNotifications();
                break;
            case "0":
                return false;
            default:
                Console.WriteLine("Unknown option. Please choose a number from the menu.");
                break;
        }

        return true;
    }

    // ---------- تسجيل جهاز جديد ----------

    private void RegisterAppliance()
    {
        Console.WriteLine("Appliance type:  1) Refrigerator   2) Washing machine   3) Air conditioner");
        int type = ConsoleHelper.ReadNumber("Choose type (1-3): ", 1, 3);

        BasicInfo info = ReadBasicInfo();

        Appliance appliance = type switch
        {
            1 => CreateRefrigerator(info),
            2 => CreateWashingMachine(info),
            _ => CreateAirConditioner(info)
        };

        _center.AddAppliance(appliance);
        ConsoleHelper.WriteSuccess("Appliance registered successfully:");
        Console.WriteLine(appliance.GetDetails());
    }

    /// <summary>يقرأ البيانات المشتركة بين كل أنواع الأجهزة.</summary>
    private BasicInfo ReadBasicInfo()
    {
        string serial = ConsoleHelper.ReadValidated(
            "Serial number (e.g. FR-100234): ",
            text => InputValidator.IsValidSerialNumber(text) && _center.FindBySerial(text) == null,
            "use 2-3 letters, a dash, then 4-8 digits, and it must not be registered already.");

        string brand = ConsoleHelper.ReadValidated(
            "Brand: ", text => InputValidator.IsValidText(text, 2, 30), "2-30 characters.");

        string owner = ConsoleHelper.ReadValidated(
            "Owner name: ", InputValidator.IsValidName, "2-50 letters only.");

        string problem = ConsoleHelper.ReadValidated(
            "Problem description: ", text => InputValidator.IsValidText(text, 3, 200), "3-200 characters.");

        int days = ConsoleHelper.ReadNumber("Expected repair duration in days (1-365): ", 1, 365);
        bool warranty = ConsoleHelper.ReadYesNo("Under warranty? (y/n): ");

        DateTime received = DateTime.Today;
        return new BasicInfo(serial, brand, owner, problem, received, received.AddDays(days), warranty);
    }

    private static Appliance CreateRefrigerator(BasicInfo info)
    {
        int capacity = ConsoleHelper.ReadNumber(
            $"Capacity in liters ({Refrigerator.MinCapacityLiters}-{Refrigerator.MaxCapacityLiters}): ",
            Refrigerator.MinCapacityLiters, Refrigerator.MaxCapacityLiters);
        bool needsGas = ConsoleHelper.ReadYesNo("Needs gas refill? (y/n): ");

        return new Refrigerator(info.Serial, info.Brand, info.Owner, info.Problem,
            info.Received, info.Expected, info.Warranty, capacity, needsGas);
    }

    private static Appliance CreateWashingMachine(BasicInfo info)
    {
        int load = ConsoleHelper.ReadNumber(
            $"Load capacity in kg ({WashingMachine.MinLoadCapacityKg}-{WashingMachine.MaxLoadCapacityKg}): ",
            WashingMachine.MinLoadCapacityKg, WashingMachine.MaxLoadCapacityKg);
        bool frontLoad = ConsoleHelper.ReadYesNo("Front load? (y/n): ");
        bool needsDrum = ConsoleHelper.ReadYesNo("Needs drum replacement? (y/n): ");

        return new WashingMachine(info.Serial, info.Brand, info.Owner, info.Problem,
            info.Received, info.Expected, info.Warranty, load, frontLoad, needsDrum);
    }

    private static Appliance CreateAirConditioner(BasicInfo info)
    {
        int btu = ConsoleHelper.ReadNumber(
            $"Capacity in BTU ({AirConditioner.MinCapacityBtu}-{AirConditioner.MaxCapacityBtu}): ",
            AirConditioner.MinCapacityBtu, AirConditioner.MaxCapacityBtu);
        bool inverter = ConsoleHelper.ReadYesNo("Inverter type? (y/n): ");
        bool onSite = ConsoleHelper.ReadYesNo("Requires an on-site visit? (y/n): ");

        return new AirConditioner(info.Serial, info.Brand, info.Owner, info.Problem,
            info.Received, info.Expected, info.Warranty, btu, inverter, onSite);
    }

    // ---------- العمليات على جهاز موجود ----------

    /// <summary>يسأل عن الرقم التسلسلي ويعيد الجهاز، أو null (مع رسالة خطأ) إذا لم يوجد.</summary>
    private Appliance? AskForAppliance()
    {
        string serial = ConsoleHelper.ReadLine("Serial number: ");
        Appliance? appliance = _center.FindBySerial(serial);

        if (appliance == null)
        {
            ConsoleHelper.WriteError($"No appliance found with serial number '{serial}'.");
        }

        return appliance;
    }

    private void CompleteRepair()
    {
        Appliance? appliance = AskForAppliance();

        if (appliance == null)
        {
            return;
        }

        if (appliance.Status != RepairStatus.UnderRepair)
        {
            ConsoleHelper.WriteError($"Appliance {appliance.SerialNumber} must be under repair first (current status: {appliance.Status}).");
            return;
        }

        appliance.LaborHours = ConsoleHelper.ReadNumber($"Total labor hours (0-{Appliance.MaxLaborHours}): ", 0.0, Appliance.MaxLaborHours);
        appliance.PartsCost = ConsoleHelper.ReadNumber("Spare parts cost in USD (0-100000): ", 0m, 100000m);
        appliance.CompleteRepair();
    }

    private void SearchBySerial()
    {
        Appliance? appliance = AskForAppliance();

        if (appliance != null)
        {
            Console.WriteLine(appliance.GetDetails());
        }
    }

    private void CheckDelays()
    {
        int daysAhead = ConsoleHelper.ReadNumber("Simulate how many days from today? (0 = today): ", 0, 365);
        DateTime checkDate = DateTime.Today.AddDays(daysAhead);

        Console.WriteLine($"Checking deliveries as of {DisplayFormatter.Date(checkDate)}...");
        _center.CheckDelays(checkDate);

        Console.WriteLine();
        PrintAppliances("Delayed appliances", _center.GetDelayed(checkDate));
    }

    // ---------- التقارير ----------

    private void ShowStatistics()
    {
        // العدّادات الثابتة (static properties) مأخوذة من الفئة Appliance مباشرة
        Console.WriteLine("--- Statistics ---");
        Console.WriteLine($"Total appliances registered   : {Appliance.TotalCount}");
        Console.WriteLine($"Received (waiting)            : {Appliance.ReceivedCount}");
        Console.WriteLine($"Under repair                  : {Appliance.UnderRepairCount}");
        Console.WriteLine($"Completed (ready for pickup)  : {Appliance.CompletedCount}");
        Console.WriteLine($"Delivered                     : {Appliance.DeliveredCount}");
        Console.WriteLine($"Total cost of all appliances  : {DisplayFormatter.Money(_center.CalculateTotalCost())}");
        Console.WriteLine($"Cost of repaired appliances   : {DisplayFormatter.Money(_center.CalculateRepairedCost())}");
    }

    private void ShowNotifications()
    {
        IReadOnlyList<string> history = _center.Notifier.History;
        Console.WriteLine($"--- Customer notifications ({history.Count}) ---");

        if (history.Count == 0)
        {
            Console.WriteLine("No notifications yet.");
            return;
        }

        foreach (string line in history)
        {
            Console.WriteLine(line);
        }
    }

    private static void PrintAppliances(string title, IReadOnlyCollection<Appliance> appliances)
    {
        Console.WriteLine($"--- {title} ({appliances.Count}) ---");

        if (appliances.Count == 0)
        {
            Console.WriteLine("No appliances to show.");
            return;
        }

        Console.WriteLine(RepairCenter.BuildReport(appliances));
    }

    /// <summary>حزمة صغيرة تجمع البيانات المشتركة أثناء تسجيل جهاز جديد.</summary>
    private sealed record BasicInfo(
        string Serial,
        string Brand,
        string Owner,
        string Problem,
        DateTime Received,
        DateTime Expected,
        bool Warranty);
}
