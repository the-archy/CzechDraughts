namespace CzechDraughts;

using CE = ConsoleExtensions;
using CC = ConsoleColor;

public class Game
{
    private static readonly Player BlackPlayer = new(GameColor.Black, CC.DarkGray);
    private static readonly Player WhitePlayer = new(GameColor.White, CC.White);

    protected internal Board GameBoard = new();
    internal Player CurrentPlayer = WhitePlayer;

    internal bool Debug = true;

    private void SwitchPlayer() => CurrentPlayer = CurrentPlayer == BlackPlayer ? WhitePlayer : BlackPlayer;
    
    private static Player GetOpponent(Player player) => player == WhitePlayer ? BlackPlayer : WhitePlayer;

    internal int[] GetMove()
    {
        try
        {
            var input = Console.ReadLine()?.Split(',');

            // Musí vrátit počáteční a koncovou pozici
            if (input is not { Length: 2 })
                throw new Exception("Invalid input: statement must include 2 positions");

            var positions = new int[4];

            for (var i = 0; i < input.Length; i++)
            {
                if (string.IsNullOrEmpty(input[i]) || input[i].Length != 2)
                    throw new Exception("Invalids input: position must be in format XY (e.g., a3).");


                var col = Board.ConvertColToIndex(input[i][0]); // Převedeme písmeno abecedy na jeho pořadí
                var row = Board.ConvertRowToIndex(input[i][1]); // Obrátíme řádky 

                if (col is > Board.BoardSize - 1 or < 0)
                    throw new Exception("Invalid input: column must be between 'a' and 'h'");
                if (row is > Board.BoardSize - 1 or < 0)
                    throw new Exception("Invalid input: row must be between 0 and 8");

                if (i == 0)
                {
                    positions[0] = row;
                    positions[1] = col;
                }
                else
                {
                    positions[2] = row;
                    positions[3] = col;
                }
            }

            return positions;
        }

        catch (Exception ex)
        {
            CE.ErrorLine(ex.Message);
            CE.TellPlayer("Please input your move in the correct format, e.g., 'a3,b4'.");
            GameBoard.Print();
            return GetMove();
        }
    }

    private bool AreMovesEqual(int[] move1, int[] move2)
    {
        if (move1.Length != move2.Length) return false;
        return !move1.Where((t, i) => t != move2[i]).Any();
    }

    internal bool IsMoveValid(int[] move)
    {
        try
        {
            if (IsJumpValid(move, out _)) return true;

            var jumps = CanJump(CurrentPlayer);
            if (jumps.Count > 0)
            {
                var moveIsValid = false;
                foreach (var jump in jumps)
                {
                    if (!AreMovesEqual(jump, move)) continue;
                    moveIsValid = true;
                    break;
                }

                if (!moveIsValid) PenalizeSkippedJump(jumps);
            }

            var startRow = move[0];
            var startCol = move[1];
            var endRow = move[2];
            var endCol = move[3];

            var piece = GameBoard[startRow, startCol];
            if (piece.GameColor != CurrentPlayer.GameColor)
                throw new Exception("You can only move your own piece");

            if (GameBoard[endRow, endCol].GameColor != GameColor.None)
                throw new Exception("End field must be empty");

            if (Math.Abs(endRow - startRow) != Math.Abs(endCol - startCol))
                throw new Exception("Move must be diagonal");

            switch (piece)
            {
                case WhiteMan or BlackMan:
                    if (Math.Abs(endRow - startRow) != 1 || Math.Abs(endCol - startCol) != 1)
                        throw new Exception("Man can only move one square diagonally unless jumping");
                    break;

                case WhiteKing or BlackKing:
                    if (Math.Abs(endRow - startRow) < 1 || Math.Abs(endCol - startCol) < 1)
                        throw new Exception("King must move at least one square diagonally");
                    break;

                default:
                    throw new Exception("Invalid piece type");
            }

            return true;
        }
        catch (Exception ex)
        {
            CE.ErrorLine($"Invalid move: {ex.Message}");
            GameBoard.Print();
            return false;
        }
    }

    private void PenalizeSkippedJump(List<int[]> jumps)
    {
        if (jumps.Count == 0) return;

        var first = jumps[0];
        GameBoard[first[0], first[1]] = new EmptyPiece();

        CE.TellPlayer($"You failed to jump! The piece at {Board.InternalToRegularCoord(first[0], first[1])} has been removed.");
        CE.TellPlayer("Press any key to continue...");
        Console.ReadKey(true);
    }

    private static int[][] PieceDirections(Piece piece) => piece switch
    {
        WhiteKing or BlackKing => [[-1, -1], [-1, 1], [1, -1], [1, 1]],
        BlackMan => [[-1, -1], [-1, 1]],
        WhiteMan => [[1, -1], [1, 1]],
        _ => []
    };


