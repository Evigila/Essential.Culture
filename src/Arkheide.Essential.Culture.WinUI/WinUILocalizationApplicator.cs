using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Arkheide.Essential.Culture.WinUI;

/// <summary>
/// Resolves Key tokens stored in common WinUI display dependency properties and refreshes
/// attached windows when the active culture changes.
/// </summary>
public sealed partial class WinUILocalizationApplicator : IWinUILocalizationApplicator, IDisposable
{
    private static readonly PropertyRule[] LocalizableProperties =
    [
        new("TextBlockText", TextBlock.TextProperty, static target => target is TextBlock),
        new(
            "ContentControlContent",
            ContentControl.ContentProperty,
            static target => target is ContentControl
        ),
        new(
            "ContentDialogTitle",
            ContentDialog.TitleProperty,
            static target => target is ContentDialog
        ),
        new(
            "ContentDialogPrimaryButtonText",
            ContentDialog.PrimaryButtonTextProperty,
            static target => target is ContentDialog
        ),
        new(
            "ContentDialogSecondaryButtonText",
            ContentDialog.SecondaryButtonTextProperty,
            static target => target is ContentDialog
        ),
        new(
            "ContentDialogCloseButtonText",
            ContentDialog.CloseButtonTextProperty,
            static target => target is ContentDialog
        ),
        new(
            "TextBoxPlaceholder",
            TextBox.PlaceholderTextProperty,
            static target => target is TextBox
        ),
        new(
            "PasswordBoxPlaceholder",
            PasswordBox.PlaceholderTextProperty,
            static target => target is PasswordBox
        ),
        new(
            "RichEditBoxPlaceholder",
            RichEditBox.PlaceholderTextProperty,
            static target => target is RichEditBox
        ),
        new(
            "AutoSuggestBoxPlaceholder",
            AutoSuggestBox.PlaceholderTextProperty,
            static target => target is AutoSuggestBox
        ),
        new("ToolTip", ToolTipService.ToolTipProperty, static _ => true),
        new("AutomationName", AutomationProperties.NameProperty, static _ => true),
    ];

    private readonly Lock gate = new();
    private readonly Dictionary<Window, WindowRegistration> windows = new(
        ReferenceEqualityComparer.Instance
    );
    private bool isDisposed;

    /// <summary>Creates a WinUI applicator backed by the active localization runtime.</summary>
    public WinUILocalizationApplicator()
    {
        Localizer.Current.Changed += Localizer_Changed;
    }

    /// <inheritdoc />
    public void Attach(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        EnsureDispatcherAccess(window.DispatcherQueue, "attach");

        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            if (windows.ContainsKey(window))
            {
                return;
            }

            windows.Add(
                window,
                new WindowRegistration(window.DispatcherQueue, CaptureWindowTitle(window))
            );
        }

