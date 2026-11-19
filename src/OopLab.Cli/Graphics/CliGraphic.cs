namespace OopLab.Cli.Graphics;

public static class CliGraphic
{
    public static void PrintBanner(string title)
    {
        var line = new string('═', title.Length + 6);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"╔{line}╗");
        Console.WriteLine($"║   {title}   ║");
        Console.WriteLine($"╚{line}╝");
        Console.ResetColor();
    }

    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[OK] {message}");
        Console.ResetColor();
    }

    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ПОМИЛКА] {message}");
        Console.ResetColor();
    }

    public static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[INFO] {message}");
        Console.ResetColor();
    }

    public static async Task RunWithSpinnerAsync(string label, Func<Task> action)
    {
        char[] frames = { '|', '/', '-', '\\' };
        using var cts = new CancellationTokenSource();

        var spinnerTask = Task.Run(async () =>
        {
            int idx = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                Console.Write($"\r{label} {frames[idx++ % frames.Length]}");
                try { await Task.Delay(80, cts.Token); }
                catch (OperationCanceledException) { break; }
            }
        });

        try
        {
            await action();
        }
        finally
        {
            cts.Cancel();
            await spinnerTask;
            Console.WriteLine($"\r{label} Готово!   ");
        }
    }

    public static string PromptRequired(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(input))
                return input;
            PrintError("Значення не може бути порожнім. Спробуйте ще раз.");
        }
    }

    public static int PromptInt(string prompt, int defaultValue, int min, int max)
    {
        Console.Write($"{prompt} [{defaultValue}]: ");
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input)) return defaultValue;
        return int.TryParse(input, out var val) ? Math.Clamp(val, min, max) : defaultValue;
    }

    public static string PromptOptional(string prompt, string defaultValue = "")
    {
        Console.Write($"{prompt}{(string.IsNullOrEmpty(defaultValue) ? "" : $" [{defaultValue}]")}: ");
        var input = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(input) ? defaultValue : input;
    }
}