    private List<int[]> CanJump(Player player)
    {
        var jumps = new List<int[]>();

        for (var row = 0; row < Board.BoardSize; row++)
        for (var col = 0; col < Board.BoardSize; col++)
        {
            var piece = GameBoard[row, col];
            if (piece.GameColor != player.GameColor) continue;

            CE.DebugLine($"Evaluating jumps for piece at ({row},{col})", Debug);

            foreach (var dir in PieceDirections(piece))
            {
                CE.DebugLine($"Direction for piece are: [{dir[0]}, {dir[1]}]", Debug);
                
                var jumpedRow = row + dir[0];
                var jumpedCol = col + dir[1];
                var endRow = row + (dir[0] * 2);
                var endCol = col + (dir[1] * 2);

                CE.DebugLine($"Jump coordinates: End at ({endRow},{endCol}), Jumped at ({jumpedRow},{jumpedCol})", Debug);

                if (!Board.IsOnBoard(endRow, endCol) || !Board.IsOnBoard(jumpedRow, jumpedCol))
                    continue;

                var jumped = GameBoard[jumpedRow, jumpedCol];
                var endPiece = GameBoard[endRow, endCol];

                CE.DebugLine($"Opponent: {GetOpponent(player).GameColor}, Checked player: {player.GameColor}", Debug);
                CE.DebugLine($"Jumped piece: {jumped.GameColor}", Debug);

                if (jumped.GameColor == GetOpponent(player).GameColor &&
                    endPiece.GameColor == GameColor.None)
                {
                    CE.DebugLine($"Valid jump found: {row},{col} -> {endRow},{endCol} over {jumpedRow},{jumpedCol}",
                        Debug);
                    jumps.Add([row, col, endRow, endCol]);
                }
                else
                    CE.DebugLine(
                        $"Invalid jump: {row},{col} -> {endRow},{endCol} (middle: {jumped.GameColor}, end: {endPiece.GameColor})",
                        Debug);
            }
        }

        return jumps;
    }


    private bool IsJumpValid(int[] move, out int[] jumped)
    {
        var startRow = move[0];
        var startCol = move[1];
        var endRow = move[2];
        var endCol = move[3];

        jumped = [(startRow + endRow) / 2, (startCol + endCol) / 2];

        if (!Board.IsOnBoard(jumped[0], jumped[1]) || !Board.IsOnBoard(endRow, endCol))
            return false;

        if (Math.Abs(endRow - startRow) != 2 || Math.Abs(endCol - startCol) != 2)
            return false;

        var middlePiece = GameBoard[jumped[0], jumped[1]];
        var endPiece = GameBoard[endRow, endCol];

        return middlePiece.GameColor != CurrentPlayer.GameColor &&
               middlePiece.GameColor != GameColor.None &&
               endPiece.GameColor == GameColor.None;
    }

    internal void PerformMove(int[] move)
    {
        var startRow = move[0];
        var startCol = move[1];
        var endRow = move[2];
        var endCol = move[3];

        CE.DebugLine($"Performing move from ({startRow},{startCol}) to ({endRow},{endCol})", Debug);
        CE.DebugLine($"Current player before move: {CurrentPlayer.GameColor}", Debug);

        var isJump = IsJumpValid(move, out var jumped);
        CE.DebugLine($"Is this a jump? {isJump}", Debug);
        
        GameBoard[endRow, endCol] = GameBoard[startRow, startCol];
        GameBoard[startRow, startCol] = new EmptyPiece();
        PromoteToKing(endRow, endCol);

        if (isJump)
        {
            GameBoard[jumped[0], jumped[1]] = new EmptyPiece(); // Remove jumped piece
            CE.DebugLine($"Jumped piece removed from ({jumped[0]},{jumped[1]})", Debug);
            GameBoard.Print();
            HandleMultiJump(endRow, endCol);
        }
        else
        {
            SwitchPlayer();
            CE.DebugLine($"Switched player to: {CurrentPlayer.GameColor}", Debug);
        }

    }

    private void HandleMultiJump(int row, int col)
    {
        CE.DebugLine($"Handling multi-jump starting at ({row},{col})", Debug);
        
        var additionalJumps = GetJumpsForPiece(row, col, CanJump(CurrentPlayer));

        CE.DebugLine($"Additional jumps for {CurrentPlayer.GameColor} available: {additionalJumps.Count}", Debug);

        if (additionalJumps.Count == 0)
        {
            CE.DebugLine("No additional jumps available, switching players", Debug);
            SwitchPlayer();
            return;
        }

        if (!Debug) Console.Clear();

        GameBoard.Print();
        CE.TellPlayer("You must continue jumping!");

        while (true)
        {
            var nextMove = GetMove();

            CE.DebugLine($"Next move attempted: {string.Join(", ", nextMove)}", Debug);

            if (additionalJumps.Any(j => j.SequenceEqual(nextMove)))
            {
                PerformMove(nextMove);
                return;
            }

            CE.ErrorLine("Invalid continuation of jump. Try again.");
        }
    }

