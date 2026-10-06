using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using CKey = ArkheideSystem.Essential.Culture.Key;

namespace ArkheideSystem.Essential.Culture.Demo.Wpf;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private bool isGirl;

    public MainWindow()
    {
        InitializeComponent();
        Localizer.Current.Changed += Localizer_Changed;
    }

    public string CurrentCulture => Localizer.Current.Culture;

    public bool IsGirl
    {
        get => isGirl;
        set
        {
            if (isGirl == value)
            {
                return;
            }

            isGirl = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(GreetingKey));
        }
    }

    public string GreetingKey => IsGirl ? CKey.Greeting_Girl : CKey.Greeting_Boy;

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
        new GreetingDialog(GreetingKey) { Owner = this }.ShowDialog();

    private void Localizer_Changed(object? sender, EventArgs e) =>
        OnPropertyChanged(nameof(CurrentCulture));

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
