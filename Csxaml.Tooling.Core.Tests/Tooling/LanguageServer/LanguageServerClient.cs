using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Csxaml.Tooling.Core.Tests.Tooling.LanguageServer;

internal sealed class LanguageServerClient : IAsyncDisposable
{
    private readonly Dictionary<int, JsonElement> _queuedResponses = new();
    private readonly List<JsonElement> _queuedNotifications = [];
    private readonly Stream _input;
    private readonly Stream _output;
    private readonly Process _process;
    private int _nextRequestId;

    private LanguageServerClient(Process process)
    {
        _process = process;
        _input = process.StandardOutput.BaseStream;
        _output = process.StandardInput.BaseStream;
    }

    public static Task<LanguageServerClient> StartAsync(string serverPath, string workingDirectory)
    {
        var startInfo = CreateStartInfo(serverPath, workingDirectory);

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The language server process did not start.");
        return Task.FromResult(new LanguageServerClient(process));
    }

    private static ProcessStartInfo CreateStartInfo(string serverPath, string workingDirectory)
    {
        if (serverPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            return new ProcessStartInfo("dotnet", $"\"{serverPath}\"")
            {
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory,
            };
        }

        return new ProcessStartInfo(serverPath)
        {
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory,
        };
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!_process.HasExited)
            {
                await SendRequestAsync("shutdown", parameters: null, CancellationToken.None);
                await SendNotificationAsync("exit", parameters: null, CancellationToken.None);
                _process.WaitForExit(5000);
            }
        }
        finally
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
            }

            _process.Dispose();
        }
    }

    public async Task<JsonElement> SendRequestAsync(string method, object? parameters, CancellationToken cancellationToken)
    {
        var id = Interlocked.Increment(ref _nextRequestId);
        await WriteMessageAsync(
            new
            {
                jsonrpc = "2.0",
                id,
                method,
                @params = parameters,
            },
            cancellationToken);

        if (_queuedResponses.Remove(id, out var queuedResponse))
        {
            return queuedResponse;
        }

        while (true)
        {
            var message = await ReadMessageAsync(cancellationToken);
            if (message.TryGetProperty("id", out var messageId) && messageId.GetInt32() == id)
            {
                return message;
            }

            QueueMessage(message);
        }
    }

    public Task SendNotificationAsync(string method, object? parameters, CancellationToken cancellationToken)
    {
        return WriteMessageAsync(
            new
            {
                jsonrpc = "2.0",
                method,
                @params = parameters,
            },
            cancellationToken);
    }

    public async Task<JsonElement> WaitForNotificationAsync(string method, CancellationToken cancellationToken)
    {
        var queuedNotification = TryTakeQueuedNotification(method);
        if (queuedNotification is not null)
        {
            return queuedNotification.Value;
        }

        while (true)
        {
            var message = await ReadMessageAsync(cancellationToken);
            if (message.TryGetProperty("method", out var methodElement) &&
                string.Equals(methodElement.GetString(), method, StringComparison.Ordinal))
            {
                return message;
            }

            QueueMessage(message);
        }
    }

    private void QueueMessage(JsonElement message)
    {
        if (message.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.Number)
        {
            _queuedResponses[idElement.GetInt32()] = message;
            return;
        }

        _queuedNotifications.Add(message);
    }

    private JsonElement? TryTakeQueuedNotification(string method)
    {
        for (var index = 0; index < _queuedNotifications.Count; index++)
        {
            var notification = _queuedNotifications[index];
            if (notification.TryGetProperty("method", out var methodElement) &&
                string.Equals(methodElement.GetString(), method, StringComparison.Ordinal))
            {
                _queuedNotifications.RemoveAt(index);
                return notification;
            }
        }

        return null;
    }

    private async Task WriteMessageAsync(object payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(payload);
        var header = Encoding.ASCII.GetBytes($"Content-Length: {json.Length}\r\n\r\n");
        await _output.WriteAsync(header, cancellationToken);
        await _output.WriteAsync(json, cancellationToken);
        await _output.FlushAsync(cancellationToken);
    }

    private async Task<JsonElement> ReadMessageAsync(CancellationToken cancellationToken)
    {
        var headerBytes = await ReadHeaderAsync(cancellationToken);
        var headerText = Encoding.ASCII.GetString(headerBytes);
        var contentLength = ParseContentLength(headerText);
        var bodyBytes = await ReadExactlyAsync(contentLength, cancellationToken);
        using var document = JsonDocument.Parse(bodyBytes);
        return document.RootElement.Clone();
    }

    private async Task<byte[]> ReadHeaderAsync(CancellationToken cancellationToken)
    {
        var buffer = new List<byte>();
        while (true)
        {
            var nextByte = await ReadByteAsync(cancellationToken);
            if (nextByte < 0)
            {
                throw new EndOfStreamException("Unexpected end of stream while reading the LSP header.");
            }

            buffer.Add((byte)nextByte);
            if (buffer.Count >= 4 &&
                buffer[^4] == '\r' &&
                buffer[^3] == '\n' &&
                buffer[^2] == '\r' &&
                buffer[^1] == '\n')
            {
                return [.. buffer];
            }
        }
    }

    private async Task<int> ReadByteAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1];
        var bytesRead = await _input.ReadAsync(buffer, cancellationToken);
        return bytesRead == 0 ? -1 : buffer[0];
    }

    private async Task<byte[]> ReadExactlyAsync(int length, CancellationToken cancellationToken)
    {
        var buffer = new byte[length];
        var offset = 0;
        while (offset < length)
        {
            var bytesRead = await _input.ReadAsync(buffer.AsMemory(offset, length - offset), cancellationToken);
            if (bytesRead == 0)
            {
                throw new EndOfStreamException("Unexpected end of stream while reading the LSP body.");
            }

            offset += bytesRead;
        }

        return buffer;
    }

    private static int ParseContentLength(string headerText)
    {
        foreach (var line in headerText.Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
        {
            const string prefix = "Content-Length:";
            if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(line[prefix.Length..].Trim(), out var contentLength))
            {
                return contentLength;
            }
        }

        throw new InvalidOperationException("The LSP header did not include a Content-Length value.");
    }
}
