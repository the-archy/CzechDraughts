namespace CzechDraughts;

using CC = ConsoleColor;

public static class ConsoleExtensions
{
    public static void WriteColor(string message, CC color) {
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ResetColor();
    }

    public static void WriteLineColor(string message, CC color) {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void DebugLine(string str, bool debug) {
        if (debug) WriteLineColor($"DEBUG: {str}", PrintColor.Debug);
    }

    public static void ErrorLine(string str)    => WriteLineColor(str, PrintColor.Error);
    public static void TellPlayer(string str)   => WriteLineColor(str, PrintColor.PlayerMessage);
}

internal struct PrintColor {
    internal const CC BoardGrid     = CC.DarkGray;
    internal const CC Error         = CC.DarkRed;
    internal const CC PlayerMessage = CC.DarkCyan;
    internal const CC Debug         = CC.DarkYellow;
}