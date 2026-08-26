using Microsoft.UI.Xaml;

namespace ArkheideSystem.Essential.Culture.WinUI;

/// <summary>
/// Tracks values produced by the generated Localize markup extension in WinUI windows and visual
/// trees.
/// </summary>
public interface IWinUILocalizationHost : IDisposable
{
    /// <summary>
    /// Starts automatic localization for a window. Repeated calls for the same window are ignored.
    /// </summary>
    void Attach(Window window);

    /// <summary>Stops automatic localization for a previously attached window.</summary>
    void Detach(Window window);

    /// <summary>
    /// Tracks Localize values in one independent visual-tree root, such as XAML-created dialog or
    /// popup content. The caller must be on the root's owning UI thread.
    /// </summary>
    void Apply(DependencyObject root);
}
