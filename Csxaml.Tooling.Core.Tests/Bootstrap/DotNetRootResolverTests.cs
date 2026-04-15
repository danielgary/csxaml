using Csxaml.Tooling.Core.Bootstrap;

namespace Csxaml.Tooling.Core.Tests.Bootstrap;

[TestClass]
public sealed class DotNetRootResolverTests
{
    [TestMethod]
    public void Resolve_prefers_compatible_configured_root()
    {
        using var workspace = new TemporaryTestDirectory();
        var executablePath = workspace.CreateExecutable("Csxaml.LanguageServer.exe", "10.0.0");
        var configuredRoot = workspace.CreateDotNetRoot("configured", "Microsoft.NETCore.App", "10.0.5");
        var defaultRoot = workspace.CreateDotNetRoot("default", "Microsoft.NETCore.App", "10.0.4");

        var resolvedRoot = DotNetRootResolver.Resolve(executablePath, configuredRoot, defaultRoot);

        Assert.AreEqual(configuredRoot, resolvedRoot);
    }

    [TestMethod]
    public void Resolve_falls_back_to_compatible_default_root_when_configured_root_is_incompatible()
    {
        using var workspace = new TemporaryTestDirectory();
        var executablePath = workspace.CreateExecutable("Csxaml.LanguageServer.exe", "10.0.0");
        var configuredRoot = workspace.CreateDotNetRoot("configured", "Microsoft.NETCore.App", "8.0.25");
        var defaultRoot = workspace.CreateDotNetRoot("default", "Microsoft.NETCore.App", "10.0.5");

        var resolvedRoot = DotNetRootResolver.Resolve(executablePath, configuredRoot, defaultRoot);

        Assert.AreEqual(defaultRoot, resolvedRoot);
    }

    [TestMethod]
    public void Resolve_returns_existing_configured_root_when_runtime_config_is_missing()
    {
        using var workspace = new TemporaryTestDirectory();
        var executablePath = Path.Combine(workspace.Path, "Csxaml.LanguageServer.exe");
        File.WriteAllText(executablePath, string.Empty);
        var configuredRoot = workspace.CreateDotNetRoot("configured", "Microsoft.NETCore.App", "8.0.25");
        var defaultRoot = workspace.CreateDotNetRoot("default", "Microsoft.NETCore.App", "10.0.5");

        var resolvedRoot = DotNetRootResolver.Resolve(executablePath, configuredRoot, defaultRoot);

        Assert.AreEqual(configuredRoot, resolvedRoot);
    }

    private sealed class TemporaryTestDirectory : IDisposable
    {
        public TemporaryTestDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"csxaml-dotnet-root-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string CreateDotNetRoot(string rootName, string frameworkName, string frameworkVersion)
        {
            var rootPath = System.IO.Path.Combine(Path, rootName);
            Directory.CreateDirectory(System.IO.Path.Combine(rootPath, "shared", frameworkName, frameworkVersion));
            return rootPath;
        }

        public string CreateExecutable(string fileName, string frameworkVersion)
        {
            var executablePath = System.IO.Path.Combine(Path, fileName);
            File.WriteAllText(executablePath, string.Empty);
            File.WriteAllText(
                System.IO.Path.ChangeExtension(executablePath, ".runtimeconfig.json"),
                $$"""
                {
                  "runtimeOptions": {
                    "framework": {
                      "name": "Microsoft.NETCore.App",
                      "version": "{{frameworkVersion}}"
                    }
                  }
                }
                """);
            return executablePath;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
