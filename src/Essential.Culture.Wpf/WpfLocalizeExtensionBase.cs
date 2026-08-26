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
        var selectedToken = Token;

        var binding = new MultiBinding
        {
            Mode = BindingMode.OneWay,
            Converter = new LocalizeMultiValueConverter(selectedToken),
        };
        binding.Bindings.Add(
            new Binding(nameof(CultureChangeSource.Version))
            {
                Mode = BindingMode.OneWay,
                Source = CultureChangeSource.Instance,
            }
        );

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

    private sealed class LocalizeMultiValueConverter(string token) : IMultiValueConverter
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
            for (var index = 1; index < values.Length; index++)
            {
                if (
                    ReferenceEquals(values[index], DependencyProperty.UnsetValue)
                    || ReferenceEquals(values[index], Binding.DoNothing)
                )
                {
                    return DependencyProperty.UnsetValue;
                }
            }

            if (values.Length == 1)
            {
                return Localizer.Parse(token);
            }

            var arguments = new object?[values.Length - 1];
            Array.Copy(values, 1, arguments, 0, arguments.Length);
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
