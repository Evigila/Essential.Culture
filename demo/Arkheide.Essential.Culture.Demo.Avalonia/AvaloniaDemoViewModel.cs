using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Arkheide.Essential.Culture.Demo.Shared;
using DemoKeys = Arkheide.Essential.Culture.Key;

namespace Arkheide.Essential.Culture.Demo.Avalonia;

public sealed class AvaloniaDemoViewModel : INotifyPropertyChanged, IDisposable
{
    private bool isDisposed;

    public AvaloniaDemoViewModel()
    {
        SwitchCultureCommand = new DelegateCommand(ToggleCulture);
        Localizer.Current.Changed += Localizer_Changed;
    }

    public string CurrentCulture =>
        Localizer.Parse(DemoKeys.Current_Culture, Localizer.Current.Culture);

    public ICommand SwitchCultureCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        Localizer.Current.Changed -= Localizer_Changed;
    }

    private static void ToggleCulture() =>
        Localizer.Current.SetCulture(
            Localizer.Current.Culture == "en-US" ? "zh-CN" : "en-US"
        );

    private void Localizer_Changed(object? sender, EventArgs e) =>
        OnPropertyChanged(nameof(CurrentCulture));

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
