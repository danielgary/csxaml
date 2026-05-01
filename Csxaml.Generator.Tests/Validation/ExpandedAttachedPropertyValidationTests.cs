namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class ExpandedAttachedPropertyValidationTests
{
    [TestMethod]
    public void Validate_BroaderLayoutAttachedProperties_AreAccepted()
    {
        var component = GeneratorTestHarness.Parse(
            "AttachedSurface.csxaml",
            """
            using Microsoft.UI.Xaml.Controls;

            component Element AttachedSurface {
                render <Grid>
                    <Canvas>
                        <Button Canvas.Left={20.5} Canvas.Top={12} Canvas.ZIndex={2} Content="Move" />
                    </Canvas>
                    <RelativePanel>
                        <TextBlock RelativePanel.AlignLeftWithPanel={true} RelativePanel.AlignTopWithPanel={true} Text="Anchor" />
                        <TextBlock RelativePanel.RightOf="Anchor" RelativePanel.Below="Anchor" Text="Follower" />
                    </RelativePanel>
                    <VariableSizedWrapGrid>
                        <Border VariableSizedWrapGrid.ColumnSpan={2} VariableSizedWrapGrid.RowSpan={3} />
                    </VariableSizedWrapGrid>
                    <Border ToolTipService.ToolTip="Saved" />
                    <ListView
                        ScrollViewer.HorizontalScrollMode={ScrollMode.Disabled}
                        ScrollViewer.VerticalScrollBarVisibility={ScrollBarVisibility.Auto}
                        ScrollViewer.VerticalScrollMode={ScrollMode.Enabled} />
                </Grid>;
            }
            """);

        GeneratorTestHarness.Validate(component);
    }

    [TestMethod]
    public void Validate_ExpandedAutomationAttachedProperties_AreAccepted()
    {
        var component = GeneratorTestHarness.Parse(
            "AutomationSurface.csxaml",
            """
            using Microsoft.UI.Xaml.Automation;
            using Microsoft.UI.Xaml.Controls;

            component Element AutomationSurface {
                render <TextBlock
                    AutomationProperties.AutomationId="TotalText"
                    AutomationProperties.Name="Total"
                    AutomationProperties.HelpText="Shows the current total"
                    AutomationProperties.ItemStatus="Ready"
                    AutomationProperties.ItemType="Summary"
                    AutomationProperties.LabeledBy={null}
                    Text="42" />;
            }
            """);

        GeneratorTestHarness.Validate(component);
    }

    [TestMethod]
    public void Validate_CanvasAttachedPropertyOutsideCanvas_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "AttachedSurface.csxaml",
            """
            using Microsoft.UI.Xaml.Controls;

            component Element AttachedSurface {
                render <StackPanel>
                    <Button Canvas.Left={20.5} Content="Move" />
                </StackPanel>;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "attached property 'Canvas.Left' on 'Button' requires parent 'Canvas'");
    }

    [TestMethod]
    public void Validate_NewAttachedPropertyOwnerTypeAlias_IsAccepted()
    {
        var component = GeneratorTestHarness.Parse(
            "AttachedSurface.csxaml",
            """
            using C = Microsoft.UI.Xaml.Controls.Canvas;
            using Microsoft.UI.Xaml.Controls;

            component Element AttachedSurface {
                render <Canvas>
                    <Button C.Left={20.5} Content="Move" />
                </Canvas>;
            }
            """);

        GeneratorTestHarness.Validate(component);
    }

    [TestMethod]
    public void Validate_TypedObjectAttachedPropertyStringLiteral_ThrowsDiagnostic()
    {
        var component = GeneratorTestHarness.Parse(
            "AutomationSurface.csxaml",
            """
            using Microsoft.UI.Xaml.Automation;

            component Element AutomationSurface {
                render <TextBlock AutomationProperties.LabeledBy="Title" Text="42" />;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(
            exception.Diagnostic.Message,
            "attached property 'AutomationProperties.LabeledBy' on 'TextBlock' requires an expression value");
    }
}
