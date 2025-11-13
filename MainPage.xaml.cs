using System.Globalization;

namespace PageWithGames;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        Routing.RegisterRoute("Minesweeper", typeof(MinesweeperPage));
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

    private void OnThemeToggled(object sender, EventArgs e)
    {
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

    private async void OnLanguageClicked(object sender, EventArgs e)
    {
        string currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        string newCulture = (currentCulture == "en") ? "en" : "ru";

        CultureInfo.CurrentCulture = new CultureInfo(newCulture);
        CultureInfo.CurrentUICulture = new CultureInfo(newCulture);

        await Shell.Current.GoToAsync("//MainPage");

        await DisplayAlert("Settings", $"Language switched to {(newCulture == "ru" ? "Русский" : "English")}", "OK");
    }
}