using Microsoft.AspNetCore.Components;

namespace ArkheideSystem.Essential.Culture.Blazor;

/// <summary>Rerenders a component when its scoped language changes and releases its subscription on disposal.</summary>
public abstract class LocalizedComponentBase : ComponentBase, IDisposable
{
    private ILocalizationService? subscribed;
    private volatile bool disposed;

    /// <summary>Gets the scoped localization service. Derived initialization overrides must call base.</summary>
    [Inject] protected ILocalizationService Localization { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        subscribed = Localization;
        subscribed.Changed += HandleChanged;
    }

    private void HandleChanged(object? sender, EventArgs args)
    {
        if (disposed) return;
        var refresh = InvokeAsync(() => { if (!disposed) StateHasChanged(); });
        if (!refresh.IsCompletedSuccessfully) _ = ObserveRefreshAsync(refresh);
    }

    private async Task ObserveRefreshAsync(Task refresh)
    {
        try { await refresh; }
        catch (Exception error)
        {
            if (!disposed) await DispatchExceptionAsync(error);
        }
    }

    /// <summary>Unsubscribes from the same scoped service used during initialization.</summary>
    public virtual void Dispose()
    {
        if (disposed) return;
        disposed = true;
        if (subscribed is not null) subscribed.Changed -= HandleChanged;
        subscribed = null;
        GC.SuppressFinalize(this);
    }
}
