using System.Windows;
using ArkheideSystem.LangKey.Demo.Shared;
using GeneratedLangKey = ArkheideSystem.LangKey.Demo.WpfDi.Generated.LangKey;

namespace ArkheideSystem.LangKey.Demo.WpfDi;

public partial class MainWindow : Window
{
    private readonly ILangKeyResolver resolver;
    private readonly DemoCultureSource cultureSource;

    public MainWindow(ILangKeyResolver resolver, DemoCultureSource cultureSource)
    {
        InitializeComponent();
        this.resolver = resolver;
        this.cultureSource = cultureSource;
        resolver.Changed += Resolver_Changed;
        RefreshCultureText();
    }

    protected override void OnClosed(EventArgs e)
    {
        resolver.Changed -= Resolver_Changed;
        base.OnClosed(e);
    }

    private void SwitchLanguage_Click(object sender, RoutedEventArgs e) => cultureSource.Toggle();

    private void ShowGreeting_Click(object sender, RoutedEventArgs e) =>
        MessageBox.Show(
            this,
            resolver.Get(GeneratedLangKey.Greeting),
            resolver.Get(GeneratedLangKey.App_Title),
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );

    private void Resolver_Changed(object? sender, LangKeyChangedEventArgs e) =>
        RefreshCultureText();

    private void RefreshCultureText() =>
        CultureText.Text = resolver.Format(GeneratedLangKey.Current_Culture, resolver.Current);
}
