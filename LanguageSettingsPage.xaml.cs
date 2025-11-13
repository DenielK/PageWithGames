using System.Globalization;
using Microsoft.Maui.Controls;
using System.Threading;

namespace PageWithGames;

public partial class LanguageSettingsPage : ContentPage
{

    public LanguageSettingsPage()
    {
        InitializeComponent();
        UpdateUiText();
    }

    private void UpdateUiText()
    {
        string currentCulture = CultureInfo.CurrentCulture.Name;
        CurrentLanguageLabel.Text = $"Текущий язык: {currentCulture.ToUpper()}";

        if (currentCulture.StartsWith("ru"))
        {
            Title = "Настройки языка";
            LanguageButton.Text = "Переключить на EN";
        }
        else
        {
            Title = "Language Settings";
            LanguageButton.Text = "Switch to RU";
        }
    }

    private void OnLanguageButtonClicked(object sender, EventArgs e)
    {
        string newCultureName = CultureInfo.CurrentCulture.Name.StartsWith("ru") ? "en-US" : "ru-RU";

        CultureInfo newCulture = new CultureInfo(newCultureName);

        CultureInfo.DefaultThreadCurrentCulture = newCulture;
        CultureInfo.DefaultThreadCurrentUICulture = newCulture;

        Thread.CurrentThread.CurrentCulture = newCulture;
        Thread.CurrentThread.CurrentUICulture = newCulture;

        UpdateUiText();
    }
}