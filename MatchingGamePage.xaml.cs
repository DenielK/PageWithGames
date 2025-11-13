using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace PageWithGames;

public partial class MatchingGamePage : ContentPage
{
    private const int GridSize = 4;
    private const int TotalCards = GridSize * GridSize;
    private const int TotalPairs = TotalCards / 2;

    private readonly List<string> AvailableSymbols = new() { "🍎", "🍌", "🥝", "🍇", "🍍", "🍓", "🍒", "🥭" };

    private List<string>? gameSymbols;
    private Button? firstCard = null;
    private Button? secondCard = null;
    private bool isBusy = false;
    private int matchesFound = 0;

    private int totalMoves = 0;
    private int secondsElapsed = 0;

    private bool isTimerActive = false;

    public MatchingGamePage()
    {
        InitializeComponent();
        OnRestartClicked(null!, null!);
    }

    // Обработчик нажатия кнопки "Перезапуск"
    private void OnRestartClicked(object? sender, EventArgs e)
    {
        SetupGame();
    }

    private void SetupGame()
    {
        //сброс состояния
        GameGrid.Children.Clear();
        firstCard = null;
        secondCard = null;
        isBusy = false;
        matchesFound = 0;
        totalMoves = 0;

        //запуск Таймера
        secondsElapsed = 0;

        isTimerActive = false;
        isTimerActive = true;

        Dispatcher.StartTimer(
            TimeSpan.FromSeconds(1),
            () =>
            {
                if (!isTimerActive)
                {
                    return false;
                }

                secondsElapsed++;
                StatusLabel.Text = $"Ходов: {totalMoves} | Время игры: {secondsElapsed} сек.";

                return true;
            }
        );

        StatusLabel.Text = $"Ходов: {totalMoves} | Время игры: {secondsElapsed} сек.";

        gameSymbols = AvailableSymbols
            .Concat(AvailableSymbols)
            .OrderBy(x => Guid.NewGuid())
            .ToList();

        //Динамическое создание кнопок
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                int index = row * GridSize + col;

                var button = new Button
                {
                    Text = "?",
                    FontSize = 40,
                    TextColor = Colors.White,
                    BackgroundColor = Color.FromArgb("#512BD4"),
                    AutomationId = gameSymbols[index],
                };

                button.Clicked += CardClicked;

                Grid.SetRow(button, row);
                Grid.SetColumn(button, col);
                GameGrid.Children.Add(button);
            }
        }
    }

    private async void CardClicked(object? sender, EventArgs e)
    {
        if (isBusy)
            return;

        var currentCard = sender as Button;

        if (currentCard == null || currentCard.Text != "?" || currentCard == firstCard)
            return;

        currentCard.Text = currentCard.AutomationId;

        if (firstCard == null)
        {
            firstCard = currentCard;
            return;
        }

        secondCard = currentCard;
        isBusy = true;

        totalMoves++;

        await Task.Delay(1200);

        await CheckForMatch();
    }

    private async Task CheckForMatch()
    {
        if (firstCard == null || secondCard == null) return;

        if (firstCard.Text == secondCard.Text)
        {
            // проверка на совпадение
            firstCard.BackgroundColor = secondCard.BackgroundColor = Colors.Green;
            firstCard.Clicked -= CardClicked;
            secondCard.Clicked -= CardClicked;

            matchesFound++;

            await CheckWin();
        }
        else
        {
            // Совпадение НЕ найдено, закрываем карты
            firstCard.Text = secondCard.Text = "?";
        }

        firstCard = null;
        secondCard = null;
        isBusy = false;
    }

    private async Task CheckWin()
    {
        if (matchesFound == TotalPairs)
        {
            isTimerActive = false;
            await DisplayAlert("Победа!", $"Вы нашли все пары за {totalMoves} ходов и {secondsElapsed} секунд!", "Отлично"); // ДОБАВЛЕНО: await
        }
    }
}