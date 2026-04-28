namespace Csxaml.LanguageServer.Protocol;

internal static class LspDocumentUriConverter
{
    public static string ToDocumentUri(string filePath)
    {
        var normalizedFilePath = NormalizePath(filePath);
        return new Uri(normalizedFilePath, UriKind.Absolute).AbsoluteUri;
    }

    public static string ToFilePath(string documentUriOrPath)
    {
        if (string.IsNullOrWhiteSpace(documentUriOrPath))
        {
            throw new ArgumentException("Document identifiers must not be empty.", nameof(documentUriOrPath));
        }

        if (TryGetFileUriPath(documentUriOrPath, out var filePath))
        {
            return NormalizePath(filePath);
        }

        return NormalizePath(documentUriOrPath);
    }

    private static string NormalizePath(string path)
    {
        var normalizedPath = Uri.UnescapeDataString(path);
        normalizedPath = NormalizeWindowsDrivePrefix(normalizedPath);
        normalizedPath = normalizedPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        return Path.GetFullPath(normalizedPath);
    }

    private static string NormalizeWindowsDrivePrefix(string path)
    {
        if (!OperatingSystem.IsWindows())
        {
            return path;
        }

        if (path.Length >= 4 &&
            IsDirectorySeparator(path[0]) &&
            char.IsAsciiLetter(path[1]) &&
            path[2] == ':' &&
            IsDirectorySeparator(path[3]))
        {
            return path[1..];
        }

        return path;
    }

    private static bool TryGetFileUriPath(string documentUriOrPath, out string filePath)
    {
        if (Uri.TryCreate(documentUriOrPath, UriKind.Absolute, out var uri) && uri.IsFile)
        {
            filePath = uri.LocalPath;
            return true;
        }

        filePath = string.Empty;
        return false;
    }

    private static bool IsDirectorySeparator(char value)
    {
        return value == Path.DirectorySeparatorChar || value == Path.AltDirectorySeparatorChar;
    }
}
