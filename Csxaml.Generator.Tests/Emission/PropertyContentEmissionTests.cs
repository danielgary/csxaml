namespace Csxaml.Generator.Tests.Emission;

[TestClass]
public sealed class PropertyContentEmissionTests
{
    [TestMethod]
    public void Emit_NativePropertyContent_BuildsDedicatedPropertyContent()
    {
        var component = GeneratorTestHarness.Parse(
            "Panel.csxaml",
            """
            component Element Panel {
                render <Border>
                    <Border.Child>
                        <TextBlock Text="Hello" />
                    </Border.Child>
                </Border>;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);

        StringAssert.Contains(emitted, "new NativePropertyContentValue(");
        StringAssert.Contains(emitted, "\"Child\"");
        StringAssert.Contains(emitted, "propertyContent");
    }
}
