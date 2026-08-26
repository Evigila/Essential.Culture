using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;
using ArkheideSystem.Essential.Culture.Wpf;

namespace ArkheideSystem.Essential.Culture.Test;

public sealed class WpfLocalizeExtensionTests
{
    [Fact]
    public void UnparameterizedValue_RefreshesWhenCultureChanges()
    {
        RunSta(() =>
        {
            Localizer.Current.SetCulture("en-US");
            var text = ParseTextBlock(
                """
                <TextBlock
                  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:test="clr-namespace:ArkheideSystem.Essential.Culture.Test;assembly=Essential.Culture.Test"
                  Text="{test:WpfTestLocalize Key.Title_Hello}" />
                """
            );
            DrainDispatcher();

            Assert.Equal("Hello World!", text.Text);

            Localizer.Current.SetCulture("zh-CN");
            DrainDispatcher();

            Assert.Equal("你好 世界！", text.Text);
        });
    }

    [Fact]
    public void ParameterizedValue_RefreshesWhenBindingOrCultureChanges()
    {
        RunSta(() =>
        {
            Localizer.Current.SetCulture("en-US");
            var text = ParseTextBlock(
                """
                <TextBlock
                  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:test="clr-namespace:ArkheideSystem.Essential.Culture.Test;assembly=Essential.Culture.Test"
                  Text="{test:WpfTestLocalize Key.Message_Count, Arg0={Binding Count}}" />
                """
            );
            var model = new WpfLocalizeTestModel { Count = 12 };
            text.DataContext = model;
            DrainDispatcher();

            Assert.Equal("Count: 12", text.Text);

            model.Count = 34;
            DrainDispatcher();
            Assert.Equal("Count: 34", text.Text);

            Localizer.Current.SetCulture("zh-CN");
            DrainDispatcher();
            Assert.Equal("数量：34", text.Text);
        });
    }

    [Fact]
    public void ArgumentsContentCollection_SupportsMoreThanThreeBindings()
    {
        RunSta(() =>
        {
            Localizer.Current.SetCulture("en-US");
            var text = ParseTextBlock(
                """
                <TextBlock
                  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:test="clr-namespace:ArkheideSystem.Essential.Culture.Test;assembly=Essential.Culture.Test">
                  <TextBlock.Text>
                    <test:WpfTestLocalize Key="Key.Message_Count">
                      <Binding Path="Count" />
                      <Binding Path="Count" />
                      <Binding Path="Count" />
                      <Binding Path="Count" />
                    </test:WpfTestLocalize>
                  </TextBlock.Text>
                </TextBlock>
                """
            );
            var model = new WpfLocalizeTestModel { Count = 56 };
            text.DataContext = model;
            DrainDispatcher();

            Assert.Equal("Count: 56", text.Text);

            model.Count = 78;
            DrainDispatcher();
            Assert.Equal("Count: 78", text.Text);
        });
    }

    [Fact]
    public void ArgumentsContentCollection_CannotBeCombinedWithInlineArguments()
    {
        RunSta(() =>
        {
            var error = Assert.Throws<XamlParseException>(() =>
                ParseTextBlock(
                    """
                    <TextBlock
                      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      xmlns:test="clr-namespace:ArkheideSystem.Essential.Culture.Test;assembly=Essential.Culture.Test">
                      <TextBlock.Text>
                        <test:WpfTestLocalize
                          Key="Key.Message_Count"
                          Arg0="{Binding Count}">
                          <Binding Path="Count" />
                        </test:WpfTestLocalize>
                      </TextBlock.Text>
                    </TextBlock>
                    """
                )
            );

            Assert.IsType<InvalidOperationException>(error.InnerException);
        });
    }

    private static TextBlock ParseTextBlock(string xaml) =>
        Assert.IsType<TextBlock>(XamlReader.Parse(xaml));

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

public sealed class WpfTestLocalizeExtension : WpfLocalizeExtensionBase
{
    private string? key;

    public WpfTestLocalizeExtension() { }

    public WpfTestLocalizeExtension(string token)
        : base(token)
    {
        key = token;
    }

    public string? Key
    {
        get => key;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            key = value;
            Token = value;
        }
    }
}

public sealed class WpfLocalizeTestModel : INotifyPropertyChanged
{
    private int count;

    public int Count
    {
        get => count;
        set
        {
            if (count == value)
            {
                return;
            }

            count = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
