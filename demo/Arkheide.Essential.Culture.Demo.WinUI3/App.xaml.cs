using Arkheide.Essential.Culture.WinUI;
using Microsoft.UI.Xaml;

namespace Arkheide.Essential.Culture.Demo.WinUI3;

public partial class App : Application
{
    private readonly WinUILocalizationApplicator applicator;
    private Window? window;

    public App()
    {
        InitializeComponent();
        applicator = new WinUILocalizationApplicator();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        window = new MainWindow(applicator);
        applicator.Attach(window);
        window.Closed += Window_Closed;
        window.Activate();
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        window = null;
        applicator.Dispose();
    }
}
