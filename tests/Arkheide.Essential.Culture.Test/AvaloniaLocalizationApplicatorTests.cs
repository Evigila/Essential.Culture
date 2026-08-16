using System.ComponentModel;
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
