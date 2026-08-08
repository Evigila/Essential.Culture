namespace ArkheideSystem.LangKey;

/// <summary>Creates and recognizes stable tokens emitted by the source generator.</summary>
public static class LangKeyToken
{
    /// <summary>Gets the prefix used by generated stable tokens.</summary>
    public const string Prefix = "LangKey.";

    /// <summary>Creates a stable token from a validated raw key.</summary>
    public static string Create(string key)
    {
        LangKeyValidation.ValidateKey(key, nameof(key));
        return Prefix + key;
    }

    /// <summary>Extracts and validates a raw key from either a token or a raw key.</summary>
    public static bool TryGetKey(string token, out string key)
    {
        key = string.Empty;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var candidate = token.StartsWith(Prefix, StringComparison.Ordinal)
            ? token[Prefix.Length..]
            : token;
        if (!LangKeyValidation.IsValidKey(candidate))
        {
            return false;
        }

        key = candidate;
        return true;
    }
}
