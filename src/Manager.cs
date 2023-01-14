using System.Text.RegularExpressions;

namespace Todo;

internal static class Manager
{
    private static readonly string CONFIG_DIR =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".todo"
        );

    private static bool IsStillRunning = true;

    private static bool ShouldClear = true;

    private static IEnumerable<(int Index, string FileName)> Contexts
    {
        get
        {
            var contexts = GetTodoContexts();
            var indexed =
                contexts
                    .Where(x => x.EndsWith(".json"))
                    .Select((x, i) => (i + 1, GetNameFromFileName(Path.GetFileName(x)[..^5])));

            return indexed;
        }
    }

    public static void Display()
    {
        CheckConfigDir();
        MessageLoop();
    }

    private static void PrintChoices()
    {
        Console.WriteLine("todo: a to-do list manager");
        Console.WriteLine("---");

        // browse config directory for todo files
        foreach (var file in Contexts)
            Console.WriteLine($"{file.Index}: {file.FileName}");

        Console.WriteLine();
        Console.WriteLine("0: Create new to-do list");
        Console.WriteLine("Q: Quit");

        Console.WriteLine();
    }

    private static void MessageLoop()
    {
        while (IsStillRunning)
        {
            CheckShouldClear();
            HandleInput();
            ClearPreviousLine();
        }
    }

    private static void ClearPreviousLine()
    {
        if (Console.CursorTop < 1)
            return;

        Console.SetCursorPosition(0, Console.CursorTop - 1);
        ClearLine();
    }

    private static void ClearLine()
    {
        if (Console.CursorTop < 0)
            return;

        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }

    private static void HandleInput()
    {
        Console.Write("> ");
        var choice = Console.ReadLine();

        choice = choice?.Trim()?.ToUpper();

        if (choice == null || choice == "Q")
        {
            IsStillRunning = false;
            return;
        }

        ClearLine();
        HandleChoice(choice);
    }

    private static void CheckConfigDir()
    {
        if (!Directory.Exists(CONFIG_DIR))
            Directory.CreateDirectory(CONFIG_DIR);
    }

    private static void CheckShouldClear()
    {
        if (ShouldClear)
        {
            Console.Clear();
            PrintChoices();

            ShouldClear = false;
        }
    }

    private static void CreateTodoContext()
    {
        Console.Write("Enter the name for the to-do list: ");
        var name = Console.ReadLine();

        ShouldClear = true;
    }

    private static IEnumerable<string> GetTodoContexts()
    {
        var files = Directory.GetFiles(CONFIG_DIR);
        var dotjson = files.Where(x => x.EndsWith(".json"));

        return dotjson;
    }

    private static void HandleChoice(string choice)
    {
        if (choice == "0")
        {
            CreateTodoContext();
            return;
        }

        Console.Write($"Error: undefined choice \"{choice}\"");
    }

    private static string GetNameFromFileName(string fileName)
    {
        var patternSpace = @"(?<!_)_(?!_)";
        var patternUnderscore = @"__";

        var result1 = Regex.Replace(fileName, patternSpace, " ");
        var result2 = Regex.Replace(result1, patternUnderscore, "_");

        return result2;
    }
}
