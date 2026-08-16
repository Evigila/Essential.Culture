using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Arkheide.Essential.Culture.Wpf;

/// <summary>
/// Resolves Key tokens stored in common WPF display dependency properties and refreshes them
/// when the active culture changes.
/// </summary>
public sealed class WpfLocalizationApplicator : IWpfLocalizationApplicator
{
    private static readonly Lock ClassHandlerGate = new();
    private static WeakReference<WpfLocalizationApplicator>[] startedSnapshot = [];
    private static bool isClassHandlerRegistered;

    private readonly ConditionalWeakTable<DependencyObject, TrackedTarget> trackedLookup = [];
    private readonly List<WeakReference<TrackedTarget>> trackedTargets = [];
    private readonly Lock gate = new();
    private Dispatcher? dispatcher;
    private int runVersion;
    private int refreshPending;
    private bool isStarted;
    private bool isDisposed;

    /// <inheritdoc />
    public void Start(Dispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            if (isStarted)
            {
                if (this.dispatcher != dispatcher)
                {
                    throw new InvalidOperationException(
                        "The WPF localization applicator is already running on another dispatcher."
                    );
                }

                return;
            }

            this.dispatcher = dispatcher;
            isStarted = true;
            runVersion = NextVersion(runVersion);
            Localizer.Current.Changed += Localizer_Changed;
        }

