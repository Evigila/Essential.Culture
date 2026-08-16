using System.Runtime.ExceptionServices;
using System.Collections;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using Arkheide.Essential.Culture.Wpf;

namespace Arkheide.Essential.Culture.Test;

public sealed class KeyWpfTests
{
    [Fact]
    public void Apply_LocalizesSupportedDisplayProperties()
    {
        Exception? failure = null;
        string? localizedText = null;
        var thread = new Thread(() =>
        {
            try
            {
                Localizer.Current.SetCulture("en-US");
                using var applicator = new WpfLocalizationApplicator();
                var text = new TextBlock { Text = "Key.Title_Hello" };
                applicator.Apply(text);
                localizedText = text.Text;
            }
            catch (Exception error)
            {
                failure = error;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }

        Assert.Equal("Hello World!", localizedText);
    }

    [Fact]
    public void CultureChange_RefreshesOnlyOwnedTrackedValue()
    {
        RunSta(() =>
        {
            Localizer.Current.SetCulture("en-US");
            using var applicator = new WpfLocalizationApplicator();
            applicator.Start(Dispatcher.CurrentDispatcher);
            var translated = new TextBlock { Text = "Key.Title_Hello" };
            var applicationOwned = new TextBlock { Text = "Key.Title_Hello" };
            applicator.Apply(translated);
            applicator.Apply(applicationOwned);
            applicationOwned.Text = "Application-owned value";

            Localizer.Current.SetCulture("zh-CN");
            DrainDispatcher();

            Assert.Equal("你好 世界！", translated.Text);
            Assert.Equal("Application-owned value", applicationOwned.Text);
        });
    }

    [Fact]
    public void Apply_DoesNotTrackOrdinaryText()
    {
        RunSta(() =>
        {
            using var applicator = new WpfLocalizationApplicator();
            var rawKey = new TextBlock { Text = "Title_Hello" };
            applicator.Apply(rawKey);
            for (var index = 0; index < 100; index++)
            {
                applicator.Apply(new TextBlock { Text = $"Ordinary text {index}" });
            }

            Assert.Equal("Title_Hello", rawKey.Text);
            Assert.Empty(GetTrackedTargets(applicator));
        });
    }

    [Fact]
    public void Tracking_DoesNotKeepTargetAlive()
    {
        RunSta(() =>
        {
            using var applicator = new WpfLocalizationApplicator();
            var target = CreateTrackedTarget(applicator);

            CollectUntilDead(target);

            Assert.False(target.TryGetTarget(out _));
        });
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<TextBlock> CreateTrackedTarget(
        WpfLocalizationApplicator applicator
    )
    {
        var text = new TextBlock { Text = "Key.Title_Hello" };
        applicator.Apply(text);
        return new WeakReference<TextBlock>(text);
    }

    private static void CollectUntilDead<T>(WeakReference<T> target)
        where T : class
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }
    }

    private static ICollection GetTrackedTargets(WpfLocalizationApplicator applicator) =>
        Assert.IsAssignableFrom<ICollection>(
            typeof(WpfLocalizationApplicator)
                .GetField("trackedTargets", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(applicator)
        );

    private static void DrainDispatcher()
    {
        var frame = new DispatcherFrame();
        _ = Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.ApplicationIdle,
            () => frame.Continue = false
        );
        Dispatcher.PushFrame(frame);
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception error)
            {
                failure = error;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}
