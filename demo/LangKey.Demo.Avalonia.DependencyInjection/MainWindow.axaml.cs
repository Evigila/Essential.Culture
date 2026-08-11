using ArkheideSystem.LangKey.Demo.Shared;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ArkheideSystem.LangKey.Demo.AvaloniaDi;

public partial class MainWindow : Window
{
    private LocalizedDemoViewModel? viewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(LocalizedDemoViewModel viewModel)
        : this()
    {
        this.viewModel = viewModel;
        DataContext = viewModel;
    }

    private async void ShowGreeting_Click(object? sender, RoutedEventArgs e)
    {
        if (viewModel is not null)
        {
            await new GreetingDialog(viewModel).ShowDialog(this);
        }
    }
}
