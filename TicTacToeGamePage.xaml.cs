using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Для Task.Delay

namespace PageWithGames;

// Атрибут для получения параметра режима игры из URL (QueryParameter)
[QueryProperty(nameof(GameMode), "mode")]
public partial class TicTacToeGamePage : ContentPage
{
    // Игровые переменные
    private bool isPlayerXTurn = true; // true = X, false = O
    private Button[] buttons = new Button[9];
    private bool isGameOver = false;

    // Переменные режима
    private List<int> availableMoves = new List<int>();
    public string GameMode { get; set; } // Получает значение "Human" или "Bot"

    public TicTacToeGamePage()
    {
        InitializeComponent();
        InitializeGameButtons();
    }

    // Вызывается при навигации к странице (используется для получения GameMode)
    protected override void OnAppearing()
    {
        base.OnAppearing();
        StartNewGame();
    }

    // Инициализация 9 кнопок и добавление их в Grid
    private void InitializeGameButtons()
    {
        for (int i = 0; i < 9; i++)
        {
            var button = new Button
            {
                FontSize = 60,
                AutomationId = i.ToString(), // Индекс 0-8 для бота
                BackgroundColor = Color.FromRgb(240, 240, 240)
            };

            int row = i / 3;
            int col = i % 3;

            Grid.SetRow(button, row);
            Grid.SetColumn(button, col);

            button.Clicked += OnButtonClicked;

            // Предполагается, что в XAML есть x:Name="GameBoard"
            GameBoard.Children.Add(button);
            buttons[i] = button;
        }
    }

    // Обработчик нажатия на кнопку игрового поля
    private async void OnButtonClicked(object sender, EventArgs e)
    {
        // Блокируем ход, если игра окончена или сейчас ход O (бота)
        if (isGameOver || (GameMode == "Bot" && !isPlayerXTurn))
            return;

        var button = sender as Button;
        if (!string.IsNullOrEmpty(button.Text))
            return;

        // 1. ОПРЕДЕЛЯЕМ ИГРОКА (ИСПРАВЛЕНО ДЛЯ РЕЖИМА "ЧЕЛОВЕК ПРОТИВ ЧЕЛОВЕКА")
        string currentPlayer = isPlayerXTurn ? "X" : "O";

        // 2. Ход игрока
        MakeMove(button, currentPlayer);

        // 3. Проверка состояния после хода игрока
        if (CheckGameState())
            return;

        // 4. Если режим "Bot", вызываем ход бота
        if (GameMode == "Bot")
        {
            // Бот всегда играет "O"

            await Task.Delay(500); // Имитация "мышления"

            BotMove();

            // 5. Проверка состояния после хода O (бота)
            CheckGameState();
        }
    }

    // Метод, делающий ход
    private void MakeMove(Button button, string player)
    {
        button.Text = player;
        button.TextColor = (player == "X") ? Colors.Blue : Colors.Red;

        // Удаляем сделанный ход из списка доступных (для бота)
        if (int.TryParse(button.AutomationId, out int index))
        {
            availableMoves.Remove(index);
        }

        // Переключаем ход 
        isPlayerXTurn = !isPlayerXTurn;

        // Обновляем статус 
        StatusLabel.Text = $"Ход игрока {(isPlayerXTurn ? "X" : "O")}";
    }

    // Логика простого бота (случайный ход)
    private void BotMove()
    {
        if (isGameOver || availableMoves.Count == 0) return;

        Random random = new Random();
        int moveIndex = availableMoves[random.Next(availableMoves.Count)];

        var botButton = buttons[moveIndex];

        MakeMove(botButton, "O");
    }

    // Централизованная проверка состояния игры
    private bool CheckGameState()
    {
        string currentPlayer = isPlayerXTurn ? "O" : "X"; // Проверяем того, кто только что походил

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

        // Если бот только что походил (O) и не выиграл, статус переключается на X (человек)
        if (GameMode == "Bot")
        {
            StatusLabel.Text = "Ваш ход (X)";
        }

        return false;
    }

    // Проверка всех 8 возможных выигрышных комбинаций
    private bool CheckForWin()
    {
        int[,] winCombinations = new int[,]
        {
            {0, 1, 2}, {3, 4, 5}, {6, 7, 8}, // Горизонтали
            {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, // Вертикали
            {0, 4, 8}, {2, 4, 6}             // Диагонали
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
                // Выделяем победную линию
                buttons[a].BackgroundColor = buttons[b].BackgroundColor = buttons[c].BackgroundColor = Colors.LightGreen;
                return true;
            }
        }
        return false;
    }

    // Проверка на ничью
    private bool CheckForDraw()
    {
        return buttons.All(b => !string.IsNullOrEmpty(b.Text));
    }

    // Обработчик нажатия кнопки "Новая игра"
    private void OnRestartClicked(object sender, EventArgs e)
    {
        StartNewGame();
    }

    // Логика начала новой игры
    private void StartNewGame()
    {
        isGameOver = false;
        isPlayerXTurn = true;

        // Очищаем и заполняем список доступных ходов (индексы 0-8)
        availableMoves.Clear();
        availableMoves.AddRange(Enumerable.Range(0, 9));

        StatusLabel.Text = "Ход игрока X";
        if (GameMode == "Bot")
        {
            StatusLabel.Text = "Ваш ход (X)";
        }

        // Очищаем все кнопки и возвращаем им исходный вид
        foreach (var button in buttons)
        {
            button.Text = string.Empty;
            button.BackgroundColor = Color.FromRgb(240, 240, 240);
        }
    }
}