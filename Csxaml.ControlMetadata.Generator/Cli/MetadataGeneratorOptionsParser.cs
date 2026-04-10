namespace Csxaml.ControlMetadata.Generator;

internal static class MetadataGeneratorOptionsParser
{
    public static MetadataGeneratorOptions Parse(string[] args)
    {
        if (args.Length == 2 &&
            string.Equals(args[0], "--out", StringComparison.Ordinal))
        {
            return new MetadataGeneratorOptions(args[1]);
        }

        throw new InvalidOperationException(
            "Usage: csxaml-control-metadata-generator --out <output-path>");
    }
}