        window.Activated += Window_Activated;
        window.Closed += Window_Closed;
        ApplyWindow(window);
    }

    /// <inheritdoc />
    public void Detach(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        EnsureDispatcherAccess(window.DispatcherQueue, "detach");

        lock (gate)
        {
            if (!windows.Remove(window))
            {
                return;
            }
        }

        window.Activated -= Window_Activated;
        window.Closed -= Window_Closed;
    }

    /// <inheritdoc />
    public void Apply(DependencyObject root)
    {
        ArgumentNullException.ThrowIfNull(root);
        ObjectDisposedException.ThrowIf(isDisposed, this);

        var dispatcherQueue = root.DispatcherQueue;
        if (dispatcherQueue.HasThreadAccess)
        {
            ApplyCore(root);
            return;
        }

        if (!dispatcherQueue.TryEnqueue(() => ApplyIfActive(root)))
        {
            throw new InvalidOperationException(
                "The WinUI dispatcher is no longer accepting localization work."
            );
        }
    }

    /// <summary>Stops all automatic window tracking and culture-change handling.</summary>
    public void Dispose()
    {
        KeyValuePair<Window, WindowRegistration>[] registrations;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            registrations = [.. windows];
            windows.Clear();
        }

        Localizer.Current.Changed -= Localizer_Changed;
        foreach (var (window, registration) in registrations)
        {
            if (registration.DispatcherQueue.HasThreadAccess)
            {
                window.Activated -= Window_Activated;
                window.Closed -= Window_Closed;
            }
            else
            {
                _ = registration.DispatcherQueue.TryEnqueue(() =>
                {
                    window.Activated -= Window_Activated;
                    window.Closed -= Window_Closed;
                });
            }
        }
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (sender is Window window)
        {
            RefreshWindowIfAttached(window);
        }
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        if (sender is Window window)
        {
            Detach(window);
        }
    }

    private void Localizer_Changed(object? sender, EventArgs args)
    {
        KeyValuePair<Window, WindowRegistration>[] registrations;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            registrations = [.. windows];
        }

        foreach (var (window, registration) in registrations)
        {
            if (registration.DispatcherQueue.HasThreadAccess)
            {
                RefreshWindowIfAttached(window);
            }
            else
            {
                _ = registration.DispatcherQueue.TryEnqueue(
                    DispatcherQueuePriority.Normal,
                    () => RefreshWindowIfAttached(window)
                );
            }
        }
    }

    private string? CaptureWindowTitle(Window window) =>
        Localizer.Contains(window.Title) ? window.Title : null;

    private void RefreshWindowIfAttached(Window window)
    {
        lock (gate)
        {
            if (isDisposed || !windows.ContainsKey(window))
            {
                return;
            }
        }

        ApplyWindow(window);
    }

    private void ApplyWindow(Window window)
    {
        WindowRegistration registration;
        lock (gate)
        {
            if (isDisposed || !windows.TryGetValue(window, out registration!))
            {
                return;
            }
        }

        if (registration.Title is null && Localizer.Contains(window.Title))
        {
            registration.Title = new TrackedValue(window.Title, window.Title);
        }

        if (registration.Title is { } title)
        {
            if (!string.Equals(window.Title, title.LastResolved, StringComparison.Ordinal))
            {
                if (!Localizer.Contains(window.Title))
                {
                    registration.Title = null;
                }
                else
                {
                    title.Key = window.Title;
                }
            }

            if (registration.Title is not null)
            {
                var resolvedTitle = Localizer.Parse(title.Key);
                title.LastResolved = resolvedTitle;
                window.Title = resolvedTitle;
            }
        }

        if (window.Content is DependencyObject root)
        {
            ApplyCore(root);
        }
    }

    private void ApplyIfActive(DependencyObject root)
    {
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }
        }

        ApplyCore(root);
    }

    private void ApplyCore(DependencyObject root)
    {
        var visited = new HashSet<DependencyObject>(ReferenceEqualityComparer.Instance);
        var pending = new Stack<DependencyObject>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
            {
                continue;
            }

            ApplyObject(current);
            var childCount = VisualTreeHelper.GetChildrenCount(current);
            for (var index = 0; index < childCount; index++)
            {
                pending.Push(VisualTreeHelper.GetChild(current, index));
            }
        }
    }

    private static void ApplyObject(DependencyObject target)
    {
        foreach (var rule in LocalizableProperties)
        {
            if (!rule.AppliesTo(target))
            {
                continue;
            }

            var property = rule.Property;
            var localValue = target.ReadLocalValue(property);
            if (localValue is not string value)
            {
                ClearTrackedValue(target, rule);
                continue;
            }

            var key = target.GetValue(rule.KeyProperty) as string;
            var lastResolved = target.GetValue(rule.LastResolvedProperty) as string;
            if (key is not null && !Localizer.Contains(key))
            {
                ClearTrackedValue(target, rule);
                key = null;
                lastResolved = null;
            }

            if (key is null)
            {
                if (!Localizer.Contains(value))
                {
                    continue;
                }

                key = value;
                target.SetValue(rule.KeyProperty, key);
            }
            else if (!string.Equals(value, lastResolved, StringComparison.Ordinal))
            {
                if (!Localizer.Contains(value))
                {
                    ClearTrackedValue(target, rule);
                    continue;
                }

                key = value;
                target.SetValue(rule.KeyProperty, key);
            }

            var resolved = Localizer.Parse(key);
            target.SetValue(rule.LastResolvedProperty, resolved);
            target.SetValue(property, resolved);
        }
    }

    private static void ClearTrackedValue(DependencyObject target, PropertyRule rule)
    {
        target.ClearValue(rule.KeyProperty);
        target.ClearValue(rule.LastResolvedProperty);
    }

    private static void EnsureDispatcherAccess(DispatcherQueue dispatcherQueue, string operation)
    {
        if (!dispatcherQueue.HasThreadAccess)
        {
            throw new InvalidOperationException(
                $"A WinUI window must be {operation}ed from its owning UI thread."
            );
        }
    }

    private sealed class WindowRegistration(DispatcherQueue dispatcherQueue, string? titleKey)
    {
        public DispatcherQueue DispatcherQueue { get; } = dispatcherQueue;

        public TrackedValue? Title { get; set; } =
            titleKey is null ? null : new TrackedValue(titleKey, titleKey);
    }

    private sealed class TrackedValue(string key, string lastResolved)
    {
        public string Key { get; set; } = key;

        public string LastResolved { get; set; } = lastResolved;
    }

    private sealed class PropertyRule(
        string name,
        DependencyProperty property,
        Func<DependencyObject, bool> appliesTo
    )
    {
        public DependencyProperty Property { get; } = property;

        public Func<DependencyObject, bool> AppliesTo { get; } = appliesTo;

        public DependencyProperty KeyProperty { get; } =
            DependencyProperty.RegisterAttached(
                $"{name}Key",
                typeof(string),
                typeof(WinUILocalizationApplicator),
                new PropertyMetadata(null)
            );

        public DependencyProperty LastResolvedProperty { get; } =
            DependencyProperty.RegisterAttached(
                $"{name}LastResolved",
                typeof(string),
                typeof(WinUILocalizationApplicator),
                new PropertyMetadata(null)
            );
    }
}
