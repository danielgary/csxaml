using System.Diagnostics;
using System.IO.Pipelines;
using Csxaml.Tooling.Core.Bootstrap;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.LanguageServer;
using Nerdbank.Streams;

namespace Csxaml.VisualStudio;

/// <summary>
/// Starts and connects Visual Studio to the CSXAML language server process.
/// </summary>
[VisualStudioContribution]
public sealed class CsxamlLanguageServerProvider : LanguageServerProvider
{
    private Process? _process;

    /// <summary>
    /// Gets the language server registration used by Visual Studio for CSXAML documents.
    /// </summary>
    public override LanguageServerProviderConfiguration LanguageServerProviderConfiguration => new(
        "%CsxamlLanguageServer.DisplayName%",
        [DocumentFilter.FromDocumentType(CsxamlDocumentTypeConfiguration.CsxamlDocumentType)]);

    /// <summary>
    /// Creates the duplex pipe used by Visual Studio to communicate with the CSXAML language server.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels language server startup.</param>
    /// <returns>The language server communication pipe.</returns>
    public override Task<IDuplexPipe?> CreateServerConnectionAsync(CancellationToken cancellationToken)
    {
        var extensionDirectory = Path.GetDirectoryName(typeof(CsxamlLanguageServerProvider).Assembly.Location)
            ?? throw new InvalidOperationException("Could not determine the extension directory.");
        var executablePath = Path.Combine(extensionDirectory, "LanguageServer", "Csxaml.LanguageServer.exe");
        CsxamlExtensionLog.Write($"Preparing to start language server from '{executablePath}'.");
        if (!File.Exists(executablePath))
        {
            CsxamlExtensionLog.Write("Language server executable was not found.");
            throw new FileNotFoundException("The CSXAML language server executable was not found.", executablePath);
        }

        var startInfo = new ProcessStartInfo(executablePath)
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        ConfigureDotNetRoot(startInfo, executablePath);

        _process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The CSXAML language server process did not start.");
        _process.EnableRaisingEvents = true;
        AttachProcessLogging(_process);
        CsxamlExtensionLog.Write($"Language server process started with pid {_process.Id}.");

        var input = PipeReader.Create(_process.StandardOutput.BaseStream);
        var output = PipeWriter.Create(_process.StandardInput.BaseStream);
        return Task.FromResult<IDuplexPipe?>(new DuplexPipe(input, output));
    }

    /// <summary>
    /// Releases the language server process owned by the provider.
    /// </summary>
    /// <param name="disposing">A value indicating whether managed resources should be disposed.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && _process is { HasExited: false })
        {
            try
            {
                CsxamlExtensionLog.Write($"Stopping language server process {_process.Id}.");
                _process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }
        }

        base.Dispose(disposing);
    }

    private static void ConfigureDotNetRoot(ProcessStartInfo startInfo, string executablePath)
    {
        var dotnetRoot = GetDotNetRoot(executablePath);
        if (dotnetRoot is null)
        {
            CsxamlExtensionLog.Write("Could not resolve DOTNET_ROOT for language server startup.");
            return;
        }

        startInfo.Environment["DOTNET_ROOT"] = dotnetRoot;
        startInfo.Environment["DOTNET_ROOT_X64"] = dotnetRoot;
        var path = startInfo.Environment["PATH"] ?? string.Empty;
        if (!path.Contains(dotnetRoot, StringComparison.OrdinalIgnoreCase))
        {
            startInfo.Environment["PATH"] = string.IsNullOrEmpty(path)
                ? dotnetRoot
                : $"{dotnetRoot}{Path.PathSeparator}{path}";
        }

        CsxamlExtensionLog.Write($"Using DOTNET_ROOT '{dotnetRoot}' for language server startup.");
    }

    private static string? GetDotNetRoot(string executablePath)
    {
        var configuredRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var defaultRoot = Path.Combine(programFiles, "dotnet");
        return DotNetRootResolver.Resolve(executablePath, configuredRoot, defaultRoot);
    }

    private static void AttachProcessLogging(Process process)
    {
        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                CsxamlExtensionLog.Write($"Language server stderr: {args.Data}");
            }
        };
        process.Exited += (_, _) => CsxamlExtensionLog.Write(
            $"Language server process {process.Id} exited with code {process.ExitCode}.");
        process.BeginErrorReadLine();
    }
}
