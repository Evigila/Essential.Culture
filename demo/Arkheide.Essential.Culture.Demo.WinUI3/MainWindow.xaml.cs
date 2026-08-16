using Arkheide.Essential.Culture.WinUI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using DemoKey = Arkheide.Essential.Culture.Key;

namespace Arkheide.Essential.Culture.Demo.WinUI3;

public sealed partial class MainWindow : Window
{
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;
    private readonly IWinUILocalizationApplicator applicator;

    public MainWindow(IWinUILocalizationApplicator applicator)
    {
        InitializeComponent();
        this.applicator = applicator;
        Localizer.Current.Changed += Localizer_Changed;
        Closed += MainWindow_Closed;
        UpdateCultureText();
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

    private void Localizer_Changed(object? sender, EventArgs e) => UpdateCultureText();

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        Localizer.Current.Changed -= Localizer_Changed;
        Closed -= MainWindow_Closed;
    }

    private void UpdateCultureText() =>
        CurrentCultureText.Text = Localizer.Parse(
            DemoKey.Current_Culture,
            Localizer.Current.Culture
        );

    private async void ShowGreeting_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = DemoKey.App_Title,
            Content = DemoKey.Greeting,
            CloseButtonText = DemoKey.Action_Close,
            XamlRoot = Root.XamlRoot,
        };

        applicator.Apply(dialog);
        await dialog.ShowAsync();
    }
}
