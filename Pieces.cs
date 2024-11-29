namespace CzechDraughts;

public class Piece(ConsoleColor consoleColor, GameColor gameColor)
{
    public ConsoleColor ConsoleColor { get; } = consoleColor;
    public GameColor GameColor { get; } = gameColor;
    public virtual string Symbol => "\u25cf"; //●
}

public class WhiteMan()     : Piece(ConsoleColor.White,     GameColor.White);
public class BlackMan()     : Piece(ConsoleColor.Gray,  GameColor.Black);
public class WhiteKing()    : Piece(ConsoleColor.Yellow,    GameColor.White);
public class BlackKing()    : Piece(ConsoleColor.DarkGray,      GameColor.Black);


public class EmptyPiece()   : Piece(Console.ForegroundColor,       GameColor.None)
{
    public override string Symbol { get; } = " ";
}