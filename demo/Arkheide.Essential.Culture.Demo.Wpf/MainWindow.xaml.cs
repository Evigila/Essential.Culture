using System.Windows;
using GeneratedKey = Arkheide.Essential.Culture.Key;

namespace Arkheide.Essential.Culture.Demo.WpfApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Localizer.Current.Changed += Localizer_Changed;
        RefreshCultureText();
    }

    protected override void OnClosed(EventArgs e)
    {
        Localizer.Current.Changed -= Localizer_Changed;
        base.OnClosed(e);
    }

    private void SwitchLanguage_Click(object sender, RoutedEventArgs e) =>
        Localizer.Current.SetCulture(
            Localizer.Current.Culture == "en-US" ? "zh-CN" : "en-US"
        );

    private void ShowGreeting_Click(object sender, RoutedEventArgs e) =>
        MessageBox.Show(
            this,
            Localizer.Parse(GeneratedKey.Greeting),
            Localizer.Parse(GeneratedKey.App_Title),
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );

    private void Localizer_Changed(object? sender, EventArgs e) =>
        RefreshCultureText();

    private void RefreshCultureText() =>
        CultureText.Text = Localizer.Parse(
            GeneratedKey.Current_Culture,
            Localizer.Current.Culture
        );
}
