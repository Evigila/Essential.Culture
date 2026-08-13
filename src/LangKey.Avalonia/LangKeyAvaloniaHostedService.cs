using global::Avalonia;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.LangKey.Avalonia;

internal sealed class LangKeyAvaloniaHostedService<TApplication>(
    TApplication application,
    LangKeyAvaloniaApplicator applicator
) : IHostedService, IDisposable
    where TApplication : Application
{
    private readonly Lock gate = new();
    private bool isStarted;
    private bool isDisposed;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            if (isStarted)
            {
                return Task.CompletedTask;
            }

            applicator.Start(application);
            isStarted = true;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        lock (gate)
        {
            if (!isStarted)
            {
                return Task.CompletedTask;
            }

            applicator.Stop();
            isStarted = false;
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            if (!isStarted)
            {
                return;
            }

            applicator.Stop();
            isStarted = false;
        }
    }
}
