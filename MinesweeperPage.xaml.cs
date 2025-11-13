using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using System.Timers;
using Timer = System.Timers.Timer;
using Microsoft.Maui.ApplicationModel;

namespace PageWithGames;

public partial class MinesweeperPage : ContentPage
{
    private const int GridSize = 8;
    private const int TotalMines = 10;

    private class Cell
    {
        public bool IsMine { get; set; } = false;
        public int NeighborMines { get; set; } = 0;
        public bool IsRevealed { get; set; } = false;
        public bool IsFlagged { get; set; } = false;
        public Button? Button { get; set; }
    }

    private Cell[,] gameGrid = new Cell[GridSize, GridSize];
    private bool isGameOver = false;
    private int unrevealedSafeCells;
    private int flagsPlaced = 0;

    //Long Press
    private Timer? longPressTimer;
    private bool isLongPress = false;
    private const int LongPressTimeMs = 500; // 0.5 секунды
    private string? currentPressedCellId;

    private readonly Dictionary<int, Color> NumberColors = new()
    {
        { 1, Color.FromArgb("#0000FF") },
        { 2, Color.FromArgb("#008000") },
        { 3, Color.FromArgb("#FF0000") },
        { 4, Color.FromArgb("#000080") },
        { 5, Color.FromArgb("#800000") },
        { 6, Color.FromArgb("#008080") },
        { 7, Color.FromArgb("#000000") },
        { 8, Color.FromArgb("#808080") }
    };

    public MinesweeperPage()
    {
        InitializeComponent();
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

        for (int r = 0; r < GridSize; r++)
        {
            for (int c = 0; c < GridSize; c++)
            {
                gameGrid[r, c] = new Cell();

                var button = new Button
                {
                    Text = "",
                    BackgroundColor = Color.FromArgb("#C0C0C0"),
                    TextColor = Colors.Black,
                    AutomationId = $"{r},{c}"
                };

                button.Pressed += OnCellPressed;
                button.Released += OnCellReleased;

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
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr == 0 && dc == 0) continue;

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

    private void OnCellPressed(object? sender, EventArgs e)
    {
        if (isGameOver) return;
        var button = sender as Button;
        if (button == null) return;

        currentPressedCellId = button.AutomationId;
        isLongPress = false;

        longPressTimer?.Dispose();

        longPressTimer = new Timer(LongPressTimeMs);
        longPressTimer.Elapsed += (s, args) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (currentPressedCellId == button.AutomationId)
                {
                    isLongPress = true;
                    ToggleFlagExecuted(currentPressedCellId);
                }
            });
            longPressTimer.Dispose();
            longPressTimer = null;
        };
        longPressTimer.AutoReset = false;
        longPressTimer.Start();
    }

    private void OnCellReleased(object? sender, EventArgs e)
    {
        if (isGameOver) return;
        var button = sender as Button;
        if (button == null) return;

        if (longPressTimer != null)
        {
            longPressTimer.Stop();
            longPressTimer.Dispose();
            longPressTimer = null;

            // Если это не было долгим нажатием, обрабатываем как обычный клик
            if (!isLongPress && currentPressedCellId == button.AutomationId)
            {
                OnCellClickedExecuted(currentPressedCellId);
            }
        }

        isLongPress = false;
        currentPressedCellId = null;
    }

    private void OnCellClickedExecuted(string automationId)
    {
        var positions = automationId.Split(',');
        int r = int.Parse(positions[0]);
        int c = int.Parse(positions[1]);

        var cell = gameGrid[r, c];

        // Игнорируем обычный клик, если стоит флажок
        if (cell.IsFlagged) return;

        RevealCell(r, c);
        CheckForWin();
    }

    private void ToggleFlagExecuted(string? automationId)
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
        if (cell.IsRevealed) return;

        if (cell.IsFlagged)
        {
            cell.IsFlagged = false;
            flagsPlaced--;
            cell.Button!.Text = "";
            cell.Button.IsEnabled = true;
        }
        else if (flagsPlaced < TotalMines)
        {
            cell.IsFlagged = true;
            flagsPlaced++;
            cell.Button!.Text = "🚩";
            cell.Button.IsEnabled = false;
        }

        StatusLabel.Text = $"Найдено мин: {flagsPlaced}/{TotalMines}";
    }


    private void RevealCell(int r, int c)
    {
        if (r < 0 || r >= GridSize || c < 0 || c >= GridSize) return;

        var cell = gameGrid[r, c];
        if (cell.IsRevealed || cell.IsFlagged) return;

        cell.IsRevealed = true;

        if (cell.IsMine)
        {
            cell.Button!.Text = "💣";
            cell.Button.BackgroundColor = Colors.Red;
            EndGame(false);
            return;
        }

        unrevealedSafeCells--;
        cell.Button!.IsEnabled = false;
        cell.Button.BackgroundColor = Color.FromArgb("#E0E0E0");

        if (cell.NeighborMines > 0)
        {
            cell.Button.Text = cell.NeighborMines.ToString();
            cell.Button.TextColor = NumberColors.GetValueOrDefault(cell.NeighborMines, Colors.Black);
        }
        else
        {
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
            EndGame(true);
        }
    }

    private async void EndGame(bool won)
    {
        isGameOver = true;

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