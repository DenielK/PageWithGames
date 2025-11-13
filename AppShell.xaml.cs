namespace PageWithGames;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("TicTacToe", typeof(TicTacToeModePage));
        Routing.RegisterRoute("TicTacToeGame", typeof(TicTacToeGamePage));
        Routing.RegisterRoute("MatchingGame", typeof(MatchingGamePage));
        Routing.RegisterRoute("Minesweeper", typeof(MinesweeperPage));
        Routing.RegisterRoute("Puzzle", typeof(ContentPage));
    }
}