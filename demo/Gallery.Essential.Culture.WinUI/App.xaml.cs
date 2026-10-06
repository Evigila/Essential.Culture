using ArkheideSystem.Essential.Culture.WinUI;
using Microsoft.UI.Xaml;

namespace ArkheideSystem.Essential.Culture.Demo.WinUI3;

public partial class App : Application
{
    private readonly WinUILocalizationHost localizationHost;
    private Window? window;

    public App()
    {
        InitializeComponent();
        localizationHost = new WinUILocalizationHost();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        window = new MainWindow();
        localizationHost.Attach(window);
        window.Closed += Window_Closed;
        window.Activate();
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        window = null;
        localizationHost.Dispose();
    }
}
