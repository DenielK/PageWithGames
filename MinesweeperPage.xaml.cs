using CommunityToolkit.Maui.Core; // <-- ИСПРАВЛЕННОЕ ПРОСТРАНСТВО ИМЕН
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace PageWithGames;

public partial class MinesweeperPage : ContentPage
{
    private const int GridSize = 8;
    private const int TotalMines = 10; // 10 мин для поля 8x8

    // Структура для хранения состояния каждой ячейки
    private class Cell
    {
        public bool IsMine { get; set; } = false;
        public int NeighborMines { get; set; } = 0;
        public bool IsRevealed { get; set; } = false;
        public bool IsFlagged { get; set; } = false; // Отслеживание флажка
        public Button? Button { get; set; }
    }

    private Cell[,] gameGrid = new Cell[GridSize, GridSize];
    private bool isGameOver = false;
    private int unrevealedSafeCells;
    private int flagsPlaced = 0; // Счетчик флажков

    // Команда, необходимая для LongPressGestureRecognizer
    public ICommand OnCellLongPressedCommand { get; }

    // Цвета для чисел (0, 1, 2, 3...)
    private readonly Dictionary<int, Color> NumberColors = new()
    {
        { 1, Color.FromArgb("#0000FF") }, // Синий
        { 2, Color.FromArgb("#008000") }, // Зеленый
        { 3, Color.FromArgb("#FF0000") }, // Красный
        { 4, Color.FromArgb("#000080") }, // Темно-синий
        { 5, Color.FromArgb("#800000") }, // Бордовый
        { 6, Color.FromArgb("#008080") }, // Бирюзовый
        { 7, Color.FromArgb("#000000") }, // Черный
        { 8, Color.FromArgb("#808080") }  // Серый
    };

    public MinesweeperPage()
    {
        InitializeComponent();
        // Инициализация команды для долгого нажатия
        OnCellLongPressedCommand = new Command<string>(OnCellLongPressedExecuted);
        InitializeGrid();
    }

    private void OnRestartClicked(object? sender, EventArgs e)
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        GameBoard.Children.Clear();
        isGameOver = false;
        flagsPlaced = 0;
        unrevealedSafeCells = GridSize * GridSize - TotalMines;

        // Инициализация ячеек и кнопок
        for (int r = 0; r < GridSize; r++)
        {
            for (int c = 0; c < GridSize; c++)
            {
                gameGrid[r, c] = new Cell();

                var button = new Button
                {
                    Text = "",
                    BackgroundColor = Color.FromArgb("#C0C0C0"), // Серый
                    TextColor = Colors.Black,
                    AutomationId = $"{r},{c}"
                };

                button.Clicked += OnCellClicked;

                // --- Добавление Long Press Gesture Recognizer ---
                // Используем полное пространство имен, чтобы избежать ошибки
                var longPressRecognizer = new CommunityToolkit.Maui.Core.LongPressGestureRecognizer();
                longPressRecognizer.Command = OnCellLongPressedCommand;
                longPressRecognizer.CommandParameter = button.AutomationId;

                button.GestureRecognizers.Add(longPressRecognizer);

                gameGrid[r, c].Button = button;
                Grid.SetRow(button, r);
                Grid.SetColumn(button, c);
                GameBoard.Children.Add(button);
            }
        }

