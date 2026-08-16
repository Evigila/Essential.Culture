using Arkheide.Essential.Culture.Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Arkheide.Essential.Culture.Demo.Avalonia;

public partial class MainWindow : Window
{
    private readonly IAvaloniaLocalizationApplicator? applicator;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(
        AvaloniaDemoViewModel viewModel,
        IAvaloniaLocalizationApplicator applicator
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
