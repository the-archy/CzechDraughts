namespace CzechDraughts;

public class Player(GameColor gameColor, ConsoleColor displayColor)
{
    public GameColor GameColor { get; } = gameColor;
    public ConsoleColor DisplayColor { get; } = displayColor;

}