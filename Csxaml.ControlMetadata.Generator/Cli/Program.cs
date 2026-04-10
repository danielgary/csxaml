namespace Csxaml.ControlMetadata.Generator;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var options = MetadataGeneratorOptionsParser.Parse(args);
            new MetadataGeneratorRunner().Generate(options);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }
}
