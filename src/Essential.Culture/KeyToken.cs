namespace ArkheideSystem.Essential.Culture;

/// <summary>Creates and recognizes stable tokens emitted by the source generator.</summary>
public static class KeyToken
{
    /// <summary>Gets the prefix used by generated stable tokens.</summary>
    public const string Prefix = "Key.";

    /// <summary>Creates a stable token from a validated raw key.</summary>
    public static string Create(string key)
    {
        KeyValidation.ValidateKey(key, nameof(key));
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
        if (!KeyValidation.IsValidKey(candidate))
        {
            return false;
        }

        key = candidate;
        return true;
    }
}
