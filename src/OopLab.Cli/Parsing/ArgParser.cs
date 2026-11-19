using System.Text;

namespace OopLab.Cli.Parsing;

public class ArgParser
{
    public Args ParseArgs(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            return new Args(string.Empty, Array.Empty<string>(), new Dictionary<string, string?>());

        var tokens = Tokenize(rawInput);
        var commandName = tokens[0].ToLowerInvariant();
        var positional = new List<string>();
        var options = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        for (int i = 1; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.StartsWith("--"))
            {
                var body = token[2..];
                var eqIndex = body.IndexOf('=');
                if (eqIndex > 0)
                    options[body[..eqIndex]] = body[(eqIndex + 1)..];
                else
                    options[body] = "true";
            }
            else
            {
                positional.Add(token);
            }
        }

        return new Args(commandName, positional, options);
    }

    private static List<string> Tokenize(string input)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        foreach (char ch in input.Trim())
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (char.IsWhiteSpace(ch) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(ch);
            }
        }

        if (current.Length > 0)
            result.Add(current.ToString());

        return result;
    }
}
