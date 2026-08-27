using Avalonia.Controls;
using Avalonia.Interactivity;
using CKey = ArkheideSystem.Essential.Culture.Key;

namespace ArkheideSystem.Essential.Culture.Demo.Avalonia;

public partial class GreetingDialog : Window
{
    public GreetingDialog()
        : this(CKey.Greeting_Boy) { }

    public GreetingDialog(string greetingKey)
    {
        GreetingKey = greetingKey;
        InitializeComponent();
        DataContext = this;
    }

    public string GreetingKey { get; }

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();
}
