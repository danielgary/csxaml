namespace Csxaml.Generator;

internal static class GeneratorOptionsParser
{
    public static GeneratorOptions Parse(string[] args)
    {
        if (args.Length == 0)
        {
            throw new InvalidOperationException(
                "Usage: Csxaml.Generator --out <output-directory> --assembly-name <assembly-name> --default-namespace <namespace> --generated-namespace <namespace> [--application-mode Hybrid|Generated] [--references-file <path>] <input1.csxaml> <input2.csxaml> ...");
        }

        string? outputDirectory = null;
        string? assemblyName = null;
        string? defaultNamespace = null;
        string? generatedNamespace = null;
        var applicationMode = CsxamlApplicationMode.Hybrid;
        string? referencesFilePath = null;

        var inputFiles = new List<string>();
        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            if (!argument.StartsWith("--", StringComparison.Ordinal))
            {
                inputFiles.Add(argument);
                continue;
            }

            if (index == args.Length - 1)
            {
                throw new InvalidOperationException(
                    $"Missing value for option '{argument}'.");
            }

            var value = args[++index];
            switch (argument)
            {
                case "--out":
                    outputDirectory = value;
                    break;

                case "--assembly-name":
                    assemblyName = value;
                    break;

                case "--default-namespace":
                    defaultNamespace = value;
                    break;

                case "--generated-namespace":
                    generatedNamespace = value;
                    break;

                case "--application-mode":
                    applicationMode = ParseApplicationMode(value);
                    break;

                case "--references-file":
                    referencesFilePath = value;
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unknown option '{argument}'.");
            }
        }

        ValidateRequiredOption("--out", outputDirectory);
        ValidateRequiredOption("--assembly-name", assemblyName);
        ValidateRequiredOption("--default-namespace", defaultNamespace);
        ValidateRequiredOption("--generated-namespace", generatedNamespace);

        if (inputFiles.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one input .csxaml file is required.");
        }

        return new GeneratorOptions(
            outputDirectory!,
            assemblyName!,
            defaultNamespace!,
            generatedNamespace!,
            applicationMode,
            ReferenceListFileReader.Read(referencesFilePath),
            inputFiles);
    }

    private static CsxamlApplicationMode ParseApplicationMode(string value)
    {
        return value switch
        {
            "Hybrid" => CsxamlApplicationMode.Hybrid,
            "Generated" => CsxamlApplicationMode.Generated,
            _ => throw new InvalidOperationException(
                $"Invalid CsxamlApplicationMode '{value}'. Expected 'Hybrid' or 'Generated'.")
        };
    }

    private static void ValidateRequiredOption(string optionName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing required option '{optionName}'.");
        }
    }
}
