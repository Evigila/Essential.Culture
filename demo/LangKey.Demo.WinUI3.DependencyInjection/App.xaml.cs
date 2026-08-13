using ArkheideSystem.LangKey.Demo.Shared;
using ArkheideSystem.LangKey.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace ArkheideSystem.LangKey.Demo.WinUI3Di;

public partial class App : Application
{
    private readonly ServiceProvider services;
    private Window? window;

    public App()
    {
        InitializeComponent();

        var collection = new ServiceCollection();
        collection.AddSingleton<DemoCultureSource>();
        collection.AddLangKeyWinUI<DemoCultureSource>("LangKey.json");
        collection.AddSingleton<MainWindow>();
        services = collection.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        window = services.GetRequiredService<MainWindow>();
        services.GetRequiredService<ILangKeyWinUIApplicator>().Attach(window);
        window.Closed += Window_Closed;
        window.Activate();
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        window = null;
        services.Dispose();
    }
}
