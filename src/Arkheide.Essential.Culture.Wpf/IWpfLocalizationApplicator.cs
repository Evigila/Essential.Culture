using System.Windows;
using System.Windows.Threading;

namespace Arkheide.Essential.Culture.Wpf;

/// <summary>Applies Key tokens to supported WPF display properties.</summary>
public interface IWpfLocalizationApplicator : IDisposable
{
    /// <summary>Starts automatic Loaded handling and culture-change refresh on a dispatcher.</summary>
    void Start(Dispatcher dispatcher);

    /// <summary>Stops automatic handling without disposing the applicator.</summary>
    void Stop();

    /// <summary>
    /// Applies localization to a WPF object tree immediately. This method must be called on
    /// the dispatcher's owning thread.
    /// </summary>
    void Apply(DependencyObject root);
}
