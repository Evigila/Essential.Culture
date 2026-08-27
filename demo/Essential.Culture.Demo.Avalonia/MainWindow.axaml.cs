using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CKey = ArkheideSystem.Essential.Culture.Key;

namespace ArkheideSystem.Essential.Culture.Demo.Avalonia;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private bool isGirl;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        Localizer.Current.Changed += Localizer_Changed;
        Closed += MainWindow_Closed;
    }

    public string CurrentCulture => Localizer.Current.Culture;

    public bool IsGirl
    {
        get => isGirl;
        set
        {
            if (isGirl == value)
            {
                return;
            }

            isGirl = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(GreetingKey));
        }
    }

    public string GreetingKey => IsGirl ? CKey.Greeting_Girl : CKey.Greeting_Boy;

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void SwitchLanguage_Click(object? sender, RoutedEventArgs e) =>
        Localizer.Current.SetCulture(
            Localizer.Current.Culture == "en-US" ? "zh-CN" : "en-US"
        );

    private async void ShowGreeting_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new GreetingDialog(GreetingKey);
        await dialog.ShowDialog(this);
    }

    private void Localizer_Changed(object? sender, EventArgs e) =>
        OnPropertyChanged(nameof(CurrentCulture));

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        Localizer.Current.Changed -= Localizer_Changed;
        Closed -= MainWindow_Closed;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
