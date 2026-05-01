namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class TextBlockValidationTests
{
    [TestMethod]
    public void Validate_TextBlock_AllowsTextWrapping()
    {
        var component = GeneratorTestHarness.Parse(
            "WrappingText.csxaml",
            """
            using Microsoft.UI.Xaml;

            component Element WrappingText {
                render <TextBlock
                    Text="Long text"
                    TextWrapping={TextWrapping.Wrap} />;
            }
            """);

        GeneratorTestHarness.Validate(component);
    }
}
