using ArkheideSystem.LangKey.Demo.Shared;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.LangKey.Demo.AvaloniaDi;

public partial class App : Application
{
    private ServiceProvider? services;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var collection = new ServiceCollection();
            collection.AddSingleton<DemoCultureSource>();
            collection.AddLangKey<DemoCultureSource>("LangKey.json");
            collection.AddSingleton<LocalizedDemoViewModel>();
            collection.AddTransient<MainWindow>();

            services = collection.BuildServiceProvider();
            desktop.MainWindow = services.GetRequiredService<MainWindow>();
            desktop.Exit += Desktop_Exit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e) =>
        services?.Dispose();
}
