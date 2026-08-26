using System.ComponentModel;
using System.Windows;
using GeneratedKey = ArkheideSystem.Essential.Culture.Key;

namespace ArkheideSystem.Essential.Culture.Demo.Wpf;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public MainWindow()
    {
        InitializeComponent();
        Localizer.Current.Changed += Localizer_Changed;
    }

    public string CurrentCulture => Localizer.Current.Culture;

    public string ProductName => "Arkheide";

    public event PropertyChangedEventHandler? PropertyChanged;

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
            Localizer.Parse(GeneratedKey.Greeting, ProductName),
            Localizer.Parse(GeneratedKey.App_Title),
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );

    private void Localizer_Changed(object? sender, EventArgs e) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
}
