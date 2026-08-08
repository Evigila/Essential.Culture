namespace ArkheideSystem.LangKey;

/// <summary>Provides culture and document change information.</summary>
public sealed class LangKeyChangedEventArgs(
    LangKeyChangeKind kind,
    string previous,
    string current
) : EventArgs
{
    /// <summary>Gets the reason for the change.</summary>
    public LangKeyChangeKind Kind { get; } = kind;

    /// <summary>Gets the culture selected before the change.</summary>
    public string Previous { get; } = previous;

    /// <summary>Gets the culture selected after the change.</summary>
    public string Current { get; } = current;
}
