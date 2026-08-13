using ArkheideSystem.LangKey.Demo.Shared;
using ArkheideSystem.LangKey.Avalonia;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.LangKey.Demo.AvaloniaDi;

public partial class App : Application
{
    private IHost? host;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Services.AddSingleton(this);
            builder.Services.AddSingleton<DemoCultureSource>();
            builder.Services.AddLangKeyAvalonia<App, DemoCultureSource>("LangKey.json");
            builder.Services.AddSingleton<AvaloniaDemoViewModel>();
            builder.Services.AddTransient<MainWindow>();

            host = builder.Build();
            host.Start();

            var window = host.Services.GetRequiredService<MainWindow>();
            host.Services.GetRequiredService<ILangKeyAvaloniaApplicator>().Apply(window);
            desktop.MainWindow = window;
            desktop.Exit += Desktop_Exit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (host is null)
        {
            return;
        }

        host.StopAsync().GetAwaiter().GetResult();
        host.Dispose();
    }
}
