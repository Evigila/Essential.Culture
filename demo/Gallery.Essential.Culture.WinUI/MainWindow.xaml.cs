using System.ComponentModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using CKey = ArkheideSystem.Essential.Culture.Key;

namespace ArkheideSystem.Essential.Culture.Demo.WinUI3;

public sealed partial class MainWindow : Window, INotifyPropertyChanged
{
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;
    private bool isGirl;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentCulture { get; private set; } = Localizer.Current.Culture;

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGirl)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GreetingKey)));
        }
    }

    public string GreetingKey => IsGirl ? CKey.Greeting_Girl : CKey.Greeting_Boy;

    public MainWindow()
    {
        InitializeComponent();
        Localizer.Current.Changed += Localizer_Changed;
        Closed += MainWindow_Closed;
        ResizeAndCenter();
    }

    private void ResizeAndCenter()
    {
        var displayArea = DisplayArea.GetFromWindowId(
            AppWindow.Id,
            DisplayAreaFallback.Nearest
        );
        if (displayArea is null)
        {
            AppWindow.Resize(new SizeInt32(WindowWidth, WindowHeight));
            return;
        }

        var workArea = displayArea.WorkArea;
        AppWindow.MoveAndResize(
            new RectInt32(
                Math.Max(0, (workArea.Width - WindowWidth) / 2),
                Math.Max(0, (workArea.Height - WindowHeight) / 2),
                WindowWidth,
                WindowHeight
            ),
            displayArea
        );
    }

    private void SwitchLanguage_Click(object sender, RoutedEventArgs e)
    {
        Localizer.Current.SetCulture(
            Localizer.Current.Culture == "en-US" ? "zh-CN" : "en-US"
        );
    }

    private void Localizer_Changed(object? sender, EventArgs e)
    {
        CurrentCulture = Localizer.Current.Culture;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        Localizer.Current.Changed -= Localizer_Changed;
        Closed -= MainWindow_Closed;
    }

    private async void ShowGreeting_Click(object sender, RoutedEventArgs e)
    {
        GreetingDialog.XamlRoot = Root.XamlRoot;
        await GreetingDialog.ShowAsync();
    }
}
