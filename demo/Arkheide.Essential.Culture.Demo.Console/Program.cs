using Arkheide.Essential.Culture;
using GeneratedKey = Arkheide.Essential.Culture.Key;

Localizer.Current.SetCulture("en-US");
WriteScreen();
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
            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
            }

            WriteScreen();
            break;
        case "2":
            Console.WriteLine(Localizer.Parse(GeneratedKey.Hello_World));
            WritePrompt();
            break;
        case "3":
            return;
        default:
            Console.WriteLine(Localizer.Parse(GeneratedKey.Console_InvalidSelection));
            WritePrompt();
            break;
    }
}

static void WriteScreen()
{
    Console.WriteLine(Localizer.Parse(GeneratedKey.App_Title));
    Console.WriteLine(Localizer.Parse(GeneratedKey.Description));
    Console.WriteLine(
        Localizer.Parse(GeneratedKey.Current_Culture, Localizer.Current.Culture)
    );
    Console.WriteLine();
    Console.WriteLine(Localizer.Parse(GeneratedKey.Console_Menu));
    WritePrompt();
}

static void WritePrompt() => Console.Write(Localizer.Parse(GeneratedKey.Console_Prompt));
