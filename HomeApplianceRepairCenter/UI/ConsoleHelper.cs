using System;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace HomeApplianceRepairCenter.UI;

/// <summary>
/// فئة ثابتة لقراءة المدخلات من لوحة المفاتيح وإعادة السؤال حتى يُدخل المستخدم قيمة صحيحة.
/// </summary>
public static class ConsoleHelper
{
    /// <summary>يقرأ سطرًا من المستخدم. إذا أُغلق الإدخال يرمي EndOfStreamException لإنهاء البرنامج بهدوء.</summary>
    public static string ReadLine(string prompt)
    {
        Console.Write(prompt);
        string? line = Console.ReadLine();

        if (line == null)
        {
            throw new EndOfStreamException("Input stream closed.");
        }

        return line.Trim();
    }

    /// <summary>يعيد السؤال حتى يجتاز النص المُدخل دالة التحقق.</summary>
    public static string ReadValidated(string prompt, Func<string, bool> isValid, string errorMessage)
    {
        while (true)
        {
            string input = ReadLine(prompt);

            if (isValid(input))
            {
                return input;
            }

            Console.WriteLine($"  Invalid input: {errorMessage}");
        }
    }

    /// <summary>يقرأ رقمًا (int أو double أو decimal) ضمن المجال [min, max].</summary>
    public static T ReadNumber<T>(string prompt, T min, T max) where T : struct, INumber<T>
    {
        while (true)
        {
            string input = ReadLine(prompt);

            if (T.TryParse(input, CultureInfo.InvariantCulture, out T value) && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine($"  Please enter a number between {min} and {max}.");
        }
    }

    /// <summary>يقرأ إجابة نعم/لا (y أو n).</summary>
    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            string input = ReadLine(prompt).ToLowerInvariant();

            if (input is "y" or "yes")
            {
                return true;
            }

            if (input is "n" or "no")
            {
                return false;
            }

            Console.WriteLine("  Please answer with y or n.");
        }
    }

    /// <summary>يطبع رسالة خطأ باللون الأحمر.</summary>
    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Error: {message}");
        Console.ResetColor();
    }

    /// <summary>يطبع رسالة نجاح باللون الأخضر.</summary>
    public static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  {message}");
        Console.ResetColor();
    }
}
