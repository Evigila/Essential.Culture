using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ArkheideSystem.Essential.Culture.WinUI;

/// <summary>
/// Resolves values produced by the generated Localize markup extension in common WinUI display
/// dependency properties and refreshes attached windows when the active culture changes.
/// </summary>
public sealed class WinUILocalizationHost : IWinUILocalizationHost
{
    private static readonly PropertyRule TextBlockText = new(
        "TextBlockText",
        TextBlock.TextProperty
    );
    private static readonly PropertyRule ContentControlContent = new(
        "ContentControlContent",
        ContentControl.ContentProperty
    );
    private static readonly PropertyRule ContentDialogTitle = new(
        "ContentDialogTitle",
        ContentDialog.TitleProperty
    );
    private static readonly PropertyRule ContentDialogPrimaryButtonText = new(
        "ContentDialogPrimaryButtonText",
        ContentDialog.PrimaryButtonTextProperty
    );
    private static readonly PropertyRule ContentDialogSecondaryButtonText = new(
        "ContentDialogSecondaryButtonText",
        ContentDialog.SecondaryButtonTextProperty
    );
    private static readonly PropertyRule ContentDialogCloseButtonText = new(
        "ContentDialogCloseButtonText",
        ContentDialog.CloseButtonTextProperty
    );
    private static readonly PropertyRule TextBoxPlaceholder = new(
        "TextBoxPlaceholder",
        TextBox.PlaceholderTextProperty
    );
    private static readonly PropertyRule PasswordBoxPlaceholder = new(
        "PasswordBoxPlaceholder",
        PasswordBox.PlaceholderTextProperty
    );
    private static readonly PropertyRule RichEditBoxPlaceholder = new(
        "RichEditBoxPlaceholder",
        RichEditBox.PlaceholderTextProperty
    );
    private static readonly PropertyRule AutoSuggestBoxPlaceholder = new(
        "AutoSuggestBoxPlaceholder",
        AutoSuggestBox.PlaceholderTextProperty
    );
    private static readonly PropertyRule ToolTip = new(
        "ToolTip",
        ToolTipService.ToolTipProperty
    );
    private static readonly PropertyRule AutomationName = new(
        "AutomationName",
        AutomationProperties.NameProperty
    );

    private readonly Lock gate = new();
    private readonly Dictionary<Window, WindowRegistration> windows = new(
        ReferenceEqualityComparer.Instance
    );
    private bool isChangedSubscribed;
    private bool isDisposed;

    /// <summary>Creates a WinUI localization lifecycle host.</summary>
    public WinUILocalizationHost() { }

    /// <inheritdoc />
    public void Attach(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        EnsureDispatcherAccess(window.DispatcherQueue, "attach");

        WindowRegistration registration;
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            if (windows.ContainsKey(window))
            {
                return;
            }

            registration = new WindowRegistration(this, window);
            windows.Add(window, registration);
            registration.SubscribeEvents();
            if (!isChangedSubscribed)
            {
                Localizer.Current.Changed += Localizer_Changed;
                isChangedSubscribed = true;
            }
        }

