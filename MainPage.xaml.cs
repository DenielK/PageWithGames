using System.Globalization;

namespace PageWithGames;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        Routing.RegisterRoute("Minesweeper", typeof(ContentPage));
        Routing.RegisterRoute("Puzzle", typeof(ContentPage));
    }

    // Обработчик нажатия на одну из игровых кнопок
    private async void OnGameSelected(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter is string route)
        {
            // Shell.Current.GoToAsync осуществляет навигацию по зарегистрированному маршруту
            await Shell.Current.GoToAsync(route);
        }
    }

    // --- ЗАГЛУШКА ДЛЯ НАСТРОЙКИ ТЕМЫ ---
    private void OnThemeToggled(object sender, EventArgs e)
    {
        // Пока просто переключаем тему, логика переключения будет в следующем шаге
        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            Application.Current.UserAppTheme = AppTheme.Light;
            DisplayAlert("Настройки", "Переключена на Светлую тему", "ОК");
        }
        else
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
            DisplayAlert("Настройки", "Переключена на Темную тему", "ОК");
        }
    }

    // --- ЗАГЛУШКА ДЛЯ НАСТРОЙКИ ЯЗЫКА ---
    private async void OnLanguageClicked(object sender, EventArgs e)
    {
        // 1. Определяем текущий язык и выбираем следующий
        string currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        string newCulture = (currentCulture == "en") ? "en" : "ru";

        // 2. Устанавливаем новую культуру
        CultureInfo.CurrentCulture = new CultureInfo(newCulture);
        CultureInfo.CurrentUICulture = new CultureInfo(newCulture);

        // 3. Перезагрузка UI для применения новых строк из RESX
        // В .NET MAUI самый простой способ заставить UI перерисоваться с новыми ресурсами
        // — это перезапустить (или переинициализировать) главную страницу.

        // Мы используем простой подход: переходим к AppShell, который заново загрузит MainPage.
        await Shell.Current.GoToAsync("//MainPage");

        // Отображение уведомления о смене языка
        await DisplayAlert("Settings", $"Language switched to {(newCulture == "ru" ? "Русский" : "English")}", "OK");
    }
}