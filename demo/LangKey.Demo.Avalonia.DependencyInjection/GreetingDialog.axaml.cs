using ArkheideSystem.LangKey.Demo.Shared;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ArkheideSystem.LangKey.Demo.AvaloniaDi;

public partial class GreetingDialog : Window
{
    public GreetingDialog()
    {
        InitializeComponent();
    }

    public GreetingDialog(LocalizedDemoViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();
}