        try
        {
            DiscoverWindow(registration);
        }
        catch
        {
            DetachCore(registration);
            throw;
        }
    }

    /// <inheritdoc />
    public void Detach(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        EnsureDispatcherAccess(window.DispatcherQueue, "detach");

        WindowRegistration? registration;
        lock (gate)
        {
            windows.TryGetValue(window, out registration);
        }

        if (registration is not null)
        {
            DetachCore(registration);
        }
    }

    /// <inheritdoc />
    public void Apply(DependencyObject root)
    {
        ArgumentNullException.ThrowIfNull(root);
        EnsureDispatcherAccess(root.DispatcherQueue, "apply localization to");

        WindowRegistration? registration = null;
        WindowRegistration? dispatcherFallback = null;
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            foreach (var candidate in windows.Values)
            {
                if (!ReferenceEquals(candidate.DispatcherQueue, root.DispatcherQueue))
                {
                    continue;
                }

                dispatcherFallback ??= candidate;
                if (
                    root is FrameworkElement { XamlRoot: { } xamlRoot }
                    && candidate.Window.Content is FrameworkElement
                    {
                        XamlRoot: { } candidateXamlRoot,
                    }
                    && ReferenceEquals(xamlRoot, candidateXamlRoot)
                )
                {
                    registration = candidate;
                    break;
                }
            }

            registration ??= dispatcherFallback;
        }

        var strongTracking = registration is not null
            && ReferenceEquals(registration.Window.Content, root);
        DiscoverTree(root, registration, strongTracking);
    }

    /// <summary>Stops all automatic window tracking and culture-change handling.</summary>
    public void Dispose()
    {
        WindowRegistration[] registrations;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            if (isChangedSubscribed)
            {
                Localizer.Current.Changed -= Localizer_Changed;
                isChangedSubscribed = false;
            }

            registrations = [.. windows.Values];
            windows.Clear();
            foreach (var registration in registrations)
            {
                registration.RefreshQueued = false;
            }
        }

        foreach (var registration in registrations)
        {
            if (registration.DispatcherQueue.HasThreadAccess)
            {
                registration.UnsubscribeEvents();
                registration.ReleaseTrackingOwnership();
            }
            else
            {
                if (
                    !registration.DispatcherQueue.TryEnqueue(() =>
                    {
                        registration.UnsubscribeEvents();
                        registration.ReleaseTrackingOwnership();
                    })
                )
                {
                    registration.ReleaseTrackingOwnership();
                }
            }
        }
    }

    private void Window_Activated(
        WindowRegistration registration,
        WindowActivatedEventArgs args
    )
    {
        if (args.WindowActivationState == WindowActivationState.Deactivated)
        {
            return;
        }

        lock (gate)
        {
            if (!IsAttached(registration) || !registration.InitialActivationPending)
            {
                return;
            }

            registration.InitialActivationPending = false;
        }

        registration.UnsubscribeActivated();
        DiscoverWindowIfAttached(registration);
    }

    private void Window_Closed(WindowRegistration registration) => DetachCore(registration);

    private void Localizer_Changed(object? sender, EventArgs args)
    {
        List<WindowRegistration> registrations;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            registrations = new List<WindowRegistration>(windows.Count);
            foreach (var registration in windows.Values)
            {
                if (registration.RefreshQueued)
                {
                    continue;
                }

                registration.RefreshQueued = true;
                registrations.Add(registration);
            }
        }

        foreach (var registration in registrations)
        {
            if (
                !registration.DispatcherQueue.TryEnqueue(
                    DispatcherQueuePriority.Normal,
                    () => RefreshQueuedWindow(registration)
                )
            )
            {
                lock (gate)
                {
                    registration.RefreshQueued = false;
                }
            }
        }
    }

    private void RefreshQueuedWindow(WindowRegistration registration)
    {
        lock (gate)
        {
            registration.RefreshQueued = false;
            if (!IsAttached(registration))
            {
                return;
            }
        }

        registration.RefreshTrackedValues();
    }

    private void DiscoverWindowIfAttached(WindowRegistration registration)
    {
        lock (gate)
        {
            if (!IsAttached(registration))
            {
                return;
            }
        }

        DiscoverWindow(registration);
    }

    private static void DiscoverWindow(WindowRegistration registration)
    {
        registration.DiscoverTitle();
        if (registration.Window.Content is DependencyObject root)
        {
            DiscoverTree(root, registration, strongTracking: true);
        }
    }

    private void DetachCore(WindowRegistration registration)
    {
        lock (gate)
        {
            if (
                !windows.TryGetValue(registration.Window, out var current)
                || !ReferenceEquals(current, registration)
            )
            {
                return;
            }

            windows.Remove(registration.Window);
            registration.RefreshQueued = false;
            if (windows.Count == 0 && isChangedSubscribed)
            {
                Localizer.Current.Changed -= Localizer_Changed;
                isChangedSubscribed = false;
            }
        }

        registration.UnsubscribeEvents();
        registration.ReleaseTrackingOwnership();
    }

    private bool IsAttached(WindowRegistration registration) =>
        !isDisposed
        && windows.TryGetValue(registration.Window, out var current)
        && ReferenceEquals(current, registration);

    private static void DiscoverTree(
        DependencyObject root,
        WindowRegistration? registration,
        bool strongTracking
    )
    {
        var pending = new Stack<DependencyObject>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            DiscoverObject(current, registration, strongTracking);
            var childCount = VisualTreeHelper.GetChildrenCount(current);
            for (var index = 0; index < childCount; index++)
            {
                pending.Push(VisualTreeHelper.GetChild(current, index));
            }
        }
    }

    private static void DiscoverObject(
        DependencyObject target,
        WindowRegistration? registration,
        bool strongTracking
    )
    {
        if (target is TextBlock)
        {
            DiscoverProperty(target, TextBlockText, registration, strongTracking);
        }

        if (target is ContentControl)
        {
            DiscoverProperty(target, ContentControlContent, registration, strongTracking);
        }

        if (target is ContentDialog)
        {
            DiscoverProperty(target, ContentDialogTitle, registration, strongTracking);
            DiscoverProperty(target, ContentDialogPrimaryButtonText, registration, strongTracking);
            DiscoverProperty(
                target,
                ContentDialogSecondaryButtonText,
                registration,
                strongTracking
            );
            DiscoverProperty(target, ContentDialogCloseButtonText, registration, strongTracking);
        }

        if (target is TextBox)
        {
            DiscoverProperty(target, TextBoxPlaceholder, registration, strongTracking);
        }

        if (target is PasswordBox)
        {
            DiscoverProperty(target, PasswordBoxPlaceholder, registration, strongTracking);
        }

        if (target is RichEditBox)
        {
            DiscoverProperty(target, RichEditBoxPlaceholder, registration, strongTracking);
        }

        if (target is AutoSuggestBox)
        {
            DiscoverProperty(target, AutoSuggestBoxPlaceholder, registration, strongTracking);
        }

        DiscoverProperty(target, ToolTip, registration, strongTracking);
        DiscoverProperty(target, AutomationName, registration, strongTracking);
    }

    private static void DiscoverProperty(
        DependencyObject target,
        PropertyRule rule,
        WindowRegistration? registration,
        bool strongTracking
    )
    {
        var localValue = target.ReadLocalValue(rule.Property);
        var tracked = target.ReadLocalValue(rule.TrackingProperty) as TrackedProperty;
        if (localValue is not string value)
        {
            tracked?.StopTracking(target, rule);
            return;
        }

        if (tracked is null)
        {
            if (!TryResolveMarker(target, value, out var resolved))
            {
                return;
            }

            tracked = new TrackedProperty(value, resolved);
            target.SetValue(rule.TrackingProperty, tracked);
            Track(registration, target, rule, tracked, strongTracking);
            tracked.ApplyResolved(target, rule, resolved);
            return;
        }

        Track(registration, target, rule, tracked, strongTracking);
        if (!tracked.Refresh(target, rule, value))
        {
            tracked.StopTracking(target, rule);
        }
    }

    private static void Track(
        WindowRegistration? registration,
        DependencyObject target,
        PropertyRule rule,
        TrackedProperty tracked,
        bool strongTracking
    )
    {
        if (registration is null)
        {
            return;
        }

        if (strongTracking)
        {
            registration.TrackStrong(target, rule, tracked);
        }
        else
        {
            registration.TrackWeak(target, rule, tracked);
        }
    }

    internal static void RefreshArguments(DependencyObject target)
    {
        if (target is TextBlock)
        {
            RefreshTrackedProperty(target, TextBlockText);
        }

        if (target is ContentControl)
        {
            RefreshTrackedProperty(target, ContentControlContent);
        }

        if (target is ContentDialog)
        {
            RefreshTrackedProperty(target, ContentDialogTitle);
            RefreshTrackedProperty(target, ContentDialogPrimaryButtonText);
            RefreshTrackedProperty(target, ContentDialogSecondaryButtonText);
            RefreshTrackedProperty(target, ContentDialogCloseButtonText);
        }

        if (target is TextBox)
        {
            RefreshTrackedProperty(target, TextBoxPlaceholder);
        }

        if (target is PasswordBox)
        {
            RefreshTrackedProperty(target, PasswordBoxPlaceholder);
        }

        if (target is RichEditBox)
        {
            RefreshTrackedProperty(target, RichEditBoxPlaceholder);
        }

        if (target is AutoSuggestBox)
        {
            RefreshTrackedProperty(target, AutoSuggestBoxPlaceholder);
        }

        RefreshTrackedProperty(target, ToolTip);
        RefreshTrackedProperty(target, AutomationName);
    }

    private static void RefreshTrackedProperty(DependencyObject target, PropertyRule rule)
    {
        if (target.ReadLocalValue(rule.TrackingProperty) is not TrackedProperty tracked)
        {
            return;
        }

        if (!tracked.Refresh(target, rule))
        {
            tracked.StopTracking(target, rule);
        }
    }

    private static bool TryResolveMarker(
        DependencyObject target,
        string value,
        out string resolved
    )
    {
        if (!WinUILocalizationMarker.TryExtract(value, out var token))
        {
            resolved = value;
            return false;
        }

        var arguments = WinUILocalizeExtensionBase.GetCurrentArguments(target);
        return arguments.Length == 0
            ? Localizer.TryParse(token, out resolved)
            : Localizer.TryParse(token, arguments, out resolved);
    }

    private static bool TryResolveMarker(string value, out string resolved)
    {
        if (!WinUILocalizationMarker.TryExtract(value, out var token))
        {
            resolved = value;
            return false;
        }

        return Localizer.TryParse(token, out resolved);
    }

    private static void EnsureDispatcherAccess(DispatcherQueue dispatcherQueue, string operation)
    {
        if (!dispatcherQueue.HasThreadAccess)
        {
            throw new InvalidOperationException(
                $"A WinUI object must be accessed from its owning UI thread to {operation}."
            );
        }
    }

    private sealed class WindowRegistration(
        WinUILocalizationHost owner,
        Window window
    )
    {
        private readonly List<StrongTrackedProperty> strongProperties = [];
        private readonly List<WeakTrackedProperty> weakProperties = [];
        private bool isActivatedSubscribed;
        private bool isClosedSubscribed;
        private TrackedValue? title;

        public Window Window { get; } = window;

        public DispatcherQueue DispatcherQueue { get; } = window.DispatcherQueue;

        public bool InitialActivationPending { get; set; } = true;

        public bool RefreshQueued { get; set; }

        public void SubscribeEvents()
        {
            Window.Activated += Window_Activated;
            Window.Closed += Window_Closed;
            isActivatedSubscribed = true;
            isClosedSubscribed = true;
        }

        public void UnsubscribeActivated()
        {
            if (!isActivatedSubscribed)
            {
                return;
            }

            Window.Activated -= Window_Activated;
            isActivatedSubscribed = false;
        }

        public void UnsubscribeEvents()
        {
            UnsubscribeActivated();
            if (!isClosedSubscribed)
            {
                return;
            }

            Window.Closed -= Window_Closed;
            isClosedSubscribed = false;
        }

        public void DiscoverTitle() => RefreshTitle(allowDiscovery: true);

        public void TrackStrong(
            DependencyObject target,
            PropertyRule rule,
            TrackedProperty tracked
        )
        {
            if (ReferenceEquals(tracked.StrongOwner, this))
            {
                return;
            }

            tracked.StrongOwner?.RemoveStrong(tracked);
            tracked.StrongOwner = this;
            tracked.WeakOwner = null;
            strongProperties.Add(new StrongTrackedProperty(target, rule, tracked));
        }

        public void TrackWeak(
            DependencyObject target,
            PropertyRule rule,
            TrackedProperty tracked
        )
        {
            if (tracked.StrongOwner is not null || ReferenceEquals(tracked.WeakOwner, this))
            {
                return;
            }

            tracked.WeakOwner = this;
            for (var index = weakProperties.Count - 1; index >= 0; index--)
            {
                if (!weakProperties[index].Target.TryGetTarget(out _))
                {
                    weakProperties.RemoveAt(index);
                }
            }

            weakProperties.Add(
                new WeakTrackedProperty(new WeakReference<DependencyObject>(target), rule, tracked)
            );
        }

        public void RefreshTrackedValues()
        {
            RefreshTitle(allowDiscovery: false);
            for (var index = strongProperties.Count - 1; index >= 0; index--)
            {
                var entry = strongProperties[index];
                if (entry.State.Refresh(entry.Target, entry.Rule))
                {
                    continue;
                }

                entry.State.ClearAttachedValue(entry.Target, entry.Rule);
                entry.State.StrongOwner = null;
                strongProperties.RemoveAt(index);
            }

            for (var index = weakProperties.Count - 1; index >= 0; index--)
            {
                var entry = weakProperties[index];
                if (
                    !entry.Target.TryGetTarget(out var target)
                    || !ReferenceEquals(entry.State.WeakOwner, this)
                )
                {
                    weakProperties.RemoveAt(index);
                    continue;
                }

                if (entry.State.Refresh(target, entry.Rule))
                {
                    continue;
                }

                entry.State.ClearAttachedValue(target, entry.Rule);
                entry.State.WeakOwner = null;
                weakProperties.RemoveAt(index);
            }
        }

        public void RemoveStrong(TrackedProperty tracked)
        {
            strongProperties.RemoveAll(entry => ReferenceEquals(entry.State, tracked));
            if (ReferenceEquals(tracked.StrongOwner, this))
            {
                tracked.StrongOwner = null;
            }
        }

        public void ReleaseTrackingOwnership()
        {
            foreach (var entry in strongProperties)
            {
                if (ReferenceEquals(entry.State.StrongOwner, this))
                {
                    entry.State.StrongOwner = null;
                }
            }

            foreach (var entry in weakProperties)
            {
                if (ReferenceEquals(entry.State.WeakOwner, this))
                {
                    entry.State.WeakOwner = null;
                }
            }

            strongProperties.Clear();
            weakProperties.Clear();
            title = null;
        }

        private void RefreshTitle(bool allowDiscovery)
        {
            var value = Window.Title;
            if (title is null)
            {
                if (!allowDiscovery || !TryResolveMarker(value, out var discovered))
                {
                    return;
                }

                title = new TrackedValue(value, discovered);
                Window.Title = discovered;
                return;
            }

            string resolved;
            if (!string.Equals(value, title.LastResolved, StringComparison.Ordinal))
            {
                if (!TryResolveMarker(value, out resolved))
                {
                    title = null;
                    return;
                }

                title.Marker = value;
            }
            else if (!TryResolveMarker(title.Marker, out resolved))
            {
                title = null;
                return;
            }

            title.LastResolved = resolved;
            Window.Title = resolved;
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args) =>
            owner.Window_Activated(this, args);

        private void Window_Closed(object sender, WindowEventArgs args) => owner.Window_Closed(this);

        private sealed record StrongTrackedProperty(
            DependencyObject Target,
            PropertyRule Rule,
            TrackedProperty State
        );

        private sealed record WeakTrackedProperty(
            WeakReference<DependencyObject> Target,
            PropertyRule Rule,
            TrackedProperty State
        );
    }

    private sealed class TrackedProperty(string marker, string lastResolved)
    {
        public WindowRegistration? StrongOwner { get; set; }

        public WindowRegistration? WeakOwner { get; set; }

        private string Marker { get; set; } = marker;

        private string LastResolved { get; set; } = lastResolved;

        public bool Refresh(DependencyObject target, PropertyRule rule)
        {
            var value = target.ReadLocalValue(rule.Property) as string;
            return value is not null && Refresh(target, rule, value);
        }

        public bool Refresh(DependencyObject target, PropertyRule rule, string value)
        {
            string resolved;
            if (!string.Equals(value, LastResolved, StringComparison.Ordinal))
            {
                if (!TryResolveMarker(target, value, out resolved))
                {
                    return false;
                }

                Marker = value;
            }
            else if (!TryResolveMarker(target, Marker, out resolved))
            {
                return false;
            }

            ApplyResolved(target, rule, resolved);
            return true;
        }

        public void ApplyResolved(DependencyObject target, PropertyRule rule, string resolved)
        {
            LastResolved = resolved;
            target.SetValue(rule.Property, resolved);
        }

        public void StopTracking(DependencyObject target, PropertyRule rule)
        {
            StrongOwner?.RemoveStrong(this);
            StrongOwner = null;
            WeakOwner = null;
            ClearAttachedValue(target, rule);
        }

        public void ClearAttachedValue(DependencyObject target, PropertyRule rule)
        {
            if (ReferenceEquals(target.ReadLocalValue(rule.TrackingProperty), this))
            {
                target.ClearValue(rule.TrackingProperty);
            }
        }
    }

    private sealed class TrackedValue(string marker, string lastResolved)
    {
        public string Marker { get; set; } = marker;

        public string LastResolved { get; set; } = lastResolved;
    }

    private sealed class PropertyRule(string name, DependencyProperty property)
    {
        public DependencyProperty Property { get; } = property;

        public DependencyProperty TrackingProperty { get; } =
            DependencyProperty.RegisterAttached(
                $"{name}Tracking",
                typeof(object),
                typeof(WinUILocalizationHost),
                new PropertyMetadata(null)
            );
    }
}
