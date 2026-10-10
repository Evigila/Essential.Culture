using System.Collections.Frozen;

namespace ArkheideSystem.Essential.Culture;

/// <summary>Defines an immutable, exact-tag startup policy for translation loading and selection.</summary>
public sealed class CatalogLoadOptions
{
    /// <summary>
    /// Copies and normalizes the supplied culture names. A null allowlist preserves all declared
    /// languages except disabled names; an explicit allowlist may include undeclared custom tags.
    /// Disabled parent tags are never retained through an enabled child's fallback chain.
    /// </summary>
    public CatalogLoadOptions(
        IEnumerable<string>? enabledCultures = null,
        IEnumerable<string>? disabledCultures = null
    )
    {
        EnabledCultures = enabledCultures is null ? null : Normalize(enabledCultures);
        DisabledCultures = Normalize(disabledCultures ?? []);
        DisabledNames = DisabledCultures.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        if (EnabledCultures is not null && EnabledCultures.Any(DisabledNames.Contains))
        {
            throw new ArgumentException(
                "Enabled and disabled cultures cannot contain the same normalized name.",
                nameof(disabledCultures)
            );
        }
    }

    /// <summary>Gets the selectable exact culture names, or null when no allowlist was supplied.</summary>
    public IReadOnlyList<string>? EnabledCultures { get; }

    /// <summary>Gets the exact culture names excluded from selection and retained translations.</summary>
    public IReadOnlyList<string> DisabledCultures { get; }

    internal FrozenSet<string> DisabledNames { get; }

    internal bool HasPolicy => EnabledCultures is not null || DisabledCultures.Count != 0;

    private static IReadOnlyList<string> Normalize(IEnumerable<string> cultures) =>
        Array.AsReadOnly(
            cultures.Select(CultureName.Normalize)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray()
        );
}
