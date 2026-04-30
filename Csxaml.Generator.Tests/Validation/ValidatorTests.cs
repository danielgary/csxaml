namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class ValidatorTests
{
    [TestMethod]
    public void Validate_MissingRequiredProp_ThrowsDiagnostic()
    {
        var card = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
                render <StackPanel>
                    <TextBlock Text={Title} />
                    <Button Content="Toggle" OnClick={OnToggle} />
                </StackPanel>;
            }
            """);

        var board = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                render <StackPanel>
                    <TodoCard Title="One" IsDone={false} />
                </StackPanel>;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(card, board));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "missing required prop 'OnToggle'");
    }

    [TestMethod]
    public void Validate_StringRefOnNativeControl_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "SearchPanel.csxaml",
            """
            component Element SearchPanel {
                render <TextBox Ref="SearchBox" />;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "native ref on 'TextBox' requires an expression value");
    }

    [TestMethod]
    public void Validate_RefOnComponent_ThrowsDiagnostic()
    {
        var card = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard(string Title) {
                render <TextBlock Text={Title} />;
            }
            """);
        var board = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                ElementRef<object> CardRef = new ElementRef<object>();

                render <TodoCard Ref={CardRef} Title="One" />;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(card, board));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "Ref is not supported on component 'TodoCard'");
    }

    [TestMethod]
    public void Validate_MetadataSingleContentProperty_NamesPropertyInDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "ControlExampleHost.csxaml",
            """
            component Element ControlExampleHost {
                render <ControlExample>
                    <TextBlock />
                    <Button />
                </ControlExample>;
            }
            """);
        var control = new Csxaml.ControlMetadata.ControlMetadata(
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
            Array.Empty<PropertyMetadata>(),
            Array.Empty<EventMetadata>());

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => ValidateRoot(component, control));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "supports only one child for 'Example'");
    }

    [TestMethod]
    public void Validate_UnsupportedContentProperty_NamesPropertyInDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "UnsupportedContentHost.csxaml",
            """
            component Element UnsupportedContentHost {
                render <UnsupportedContentExample>
                    <TextBlock />
                </UnsupportedContentExample>;
            }
            """);
        var control = new Csxaml.ControlMetadata.ControlMetadata(
            "UnsupportedContentExample",
            "MyApp.Controls.UnsupportedContentExample",
            null,
            ControlChildKind.None,
            new ControlContentMetadata(
                "UnsupportedContent",
                ControlContentKind.None,
                typeof(int).FullName!,
                null,
                ControlContentSource.ContentPropertyAttribute),
            Array.Empty<PropertyMetadata>(),
            Array.Empty<EventMetadata>());

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => ValidateRoot(component, control));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "content property 'UnsupportedContent' has unsupported type 'System.Int32'");
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
}
