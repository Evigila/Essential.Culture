using System.Windows;
using System.Windows.Threading;

namespace Arkheide.Essential.Culture.Wpf;

/// <summary>Applies Key tokens to supported WPF display properties.</summary>
public interface IWpfLocalizationApplicator
{
    /// <summary>Starts automatic Loaded handling and culture-change refresh on a dispatcher.</summary>
    void Start(Dispatcher dispatcher);

    /// <summary>Stops automatic handling without disposing the applicator.</summary>
    void Stop();

    /// <summary>Applies localization to a WPF object tree immediately.</summary>
    void Apply(DependencyObject root);
}
