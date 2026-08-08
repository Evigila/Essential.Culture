using System.Collections;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ArkheideSystem.LangKey.Wpf;

/// <summary>
/// Resolves LangKey tokens stored in common WPF display dependency properties and refreshes them
/// when the parser culture changes.
/// </summary>
public sealed class LangKeyWpfApplicator : ILangKeyWpfApplicator, IDisposable
{
    private static readonly Lock ClassHandlerGate = new();
    private static readonly List<WeakReference<LangKeyWpfApplicator>> StartedApplicators = [];
    private static bool isClassHandlerRegistered;

    private readonly ILangKeyResolver resolver;
    private readonly ConditionalWeakTable<
        DependencyObject,
        Dictionary<DependencyProperty, string>
    > resourceKeys = new();
    private readonly Lock gate = new();
    private Dispatcher? dispatcher;
    private bool isStarted;
    private bool isDisposed;

    /// <summary>Creates a WPF applicator backed by a LangKey parser.</summary>
    public LangKeyWpfApplicator(ILangKeyResolver resolver)
    {
        this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

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
                        "The LangKey WPF applicator is already running on another dispatcher."
                    );
                }

                return;
            }

            this.dispatcher = dispatcher;
            isStarted = true;
            resolver.Changed += Parser_Changed;
        }

        RegisterStartedApplicator(this);
    }

    /// <inheritdoc />
    public void Apply(DependencyObject root)
    {
        ArgumentNullException.ThrowIfNull(root);
        if (root.Dispatcher.CheckAccess())
        {
            ApplyCore(root);
        }
        else
        {
            root.Dispatcher.Invoke(() => ApplyCore(root));
        }
    }

    /// <summary>Stops culture-change handling for this applicator.</summary>
    public void Dispose()
    {
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
        }

        StopCore();
    }

    /// <summary>Stops automatic handling without permanently disposing this applicator.</summary>
    internal void Stop() => StopCore();

    private void StopCore()
    {
        var unregister = false;
        lock (gate)
        {
            if (isStarted)
            {
                isStarted = false;
                dispatcher = null;
                resolver.Changed -= Parser_Changed;
                unregister = true;
            }
        }

        if (unregister)
        {
            UnregisterStartedApplicator(this);
        }
    }

    private static void Element_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not DependencyObject dependencyObject)
        {
            return;
        }

        LangKeyWpfApplicator[] applicators;
        lock (ClassHandlerGate)
        {
            StartedApplicators.RemoveAll(reference => !reference.TryGetTarget(out _));
            applicators = StartedApplicators
                .Select(reference =>
                    reference.TryGetTarget(out var applicator) ? applicator : null
                )
                .OfType<LangKeyWpfApplicator>()
                .ToArray();
        }

        foreach (var applicator in applicators)
        {
            applicator.ApplyLoadedObject(dependencyObject);
        }
    }

    private void ApplyLoadedObject(DependencyObject target)
    {
        lock (gate)
        {
            if (!isStarted || isDisposed || dispatcher != target.Dispatcher)
            {
                return;
            }
        }

        ApplyObject(target);
    }

    private static void RegisterStartedApplicator(LangKeyWpfApplicator applicator)
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

            StartedApplicators.RemoveAll(reference => !reference.TryGetTarget(out _));
            StartedApplicators.Add(new WeakReference<LangKeyWpfApplicator>(applicator));
        }
    }

    private static void UnregisterStartedApplicator(LangKeyWpfApplicator applicator)
    {
        lock (ClassHandlerGate)
        {
            StartedApplicators.RemoveAll(reference =>
                !reference.TryGetTarget(out var target) || ReferenceEquals(target, applicator)
            );
        }
    }

    private void Parser_Changed(object? sender, LangKeyChangedEventArgs e)
    {
        var targetDispatcher = dispatcher;
        if (targetDispatcher is null)
        {
            return;
        }

        void Refresh()
        {
            if (Application.Current is not { } application)
            {
                return;
            }

            foreach (Window window in application.Windows)
            {
                ApplyCore(window);
            }
        }

        if (targetDispatcher.CheckAccess())
        {
            Refresh();
        }
        else
        {
            _ = targetDispatcher.BeginInvoke(DispatcherPriority.DataBind, Refresh);
        }
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
            foreach (var child in EnumerateChildren(current))
            {
                pending.Push(child);
            }
        }
    }

    private void ApplyObject(DependencyObject target)
    {
        ApplyLocalValues(target);
        if (target is DataGrid dataGrid)
        {
            foreach (var column in dataGrid.Columns)
            {
                ApplyLocalValues(column);
            }
        }

        if (target is ItemsControl { ItemsSource: IEnumerable items })
        {
            foreach (var item in items.OfType<ILangKeyLocalizable>())
            {
                item.ApplyLocalization(resolver);
            }
        }
    }

    private void ApplyLocalValues(DependencyObject target)
    {
        var entries = target.GetLocalValueEnumerator();
        while (entries.MoveNext())
        {
            var entry = entries.Current;
            if (entry.Value is not string value || !ShouldLocalize(target, entry.Property))
            {
                continue;
            }

            var values = resourceKeys.GetOrCreateValue(target);
            if (!values.TryGetValue(entry.Property, out var key))
            {
                if (!resolver.Contains(value))
                {
                    continue;
                }

                key = value;
                values.Add(entry.Property, key);
            }

            target.SetCurrentValue(entry.Property, resolver.Get(key));
        }

        if (target is DataGridColumn { Header: string header } column)
        {
            var values = resourceKeys.GetOrCreateValue(column);
            if (!values.TryGetValue(DataGridColumn.HeaderProperty, out var key))
            {
                if (!resolver.Contains(header))
                {
                    return;
                }

                key = header;
                values.Add(DataGridColumn.HeaderProperty, key);
            }

            column.SetCurrentValue(DataGridColumn.HeaderProperty, resolver.Get(key));
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

    private static IEnumerable<DependencyObject> EnumerateChildren(DependencyObject parent)
    {
        if (parent is FrameworkElement or FrameworkContentElement)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject dependencyObject)
                {
                    yield return dependencyObject;
                }
            }
        }

        if (parent is not Visual and not System.Windows.Media.Media3D.Visual3D)
        {
            yield break;
        }

        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var index = 0; index < count; index++)
        {
            yield return VisualTreeHelper.GetChild(parent, index);
        }
    }
}
