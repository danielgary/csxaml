using System.Text.Json;

namespace Csxaml.Generator;

internal static class GeneratedSourceMapWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static string Write(GeneratedSourceMap map)
    {
        return JsonSerializer.Serialize(map, JsonOptions);
    }
}
