using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ArkheideSystem.Essential.Culture.Blazor;

/// <summary>Renders encoded localized text and refreshes when the current scope changes language.</summary>
public sealed class LocalizedText : LocalizedComponentBase
{
    /// <summary>Gets or sets a raw key or generated Key.* token.</summary>
    [Parameter, EditorRequired] public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the catalog identity, or null for the default catalog.</summary>
    [Parameter] public string? CatalogId { get; set; }

    /// <summary>Gets or sets composite-format arguments.</summary>
    [Parameter] public object?[] Arguments { get; set; } = [];

    /// <summary>Gets or sets literal fallback text for a missing key, or null to retain Parse behavior.</summary>
    [Parameter] public string? Fallback { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Key);
        ArgumentNullException.ThrowIfNull(Arguments);
        string text;
        if (Fallback is not null)
        {
            var found = CatalogId is null
                ? Localization.TryParse(Key, Arguments, out text!)
                : Localization.TryParseFrom(CatalogId, Key, Arguments, out text!);
            if (!found) text = Fallback;
        }
        else text = CatalogId is null ? Localization.Parse(Key, Arguments) : Localization.ParseFrom(CatalogId, Key, Arguments);
        builder.AddContent(0, text);
    }
}
