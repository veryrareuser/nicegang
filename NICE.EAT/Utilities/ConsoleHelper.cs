namespace NICE.EAT.Utilities;

/// <summary>
/// Provides formatted console output helpers (colored headers, tables, separators).
/// </summary>
public static class ConsoleHelper
{
    // ─── Color palette ───────────────────────────────────────────────────────
    private const ConsoleColor HeaderColor    = ConsoleColor.Cyan;
    private const ConsoleColor AccentColor    = ConsoleColor.Yellow;
    private const ConsoleColor SuccessColor   = ConsoleColor.Green;
    private const ConsoleColor ErrorColor     = ConsoleColor.Red;
    private const ConsoleColor SubtleColor    = ConsoleColor.DarkGray;

    // ─── Banner / branding ───────────────────────────────────────────────────

    public static void PrintBanner()
    {
        Console.Clear();
        WriteColored(ConsoleColor.Cyan, @"
  ███╗   ██╗██╗ ██████╗███████╗   ███████╗ █████╗ ████████╗
  ████╗  ██║██║██╔════╝██╔════╝   ██╔════╝██╔══██╗╚══██╔══╝
  ██╔██╗ ██║██║██║     █████╗     █████╗  ███████║   ██║   
  ██║╚██╗██║██║██║     ██╔══╝     ██╔══╝  ██╔══██║   ██║   
  ██║ ╚████║██║╚██████╗███████╗   ███████╗██║  ██║   ██║   
  ╚═╝  ╚═══╝╚═╝ ╚═════╝╚══════╝   ╚══════╝╚═╝  ╚═╝   ╚═╝   
");
        WriteColored(ConsoleColor.DarkCyan, "  🍽️   Your Smart Restaurant Recommendation App\n");
        PrintSeparator();
    }

    public static void PrintSeparator(char ch = '─', int width = 62)
    {
        WriteColoredLine(SubtleColor, new string(ch, width));
    }

    public static void PrintHeader(string title)
    {
        Console.WriteLine();
        PrintSeparator('═');
        WriteColoredLine(HeaderColor, $"  ✦  {title.ToUpper()}");
        PrintSeparator('═');
    }

    // ─── Prompts ─────────────────────────────────────────────────────────────

    public static string AskInput(string prompt)
    {
        WriteColored(AccentColor, $"\n  ▶ {prompt}: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static int AskInt(string prompt, int min, int max)
    {
        while (true)
        {
            string raw = AskInput($"{prompt} ({min}–{max})");
            if (int.TryParse(raw, out int val) && val >= min && val <= max)
                return val;
            PrintError($"Please enter a number between {min} and {max}.");
        }
    }

    public static bool AskYesNo(string prompt)
    {
        while (true)
        {
            string raw = AskInput($"{prompt} (y/n)").ToLowerInvariant();
            if (raw == "y" || raw == "yes") return true;
            if (raw == "n" || raw == "no")  return false;
            PrintError("Please type y or n.");
        }
    }

    // ─── Feedback ────────────────────────────────────────────────────────────

    public static void PrintSuccess(string msg)
        => WriteColoredLine(SuccessColor, $"\n  ✔  {msg}");

    public static void PrintError(string msg)
        => WriteColoredLine(ErrorColor, $"\n  ✖  {msg}");

    public static void PrintInfo(string msg)
        => WriteColoredLine(SubtleColor, $"  ℹ  {msg}");

    public static void PrintLabel(string label, string value)
    {
        WriteColored(SubtleColor, $"  {label,-18}: ");
        Console.WriteLine(value);
    }

    // ─── Spicy / vegan indicators ────────────────────────────────────────────

    public static string SpicyBar(int level) =>
        level switch
        {
            0 => "⚪⚪⚪⚪⚪",
            1 => "🌶️⚪⚪⚪⚪",
            2 => "🌶️🌶️⚪⚪⚪",
            3 => "🌶️🌶️🌶️⚪⚪",
            4 => "🌶️🌶️🌶️🌶️⚪",
            _ => "🌶️🌶️🌶️🌶️🌶️"
        };

    public static string VeganBadge(bool isVegan) => isVegan ? "🌿 Vegan" : "🍖 Non-Vegan";

    public static string WaitBadge(int minutes) => minutes switch
    {
        <= 10 => $"⚡ {minutes} min (Fast!)",
        <= 25 => $"🕐 {minutes} min (Normal)",
        _     => $"🔥 {minutes} min (Busy)"
    };

    // ─── Helpers ─────────────────────────────────────────────────────────────

    public static void PressAnyKey()
    {
        Console.WriteLine();
        WriteColoredLine(SubtleColor, "  Press any key to continue...");
        Console.ReadKey(true);
    }

    private static void WriteColored(ConsoleColor color, string text)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    private static void WriteColoredLine(ConsoleColor color, string text)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }
}
