using System;

namespace AuthManSys.Console.Utilities;

public static class SafeConsole
{
    public static bool IsInputRedirected => System.Console.IsInputRedirected;

    public static string? ReadLine()
    {
        if (IsInputRedirected)
        {
            return null; // Cannot read input when redirected
        }

        try
        {
            return System.Console.ReadLine();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public static ConsoleKeyInfo? ReadKey(bool intercept = false)
    {
        if (IsInputRedirected)
        {
            return null; // Cannot read keys when redirected
        }

        try
        {
            return System.Console.ReadKey(intercept);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public static void WriteLine(string message)
    {
        System.Console.WriteLine(message);
    }

    public static void WriteLine()
    {
        System.Console.WriteLine();
    }

    public static void Write(string message)
    {
        System.Console.Write(message);
    }
}