using Arkheide.Essential.Culture.Avalonia;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
namespace Arkheide.Essential.Culture.Demo.Avalonia;

public partial class App : Application
{
    private AvaloniaLocalizationApplicator? applicator;
    private AvaloniaDemoViewModel? viewModel;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Localizer.Current.SetCulture("en-US");
            applicator = new AvaloniaLocalizationApplicator();
            applicator.Start(this);
            viewModel = new AvaloniaDemoViewModel();
            var window = new MainWindow(viewModel, applicator);
            applicator.Apply(window);
            desktop.MainWindow = window;
            desktop.Exit += Desktop_Exit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        viewModel?.Dispose();
        applicator?.Dispose();
        viewModel = null;
        applicator = null;
    }
}
