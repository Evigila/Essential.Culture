using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ArkheideSystem.LangKey.Demo.AvaloniaDi;

public partial class GreetingDialog : Window
{
    public GreetingDialog()
    {
        InitializeComponent();
    }

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();
}
