namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class PropertyContentValidationTests
{
    [TestMethod]
    public void Validate_NativePropertyContent_AllowsDefaultContentProperty()
    {
        var component = GeneratorTestHarness.Parse(
            "ControlExampleHost.csxaml",
            """
            component Element ControlExampleHost {
                render <ControlExample>
                    <ControlExample.Example>
                        <TextBlock Text="Run" />
                    </ControlExample.Example>
                </ControlExample>;
            }
            """);
        var control = CreateControlExampleMetadata();

        ValidateRoot(component, control);
    }

    [TestMethod]
    public void Validate_NativePropertyContent_RejectsWrongOwner()
    {
        var component = GeneratorTestHarness.Parse(
            "ControlExampleHost.csxaml",
            """
            component Element ControlExampleHost {
                render <ControlExample>
                    <Other.Example>
                        <TextBlock Text="Run" />
                    </Other.Example>
                </ControlExample>;
            }
            """);
        var control = CreateControlExampleMetadata();

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => ValidateRoot(component, control));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "property content owner 'Other' does not match parent 'ControlExample'");
    }

    [TestMethod]
    public void Validate_NativePropertyContent_RejectsAttributeCollision()
    {
        var component = GeneratorTestHarness.Parse(
            "ControlExampleHost.csxaml",
            """
            component Element ControlExampleHost {
                render <ControlExample Example={CreateExample()}>
                    <ControlExample.Example>
                        <TextBlock Text="Run" />
                    </ControlExample.Example>
                </ControlExample>;
            }
            """);
        var control = CreateControlExampleMetadata();

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => ValidateRoot(component, control));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "property 'Example' on native control 'ControlExample' is assigned by both attribute and property content");
    }

    [TestMethod]
    public void Validate_NativePropertyContent_RejectsDuplicateSingleProperty()
    {
        var component = GeneratorTestHarness.Parse(
            "ControlExampleHost.csxaml",
            """
            component Element ControlExampleHost {
                render <ControlExample>
                    <ControlExample.Example>
                        <TextBlock Text="One" />
                    </ControlExample.Example>
                    <ControlExample.Example>
                        <TextBlock Text="Two" />
                    </ControlExample.Example>
                </ControlExample>;
            }
            """);
        var control = CreateControlExampleMetadata();

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => ValidateRoot(component, control));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "property content 'ControlExample.Example' is assigned more than once");
    }

    [TestMethod]
    public void Validate_NativePropertyContent_RejectsUnknownProperty()
    {
        var component = GeneratorTestHarness.Parse(
            "ControlExampleHost.csxaml",
            """
            component Element ControlExampleHost {
                render <ControlExample>
                    <ControlExample.Unknown>
                        <TextBlock Text="Run" />
                    </ControlExample.Unknown>
                </ControlExample>;
            }
            """);
        var control = CreateControlExampleMetadata();

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => ValidateRoot(component, control));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "unknown property content 'Unknown' on native control 'ControlExample'");
    }

    [TestMethod]
    public void Validate_NativePropertyContent_RejectsMultipleChildrenForSingleProperty()
    {
        var component = GeneratorTestHarness.Parse(
            "ControlExampleHost.csxaml",
            """
            component Element ControlExampleHost {
                render <ControlExample>
                    <ControlExample.Example>
                        <TextBlock Text="One" />
                        <TextBlock Text="Two" />
                    </ControlExample.Example>
                </ControlExample>;
            }
            """);
        var control = CreateControlExampleMetadata();

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => ValidateRoot(component, control));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "property content 'ControlExample.Example' supports only one child");
    }

    [TestMethod]
    public void Validate_NativePropertyContent_RejectsAttributes()
    {
        var component = GeneratorTestHarness.Parse(
            "ControlExampleHost.csxaml",
            """
            component Element ControlExampleHost {
                render <ControlExample>
                    <ControlExample.Example Key="example">
                        <TextBlock Text="Run" />
                    </ControlExample.Example>
                </ControlExample>;
            }
            """);
        var control = CreateControlExampleMetadata();

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => ValidateRoot(component, control));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "property content 'ControlExample.Example' does not support attributes");
    }

    [TestMethod]
    public void Validate_RootPropertyContent_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "InvalidRoot.csxaml",
            """
            component Element InvalidRoot {
                render <Button.Flyout />;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "property content cannot be the component root");
    }

    private static void ValidateRoot(
        ParsedComponent component,
        Csxaml.ControlMetadata.ControlMetadata control)
    {
        var node = (MarkupNode)component.File.Component.Root;
        new NativeElementValidator().Validate(
            component.Source,
            node,
            control,
            parentTagName: null,
            new AttachedPropertyBindingResolver(component));
    }

    private static Csxaml.ControlMetadata.ControlMetadata CreateControlExampleMetadata()
    {
        return new Csxaml.ControlMetadata.ControlMetadata(
            "ControlExample",
            "MyApp.Controls.ControlExample",
            null,
            ControlChildKind.Single,
            new ControlContentMetadata(
                "Example",
                ControlContentKind.Single,
                "Microsoft.UI.Xaml.UIElement",
                null,
                ControlContentSource.ContentPropertyAttribute),
            [
                new PropertyMetadata(
                    "Example",
                    "Microsoft.UI.Xaml.UIElement",
                    true,
                    false,
                    false,
                    true,
                    ValueKindHint.Object)
            ],
            Array.Empty<EventMetadata>());
    }
}
