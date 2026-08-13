using ArkheideSystem.LangKey.Demo.Shared;
using ArkheideSystem.LangKey.WinUI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using DemoLangKey = ArkheideSystem.LangKey.Demo.Shared.LangKey;

namespace ArkheideSystem.LangKey.Demo.WinUI3Di;

public sealed partial class MainWindow : Window
{
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;
    private readonly DemoCultureSource cultureSource;
    private readonly global::ArkheideSystem.LangKey.ILangKeyResolver resolver;
    private readonly ILangKeyWinUIApplicator applicator;

    public MainWindow(
        DemoCultureSource cultureSource,
        global::ArkheideSystem.LangKey.ILangKeyResolver resolver,
        ILangKeyWinUIApplicator applicator
    )
    {
        InitializeComponent();
        this.cultureSource = cultureSource;
        this.resolver = resolver;
        this.applicator = applicator;
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
        cultureSource.Toggle();
        UpdateCultureText();
    }

    private void UpdateCultureText() =>
        CurrentCultureText.Text = resolver.Format(DemoLangKey.Current_Culture, resolver.Current);

    private async void ShowGreeting_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = DemoLangKey.App_Title,
            Content = DemoLangKey.Greeting,
            CloseButtonText = DemoLangKey.Action_Close,
            XamlRoot = Root.XamlRoot,
        };

        applicator.Apply(dialog);
        await dialog.ShowAsync();
    }
}
