using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ArkheideSystem.LangKey.Demo.Shared;

public sealed class LocalizedDemoViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ILangKeyResolver resolver;
    private readonly DemoCultureSource cultureSource;
    private bool isDisposed;

    public LocalizedDemoViewModel(
        ILangKeyResolver resolver,
        DemoCultureSource cultureSource
    )
    {
        this.resolver = resolver;
        this.cultureSource = cultureSource;
        SwitchCultureCommand = new DelegateCommand(cultureSource.Toggle);
        resolver.Changed += Resolver_Changed;
    }

    public string Title => resolver.Get(LangKey.App_Title);

    public string Greeting => resolver.Get(LangKey.Greeting);

    public string Description => resolver.Get(LangKey.Description);

    public string CurrentCulture => resolver.Format(LangKey.Current_Culture, resolver.Current);

    public string SwitchLanguageText => resolver.Get(LangKey.Action_SwitchLanguage);

    public string ShowGreetingText => resolver.Get(LangKey.Action_ShowGreeting);

    public string CloseText => resolver.Get(LangKey.Action_Close);

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
        OnPropertyChanged(string.Empty);

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
