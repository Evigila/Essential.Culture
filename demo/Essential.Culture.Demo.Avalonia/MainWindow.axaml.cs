using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ArkheideSystem.Essential.Culture.Demo.Avalonia;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        Localizer.Current.Changed += Localizer_Changed;
        Closed += MainWindow_Closed;
    }

    public string CurrentCulture => Localizer.Current.Culture;

    public string ProductName => "Arkheide";

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void SwitchLanguage_Click(object? sender, RoutedEventArgs e) =>
        Localizer.Current.SetCulture(
            Localizer.Current.Culture == "en-US" ? "zh-CN" : "en-US"
        );

    private async void ShowGreeting_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new GreetingDialog();
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
