using System.Runtime.CompilerServices;
using global::Avalonia;
using global::Avalonia.Automation;
using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Interactivity;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

namespace Arkheide.Essential.Culture.Avalonia;

/// <summary>
/// Resolves Key tokens stored in common Avalonia display properties and refreshes them when
/// the active culture changes.
/// </summary>
public sealed class AvaloniaLocalizationApplicator : IAvaloniaLocalizationApplicator
{
    private static readonly Lock ClassHandlerGate = new();
    private static WeakReference<AvaloniaLocalizationApplicator>[] startedSnapshot = [];
    private static bool isClassHandlerRegistered;

    private readonly ConditionalWeakTable<AvaloniaObject, TrackedTarget> trackedLookup = [];
    private readonly List<WeakReference<TrackedTarget>> trackedTargets = [];
    private readonly Lock gate = new();
    private Application? application;
    private int runVersion;
    private int refreshPending;
    private bool isStarted;
    private bool isDisposed;

    /// <inheritdoc />
    public void Start(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        if (!Dispatcher.UIThread.CheckAccess())
        {
            throw new InvalidOperationException(
                "Avalonia localization must be started on the UI thread."
            );
        }

        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            if (isStarted)
            {
                if (!ReferenceEquals(this.application, application))
                {
                    throw new InvalidOperationException(
                        "The Avalonia localization applicator is already running for another application."
                    );
                }

                return;
            }

            this.application = application;
            isStarted = true;
            runVersion = NextVersion(runVersion);
            Localizer.Current.Changed += Localizer_Changed;
        }

