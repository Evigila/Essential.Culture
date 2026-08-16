using System.ComponentModel;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Arkheide.Essential.Culture.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace Arkheide.Essential.Culture.Test;

public sealed class KeyAvaloniaTests
{
    [Fact]
    public void Apply_LocalizesSupportedDisplayProperties()
    {
        Localizer.Current.SetCulture("en-US");
        using var applicator = new AvaloniaLocalizationApplicator();
        var text = new TextBlock { Text = "Key.Title_Hello" };
        var button = new Button { Content = "Key.Title_Hello" };
        var root = new StackPanel { Children = { text, button } };

        applicator.Apply(root);

        Assert.Equal("Hello World!", text.Text);
        Assert.Equal("Hello World!", button.Content);
    }

    [Fact]
    public void Apply_DoesNotOverwriteAChangedBindingValue()
    {
        Localizer.Current.SetCulture("en-US");
        using var applicator = new AvaloniaLocalizationApplicator();
        var source = new BindingSource { Value = "Key.Title_Hello" };
        var text = new TextBlock();
        text.Bind(
            TextBlock.TextProperty,
            new Binding(nameof(BindingSource.Value)) { Source = source }
        );

        applicator.Apply(text);
        source.Value = "Application-owned value";
        Assert.Equal("Application-owned value", text.Text);

        Localizer.Current.SetCulture("zh-CN");
        applicator.Apply(text);

        Assert.Equal("Application-owned value", text.Text);
    }

    [Fact]
    public void Apply_DoesNotTrackOrdinaryText()
    {
        using var applicator = new AvaloniaLocalizationApplicator();
        var rawKey = new TextBlock { Text = "Title_Hello" };
        applicator.Apply(rawKey);
        for (var index = 0; index < 100; index++)
        {
            applicator.Apply(new TextBlock { Text = $"Ordinary text {index}" });
        }

        Assert.Equal("Title_Hello", rawKey.Text);
        var trackedTargets = Assert.IsAssignableFrom<ICollection>(
            typeof(AvaloniaLocalizationApplicator)
                .GetField("trackedTargets", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(applicator)
        );
        Assert.Empty(trackedTargets);
    }

    [Fact]
    public void Tracking_DoesNotKeepTargetAlive()
    {
        using var applicator = new AvaloniaLocalizationApplicator();
        var target = CreateTrackedTarget(applicator);

        CollectUntilDead(target);

        Assert.False(target.TryGetTarget(out _));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<TextBlock> CreateTrackedTarget(
        AvaloniaLocalizationApplicator applicator
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

    private sealed class BindingSource : INotifyPropertyChanged
    {
        private string value = string.Empty;

        public required string Value
        {
            get => value;
            set
            {
                if (this.value == value)
                {
                    return;
                }

                this.value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
