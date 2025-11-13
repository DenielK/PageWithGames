namespace PageWithGames;

public partial class TicTacToeModePage : ContentPage
{
    public TicTacToeModePage()
    {
        InitializeComponent();
    }

    private async void OnModeSelected(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter is string mode)
        {
            // Передаем выбранный режим (Human или Bot) как параметр запроса (query parameter)
            await Shell.Current.GoToAsync($"TicTacToeGame?mode={mode}");
        }
    }
}