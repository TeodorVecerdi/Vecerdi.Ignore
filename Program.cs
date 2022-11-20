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
            name: "ignores",
            description: "One or more gitignore.io templates to use"
        ) {
            Arity = ArgumentArity.OneOrMore
        };

        Option<FileInfo> outputOption = new(
            aliases: new []{"--output", "-o"},
            description: "Set the output file to write the .gitignore to",
            getDefaultValue: () => new FileInfo(".gitignore")
        ) {
            IsRequired = false,
            ArgumentHelpName = "file",
            Arity = ArgumentArity.ExactlyOne,
        };

        rootCommand.AddArgument(ignoresArgument);
        rootCommand.AddOption(outputOption);
        rootCommand.SetHandler(HandleGenerateCommand, ignoresArgument, outputOption);
        return await rootCommand.InvokeAsync(args);
    }

    private static async Task HandleListCommand() {
        Console.WriteLine("Available templates:");
        await foreach (string template in GetTemplates()) {
            Console.WriteLine(template);
        }

        Environment.Exit(0);
    }

    private static async Task HandleGenerateCommand(string[] templates, FileInfo output) {
        if (templates is [var joinedTemplates] && joinedTemplates.Contains(',', StringComparison.Ordinal)) {
            templates = templates[0].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        Console.WriteLine($"Generating '{output}' using templates: {string.Join(", ", templates)}");
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