        RegisterStartedApplicator(this);
        DiscoverApplicationRoots(application);
    }

    /// <inheritdoc />
    public void Apply(Visual root)
    {
        ArgumentNullException.ThrowIfNull(root);
        if (!Dispatcher.UIThread.CheckAccess())
        {
            throw new InvalidOperationException(
                "Avalonia localization must be applied on the UI thread."
            );
        }

        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
        }

        DiscoverTree(root);
    }

    /// <inheritdoc />
    public void Stop() => StopCore(dispose: false);

    /// <inheritdoc />
    public void Dispose() => StopCore(dispose: true);

    private void StopCore(bool dispose)
    {
        var unregister = false;
        lock (gate)
        {
            if (dispose)
            {
                if (isDisposed)
                {
                    return;
                }

                isDisposed = true;
            }

            if (isStarted)
            {
                isStarted = false;
                application = null;
                runVersion = NextVersion(runVersion);
                Localizer.Current.Changed -= Localizer_Changed;
                unregister = true;
            }
        }

        Interlocked.Exchange(ref refreshPending, 0);
        if (unregister)
        {
            UnregisterStartedApplicator(this);
        }
    }

    private static void Control_Loaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        var snapshot = Volatile.Read(ref startedSnapshot);
        for (var index = 0; index < snapshot.Length; index++)
        {
            if (snapshot[index].TryGetTarget(out var applicator))
            {
                applicator.DiscoverLoadedControl(control);
            }
        }
    }

    private void DiscoverLoadedControl(Control control)
    {
        lock (gate)
        {
            if (!isStarted || isDisposed)
            {
                return;
            }
        }

        DiscoverObject(control);
    }

    private static void RegisterStartedApplicator(AvaloniaLocalizationApplicator applicator)
    {
        lock (ClassHandlerGate)
        {
            if (!isClassHandlerRegistered)
            {
                Control.LoadedEvent.AddClassHandler<Control>(Control_Loaded);
                isClassHandlerRegistered = true;
            }

            var current = startedSnapshot;
            var liveCount = 0;
            for (var index = 0; index < current.Length; index++)
            {
                if (
                    current[index].TryGetTarget(out var target)
                    && !ReferenceEquals(target, applicator)
                )
                {
                    liveCount++;
                }
            }

            var next = new WeakReference<AvaloniaLocalizationApplicator>[liveCount + 1];
            var nextIndex = 0;
            for (var index = 0; index < current.Length; index++)
            {
                if (
                    current[index].TryGetTarget(out var target)
                    && !ReferenceEquals(target, applicator)
                )
                {
                    next[nextIndex++] = current[index];
                }
            }

            next[nextIndex] = new WeakReference<AvaloniaLocalizationApplicator>(applicator);
            Volatile.Write(ref startedSnapshot, next);
        }
    }

    private static void UnregisterStartedApplicator(AvaloniaLocalizationApplicator applicator)
    {
        lock (ClassHandlerGate)
        {
            var current = startedSnapshot;
            var liveCount = 0;
            for (var index = 0; index < current.Length; index++)
            {
                if (
                    current[index].TryGetTarget(out var target)
                    && !ReferenceEquals(target, applicator)
                )
                {
                    liveCount++;
                }
            }

            if (liveCount == current.Length)
            {
                return;
            }

            var next = new WeakReference<AvaloniaLocalizationApplicator>[liveCount];
            var nextIndex = 0;
            for (var index = 0; index < current.Length; index++)
            {
                if (
                    current[index].TryGetTarget(out var target)
                    && !ReferenceEquals(target, applicator)
                )
                {
                    next[nextIndex++] = current[index];
                }
            }

            Volatile.Write(ref startedSnapshot, next);
        }
    }

    private void Localizer_Changed(object? sender, EventArgs e)
    {
        int version;
        lock (gate)
        {
            if (!isStarted || isDisposed)
            {
                return;
            }

            version = runVersion;
        }

        if (!TryMarkRefreshPending(version))
        {
            return;
        }

        Dispatcher.UIThread.Post(
            () => RefreshPending(version),
            DispatcherPriority.Normal
        );
    }

    private void RefreshPending(int version)
    {
        if (Volatile.Read(ref refreshPending) != version)
        {
            return;
        }

        Interlocked.CompareExchange(ref refreshPending, 0, version);
        lock (gate)
        {
            if (!isStarted || isDisposed || runVersion != version)
            {
                return;
            }
        }

        RefreshTracked();
    }

    private void DiscoverApplicationRoots(Application targetApplication)
    {
        switch (targetApplication.ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                for (var index = 0; index < desktop.Windows.Count; index++)
                {
                    DiscoverTree(desktop.Windows[index]);
                }

                break;
            case ISingleViewApplicationLifetime singleView when singleView.MainView is { } mainView:
                DiscoverTree(mainView);
                break;
        }
    }

    private void DiscoverTree(Visual root)
    {
        DiscoverObject(root);
        foreach (var descendant in root.GetVisualDescendants())
        {
            DiscoverObject(descendant);
        }
    }

    private void DiscoverObject(AvaloniaObject target)
    {
        if (target is Window)
        {
            DiscoverProperty(target, Window.TitleProperty);
        }

        if (target is TextBlock)
        {
            DiscoverProperty(target, TextBlock.TextProperty);
        }

        if (target is ContentControl)
        {
            DiscoverProperty(target, ContentControl.ContentProperty);
        }

        if (target is HeaderedContentControl)
        {
            DiscoverProperty(target, HeaderedContentControl.HeaderProperty);
        }

        if (target is HeaderedItemsControl)
        {
            DiscoverProperty(target, HeaderedItemsControl.HeaderProperty);
        }

        if (target is TextBox)
        {
            DiscoverProperty(target, TextBox.PlaceholderTextProperty);
        }

        if (target is StyledElement)
        {
            DiscoverProperty(target, ToolTip.TipProperty);
            DiscoverProperty(target, AutomationProperties.NameProperty);
        }
    }

    private void DiscoverProperty(AvaloniaObject target, AvaloniaProperty property)
    {
        var current = target.GetValue(property);
        if (trackedLookup.TryGetValue(target, out var trackedTarget))
        {
            var index = trackedTarget.IndexOf(property);
            if (index >= 0)
            {
                var tracked = trackedTarget.Resources[index];
                if (Equals(current, tracked.LastApplied))
                {
                    if (Localizer.TryParse(tracked.Key, out var translation))
                    {
                        SetTranslation(target, property, current, translation);
                        tracked.LastApplied = translation;
                    }
                    else
                    {
                        RemoveTrackedResource(target, trackedTarget, index);
                    }
                }
                else if (TryTranslate(current, out var key, out var translation))
                {
                    tracked.Key = key;
                    SetTranslation(target, property, current, translation);
                    tracked.LastApplied = translation;
                }
                else
                {
                    RemoveTrackedResource(target, trackedTarget, index);
                }

                return;
            }
        }

        if (!TryTranslate(current, out var newKey, out var newTranslation))
        {
            return;
        }

        if (trackedTarget is null)
        {
            trackedTarget = new TrackedTarget(target);
            trackedLookup.Add(target, trackedTarget);
            trackedTargets.Add(new WeakReference<TrackedTarget>(trackedTarget));
        }

        trackedTarget.Resources.Add(new TrackedResource(property, newKey, newTranslation));
        SetTranslation(target, property, current, newTranslation);
    }

    private void RefreshTracked()
    {
        for (var targetIndex = trackedTargets.Count - 1; targetIndex >= 0; targetIndex--)
        {
            if (!trackedTargets[targetIndex].TryGetTarget(out var trackedTarget))
            {
                trackedTargets.RemoveAt(targetIndex);
                continue;
            }

            var target = trackedTarget.Target;

            for (
                var resourceIndex = trackedTarget.Resources.Count - 1;
                resourceIndex >= 0;
                resourceIndex--
            )
            {
                var tracked = trackedTarget.Resources[resourceIndex];
                var current = target.GetValue(tracked.Property);
                string translation;
                if (Equals(current, tracked.LastApplied))
                {
                    if (!Localizer.TryParse(tracked.Key, out translation))
                    {
                        trackedTarget.Resources.RemoveAt(resourceIndex);
                        continue;
                    }
                }
                else if (TryTranslate(current, out var key, out translation))
                {
                    tracked.Key = key;
                }
                else
                {
                    trackedTarget.Resources.RemoveAt(resourceIndex);
                    continue;
                }

                SetTranslation(target, tracked.Property, current, translation);
                tracked.LastApplied = translation;
            }

            if (trackedTarget.Resources.Count == 0)
            {
                trackedLookup.Remove(target);
                trackedTargets.RemoveAt(targetIndex);
            }
        }
    }

    private void RemoveTrackedResource(
        AvaloniaObject target,
        TrackedTarget trackedTarget,
        int resourceIndex
    )
    {
        trackedTarget.Resources.RemoveAt(resourceIndex);
        if (trackedTarget.Resources.Count != 0)
        {
            return;
        }

        trackedLookup.Remove(target);
        RemoveTrackedTargetFromRegistry(trackedTarget);
    }

    private void RemoveTrackedTargetFromRegistry(TrackedTarget trackedTarget)
    {
        for (var index = trackedTargets.Count - 1; index >= 0; index--)
        {
            if (
                !trackedTargets[index].TryGetTarget(out var candidate)
                || ReferenceEquals(candidate, trackedTarget)
            )
            {
                trackedTargets.RemoveAt(index);
            }
        }
    }

    private static bool TryTranslate(
        object? value,
        out string key,
        out string translation
    )
    {
        if (
            value is string token
            && token.StartsWith(KeyToken.Prefix, StringComparison.Ordinal)
            && Localizer.TryParse(token, out translation)
        )
        {
            key = token;
            return true;
        }

        key = string.Empty;
        translation = string.Empty;
        return false;
    }

    private static void SetTranslation(
        AvaloniaObject target,
        AvaloniaProperty property,
        object? current,
        string translation
    )
    {
        if (!string.Equals(current as string, translation, StringComparison.Ordinal))
        {
            target.SetCurrentValue(property, translation);
        }
    }

    private static int NextVersion(int current) => current == int.MaxValue ? 1 : current + 1;

    private bool TryMarkRefreshPending(int version)
    {
        while (true)
        {
            var pending = Volatile.Read(ref refreshPending);
            if (pending == version)
            {
                return false;
            }

            if (Interlocked.CompareExchange(ref refreshPending, version, pending) == pending)
            {
                return true;
            }
        }
    }

    private sealed class TrackedTarget(AvaloniaObject target)
    {
        internal AvaloniaObject Target { get; } = target;

        internal List<TrackedResource> Resources { get; } = [];

        internal int IndexOf(AvaloniaProperty property)
        {
            for (var index = 0; index < Resources.Count; index++)
            {
                if (Resources[index].Property == property)
                {
                    return index;
                }
            }

            return -1;
        }
    }

    private sealed class TrackedResource(
        AvaloniaProperty property,
        string key,
        string lastApplied
    )
    {
        internal AvaloniaProperty Property { get; } = property;

        internal string Key { get; set; } = key;

        internal string LastApplied { get; set; } = lastApplied;
    }
}
