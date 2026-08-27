using System.ComponentModel;
using ArkheideSystem.Essential.Culture.Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace ArkheideSystem.Essential.Culture.Test;

public sealed class AvaloniaLocalizeExtensionTests
{
    [Fact]
    public void LocalizeBinding_AlsoSupportsKeysWithoutArguments()
    {
        Localizer.Current.SetCulture("en-US");
        var extension = new TestLocalizeExtension("Key.Title_Hello");
        var text = new TextBlock();

        text.Bind(TextBlock.TextProperty, extension.CreateBinding());
        Assert.Equal("Hello World!", text.Text);

        Localizer.Current.SetCulture("zh-CN");

        Assert.Equal("你好 世界！", text.Text);
    }

    [Fact]
    public void LocalizeBinding_RefreshesWhenArgumentChanges()
    {
        Localizer.Current.SetCulture("en-US");
        var source = new BindingSource { Value = "one" };
        var extension = new TestLocalizeExtension("Key.Message_Count")
        {
            Arg0 = new Binding(nameof(BindingSource.Value)) { Source = source },
        };
        var text = new TextBlock();

        text.Bind(TextBlock.TextProperty, extension.CreateBinding());
        Assert.Equal("Count: one", text.Text);

        source.Value = "two";

        Assert.Equal("Count: two", text.Text);
    }

    [Fact]
    public void LocalizeBinding_RefreshesWhenCultureChanges()
    {
        Localizer.Current.SetCulture("en-US");
        var source = new BindingSource { Value = "5" };
        var extension = new TestLocalizeExtension("Key.Message_Count")
        {
            Arg0 = new Binding(nameof(BindingSource.Value)) { Source = source },
        };
        var text = new TextBlock();

        text.Bind(TextBlock.TextProperty, extension.CreateBinding());
        Assert.Equal("Count: 5", text.Text);

        Localizer.Current.SetCulture("zh-CN");

        Assert.Equal("数量：5", text.Text);
    }

    [Fact]
    public void KeyBinding_RefreshesWhenKeyOrCultureChanges()
    {
        Localizer.Current.SetCulture("en-US");
        var source = new BindingSource { Value = "Key.Title_Hello" };
        var extension = new DeferredTestLocalizeExtension
        {
            KeyBinding = new Binding(nameof(BindingSource.Value)) { Source = source },
        };
        var text = new TextBlock();

        text.Bind(TextBlock.TextProperty, extension.CreateBinding());
        Assert.Equal("Hello World!", text.Text);

        source.Value = "Key.Message_Count";
        Assert.Equal("Count: {0}", text.Text);

        Localizer.Current.SetCulture("zh-CN");
        Assert.Equal("数量：{0}", text.Text);
    }

    [Fact]
    public void KeyBinding_RefreshesWhenAnArgumentChanges()
    {
        Localizer.Current.SetCulture("en-US");
        var keySource = new BindingSource { Value = "Key.Message_Count" };
        var argumentSource = new BindingSource { Value = "one" };
        var extension = new DeferredTestLocalizeExtension
        {
            KeyBinding = new Binding(nameof(BindingSource.Value)) { Source = keySource },
            Arg0 = new Binding(nameof(BindingSource.Value)) { Source = argumentSource },
        };
        var text = new TextBlock();

        text.Bind(TextBlock.TextProperty, extension.CreateBinding());
        Assert.Equal("Count: one", text.Text);

        argumentSource.Value = "two";
        Assert.Equal("Count: two", text.Text);
    }

    [Fact]
    public void KeyBinding_CannotBeCombinedWithAStaticKey()
    {
        var extension = new TestLocalizeExtension("Key.Title_Hello")
        {
            KeyBinding = new Binding { Source = "Key.Message_Count" },
        };

        var error = Assert.Throws<InvalidOperationException>(() => extension.CreateBinding());

        Assert.Contains("cannot be combined", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void LocalizeBinding_RejectsMixedArgumentForms()
    {
        var extension = new TestLocalizeExtension("Key.Message_Count")
        {
            Arg0 = new Binding("Value"),
        };
        extension.Arguments.Add(new Binding("OtherValue"));

        var error = Assert.Throws<InvalidOperationException>(() => extension.CreateBinding());

        Assert.Contains("cannot be combined", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void LocalizeBinding_AcceptsAnArbitraryNumberOfArgumentBindings()
    {
        Localizer.Current.SetCulture("en-US");
        var extension = new TestLocalizeExtension("Key.Message_Count");
        extension.Arguments.Add(
            new Binding(nameof(BindingSource.Value))
            {
                Source = new BindingSource { Value = "first" },
            }
        );
        extension.Arguments.Add(new Binding { Source = "second" });
        extension.Arguments.Add(new Binding { Source = "third" });
        extension.Arguments.Add(new Binding { Source = "fourth" });
        var text = new TextBlock();

        var binding = extension.CreateBinding();
        text.Bind(TextBlock.TextProperty, binding);

        Assert.Equal(5, binding.Bindings.Count);
        Assert.Equal("Count: first", text.Text);
    }

    [Fact]
    public void LocalizeBinding_RequiresAKeyWhenUsingTheParameterlessConstructor()
    {
        var extension = new DeferredTestLocalizeExtension();

        var error = Assert.Throws<InvalidOperationException>(() => extension.CreateBinding());

        Assert.Contains("Localize.Key or Localize.KeyBinding", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void LocalizeBinding_AllowsAKeyToBeAssignedAfterConstruction()
    {
        Localizer.Current.SetCulture("en-US");
        var extension = new DeferredTestLocalizeExtension();
        extension.SetToken("Key.Title_Hello");
        var text = new TextBlock();

        text.Bind(TextBlock.TextProperty, extension.CreateBinding());

        Assert.Equal("Hello World!", text.Text);
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

    private sealed class TestLocalizeExtension(string token)
        : AvaloniaLocalizeExtensionBase(token)
    {
        internal MultiBinding CreateBinding() => ProvideValue(new TestServiceProvider());
    }

    private sealed class DeferredTestLocalizeExtension : AvaloniaLocalizeExtensionBase
    {
        internal MultiBinding CreateBinding() => ProvideValue(new TestServiceProvider());

        internal void SetToken(string token) => Token = token;
    }

    private sealed class TestServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
