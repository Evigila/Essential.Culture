using ArkheideSystem.LangKey;
using GeneratedLangKey = ArkheideSystem.LangKey.Demo.ConsoleApp.Generated.LangKey;

var documentPath = Path.Combine(AppContext.BaseDirectory, "LangKey.json");
using var parser = new LangKeyParser(documentPath, "en-US");

WriteScreen(parser);
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
            parser.Current = parser.Current == "en-US" ? "zh-CN" : "en-US";
            Console.Clear();
            WriteScreen(parser);
            break;
        case "2":
            Console.WriteLine(parser.Get(GeneratedLangKey.Hello_World));
            WritePrompt(parser);
            break;
        case "3":
            return;
        default:
            Console.WriteLine(parser.Get(GeneratedLangKey.Console_InvalidSelection));
            WritePrompt(parser);
            break;
    }
}

static void WriteScreen(ILangKeyResolver resolver)
{
    Console.WriteLine(resolver.Get(GeneratedLangKey.App_Title));
    Console.WriteLine(resolver.Get(GeneratedLangKey.Description));
    Console.WriteLine(resolver.Format(GeneratedLangKey.Current_Culture, resolver.Current));
    Console.WriteLine();
    Console.WriteLine(resolver.Get(GeneratedLangKey.Console_Menu));
    WritePrompt(resolver);
}

static void WritePrompt(ILangKeyResolver resolver) =>
    Console.Write(resolver.Get(GeneratedLangKey.Console_Prompt));
