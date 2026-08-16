using Arkheide.Essential.Culture;
using GeneratedKey = Arkheide.Essential.Culture.Key;

Localizer.Current.SetCulture("en-US");
var messageHistory = new List<string>();
Localizer.Current.Changed += (_, _) =>
{
    Console.WriteLine();
    WriteScreen(messageHistory);
};

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
            break;
        case "2":
            WriteMessage(GeneratedKey.Hello_World, messageHistory);
            break;
        case "3":
            return;
        default:
            WriteMessage(GeneratedKey.Console_InvalidSelection, messageHistory);
            break;
    }
}

static void WriteScreen(IReadOnlyList<string> messageHistory)
{
    Console.WriteLine(Localizer.Parse(GeneratedKey.App_Title));
    Console.WriteLine(Localizer.Parse(GeneratedKey.Description));
    Console.WriteLine(
        Localizer.Parse(GeneratedKey.Current_Culture, Localizer.Current.Culture)
    );
    Console.WriteLine();
    foreach (var message in messageHistory)
    {
        Console.WriteLine(Localizer.Parse(message));
    }

    if (messageHistory.Count > 0)
    {
        Console.WriteLine();
    }

    Console.WriteLine(Localizer.Parse(GeneratedKey.Console_Menu));
    WritePrompt();
}

static void WritePrompt() => Console.Write(Localizer.Parse(GeneratedKey.Console_Prompt));

static void WriteMessage(string token, ICollection<string> messageHistory)
{
    messageHistory.Add(token);
    Console.WriteLine(Localizer.Parse(token));
    WritePrompt();
}
