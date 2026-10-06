using ArkheideSystem.Essential.Culture;
using GeneratedKey = ArkheideSystem.Essential.Culture.Key;

var messageHistory = new List<LocalizedMessage>();

WriteScreen(messageHistory);
while (true)
{
    var input = Console.ReadLine();
    if (input is null)
    {
        return;
    }

    switch (input.Trim())
    {
        case "1":
            Localizer.Current.SetCulture(
                Localizer.Current.Culture == "en-US" ? "zh-CN" : "en-US"
            );
            WriteScreen(messageHistory);
            break;
        case "2":
            WriteMessage(GeneratedKey.Greeting, ["Arkheide"], messageHistory);
            break;
        case "3":
            return;
        default:
            WriteMessage(GeneratedKey.Console_InvalidSelection, [], messageHistory);
            break;
    }
}

static void WriteScreen(IReadOnlyList<LocalizedMessage> messageHistory)
{
    Console.WriteLine(Localizer.Parse(GeneratedKey.App_Title));
    Console.WriteLine(Localizer.Parse(GeneratedKey.Description));
    Console.WriteLine(
        Localizer.Parse(GeneratedKey.Current_Culture, Localizer.Current.Culture)
    );
    Console.WriteLine();
    foreach (var message in messageHistory)
    {
        Console.WriteLine(Localizer.Parse(message.Token, message.Arguments));
    }

    if (messageHistory.Count > 0)
    {
        Console.WriteLine();
    }

    Console.WriteLine(Localizer.Parse(GeneratedKey.Console_Menu));
    WritePrompt();
}

static void WritePrompt() => Console.Write(Localizer.Parse(GeneratedKey.Console_Prompt));

static void WriteMessage(
    string token,
    object?[] arguments,
    ICollection<LocalizedMessage> messageHistory
)
{
    messageHistory.Add(new LocalizedMessage(token, arguments));
    Console.WriteLine(Localizer.Parse(token, arguments));
    WritePrompt();
}

internal sealed record LocalizedMessage(string Token, object?[] Arguments);
