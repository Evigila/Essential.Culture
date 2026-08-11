using ArkheideSystem.LangKey.Demo.Shared;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace ArkheideSystem.LangKey.Demo.WinUI3Di;

public sealed partial class MainWindow : Window
{
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;
    private readonly LocalizedDemoViewModel viewModel;

    public MainWindow(LocalizedDemoViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        Root.DataContext = viewModel;
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

    private async void ShowGreeting_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = viewModel.Title,
            Content = viewModel.Greeting,
            CloseButtonText = viewModel.CloseText,
            XamlRoot = Root.XamlRoot,
        };

        await dialog.ShowAsync();
    }
}
