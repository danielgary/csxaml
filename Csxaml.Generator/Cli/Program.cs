namespace Csxaml.Generator;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var options = GeneratorOptionsParser.Parse(args);
            var generatedFiles = new GeneratorRunner().GenerateFiles(options);
            OutputWriter.WriteAll(options.OutputDirectory, generatedFiles);
            return 0;
        }
        catch (DiagnosticException exception)
        {
            Console.Error.WriteLine(exception.Diagnostic);
            return 1;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }
}
