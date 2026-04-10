namespace Csxaml.Generator;

internal static class GeneratorOptionsParser
{
    public static GeneratorOptions Parse(string[] args)
    {
        if (args.Length < 3 || !string.Equals(args[0], "--out", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Usage: Csxaml.Generator --out <output-directory> <input1.csxaml> <input2.csxaml> ...");
        }

        var outputDirectory = args[1];
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new InvalidOperationException("Output directory is required.");
        }

        var inputFiles = args[2..];
        if (inputFiles.Length == 0)
        {
            throw new InvalidOperationException(
                "At least one input .csxaml file is required.");
        }

        return new GeneratorOptions(outputDirectory, inputFiles);
    }
}
