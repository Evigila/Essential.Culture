using System.Windows;
using GeneratedLangKey = ArkheideSystem.LangKey.Demo.WpfApp.Generated.LangKey;

namespace ArkheideSystem.LangKey.Demo.WpfApp;

public partial class MainWindow : Window
{
    private readonly ILangKeyParser parser;

    public MainWindow(ILangKeyParser parser)
    {
        InitializeComponent();
        this.parser = parser;
        parser.Changed += Parser_Changed;
        RefreshCultureText();
    }

    protected override void OnClosed(EventArgs e)
    {
        parser.Changed -= Parser_Changed;
        base.OnClosed(e);
    }

    private void SwitchLanguage_Click(object sender, RoutedEventArgs e) =>
        parser.Current = parser.Current == "en-US" ? "zh-CN" : "en-US";

    private void ShowGreeting_Click(object sender, RoutedEventArgs e) =>
        MessageBox.Show(
            this,
            parser.Get(GeneratedLangKey.Greeting),
            parser.Get(GeneratedLangKey.App_Title),
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );

    private void Parser_Changed(object? sender, LangKeyChangedEventArgs e) =>
        RefreshCultureText();

    private void RefreshCultureText() =>
        CultureText.Text = parser.Format(GeneratedLangKey.Current_Culture, parser.Current);
}
