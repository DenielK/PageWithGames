using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PageWithGames;

[QueryProperty(nameof(GameMode), "mode")]
public partial class TicTacToeGamePage : ContentPage
{
    // Игровые переменные
    private bool isPlayerXTurn = true; // true = X, false = O
    private Button[] buttons = new Button[9];
    private bool isGameOver = false;

    private List<int> availableMoves = new List<int>();
    public string GameMode { get; set; } // Получает значение "Human" или "Bot"

    public TicTacToeGamePage()
    {
        InitializeComponent();
        InitializeGameButtons();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StartNewGame();
    }

    private void InitializeGameButtons()
    {
        for (int i = 0; i < 9; i++)
        {
            var button = new Button
            {
                FontSize = 60,
                AutomationId = i.ToString(),
                BackgroundColor = Color.FromRgb(240, 240, 240)
            };

            int row = i / 3;
            int col = i % 3;

            Grid.SetRow(button, row);
            Grid.SetColumn(button, col);

            button.Clicked += OnButtonClicked;

            GameBoard.Children.Add(button);
            buttons[i] = button;
        }
    }

    private async void OnButtonClicked(object sender, EventArgs e)
    {
        // Блокируем ход, если игра окончена или сейчас ход O (бота)
        if (isGameOver || (GameMode == "Bot" && !isPlayerXTurn))
            return;

        var button = sender as Button;
        if (!string.IsNullOrEmpty(button.Text))
            return;

        string currentPlayer = isPlayerXTurn ? "X" : "O";

        MakeMove(button, currentPlayer);

        if (CheckGameState())
            return;

        if (GameMode == "Bot")
        {
            // Бот всегда играет "O"

            await Task.Delay(500);

            BotMove();

            CheckGameState();
        }
    }

    private void MakeMove(Button button, string player)
    {
        button.Text = player;
        button.TextColor = (player == "X") ? Colors.Blue : Colors.Red;

        // Удаляем сделанный ход из списка доступных (для бота)
        if (int.TryParse(button.AutomationId, out int index))
        {
            availableMoves.Remove(index);
        }

        isPlayerXTurn = !isPlayerXTurn;
 
        StatusLabel.Text = $"Ход игрока {(isPlayerXTurn ? "X" : "O")}";
    }

    private void BotMove()
    {
        if (isGameOver || availableMoves.Count == 0) return;

        Random random = new Random();
        int moveIndex = availableMoves[random.Next(availableMoves.Count)];

        var botButton = buttons[moveIndex];

        MakeMove(botButton, "O");
    }

    private bool CheckGameState()
    {
        string currentPlayer = isPlayerXTurn ? "O" : "X";

        if (CheckForWin())
        {
            StatusLabel.Text = $"Победил игрок {currentPlayer}!";
            isGameOver = true;
            return true;
        }

        if (CheckForDraw())
        {
            StatusLabel.Text = "Ничья!";
            isGameOver = true;
            return true;
        }

        if (GameMode == "Bot")
        {
            StatusLabel.Text = "Ваш ход (X)";
        }

        return false;
    }

    private bool CheckForWin()
    {
        int[,] winCombinations = new int[,]
        {
            {0, 1, 2}, {3, 4, 5}, {6, 7, 8}, 
            {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, 
            {0, 4, 8}, {2, 4, 6}             
        };

        for (int i = 0; i < 8; i++)
        {
            int a = winCombinations[i, 0];
            int b = winCombinations[i, 1];
            int c = winCombinations[i, 2];

            if (!string.IsNullOrEmpty(buttons[a].Text) &&
                buttons[a].Text == buttons[b].Text &&
                buttons[b].Text == buttons[c].Text)
            {
                buttons[a].BackgroundColor = buttons[b].BackgroundColor = buttons[c].BackgroundColor = Colors.LightGreen;
                return true;
            }
        }
        return false;
    }

    private bool CheckForDraw()
    {
        return buttons.All(b => !string.IsNullOrEmpty(b.Text));
    }

    private void OnRestartClicked(object sender, EventArgs e)
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        isGameOver = false;
        isPlayerXTurn = true;

        availableMoves.Clear();
        availableMoves.AddRange(Enumerable.Range(0, 9));

        StatusLabel.Text = "Ход игрока X";
        if (GameMode == "Bot")
        {
            StatusLabel.Text = "Ваш ход (X)";
        }

        foreach (var button in buttons)
        {
            button.Text = string.Empty;
            button.BackgroundColor = Color.FromRgb(240, 240, 240);
        }
    }
}