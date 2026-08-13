using System.Runtime.CompilerServices;
using global::Avalonia;
using global::Avalonia.Automation;
using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Interactivity;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

namespace ArkheideSystem.LangKey.Avalonia;

/// <summary>
/// Resolves LangKey tokens stored in common Avalonia display properties and refreshes them when
/// the parser culture changes.
/// </summary>
public sealed class LangKeyAvaloniaApplicator : ILangKeyAvaloniaApplicator, IDisposable
{
    private static readonly Lock ClassHandlerGate = new();
    private static readonly List<WeakReference<LangKeyAvaloniaApplicator>> StartedApplicators = [];
    private static bool isClassHandlerRegistered;

    private readonly ILangKeyResolver resolver;
    private readonly ConditionalWeakTable<
        AvaloniaObject,
        Dictionary<AvaloniaProperty, TrackedResource>
    > resourceKeys = new();
    private readonly Lock gate = new();
    private Application? application;
    private bool isStarted;
    private bool isDisposed;

    /// <summary>Creates an Avalonia applicator backed by a LangKey resolver.</summary>
    public LangKeyAvaloniaApplicator(ILangKeyResolver resolver)
    {
        this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    /// <inheritdoc />
    public void Start(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            if (isStarted)
            {
                if (!ReferenceEquals(this.application, application))
                {
                    throw new InvalidOperationException(
                        "The LangKey Avalonia applicator is already running for another application."
                    );
                }

                return;
            }

            this.application = application;
            isStarted = true;
            resolver.Changed += Resolver_Changed;
        }

        RegisterStartedApplicator(this);
        RefreshApplicationRoots();
    }

    /// <inheritdoc />
    public void Apply(Visual root)
    {
        ArgumentNullException.ThrowIfNull(root);
        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplyCore(root);
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() => ApplyCore(root)).GetAwaiter().GetResult();
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

    /// <inheritdoc />
    public void Stop() => StopCore();

    private void StopCore()
    {
        var unregister = false;
        lock (gate)
        {
            if (isStarted)
            {
                isStarted = false;
                application = null;
                resolver.Changed -= Resolver_Changed;
                unregister = true;
            }
        }

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

        LangKeyAvaloniaApplicator[] applicators;
        lock (ClassHandlerGate)
        {
            StartedApplicators.RemoveAll(reference => !reference.TryGetTarget(out _));
            applicators = StartedApplicators
                .Select(reference =>
                    reference.TryGetTarget(out var applicator) ? applicator : null
                )
                .OfType<LangKeyAvaloniaApplicator>()
                .ToArray();
        }

        foreach (var applicator in applicators)
        {
            applicator.ApplyLoadedControl(control);
        }
    }

    private void ApplyLoadedControl(Control control)
    {
        lock (gate)
        {
            if (!isStarted || isDisposed)
            {
                return;
            }
        }

        ApplyObject(control);
    }

    private static void RegisterStartedApplicator(LangKeyAvaloniaApplicator applicator)
    {
        lock (ClassHandlerGate)
        {
            if (!isClassHandlerRegistered)
            {
                Control.LoadedEvent.AddClassHandler<Control>(Control_Loaded);
                isClassHandlerRegistered = true;
            }

            StartedApplicators.RemoveAll(reference => !reference.TryGetTarget(out _));
            StartedApplicators.Add(new WeakReference<LangKeyAvaloniaApplicator>(applicator));
        }
    }

    private static void UnregisterStartedApplicator(LangKeyAvaloniaApplicator applicator)
    {
        lock (ClassHandlerGate)
        {
            StartedApplicators.RemoveAll(reference =>
                !reference.TryGetTarget(out var target) || ReferenceEquals(target, applicator)
            );
        }
    }

    private void Resolver_Changed(object? sender, LangKeyChangedEventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            RefreshApplicationRoots();
        }
        else
        {
            Dispatcher.UIThread.Post(RefreshApplicationRoots, DispatcherPriority.Normal);
        }
    }

    private void RefreshApplicationRoots()
    {
        Application? targetApplication;
        lock (gate)
        {
            if (!isStarted || isDisposed)
            {
                return;
            }

            targetApplication = application;
        }

        switch (targetApplication?.ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                foreach (var window in desktop.Windows)
                {
                    ApplyCore(window);
                }

                break;
            case ISingleViewApplicationLifetime singleView when singleView.MainView is { } mainView:
                ApplyCore(mainView);
                break;
        }
    }

    private void ApplyCore(Visual root)
    {
        ApplyObject(root);
        foreach (var descendant in root.GetVisualDescendants())
        {
            ApplyObject(descendant);
        }
    }

    private void ApplyObject(AvaloniaObject target)
    {
        if (target is Window)
        {
            ApplyProperty(target, Window.TitleProperty);
        }

        if (target is TextBlock)
        {
            ApplyProperty(target, TextBlock.TextProperty);
        }

        if (target is ContentControl)
        {
            ApplyProperty(target, ContentControl.ContentProperty);
        }

        if (target is HeaderedContentControl)
        {
            ApplyProperty(target, HeaderedContentControl.HeaderProperty);
        }

        if (target is HeaderedItemsControl)
        {
            ApplyProperty(target, HeaderedItemsControl.HeaderProperty);
        }

        if (target is TextBox)
        {
            ApplyProperty(target, TextBox.PlaceholderTextProperty);
        }

        if (target is StyledElement)
        {
            ApplyProperty(target, ToolTip.TipProperty);
            ApplyProperty(target, AutomationProperties.NameProperty);
        }
    }

    private void ApplyProperty(AvaloniaObject target, AvaloniaProperty property)
    {
        var current = target.GetValue(property);
        var values = resourceKeys.GetOrCreateValue(target);
        string key;
        if (!values.TryGetValue(property, out var tracked))
        {
            if (current is not string token || !resolver.Contains(token))
            {
                return;
            }

            key = token;
        }
        else if (current is string token && resolver.Contains(token))
        {
            key = token;
        }
        else if (Equals(current, tracked.LastApplied))
        {
            key = tracked.Key;
        }
        else
        {
            values.Remove(property);
            return;
        }

        var translation = resolver.Get(key);
        target.SetCurrentValue(property, translation);
        values[property] = new TrackedResource(key, translation);
    }

    private sealed record TrackedResource(string Key, string LastApplied);
}
