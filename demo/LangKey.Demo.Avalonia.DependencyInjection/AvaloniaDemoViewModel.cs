using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ArkheideSystem.LangKey.Demo.Shared;
using DemoKeys = ArkheideSystem.LangKey.Demo.Shared.LangKey;

namespace ArkheideSystem.LangKey.Demo.AvaloniaDi;

public sealed class AvaloniaDemoViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ILangKeyResolver resolver;
    private bool isDisposed;

    public AvaloniaDemoViewModel(ILangKeyResolver resolver, DemoCultureSource cultureSource)
    {
        this.resolver = resolver;
        SwitchCultureCommand = new DelegateCommand(cultureSource.Toggle);
        resolver.Changed += Resolver_Changed;
    }

    public string CurrentCulture => resolver.Format(DemoKeys.Current_Culture, resolver.Current);

    public ICommand SwitchCultureCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        resolver.Changed -= Resolver_Changed;
    }

    private void Resolver_Changed(object? sender, LangKeyChangedEventArgs e) =>
        OnPropertyChanged(nameof(CurrentCulture));

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
