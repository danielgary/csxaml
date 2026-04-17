namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class AttachedPropertyValidationTests
{
    [TestMethod]
    public void Validate_GridAttachedPropertyInsideGrid_IsAccepted()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using Microsoft.UI.Xaml.Controls;

            component Element TodoBoard {
                render <Grid>
                    <TextBlock Grid.Row={1} Grid.Column={0} Text="Todo Board" />
                </Grid>;
            }
            """);

        GeneratorTestHarness.Validate(component);
    }

    [TestMethod]
    public void Validate_ComponentAttachedPropertyInsideGrid_IsAccepted()
    {
        var editor = GeneratorTestHarness.Parse(
            "TodoEditor.csxaml",
            """
            component Element TodoEditor(string Title) {
                render <Border>
                    <TextBlock Text={Title} />
                </Border>;
            }
            """);
        var board = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using Microsoft.UI.Xaml.Automation;
            using Microsoft.UI.Xaml.Controls;

            component Element TodoBoard {
                render <Grid>
                    <TodoEditor Grid.Column={1} AutomationProperties.Name="Task Editor" Title="Draft plan" />
                </Grid>;
            }
            """);

        GeneratorTestHarness.Validate(editor, board);
    }

    [TestMethod]
    public void Validate_GridAttachedPropertyInsideForeachUnderGrid_IsAccepted()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using Microsoft.UI.Xaml.Controls;

            component Element TodoBoard {
                render <Grid>
                    foreach (var item in new[] { 0, 1 }) {
                        <TextBlock Grid.Row={item} Text="Todo Board" />
                    }
                </Grid>;
            }
            """);

        GeneratorTestHarness.Validate(component);
    }

    [TestMethod]
    public void Validate_GridAttachedPropertyOutsideGrid_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                render <StackPanel>
                    <TextBlock Grid.Row={1} Text="Todo Board" />
                </StackPanel>;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "unknown attached property 'Grid.Row' on 'TextBlock'");
    }

    [TestMethod]
    public void Validate_DuplicateAttachedProperty_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using Microsoft.UI.Xaml.Controls;

            component Element TodoBoard {
                render <Grid>
                    <TextBlock Grid.Row={0} Grid.Row={1} Text="Todo Board" />
                </Grid>;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "duplicate attribute name 'Grid.Row' on native control 'TextBlock'");
    }

    [TestMethod]
    public void Validate_NumericStringAttachedPropertyValue_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using Microsoft.UI.Xaml.Controls;

            component Element TodoBoard {
                render <Grid>
                    <TextBlock Grid.Row="1" Text="Todo Board" />
                </Grid>;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "attached property 'Grid.Row' on 'TextBlock' requires an expression value");
    }

    [TestMethod]
    public void Validate_AttachedPropertyOwnerTypeAlias_IsAccepted()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using AP = Microsoft.UI.Xaml.Automation.AutomationProperties;

            component Element TodoBoard {
                render <TextBlock AP.Name="Todo Board" />;
            }
            """);

        GeneratorTestHarness.Validate(component);
    }

    [TestMethod]
    public void Validate_AttachedPropertyOwnerNamespaceAlias_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using Automation = Microsoft.UI.Xaml.Automation;

            component Element TodoBoard {
                render <TextBlock Automation.Name="Todo Board" />;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "unknown attached property 'Automation.Name' on 'TextBlock'");
    }

    [TestMethod]
    public void Validate_AttachedPropertyRequiresVisibleOwnerType()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                render <Grid>
                    <TextBlock Grid.Row={0} Text="Todo Board" />
                </Grid>;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "unknown attached property 'Grid.Row' on 'TextBlock'");
    }
}
