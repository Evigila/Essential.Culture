using Microsoft.UI.Xaml;

namespace ArkheideSystem.LangKey.WinUI;

/// <summary>Applies LangKey tokens to WinUI windows and visual trees.</summary>
public interface ILangKeyWinUIApplicator
{
    /// <summary>
    /// Starts automatic localization for a window. Repeated calls for the same window are ignored.
    /// </summary>
    void Attach(Window window);

    /// <summary>Stops automatic localization for a previously attached window.</summary>
    void Detach(Window window);

    /// <summary>
    /// Applies the current culture to one visual-tree root, such as a dialog or popup content.
    /// </summary>
    void Apply(DependencyObject root);
}
