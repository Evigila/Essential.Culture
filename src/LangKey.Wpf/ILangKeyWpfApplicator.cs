using System.Windows;
using System.Windows.Threading;

namespace ArkheideSystem.LangKey.Wpf;

/// <summary>Applies LangKey tokens to supported WPF display properties.</summary>
public interface ILangKeyWpfApplicator
{
    /// <summary>Starts automatic Loaded handling and culture-change refresh on a dispatcher.</summary>
    void Start(Dispatcher dispatcher);

    /// <summary>Applies localization to a WPF object tree immediately.</summary>
    void Apply(DependencyObject root);
}
