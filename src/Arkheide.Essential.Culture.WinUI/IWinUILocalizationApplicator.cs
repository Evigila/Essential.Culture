using Microsoft.UI.Xaml;

namespace Arkheide.Essential.Culture.WinUI;

/// <summary>Applies Key tokens to WinUI windows and visual trees.</summary>
public interface IWinUILocalizationApplicator : IDisposable
{
    /// <summary>
    /// Starts automatic localization for a window. Repeated calls for the same window are ignored.
    /// </summary>
    void Attach(Window window);

    /// <summary>Stops automatic localization for a previously attached window.</summary>
    void Detach(Window window);

    /// <summary>
    /// Applies the current culture to one visual-tree root, such as a dialog or popup content.
    /// The caller must be on the root's owning UI thread.
    /// </summary>
    void Apply(DependencyObject root);
}
