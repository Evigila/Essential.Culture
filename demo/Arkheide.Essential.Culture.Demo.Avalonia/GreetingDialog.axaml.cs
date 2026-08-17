using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Arkheide.Essential.Culture.Demo.Avalonia;

public partial class GreetingDialog : Window
{
    public GreetingDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string ProductName => "Arkheide";

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();
}
