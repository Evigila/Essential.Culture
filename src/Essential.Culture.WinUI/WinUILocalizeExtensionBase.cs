using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace ArkheideSystem.Essential.Culture.WinUI;

/// <summary>
/// Provides the WinUI implementation used by generated strongly typed localization markup
/// extensions.
/// </summary>
public abstract class WinUILocalizeExtensionBase : MarkupExtension
{
    /// <summary>
    /// Identifies the localization token binding used when the key is selected at runtime.
    /// </summary>
    public static readonly DependencyProperty KeyBindingProperty =
        DependencyProperty.RegisterAttached(
            "KeyBinding",
            typeof(string),
            typeof(WinUILocalizeExtensionBase),
            new PropertyMetadata(null, OnLocalizationInputChanged)
        );

    /// <summary>Identifies the first localization format argument.</summary>
    public static readonly DependencyProperty Argument0Property =
        DependencyProperty.RegisterAttached(
            "Argument0",
            typeof(object),
            typeof(WinUILocalizeExtensionBase),
            new PropertyMetadata(null, OnLocalizationInputChanged)
        );

    /// <summary>Identifies the second localization format argument.</summary>
    public static readonly DependencyProperty Argument1Property =
        DependencyProperty.RegisterAttached(
            "Argument1",
            typeof(object),
            typeof(WinUILocalizeExtensionBase),
            new PropertyMetadata(null, OnLocalizationInputChanged)
        );

    /// <summary>Identifies the third localization format argument.</summary>
    public static readonly DependencyProperty Argument2Property =
        DependencyProperty.RegisterAttached(
            "Argument2",
            typeof(object),
            typeof(WinUILocalizeExtensionBase),
            new PropertyMetadata(null, OnLocalizationInputChanged)
        );

    /// <summary>
    /// Identifies an arbitrary-length localization format argument list. When non-null, this
    /// list takes precedence over the three indexed argument properties.
    /// </summary>
    public static readonly DependencyProperty ArgumentsProperty =
        DependencyProperty.RegisterAttached(
            "Arguments",
            typeof(IList<object>),
            typeof(WinUILocalizeExtensionBase),
            new PropertyMetadata(null, OnLocalizationInputChanged)
        );

    private string? token;

    /// <summary>
    /// Creates an uninitialized markup extension for the WinUI XAML loader. A generated facade
    /// must assign <see cref="Token"/> before the value is requested.
    /// </summary>
    protected WinUILocalizeExtensionBase() { }

    /// <summary>Creates a markup extension that supplies a stable localization token.</summary>
    /// <param name="token">The generated <c>Key.*</c> token.</param>
    protected WinUILocalizeExtensionBase(string token)
    {
        Token = token;
    }

    /// <summary>Gets or sets the stable localization token supplied by a generated facade.</summary>
    protected string Token
    {
        get =>
            token
            ?? throw new InvalidOperationException(
                "The generated localization key must be assigned before ProvideValue is called."
            );
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            token = value;
        }
    }

    /// <summary>Gets the localization token selected by a runtime binding.</summary>
    public static string? GetKeyBinding(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return target.GetValue(KeyBindingProperty) as string;
    }

    /// <summary>Sets the localization token selected by a runtime binding.</summary>
    public static void SetKeyBinding(DependencyObject target, string? value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(KeyBindingProperty, value);
    }

    /// <summary>Gets the first localization format argument.</summary>
    public static object? GetArgument0(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return target.GetValue(Argument0Property);
    }

    /// <summary>Sets the first localization format argument.</summary>
    public static void SetArgument0(DependencyObject target, object? value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(Argument0Property, value);
    }

    /// <summary>Gets the second localization format argument.</summary>
    public static object? GetArgument1(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return target.GetValue(Argument1Property);
    }

    /// <summary>Sets the second localization format argument.</summary>
    public static void SetArgument1(DependencyObject target, object? value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(Argument1Property, value);
    }

    /// <summary>Gets the third localization format argument.</summary>
    public static object? GetArgument2(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return target.GetValue(Argument2Property);
    }

    /// <summary>Sets the third localization format argument.</summary>
    public static void SetArgument2(DependencyObject target, object? value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(Argument2Property, value);
    }

    /// <summary>Gets the arbitrary-length localization format argument list.</summary>
    public static IList<object?>? GetArguments(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return target.GetValue(ArgumentsProperty) as IList<object?>;
    }

    /// <summary>Sets the arbitrary-length localization format argument list.</summary>
    public static void SetArguments(DependencyObject target, IList<object?>? value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(ArgumentsProperty, value);
    }

    /// <inheritdoc />
    protected override object ProvideValue() =>
        token is null
            ? WinUILocalizationMarker.CreateDynamic()
            : WinUILocalizationMarker.Create(token);

    internal static object?[] GetCurrentArguments(DependencyObject target)
    {
        if (GetArguments(target) is { } arguments)
        {
            return [.. arguments];
        }

        var count = 0;
        if (target.ReadLocalValue(Argument2Property) != DependencyProperty.UnsetValue)
        {
            count = 3;
        }
        else if (target.ReadLocalValue(Argument1Property) != DependencyProperty.UnsetValue)
        {
            count = 2;
        }
        else if (target.ReadLocalValue(Argument0Property) != DependencyProperty.UnsetValue)
        {
            count = 1;
        }

        return count switch
        {
            0 => [],
            1 => [GetArgument0(target)],
            2 => [GetArgument0(target), GetArgument1(target)],
            _ => [GetArgument0(target), GetArgument1(target), GetArgument2(target)],
        };
    }

    private static void OnLocalizationInputChanged(
        DependencyObject target,
        DependencyPropertyChangedEventArgs args
    ) => WinUILocalizationHost.RefreshInputs(target);
}