    private List<int[]> GetJumpsForPiece(int row, int col, List<int[]> jumps)
    {
        var result = new List<int[]>();
        foreach (var jump in jumps)
            if (jump[0] == row && jump[1] == col)
                result.Add(jump);

        return result;
    }


    private void PromoteToKing(int row, int col)
    {
        var piece = GameBoard[row, col];
        if ((piece is not WhiteMan || row != Board.BoardSize - 1) &&
            (piece is not BlackMan || row != 0)) return;
        
        GameBoard[row, col] = piece.GameColor switch
        {
            GameColor.White => new WhiteKing(),
            GameColor.Black => new BlackKing(),
            _ => GameBoard[row, col]
        };
        CE.DebugLine($"Piece at ({row}, {col}) promoted to King!", Debug);
    }


    internal bool IsGameOver(out GameColor winner)
    {
        short numWhite = 0, numBlack = 0;
        bool whiteCanMove = false, blackCanMove = false;

        for (var row = 0; row < Board.BoardSize; row++)
        for (var col = 0; col < Board.BoardSize; col++)
        {
            var piece = GameBoard[row, col];
            var color = piece.GameColor;

            switch (color)
            {
                case GameColor.White:
                    numWhite++;
                    whiteCanMove = whiteCanMove || CanPieceMove(row, col, WhitePlayer);
                    break;
                case GameColor.Black:
                    numBlack++;
                    blackCanMove = blackCanMove || CanPieceMove(row, col, BlackPlayer);
                    break;
            }
        }

        CE.DebugLine($"White pieces: {numWhite}, Can move: {whiteCanMove}", Debug);
        CE.DebugLine($"Black pieces: {numBlack}, Can move: {blackCanMove}", Debug);

        if (numWhite == 0 || !whiteCanMove)
        {
            winner = GameColor.Black;
            return true;
        }

        if (numBlack == 0 || !blackCanMove)
        {
            winner = GameColor.White;
            return true;
        }

        winner = GameColor.None;
        return false;
    }


    private bool CanPieceMove(int row, int col, Player player)
    {
        var piece = GameBoard[row, col];
        if (piece is EmptyPiece)
            return false;

        var directions = PieceDirections(piece);
        foreach (var dir in directions)
        {
            var middleRow = row + dir[0];
            var middleCol = col + dir[1];
            var endRow = row + dir[0] * 2;
            var endCol = col + dir[1] * 2;

            if (!Board.IsOnBoard(middleRow, middleCol) || !Board.IsOnBoard(endRow, endCol)) continue;

            var middlePiece = GameBoard[middleRow, middleCol];
            var endPiece = GameBoard[endRow, endCol];

            if (middlePiece.GameColor != GetOpponent(player).GameColor || endPiece.GameColor != GameColor.None) continue;
            
            CE.DebugLine($"Valid jump: {row},{col} -> {endRow},{endCol} over {middleRow},{middleCol}", Debug);
            return true;
        }

        foreach (var dir in directions)
        {
            var newRow = row + dir[0];
            var newCol = col + dir[1];

            CE.DebugLine($"Checking normal move: {row},{col} -> {newRow},{newCol}", Debug);

            if (!Board.IsOnBoard(newRow, newCol))
            {
                CE.DebugLine($"Move out of bounds: {newRow},{newCol}", Debug);
                continue;
            }

            if (GameBoard[newRow, newCol].GameColor != GameColor.None) continue;

            CE.DebugLine(
                $"Piece at {Board.InternalToRegularCoord(row, col)} can move to {Board.InternalToRegularCoord(newRow, newCol)}",
                Debug);
            return true;
        }

        CE.DebugLine($"Piece at ({row},{col}) has no valid moves or jumps", Debug);
        return false;
    }
    
    public bool Confirmation()
    {
        while (true)
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.Y: return true;
                case ConsoleKey.N: return false;
                default:
                    CE.TellPlayer("Invalid input. Please press 'Y' or 'N'.");
                    break;
            }
    }
    
}

public enum GameColor { White, Black, None }

internal struct CaseTests
{
    
    public static Piece[,] SetupTestMultiJump()
    {
        var board = Board.EmtpyBoard();

        board[2, 5] = new WhiteMan();

        board[3, 4] = board[5, 2] = board[7, 0] = new BlackMan();

        Console.WriteLine("Test board for multi-jumps set up!");
        return board;
    }
    
    public static Piece[,] SetupPromotionScenario()
    {
        var board = Board.EmtpyBoard();

        board[6, 4] = new WhiteMan(); 

        board[5, 3] = new BlackMan(); 
        board[5, 5] = new BlackMan();

        Console.WriteLine("Test board for promotion scenario set up!");
        return board;
    }
}