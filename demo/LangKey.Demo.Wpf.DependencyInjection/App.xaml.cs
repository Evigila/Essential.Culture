using System.Windows;
using ArkheideSystem.LangKey.Demo.Shared;
using ArkheideSystem.LangKey.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.LangKey.Demo.WpfDi;

public partial class App : Application
{
    private IHost? host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton(this);
        builder.Services.AddSingleton<DemoCultureSource>();
        builder.Services.AddLangKeyWpf<App, DemoCultureSource>("LangKey.json");
        builder.Services.AddSingleton<MainWindow>();

        host = builder.Build();
        await host.StartAsync();

        var window = host.Services.GetRequiredService<MainWindow>();
        host.Services.GetRequiredService<ILangKeyWpfApplicator>().Apply(window);
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (host is not null)
        {
            host.StopAsync().GetAwaiter().GetResult();
            host.Dispose();
        }

        base.OnExit(e);
    }
}
