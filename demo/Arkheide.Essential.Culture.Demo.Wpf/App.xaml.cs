using System.Windows;
using Arkheide.Essential.Culture.Wpf;

namespace Arkheide.Essential.Culture.Demo.WpfApp;

public partial class App : Application
{
    private WpfLocalizationApplicator? applicator;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        applicator = new WpfLocalizationApplicator();
        applicator.Start(Dispatcher);

        var window = new MainWindow();
        applicator.Apply(window);
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        applicator?.Dispose();
        base.OnExit(e);
    }
}
