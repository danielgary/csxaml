namespace Csxaml.Generator.Tests.Emission;

[TestClass]
public sealed class UsingDirectiveEmissionTests
{
    [TestMethod]
    public void Emit_StaticUsingDirective_PreservesStaticImport()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using static System.Math;

            component Element TodoBoard {
                render <TextBlock Text={Max(1, 2).ToString()} />;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);

        StringAssert.Contains(emitted, "using static System.Math;");
        StringAssert.Contains(emitted, "Max(1, 2).ToString()");
    }
}
