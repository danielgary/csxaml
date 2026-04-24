namespace Csxaml.Tooling.Core.Projects;

internal readonly record struct CsxamlFileStamp(long Length, long LastWriteTimeUtcTicks)
{
    public static CsxamlFileStamp Read(string filePath)
    {
        var file = new FileInfo(filePath);
        return file.Exists
            ? new CsxamlFileStamp(file.Length, file.LastWriteTimeUtc.Ticks)
            : default;
    }
}
