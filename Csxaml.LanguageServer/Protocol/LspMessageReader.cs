using System.Text;
using System.Text.Json;
using System.Globalization;

namespace Csxaml.LanguageServer.Protocol;

internal sealed class LspMessageReader
{
    private readonly Stream _stream;

    public LspMessageReader(Stream stream)
    {
        _stream = stream;
    }

    public async Task<JsonDocument?> ReadAsync(CancellationToken cancellationToken)
    {
        var contentLength = await ReadContentLengthAsync(cancellationToken);
        if (contentLength is null)
        {
            return null;
        }

        var buffer = new byte[contentLength.Value];
        var bytesRead = 0;
        while (bytesRead < buffer.Length)
        {
            var read = await _stream.ReadAsync(buffer.AsMemory(bytesRead), cancellationToken);
            if (read == 0)
            {
                return null;
            }

            bytesRead += read;
        }

        return JsonDocument.Parse(buffer);
    }

    private async Task<int?> ReadContentLengthAsync(CancellationToken cancellationToken)
    {
        string? line;
        var contentLength = default(int?);
        do
        {
            line = await ReadLineAsync(cancellationToken);
            if (line is null)
            {
                return null;
            }

            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                contentLength = int.Parse(line["Content-Length:".Length..].Trim(), CultureInfo.InvariantCulture);
            }
        }
        while (!string.IsNullOrEmpty(line));

        return contentLength;
    }

    private async Task<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        var bytes = new List<byte>();
        while (true)
        {
            var buffer = new byte[1];
            var read = await _stream.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                return bytes.Count == 0 ? null : Encoding.ASCII.GetString(bytes.ToArray());
            }

            if (buffer[0] == '\n')
            {
                break;
            }

            if (buffer[0] != '\r')
            {
                bytes.Add(buffer[0]);
            }
        }

        return Encoding.ASCII.GetString(bytes.ToArray());
    }
}
