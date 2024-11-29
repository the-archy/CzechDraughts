
using System.Text;

namespace CzechDraughts;

using CE = ConsoleExtensions;
using CC = ConsoleColor;
using BB = BoardBorder;
using BP = BorderPosition;

public class Board {
	
	internal const int BoardSize = 8;

	private const int WhiteRowStart = 0;
	private const int BlackRowStart = 5;

	private Piece[,] _gameBoard = InitializeBoard();

	private const CC BoardColor = PrintColor.BoardGrid;

	public Piece this[int row, int col] {
		get => _gameBoard[row, col];
		set => _gameBoard[row, col] = value;
	}

	internal static Piece[,] EmtpyBoard()
	{
		var board = new Piece[BoardSize, BoardSize];
		for (var row = 0; row < BoardSize; row++)
		for (var col = 0; col < BoardSize; col++)
			board[row, col] = new EmptyPiece();
        
		return board;
	}
	
	private static Piece[,] InitializeBoard()
	{
		var board = EmtpyBoard();

		for (var col = 0; col < BoardSize - 1; col += 2) {
			board[WhiteRowStart, col]		= board[WhiteRowStart + 1, col + 1] = board[WhiteRowStart + 2, col]		= new WhiteMan();
			board[BlackRowStart, col + 1]	= board[BlackRowStart + 1, col]		= board[BlackRowStart + 2, col + 1] = new BlackMan();
		}

		return board;
	}

	private string BuildBorderLine(BP bp) {
		var line = new StringBuilder();
		line.Append($"  {(bp == BP.Top ? BB.TopLeft : BB.BottomLeft)}");

		for (var i = 0; i < BoardSize; i++) {
			line.Append(new string(BB.Horizontal, 3));
			if (i < BoardSize - 1) line.Append(bp == BP.Top ? BB.HorizontalDownG : BB.HorizontalUpG);
		}

		line.Append(bp == BP.Top ? BB.TopRight : BB.BottomRight);
		return line.ToString();
	}

	private void PrintRow(int row) {

		CE.WriteColor($"{BoardSize - row}", BoardColor);
		CE.WriteColor($" {BB.Vertical} ", BoardColor);

		for (var col = 0; col < BoardSize; col++) {
			var piece = _gameBoard[row, col];
			CE.WriteColor(piece.Symbol, piece.ConsoleColor);
			CE.WriteColor(col == BoardSize - 1 ? $" {BB.Vertical} " : $" {Grid.Vertical} ", BoardColor);
		}

		Console.WriteLine();
	}

	private string BuildGridLine() {
		var line = new StringBuilder("  ");
		line.Append(BB.VerticalLeftG);

		for (var i = 0; i < BoardSize; i++) {
			if (i > 0) line.Append(Grid.Cross);
			line.Append(new string(Grid.Horizontal, 3));
		}

		line.Append(BB.VerticalRightG);
		return line.ToString();
	}

	public void Print() {
		Console.OutputEncoding = Encoding.UTF8;

		CE.WriteLineColor("    a   b   c   d   e   f   g   h", BoardColor);
		CE.WriteLineColor(BuildBorderLine(BP.Top), BoardColor);

		for (var row = 0; row < BoardSize; row++) {
			PrintRow(row);
			if (row < BoardSize - 1) CE.WriteLineColor(BuildGridLine(), BoardColor);
		}

		CE.WriteLineColor(BuildBorderLine(BP.Bottom), BoardColor);
	}
	
	public static bool IsOnBoard(int row, int col) => row is >= 0 and < BoardSize && col is >= 0 and < BoardSize;
	
	public static string InternalToRegularCoord(int row, int col)
	{
		if (!IsOnBoard(row, col))
			throw new ArgumentOutOfRangeException($"Invalid board position: ({row}, {col})");

		var columnLetter = (char)('a' + col); 
		var rowNumber = BoardSize - row; 

		return $"{columnLetter}{rowNumber}";
	}
	
	public static int ConvertColToIndex(char col) => char.ToLower(col) - 'a';
	public static int ConvertRowToIndex(char row) => BoardSize - (row - '0');
}

internal struct BoardBorder {
	internal const char Vertical        = '\u2551'; //║
	internal const char Horizontal      = '\u2550'; //═
	internal const char VerticalLeftG   = '\u255f'; //╟
	internal const char VerticalRightG  = '\u2562'; //╢
	internal const char HorizontalDownG = '\u2564'; //╤
	internal const char HorizontalUpG   = '\u2567'; //╧
	internal const char TopLeft         = '\u2554'; //╔
	internal const char TopRight        = '\u2557'; //╗
	internal const char BottomLeft      = '\u255a'; //╚
	internal const char BottomRight     = '\u255d'; //╝
}

internal struct Grid {
	internal const char Cross      = '\u253c'; //┼
	internal const char Vertical   = '\u2502'; //│
	internal const char Horizontal = '\u2500'; //─
}
internal enum BorderPosition { Top, Bottom }