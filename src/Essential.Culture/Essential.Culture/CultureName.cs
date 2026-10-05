namespace ArkheideSystem.Essential.Culture;

/// <summary>Normalizes culture names consistently with catalog validation and context selection.</summary>
public static class CultureName
{
    /// <summary>
    /// Normalizes separators and casing while preserving syntactically valid custom culture names.
    /// Invalid or empty names throw an argument exception.
    /// </summary>
    public static string Normalize(string culture) =>
        KeyValidation.NormalizeCulture(culture, nameof(culture));
}