        PlaceMines();
        CalculateNeighborMines();
        StatusLabel.Text = $"Найдено мин: {flagsPlaced}/{TotalMines}";
    }

    private void PlaceMines()
    {
        Random rand = new Random();
        int minesPlaced = 0;

        while (minesPlaced < TotalMines)
        {
            int r = rand.Next(GridSize);
            int c = rand.Next(GridSize);

            if (!gameGrid[r, c].IsMine)
            {
                gameGrid[r, c].IsMine = true;
                minesPlaced++;
            }
        }
    }

    private void CalculateNeighborMines()
    {
        for (int r = 0; r < GridSize; r++)
        {
            for (int c = 0; c < GridSize; c++)
            {
                if (gameGrid[r, c].IsMine) continue;

                int mineCount = 0;
                // Проверяем 8 соседей
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr == 0 && dc == 0) continue; // Текущая ячейка

                        int nr = r + dr;
                        int nc = c + dc;

                        if (nr >= 0 && nr < GridSize && nc >= 0 && nc < GridSize)
                        {
                            if (gameGrid[nr, nc].IsMine)
                            {
                                mineCount++;
                            }
                        }
                    }
                }
                gameGrid[r, c].NeighborMines = mineCount;
            }
        }
    }

    // --- ОБРАБОТЧИКИ КЛИКОВ И ФЛАГОВ ---

    private void OnCellClicked(object? sender, EventArgs e)
    {
        if (isGameOver) return;

        var button = sender as Button;
        if (button == null) return;

        var positions = button.AutomationId.Split(',');
        int r = int.Parse(positions[0]);
        int c = int.Parse(positions[1]);

        var cell = gameGrid[r, c];

        // Игнорируем обычный клик, если стоит флажок
        if (cell.IsFlagged) return;

        RevealCell(r, c);
        CheckForWin();
    }

    // Исполняемая команда для LongPressGestureRecognizer (флаг)
    private void OnCellLongPressedExecuted(string? automationId)
    {
        if (isGameOver) return;
        if (automationId == null) return;

        var positions = automationId.Split(',');
        int r = int.Parse(positions[0]);
        int c = int.Parse(positions[1]);

        ToggleFlag(r, c);
    }

    private void ToggleFlag(int r, int c)
    {
        var cell = gameGrid[r, c];
        if (cell.IsRevealed) return; // Нельзя ставить флаг на открытую ячейку

        if (cell.IsFlagged)
        {
            // Снять флажок
            cell.IsFlagged = false;
            flagsPlaced--;
            cell.Button!.Text = "";
            cell.Button.IsEnabled = true; // Разблокировать для обычного клика
        }
        else if (flagsPlaced < TotalMines) // Нельзя ставить больше флажков, чем мин
        {
            // Установить флажок
            cell.IsFlagged = true;
            flagsPlaced++;
            cell.Button!.Text = "🚩";
            cell.Button.IsEnabled = false; // Блокируем кнопку
        }

        StatusLabel.Text = $"Найдено мин: {flagsPlaced}/{TotalMines}";
    }

    // --- ОСНОВНАЯ ЛОГИКА ИГРЫ ---

    private void RevealCell(int r, int c)
    {
        if (r < 0 || r >= GridSize || c < 0 || c >= GridSize) return;

        var cell = gameGrid[r, c];
        if (cell.IsRevealed || cell.IsFlagged) return; // Игнорируем флаги при раскрытии

        cell.IsRevealed = true;

        // 1. Попали на мину?
        if (cell.IsMine)
        {
            cell.Button!.Text = "💣";
            cell.Button.BackgroundColor = Colors.Red;
            EndGame(false); // Поражение
            return;
        }

        // 2. Безопасная ячейка с соседями
        unrevealedSafeCells--;
        cell.Button!.IsEnabled = false;
        cell.Button.BackgroundColor = Color.FromArgb("#E0E0E0"); // Светло-серый

        if (cell.NeighborMines > 0)
        {
            cell.Button.Text = cell.NeighborMines.ToString();
            cell.Button.TextColor = NumberColors.GetValueOrDefault(cell.NeighborMines, Colors.Black);
        }
        else
        {
            // 3. Пустая ячейка (NeighborMines == 0). Раскрываем соседей рекурсивно
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    RevealCell(r + dr, c + dc);
                }
            }
        }
    }

    private void CheckForWin()
    {
        if (unrevealedSafeCells == 0)
        {
            EndGame(true); // Победа
        }
    }

    private async void EndGame(bool won)
    {
        isGameOver = true;

        // Раскрыть все мины
        for (int r = 0; r < GridSize; r++)
        {
            for (int c = 0; c < GridSize; c++)
            {
                var cell = gameGrid[r, c];
                cell.Button!.IsEnabled = false;

                if (cell.IsMine)
                {
                    if (!cell.IsFlagged)
                    {
                        cell.Button.Text = "💣";
                    }
                    cell.Button.BackgroundColor = Colors.DarkGray;
                }
                else if (cell.IsFlagged && !cell.IsMine)
                {
                    // Показать неправильно поставленные флаги
                    cell.Button.Text = "❌";
                }
            }
        }

        if (won)
        {
            StatusLabel.Text = "ПОБЕДА! Вы нашли все мины!";
            await DisplayAlert("Победа!", "Вы успешно очистили поле от мин!", "Отлично");
        }
        else
        {
            StatusLabel.Text = "ПОРАЖЕНИЕ! Вы нажали на мину.";
            await DisplayAlert("Поражение!", "К сожалению, вы нажали на мину.", "Новая игра");
        }
    }
}