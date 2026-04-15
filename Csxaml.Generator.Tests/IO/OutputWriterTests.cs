namespace Csxaml.Generator.Tests.IO;

[TestClass]
public sealed class OutputWriterTests
{
    [TestMethod]
    public void WriteAll_PrunesStaleFilesWhenTheGeneratedSetShrinks()
    {
        var outputDirectory = CreateOutputDirectory();

        try
        {
            var firstFile = Path.Combine(outputDirectory, "First.g.cs");
            var secondFile = Path.Combine(outputDirectory, "Second.g.cs");
            OutputWriter.WriteAll(
                outputDirectory,
                new[]
                {
                    new GeneratedFile(firstFile, "first"),
                    new GeneratedFile(secondFile, "second")
                });

            OutputWriter.WriteAll(
                outputDirectory,
                new[]
                {
                    new GeneratedFile(firstFile, "first")
                });

            Assert.IsTrue(File.Exists(firstFile));
            Assert.IsFalse(File.Exists(secondFile));
        }
        finally
        {
            DeleteOutputDirectory(outputDirectory);
        }
    }

    [TestMethod]
    public void WriteAll_DoesNotRewriteUnchangedFiles()
    {
        var outputDirectory = CreateOutputDirectory();

        try
        {
            var outputPath = Path.Combine(outputDirectory, "Stable.g.cs");
            OutputWriter.WriteAll(
                outputDirectory,
                new[]
                {
                    new GeneratedFile(outputPath, "same content")
                });

            var originalWriteTime = File.GetLastWriteTimeUtc(outputPath);
            Thread.Sleep(1100);

            OutputWriter.WriteAll(
                outputDirectory,
                new[]
                {
                    new GeneratedFile(outputPath, "same content")
                });

            Assert.AreEqual(originalWriteTime, File.GetLastWriteTimeUtc(outputPath));
        }
        finally
        {
            DeleteOutputDirectory(outputDirectory);
        }
    }

    [TestMethod]
    public void WriteAll_RejectsPathsOutsideTheConfiguredOutputDirectory()
    {
        var outputDirectory = CreateOutputDirectory();
        var invalidPath = Path.Combine(outputDirectory, "..", "..", "Escaped.g.cs");

        try
        {
            var exception = Assert.ThrowsExactly<InvalidOperationException>(
                () => OutputWriter.WriteAll(
                    outputDirectory,
                    new[]
                    {
                        new GeneratedFile(invalidPath, "bad")
                    }));

            StringAssert.Contains(exception.Message, "must stay under");
        }
        finally
        {
            DeleteOutputDirectory(outputDirectory);
        }
    }

    private static string CreateOutputDirectory()
    {
        var root = Path.Combine(
            AppContext.BaseDirectory,
            "OutputWriterTests",
            Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "Generated");
        Directory.CreateDirectory(outputDirectory);
        return outputDirectory;
    }

    private static void DeleteOutputDirectory(string outputDirectory)
    {
        var root = Directory.GetParent(outputDirectory)?.FullName ?? outputDirectory;
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
