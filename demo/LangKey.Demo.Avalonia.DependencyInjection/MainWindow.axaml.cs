using ArkheideSystem.LangKey.Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ArkheideSystem.LangKey.Demo.AvaloniaDi;

public partial class MainWindow : Window
{
    private readonly ILangKeyAvaloniaApplicator? applicator;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(
        AvaloniaDemoViewModel viewModel,
        ILangKeyAvaloniaApplicator applicator
    )
        : this()
    {
        this.applicator = applicator;
        DataContext = viewModel;
    }

    private async void ShowGreeting_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new GreetingDialog();
        applicator?.Apply(dialog);
        await dialog.ShowDialog(this);
    }
}
