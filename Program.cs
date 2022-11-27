using System.CommandLine;

internal static class Program {
    public static async Task<int> Main(string[] args) {
        RootCommand rootCommand = new() {
            Name = "ignore",
            Description = "Generate a .gitignore file based on one or more gitignore.io templates"
        };

        Command listSubcommand = new("list", "List all available templates");
        listSubcommand.SetHandler(HandleListCommand);
        rootCommand.AddCommand(listSubcommand);

        Argument<string[]> ignoresArgument = new(
            "ignores", "One or more gitignore.io templates to use"
        ) {
            Arity = ArgumentArity.OneOrMore
        };

        Option<FileInfo> outputOption = new(
            new[] { "--output", "-o" },
            () => new FileInfo(".gitignore"),
            "Set the output file to write the .gitignore to"
        ) {
            ArgumentHelpName = "file"
        };

        Option<bool> silentOption = new(new[] { "--silent", "-s" }, "Do not output anything to the console");

        rootCommand.AddArgument(ignoresArgument);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(silentOption);
        rootCommand.SetHandler(HandleGenerateCommand, ignoresArgument, outputOption, silentOption);
        return await rootCommand.InvokeAsync(args);
    }

    private static async Task HandleListCommand() {
        Console.WriteLine("Available templates:");
        await foreach (string template in GetTemplates()) {
            Console.WriteLine(template);
        }

        Environment.Exit(0);
    }

    private static async Task HandleGenerateCommand(string[] templates, FileInfo output, bool silent) {
        if (templates is [var joinedTemplates] && joinedTemplates.Contains(',', StringComparison.Ordinal)) {
            templates = templates[0].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        if (!silent) {
            Console.WriteLine($"Generating '{output}' using templates: {string.Join(", ", templates)}");
        }

        string content = await GetContent(templates);
        await File.WriteAllTextAsync(output.FullName, content);
    }

    private static async IAsyncEnumerable<string> GetTemplates() {
        using HttpClient client = new();
        await using Stream stream = await client.GetStreamAsync("https://www.toptal.com/developers/gitignore/api/list?format=lines");
        using StreamReader reader = new(stream);

        while (await reader.ReadLineAsync() is { } line) {
            yield return line;
        }
    }

    private static async Task<string> GetContent(string[] templates) {
        using HttpClient client = new();
        return await (await client.GetAsync($"https://www.toptal.com/developers/gitignore/api/{string.Join(",", templates)}"))
                     .Content.ReadAsStringAsync();
    }
}