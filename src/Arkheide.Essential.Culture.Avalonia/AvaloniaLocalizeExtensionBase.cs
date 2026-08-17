using System.ComponentModel;
using System.Globalization;
using global::Avalonia;
using global::Avalonia.Data;
using global::Avalonia.Data.Converters;
using global::Avalonia.Metadata;
using global::Avalonia.Threading;

namespace Arkheide.Essential.Culture.Avalonia;

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
        if (string.IsNullOrWhiteSpace(Token))
        {
            throw new InvalidOperationException(
                "Localize.Key must be set before the markup extension is evaluated."
            );
        }

        ValidateArguments();

        var result = new MultiBinding
        {
            Converter = new LocalizationConverter(Token),
            Mode = BindingMode.OneWay,
        };

        result.Bindings.Add(
            new Binding(nameof(CultureChangeSignal.Version))
            {
                Source = CultureChangeSignal.Instance,
                Mode = BindingMode.OneWay,
            }
        );

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

    private sealed class LocalizationConverter(string token) : IMultiValueConverter
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

            var argumentCount = values.Count - 1;
            if (argumentCount == 0)
            {
                return Localizer.Parse(token);
            }

            var arguments = new object?[argumentCount];
            for (var index = 0; index < argumentCount; index++)
            {
                var value = values[index + 1];
                if (ReferenceEquals(value, AvaloniaProperty.UnsetValue))
                {
                    return BindingOperations.DoNothing;
                }

                arguments[index] = value;
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