        RegisterStartedApplicator(this);
    }

    /// <inheritdoc />
    public void Apply(DependencyObject root)
    {
        ArgumentNullException.ThrowIfNull(root);
        if (!root.Dispatcher.CheckAccess())
        {
            throw new InvalidOperationException(
                "WPF localization must be applied on the target object's dispatcher thread."
            );
        }

        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            if (isStarted && dispatcher != root.Dispatcher)
            {
                throw new InvalidOperationException(
                    "The target object belongs to a different dispatcher than the running applicator."
                );
            }
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
                dispatcher = null;
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

    private static void Element_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not DependencyObject target)
        {
            return;
        }

        var snapshot = Volatile.Read(ref startedSnapshot);
        for (var index = 0; index < snapshot.Length; index++)
        {
            if (snapshot[index].TryGetTarget(out var applicator))
            {
                applicator.DiscoverLoadedObject(target);
            }
        }
    }

    private void DiscoverLoadedObject(DependencyObject target)
    {
        lock (gate)
        {
            if (!isStarted || isDisposed || dispatcher != target.Dispatcher)
            {
                return;
            }
        }

        DiscoverObject(target);
    }

    private static void RegisterStartedApplicator(WpfLocalizationApplicator applicator)
    {
        lock (ClassHandlerGate)
        {
            if (!isClassHandlerRegistered)
            {
                EventManager.RegisterClassHandler(
                    typeof(FrameworkElement),
                    FrameworkElement.LoadedEvent,
                    new RoutedEventHandler(Element_Loaded),
                    handledEventsToo: true
                );
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

            var next = new WeakReference<WpfLocalizationApplicator>[liveCount + 1];
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

            next[nextIndex] = new WeakReference<WpfLocalizationApplicator>(applicator);
            Volatile.Write(ref startedSnapshot, next);
        }
    }

    private static void UnregisterStartedApplicator(WpfLocalizationApplicator applicator)
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

            var next = new WeakReference<WpfLocalizationApplicator>[liveCount];
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
        Dispatcher? targetDispatcher;
        int version;
        lock (gate)
        {
            if (!isStarted || isDisposed)
            {
                return;
            }

            targetDispatcher = dispatcher;
            version = runVersion;
        }

        if (
            targetDispatcher is null
            || targetDispatcher.HasShutdownStarted
            || targetDispatcher.HasShutdownFinished
            || !TryMarkRefreshPending(version)
        )
        {
            return;
        }

        try
        {
            _ = targetDispatcher.BeginInvoke(
                DispatcherPriority.DataBind,
                () => RefreshPending(version, targetDispatcher)
            );
        }
        catch (InvalidOperationException)
        {
            Interlocked.CompareExchange(ref refreshPending, 0, version);
        }
    }

    private void RefreshPending(int version, Dispatcher targetDispatcher)
    {
        if (Volatile.Read(ref refreshPending) != version)
        {
            return;
        }

        Interlocked.CompareExchange(ref refreshPending, 0, version);
        lock (gate)
        {
            if (
                !isStarted
                || isDisposed
                || runVersion != version
                || dispatcher != targetDispatcher
            )
            {
                return;
            }
        }

        RefreshTracked(targetDispatcher);
    }

    private void DiscoverTree(DependencyObject root)
    {
        var visited = new HashSet<DependencyObject>(ReferenceEqualityComparer.Instance);
        var pending = new Stack<DependencyObject>();
        pending.Push(root);

        while (pending.TryPop(out var current))
        {
            if (!visited.Add(current))
            {
                continue;
            }

            DiscoverObject(current);
            PushChildren(current, pending);
        }
    }

    private void DiscoverObject(DependencyObject target)
    {
        DiscoverLocalValues(target);
        if (target is not DataGrid dataGrid)
        {
            return;
        }

        // DataGridColumn is neither a logical nor a visual child. Its local Header value is
        // discovered here exactly once per column visit; there is no second Header special case.
        for (var index = 0; index < dataGrid.Columns.Count; index++)
        {
            DiscoverLocalValues(dataGrid.Columns[index]);
        }
    }

    private void DiscoverLocalValues(DependencyObject target)
    {
        var entries = target.GetLocalValueEnumerator();
        while (entries.MoveNext())
        {
            var property = entries.Current.Property;
            if (!ShouldLocalize(target, property))
            {
                continue;
            }

            DiscoverProperty(target, property, target.GetValue(property));
        }
    }

    private void DiscoverProperty(
        DependencyObject target,
        DependencyProperty property,
        object? current
    )
    {
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

    private void RefreshTracked(Dispatcher targetDispatcher)
    {
        for (var targetIndex = trackedTargets.Count - 1; targetIndex >= 0; targetIndex--)
        {
            if (!trackedTargets[targetIndex].TryGetTarget(out var trackedTarget))
            {
                trackedTargets.RemoveAt(targetIndex);
                continue;
            }

            var target = trackedTarget.Target;

            if (target.Dispatcher != targetDispatcher)
            {
                continue;
            }

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
        DependencyObject target,
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
        DependencyObject target,
        DependencyProperty property,
        object? current,
        string translation
    )
    {
        if (!string.Equals(current as string, translation, StringComparison.Ordinal))
        {
            target.SetCurrentValue(property, translation);
        }
    }

    private static bool ShouldLocalize(DependencyObject target, DependencyProperty property)
    {
        if (property == AutomationProperties.NameProperty)
        {
            return true;
        }

        if (property.Name is "Title" or "Content" or "Header" or "Placeholder" or "ToolTip")
        {
            return true;
        }

        return property.Name == "Text"
            && target is not System.Windows.Controls.Primitives.TextBoxBase;
    }

    private static void PushChildren(
        DependencyObject parent,
        Stack<DependencyObject> pending
    )
    {
        if (parent is FrameworkElement or FrameworkContentElement)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject dependencyObject)
                {
                    pending.Push(dependencyObject);
                }
            }
        }

        if (parent is not Visual and not System.Windows.Media.Media3D.Visual3D)
        {
            return;
        }

        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var index = 0; index < count; index++)
        {
            pending.Push(VisualTreeHelper.GetChild(parent, index));
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

    private sealed class TrackedTarget(DependencyObject target)
    {
        internal DependencyObject Target { get; } = target;

        internal List<TrackedResource> Resources { get; } = [];

        internal int IndexOf(DependencyProperty property)
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
        DependencyProperty property,
        string key,
        string lastApplied
    )
    {
        internal DependencyProperty Property { get; } = property;

        internal string Key { get; set; } = key;

        internal string LastApplied { get; set; } = lastApplied;
    }
}
