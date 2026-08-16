using Arkheide.Essential.Culture.Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DemoKey = Arkheide.Essential.Culture.Key;

namespace Arkheide.Essential.Culture.Demo.Avalonia;

public partial class MainWindow : Window
{
    private readonly IAvaloniaLocalizationApplicator? applicator;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(IAvaloniaLocalizationApplicator applicator)
        : this()
    {
        this.applicator = applicator;
        Localizer.Current.Changed += Localizer_Changed;
        Closed += MainWindow_Closed;
        UpdateCultureText();
    }

    private void SwitchLanguage_Click(object? sender, RoutedEventArgs e) =>
        Localizer.Current.SetCulture(
            Localizer.Current.Culture == "en-US" ? "zh-CN" : "en-US"
        );

    private async void ShowGreeting_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new GreetingDialog();
        applicator?.Apply(dialog);
        await dialog.ShowDialog(this);
    }

    private void Localizer_Changed(object? sender, EventArgs e) => UpdateCultureText();

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        Localizer.Current.Changed -= Localizer_Changed;
        Closed -= MainWindow_Closed;
    }

    private void UpdateCultureText() =>
        CurrentCultureText.Text = Localizer.Parse(
            DemoKey.Current_Culture,
            Localizer.Current.Culture
        );
}
