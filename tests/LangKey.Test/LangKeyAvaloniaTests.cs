using System.ComponentModel;
using ArkheideSystem.LangKey.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.LangKey.Test;

public sealed class LangKeyAvaloniaTests
{
    [Fact]
    public void Apply_LocalizesSupportedDisplayProperties()
    {
        using var parser = new LangKeyParser(GetDocumentPath(), "en-US");
        using var applicator = new LangKeyAvaloniaApplicator(parser);
        var text = new TextBlock { Text = "LangKey.Title_Hello" };
        var button = new Button { Content = "LangKey.Title_Hello" };
        var root = new StackPanel { Children = { text, button } };

        applicator.Apply(root);

        Assert.Equal("Hello World!", text.Text);
        Assert.Equal("Hello World!", button.Content);
    }

    [Fact]
    public void Apply_DoesNotOverwriteAChangedBindingValue()
    {
        using var parser = new LangKeyParser(GetDocumentPath(), "en-US");
        using var applicator = new LangKeyAvaloniaApplicator(parser);
        var source = new BindingSource { Value = "LangKey.Title_Hello" };
        var text = new TextBlock();
        text.Bind(
            TextBlock.TextProperty,
            new Binding(nameof(BindingSource.Value)) { Source = source }
        );

        applicator.Apply(text);
        source.Value = "Application-owned value";
        Assert.Equal("Application-owned value", text.Text);

        parser.Current = "zh-CN";
        applicator.Apply(text);

        Assert.Equal("Application-owned value", text.Text);
    }

    [Fact]
    public void AddLangKeyAvalonia_RegistersOneSharedRuntime()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestApplication>();
        services.AddLangKeyAvalonia<TestApplication>(GetDocumentPath(), _ => "zh-CN");

        using var provider = services.BuildServiceProvider();

        Assert.Same(
            provider.GetRequiredService<LangKeyAvaloniaApplicator>(),
            provider.GetRequiredService<ILangKeyAvaloniaApplicator>()
        );
        Assert.Equal("zh-CN", provider.GetRequiredService<ILangKeyParser>().Current);
        Assert.Single(provider.GetServices<IHostedService>());
    }

    [Fact]
    public void AddLangKeyAvalonia_RejectsDuplicateRegistration()
    {
        var services = new ServiceCollection();
        services.AddLangKeyAvalonia<TestApplication>(GetDocumentPath());

        var error = Assert.Throws<InvalidOperationException>(() =>
            services.AddLangKeyAvalonia<TestApplication>(GetDocumentPath())
        );

        Assert.Contains("already registered", error.Message, StringComparison.Ordinal);
    }

    private static string GetDocumentPath() =>
        Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "LangKey.json")
        );

    private sealed class TestApplication : Application;

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
