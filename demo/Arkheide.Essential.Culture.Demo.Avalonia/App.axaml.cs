using Arkheide.Essential.Culture.Avalonia;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
namespace Arkheide.Essential.Culture.Demo.Avalonia;

public partial class App : Application
{
    private AvaloniaLocalizationApplicator? applicator;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            applicator = new AvaloniaLocalizationApplicator();
            applicator.Start(this);
            var window = new MainWindow(applicator);
            applicator.Apply(window);
            desktop.MainWindow = window;
            desktop.Exit += Desktop_Exit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        applicator?.Dispose();
        applicator = null;
    }
}
