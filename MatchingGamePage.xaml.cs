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

    // Игровые переменные (Исправлено: добавлены '?')
    private List<string>? gameSymbols; // Может быть null до первого запуска SetupGame()
    private Button? firstCard = null;
    private Button? secondCard = null;
    private bool isBusy = false;
    private int matchesFound = 0;

    private int totalMoves = 0;
    private int secondsElapsed = 0;

    // Флаг для контроля Dispatcher.StartTimer (заменяет IDispatcherTimer)
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
        // 1. Очистка и сброс состояния
        GameGrid.Children.Clear();
        firstCard = null;
        secondCard = null;
        isBusy = false;
        matchesFound = 0;
        totalMoves = 0;

        // 2. Установка и запуск Таймера (С ИСПОЛЬЗОВАНИЕМ StartTimer)
        secondsElapsed = 0;

        // Выключаем старый таймер и включаем новый флаг
        isTimerActive = false;
        isTimerActive = true;

        // ИСПОЛЬЗУЕМ Dispatcher.StartTimer - наиболее надежный метод в MAUI
        Dispatcher.StartTimer(
            TimeSpan.FromSeconds(1),
            () => // Функция обратного вызова (Func<bool>)
            {
                if (!isTimerActive)
                {
                    return false; // Останавливаем таймер
                }

                // Логика тика
                secondsElapsed++;
                // Обновление UI
                StatusLabel.Text = $"Ходов: {totalMoves} | Время игры: {secondsElapsed} сек.";

                return true; // Продолжаем работу
            }
        );

        StatusLabel.Text = $"Ходов: {totalMoves} | Время игры: {secondsElapsed} сек.";

        // 3. Генерация и перемешивание символов
        gameSymbols = AvailableSymbols
            .Concat(AvailableSymbols)
            .OrderBy(x => Guid.NewGuid())
            .ToList();

        // 4. Динамическое создание кнопок
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
        isBusy = false; // Разблокируем клики
    }

    private async Task CheckWin()
    {
        if (matchesFound == TotalPairs)
        {
            isTimerActive = false; // ОСТАНОВКА ТАЙМЕРА через флаг
            await DisplayAlert("Победа!", $"Вы нашли все пары за {totalMoves} ходов и {secondsElapsed} секунд!", "Отлично"); // ДОБАВЛЕНО: await
        }
    }
}