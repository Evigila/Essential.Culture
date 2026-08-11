using System.Windows;
using ArkheideSystem.LangKey.Wpf;

namespace ArkheideSystem.LangKey.Demo.WpfApp;

public partial class App : Application
{
    private LangKeyParser? parser;
    private LangKeyWpfApplicator? applicator;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var documentPath = System.IO.Path.Combine(AppContext.BaseDirectory, "LangKey.json");
        parser = new LangKeyParser(documentPath, "en-US");
        applicator = new LangKeyWpfApplicator(parser);
        applicator.Start(Dispatcher);

        var window = new MainWindow(parser);
        applicator.Apply(window);
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        applicator?.Dispose();
        parser?.Dispose();
        base.OnExit(e);
    }
}
