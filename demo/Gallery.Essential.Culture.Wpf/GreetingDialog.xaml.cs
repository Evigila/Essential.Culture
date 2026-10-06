using System.Windows;

namespace ArkheideSystem.Essential.Culture.Demo.Wpf;

public partial class GreetingDialog : Window
{
    public GreetingDialog(string greetingKey)
    {
        GreetingKey = greetingKey;
        InitializeComponent();
        DataContext = this;
    }

    public string GreetingKey { get; }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
