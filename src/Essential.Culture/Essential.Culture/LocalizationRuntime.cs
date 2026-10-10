namespace ArkheideSystem.Essential.Culture;

/// <summary>Preserves the process-wide desktop facade over an isolated catalog/context implementation.</summary>
internal sealed class LocalizationRuntime
{
    private static readonly Lock sharedGate = new();
    private static LocalizationRuntime? shared;
    private static bool started;

    private static class DefaultHolder
    {
        static DefaultHolder() { }

        internal static readonly LocalizationRuntime Instance = new(
            Path.Combine(AppContext.BaseDirectory, "Culture.json"), "en-US", "en-US"
        );
    }

    private readonly LocalizationContext context;

    internal static LocalizationRuntime Shared
    {
        get
        {
            if (Volatile.Read(ref shared) is { } current) return current;
            lock (sharedGate)
            {
                if (shared is null)
                {
                    started = true;
                    var created = DefaultHolder.Instance;
                    Volatile.Write(ref shared, created);
                }
                return shared;
            }
        }
    }

    internal static void Configure(LocalizationCatalog catalog, string culture, string? formatCulture)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        lock (sharedGate)
        {
            if (started)
            {
                throw new InvalidOperationException("The static localizer has already been configured or used.");
            }
            var configured = new LocalizationRuntime(catalog, culture, formatCulture);
            started = true;
            Volatile.Write(ref shared, configured);
        }
    }

    internal LocalizationRuntime(string path, string current, string fallback = "en-US")
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The file path cannot be empty.", nameof(path));
        }

        var sourcePath = Path.GetFullPath(path);
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"File '{sourcePath}' does not exist.", sourcePath);
        }

        var normalizedFallback = KeyValidation.NormalizeCulture(fallback, nameof(fallback));
        var catalog = LocalizationCatalog.LoadValidatedFile(sourcePath, normalizedFallback);
        context = new LocalizationContext(
            catalog, KeyValidation.NormalizeCulture(current, nameof(current))
        );
        context.Changed += (_, args) => Changed?.Invoke(this, args);
    }

    private LocalizationRuntime(LocalizationCatalog catalog, string culture, string? formatCulture)
    {
        context = new LocalizationContext(catalog, culture, formatCulture);
        context.Changed += (_, args) => Changed?.Invoke(this, args);
    }

    internal string Culture => context.Culture;

    internal IReadOnlyList<string> AvailableCultures => context.AvailableCultures;

    internal event EventHandler? Changed;

    internal void SetCulture(string culture) => context.SetCulture(culture);

    /// <summary>Resolves a stable token using the current culture.</summary>
    internal string Parse(string token) => context.Parse(token);

    /// <summary>Resolves and formats a stable token using the current culture.</summary>
    internal string Parse(string token, params object?[] arguments) =>
        context.Parse(token, arguments);

    /// <summary>Resolves and formats a token without allocating an argument array.</summary>
    internal string Parse<TArg0>(string token, TArg0 argument0) =>
        context.Parse(token, argument0);

    /// <summary>Resolves and formats a token without allocating an argument array.</summary>
    internal string Parse<TArg0, TArg1>(string token, TArg0 argument0, TArg1 argument1) =>
        context.Parse(token, argument0, argument1);

    /// <summary>Resolves and formats a token without allocating an argument array.</summary>
    internal string Parse<TArg0, TArg1, TArg2>(
        string token,
        TArg0 argument0,
        TArg1 argument1,
        TArg2 argument2
    ) => context.Parse(token, argument0, argument1, argument2);

    /// <summary>Attempts to resolve a stable token using the current culture.</summary>
    internal bool TryParse(string token, out string value) =>
        context.TryParse(token, out value);

    /// <summary>Attempts to resolve and format a stable token using the current culture.</summary>
    internal bool TryParse(string token, object?[] arguments, out string value) =>
        context.TryParse(token, arguments, out value);

    /// <summary>Attempts to resolve and format a token without allocating an argument array.</summary>
    internal bool TryParse<TArg0>(string token, TArg0 argument0, out string value) =>
        context.TryParse(token, argument0, out value);

    /// <summary>Attempts to resolve and format a token without allocating an argument array.</summary>
    internal bool TryParse<TArg0, TArg1>(
        string token,
        TArg0 argument0,
        TArg1 argument1,
        out string value
    ) => context.TryParse(token, argument0, argument1, out value);

    /// <summary>Attempts to resolve and format a token without allocating an argument array.</summary>
    internal bool TryParse<TArg0, TArg1, TArg2>(
        string token,
        TArg0 argument0,
        TArg1 argument1,
        TArg2 argument2,
        out string value
    ) => context.TryParse(token, argument0, argument1, argument2, out value);

    /// <summary>Returns whether a raw key or prefixed token is declared.</summary>
    internal bool Contains(string token) => context.Contains(token);
}
