using ArkheideSystem.LangKey;

namespace ArkheideSystem.LangKey.Demo.Shared;

public sealed class DemoCultureSource : ILangKeyCultureSource
{
    public string CurrentCulture { get; private set; } = "en-US";

    public event EventHandler<LangKeyCultureChangedEventArgs>? Changed;

    public void Toggle()
    {
        CurrentCulture = CurrentCulture == "en-US" ? "zh-CN" : "en-US";
        Changed?.Invoke(this, new LangKeyCultureChangedEventArgs(CurrentCulture));
    }
}
