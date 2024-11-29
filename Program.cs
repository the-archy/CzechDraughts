using CzechDraughts;

var game = new Game();
var gameOver = false;
game.Debug = false;
while (!gameOver) {
    
    if (!game.Debug)
        Console.Clear();
    
    ConsoleExtensions.DebugLine($"Current player: {game.CurrentPlayer.GameColor}", game.Debug);
    
    ConsoleExtensions.TellPlayer("Press D for a draw, H twice for the rules, else any key to declare move");
    game.GameBoard.Print();
    
    switch (Console.ReadKey(true).Key)
    {
        case ConsoleKey.D:
            ConsoleExtensions.TellPlayer("Draw requested by player. Opponent, do you agree to a draw? (y/n)");
            if (game.Confirmation())
            { 
                Console.WriteLine("Game ended in a draw.");
                Environment.Exit(0);
            }
            break;

        case ConsoleKey.H:
            Console.Clear();
            Console.WriteLine(GameRules.Rules);
            ConsoleExtensions.TellPlayer("\nPress 'Y' to continue or 'N' to cancel...");
            ConsoleExtensions.TellPlayer(game.Confirmation()? "Continuing..." : "Returning to the game...");
            break;
    }
    
    ConsoleExtensions.WriteColor($"{game.CurrentPlayer.GameColor}", game.CurrentPlayer.DisplayColor); Console.WriteLine(" moves, declare your move (e.g., a3,b4): ");
    

    var move = game.GetMove();
    ConsoleExtensions.DebugLine($"Move attempted: {string.Join(", ", move)}", game.Debug);
    if (game.IsMoveValid(move)) {
        game.PerformMove(move);
        
        ConsoleExtensions.DebugLine("Move performed", game.Debug);
        
        gameOver = game.IsGameOver(out var winner);
        if (gameOver)
        {
            if (!game.Debug) Console.Clear();
            game.GameBoard.Print();
            ConsoleExtensions.WriteColor(
                $"{winner} won!",
                winner == GameColor.Black ? ConsoleColor.Black : ConsoleColor.White
                );
        }
    }
    else Console.WriteLine("Move was invalid");
}

