using System.ComponentModel;
using System.Globalization;
using global::Avalonia;
using global::Avalonia.Data;
using global::Avalonia.Data.Converters;
using global::Avalonia.Metadata;
using global::Avalonia.Threading;

namespace ArkheideSystem.Essential.Culture.Avalonia;

/// <summary>
/// Provides the Avalonia binding implementation used by generated strongly typed
/// <c>Localize</c> markup extensions.
/// </summary>
public abstract class AvaloniaLocalizeExtensionBase
{
    /// <summary>
    /// Initializes a localization markup extension whose token will be assigned by a
    /// generated strongly typed property.
    /// </summary>
    protected AvaloniaLocalizeExtensionBase() { }

    /// <summary>
    /// Initializes a localization markup extension for a stable culture token.
    /// </summary>
    /// <param name="token">The stable token generated from Culture.json.</param>
    protected AvaloniaLocalizeExtensionBase(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        Token = token;
    }

    /// <summary>Gets or sets the stable token resolved by this markup extension.</summary>
    protected string? Token { get; set; }

    /// <summary>
    /// Gets or sets a binding that supplies the stable localization token at runtime.
    /// This property cannot be combined with the generated static <c>Key</c> property.
    /// </summary>
    public BindingBase? KeyBinding { get; set; }

    /// <summary>Gets or sets the first argument binding.</summary>
    public BindingBase? Arg0 { get; set; }

    /// <summary>Gets or sets the second argument binding.</summary>
    public BindingBase? Arg1 { get; set; }

    /// <summary>Gets or sets the third argument binding.</summary>
    public BindingBase? Arg2 { get; set; }

    /// <summary>
    /// Gets the ordered argument bindings used when more than three arguments are needed.
    /// </summary>
    /// <remarks>
    /// Do not combine this collection with <see cref="Arg0"/>, <see cref="Arg1"/>, or
    /// <see cref="Arg2"/>.
    /// </remarks>
    [Content]
    public IList<BindingBase> Arguments { get; } = [];

    /// <summary>Creates a one-way binding that refreshes for argument and culture changes.</summary>
    public MultiBinding ProvideValue(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        var hasStaticToken = !string.IsNullOrWhiteSpace(Token);
        if (hasStaticToken && KeyBinding is not null)
        {
            throw new InvalidOperationException(
                "Localize.Key and Localize.KeyBinding cannot be combined."
            );
        }

        if (!hasStaticToken && KeyBinding is null)
        {
            throw new InvalidOperationException(
                "Localize.Key or Localize.KeyBinding must be set before the markup extension is evaluated."
            );
        }

        ValidateArguments();

        var result = new MultiBinding
        {
            Converter = new LocalizationConverter(Token, KeyBinding is not null),
            Mode = BindingMode.OneWay,
        };

        result.Bindings.Add(
            new Binding(nameof(CultureChangeSignal.Version))
            {
                Source = CultureChangeSignal.Instance,
                Mode = BindingMode.OneWay,
            }
        );

        if (KeyBinding is not null)
        {
            result.Bindings.Add(KeyBinding);
        }

        if (Arguments.Count != 0)
        {
            foreach (var argument in Arguments)
            {
                result.Bindings.Add(argument);
            }
        }
        else
        {
            AddArgument(result, Arg0);
            AddArgument(result, Arg1);
            AddArgument(result, Arg2);
        }

        return result;
    }

    private void ValidateArguments()
    {
        if (Arguments.Count != 0 && (Arg0 is not null || Arg1 is not null || Arg2 is not null))
        {
            throw new InvalidOperationException(
                "Localize.Arguments cannot be combined with Arg0, Arg1, or Arg2."
            );
        }

        if (Arg1 is not null && Arg0 is null)
        {
            throw new InvalidOperationException("Localize.Arg1 requires Arg0.");
        }

        if (Arg2 is not null && Arg1 is null)
        {
            throw new InvalidOperationException("Localize.Arg2 requires Arg1.");
        }
    }

    private static void AddArgument(MultiBinding target, BindingBase? argument)
    {
        if (argument is not null)
        {
            target.Bindings.Add(argument);
        }
    }

    private sealed class LocalizationConverter(string? staticToken, bool usesKeyBinding)
        : IMultiValueConverter
    {
        public object? Convert(
            IList<object?> values,
            Type targetType,
            object? parameter,
            CultureInfo culture
        )
        {
            if (values.Count == 0)
            {
                return BindingOperations.DoNothing;
            }

            var argumentStart = usesKeyBinding ? 2 : 1;
            var resolvedToken = staticToken;
            if (usesKeyBinding)
            {
                if (
                    values.Count < 2
                    || ReferenceEquals(values[1], AvaloniaProperty.UnsetValue)
                    || ReferenceEquals(values[1], BindingOperations.DoNothing)
                    || values[1] is not string dynamicToken
                    || string.IsNullOrWhiteSpace(dynamicToken)
                )
                {
                    return BindingOperations.DoNothing;
                }

                resolvedToken = dynamicToken;
            }

            var argumentCount = values.Count - argumentStart;
            for (var index = argumentStart; index < values.Count; index++)
            {
                var value = values[index];
                if (
                    ReferenceEquals(value, AvaloniaProperty.UnsetValue)
                    || ReferenceEquals(value, BindingOperations.DoNothing)
                )
                {
                    return BindingOperations.DoNothing;
                }
            }

            var token = resolvedToken!;
            return argumentCount switch
            {
                0 => Localizer.Parse(token),
                1 => Localizer.Parse(token, values[argumentStart]),
                2 => Localizer.Parse(token, values[argumentStart], values[argumentStart + 1]),
                3 => Localizer.Parse(
                    token,
                    values[argumentStart],
                    values[argumentStart + 1],
                    values[argumentStart + 2]
                ),
                _ => ParseMany(token, values, argumentStart, argumentCount),
            };
        }

        private static string ParseMany(
            string token,
            IList<object?> values,
            int argumentStart,
            int argumentCount
        )
        {
            var arguments = new object?[argumentCount];
            for (var index = 0; index < argumentCount; index++)
            {
                arguments[index] = values[index + argumentStart];
            }

            return Localizer.Parse(token, arguments);
        }
    }

    private sealed class CultureChangeSignal : INotifyPropertyChanged
    {
        internal static CultureChangeSignal Instance { get; } = new();

        private int version;

        private CultureChangeSignal()
        {
            Localizer.Current.Changed += Localizer_Changed;
        }

        public int Version => version;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Localizer_Changed(object? sender, EventArgs e)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                NotifyChanged();
            }
            else
            {
                Dispatcher.UIThread.Post(NotifyChanged, DispatcherPriority.Normal);
            }
        }

        private void NotifyChanged()
        {
            version = version == int.MaxValue ? 1 : version + 1;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Version)));
        }
    }
}
