using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ArkheideSystem.Essential.Culture.Wpf;

/// <summary>
/// Provides the WPF binding implementation used by the generated, strongly typed
/// <c>Localize</c> markup extension.
/// </summary>
[ContentProperty(nameof(Arguments))]
[MarkupExtensionReturnType(typeof(object))]
public abstract class WpfLocalizeExtensionBase : MarkupExtension
{
    private string? token;

    /// <summary>
    /// Initializes an empty localization markup extension. A derived extension must set
    /// <see cref="Token"/> before <see cref="ProvideValue"/> is called.
    /// </summary>
    protected WpfLocalizeExtensionBase() { }

    /// <summary>Initializes a localization markup extension for a stable key token.</summary>
    /// <param name="token">The stable token selected by the generated key facade.</param>
    protected WpfLocalizeExtensionBase(string token)
    {
        Token = token;
    }

    /// <summary>Gets or sets the stable token selected by the generated extension.</summary>
    protected string Token
    {
        get =>
            token
            ?? throw new InvalidOperationException(
                "A localization key must be selected before the markup extension is evaluated."
            );
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            token = value;
        }
    }

    /// <summary>
    /// Gets or sets a binding that supplies the stable localization token at runtime.
    /// This property cannot be combined with the generated static <c>Key</c> property.
    /// </summary>
    public BindingBase? KeyBinding { get; set; }

    /// <summary>Gets or sets the first dynamic format argument.</summary>
    public BindingBase? Arg0 { get; set; }

    /// <summary>Gets or sets the second dynamic format argument.</summary>
    public BindingBase? Arg1 { get; set; }

    /// <summary>Gets or sets the third dynamic format argument.</summary>
    public BindingBase? Arg2 { get; set; }

    /// <summary>
    /// Gets the ordered format argument bindings used when more than three arguments are needed.
    /// This collection cannot be combined with <see cref="Arg0"/>, <see cref="Arg1"/>, or
    /// <see cref="Arg2"/>.
    /// </summary>
    public Collection<BindingBase> Arguments { get; } = [];

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        var hasStaticToken = !string.IsNullOrWhiteSpace(token);
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

        var binding = new MultiBinding
        {
            Mode = BindingMode.OneWay,
            Converter = new LocalizeMultiValueConverter(token, KeyBinding is not null),
        };
        binding.Bindings.Add(
            new Binding(nameof(CultureChangeSource.Version))
            {
                Mode = BindingMode.OneWay,
                Source = CultureChangeSource.Instance,
            }
        );

        if (KeyBinding is not null)
        {
            binding.Bindings.Add(KeyBinding);
        }

        AddArgumentBindings(binding);
        return binding.ProvideValue(serviceProvider);
    }

    private void AddArgumentBindings(MultiBinding binding)
    {
        if (Arguments.Count != 0)
        {
            if (Arg0 is not null || Arg1 is not null || Arg2 is not null)
            {
                throw new InvalidOperationException(
                    $"{nameof(Arguments)} cannot be combined with {nameof(Arg0)}, "
                        + $"{nameof(Arg1)}, or {nameof(Arg2)}."
                );
            }

            foreach (var argument in Arguments)
            {
                binding.Bindings.Add(argument);
            }

            return;
        }

        if (Arg1 is not null && Arg0 is null)
        {
            throw new InvalidOperationException(
                $"{nameof(Arg0)} must be specified before {nameof(Arg1)}."
            );
        }

        if (Arg2 is not null && Arg1 is null)
        {
            throw new InvalidOperationException(
                $"{nameof(Arg1)} must be specified before {nameof(Arg2)}."
            );
        }

        AddIfPresent(binding, Arg0);
        AddIfPresent(binding, Arg1);
        AddIfPresent(binding, Arg2);
    }

    private static void AddIfPresent(MultiBinding binding, BindingBase? argument)
    {
        if (argument is not null)
        {
            binding.Bindings.Add(argument);
        }
    }

    private sealed class LocalizeMultiValueConverter(string? staticToken, bool usesKeyBinding)
        : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            // values[0] is the culture version signal. It is intentionally not forwarded to
            // composite formatting; its only purpose is to invalidate this MultiBinding.
            var argumentStart = usesKeyBinding ? 2 : 1;
            var resolvedToken = staticToken;
            if (usesKeyBinding)
            {
                if (
                    values.Length < 2
                    || ReferenceEquals(values[1], DependencyProperty.UnsetValue)
                    || ReferenceEquals(values[1], Binding.DoNothing)
                    || values[1] is not string dynamicToken
                    || string.IsNullOrWhiteSpace(dynamicToken)
                )
                {
                    return DependencyProperty.UnsetValue;
                }

                resolvedToken = dynamicToken;
            }

            for (var index = argumentStart; index < values.Length; index++)
            {
                if (
                    ReferenceEquals(values[index], DependencyProperty.UnsetValue)
                    || ReferenceEquals(values[index], Binding.DoNothing)
                )
                {
                    return DependencyProperty.UnsetValue;
                }
            }

            var token = resolvedToken!;
            var argumentCount = values.Length - argumentStart;
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
            object[] values,
            int argumentStart,
            int argumentCount
        )
        {
            var arguments = new object?[argumentCount];
            Array.Copy(values, argumentStart, arguments, 0, argumentCount);
            return Localizer.Parse(token, arguments);
        }

        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture
        ) => throw new NotSupportedException("Localized values are one-way bindings.");
    }

    private sealed class CultureChangeSource : INotifyPropertyChanged
    {
        private int version;

        private CultureChangeSource()
        {
            Localizer.Current.Changed += Localizer_Changed;
        }

        internal static CultureChangeSource Instance { get; } = new();

        public int Version => Volatile.Read(ref version);

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Localizer_Changed(object? sender, EventArgs e)
        {
            _ = Interlocked.Increment(ref version);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Version)));
        }
    }
}
