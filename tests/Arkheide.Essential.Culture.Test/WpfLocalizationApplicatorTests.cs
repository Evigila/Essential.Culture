using System.Runtime.ExceptionServices;
using System.Windows.Controls;
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
}
