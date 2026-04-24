using System.Text;
using System.Text.Json;

namespace Csxaml.LanguageServer.Protocol;

internal sealed class LspMessageWriter
{
    private readonly Stream _stream;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public LspMessageWriter(Stream stream)
    {
        _stream = stream;
    }

    public Task WriteErrorAsync(JsonElement id, int code, string message, CancellationToken cancellationToken)
    {
        return WriteAsync(
            new
            {
                jsonrpc = "2.0",
                id,
                error = new
                {
                    code,
                    message,
                },
            },
            cancellationToken);
    }

    public Task WriteNotificationAsync(string method, object @params, CancellationToken cancellationToken)
    {
        return WriteAsync(
            new
            {
                jsonrpc = "2.0",
                method,
                @params,
            },
            cancellationToken);
    }

    public Task WriteResponseAsync(JsonElement id, object? result, CancellationToken cancellationToken)
    {
        return WriteAsync(
            new
            {
                jsonrpc = "2.0",
                id,
                result,
            },
            cancellationToken);
    }

    private async Task WriteAsync(object payload, CancellationToken cancellationToken)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, _serializerOptions);
        var header = Encoding.ASCII.GetBytes($"Content-Length: {bytes.Length}\r\n\r\n");
        await _stream.WriteAsync(header, cancellationToken);
        await _stream.WriteAsync(bytes, cancellationToken);
        await _stream.FlushAsync(cancellationToken);
    }
}
