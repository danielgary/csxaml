namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class ExternalControlValidationTests
{
    [TestMethod]
    public void Validate_AliasQualifiedExternalControl_AllowsSupportedPropsAndEvents()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using Demo = MyApp.Controls;

            component Element TodoBoard {
                render <Demo:StatusButton BadgeText="todo-1" OnClick={() => { }}>
                    <TextBlock Text="Selected item" />
                </Demo:StatusButton>;
            }
            """);
        var compilation = CreateCompilation(
            new Csxaml.ControlMetadata.ControlMetadata(
                "MyApp.Controls.StatusButton",
                "MyApp.Controls.StatusButton",
                "Microsoft.UI.Xaml.Controls.Button",
                ControlChildKind.Single,
                [
                    new PropertyMetadata("BadgeText", "System.String", true, true, false, true, ValueKindHint.String)
                ],
                [
                    new EventMetadata("Click", "OnClick", "System.Action", true, ValueKindHint.Unknown, EventBindingKind.Direct)
                ]));

        new MarkupValidator().Validate(
            component.Source,
            component,
            TestAstAssertions.RequireMarkup(component.Definition.Root),
            compilation);
    }

    [TestMethod]
    public void Validate_AliasQualifiedExternalControl_AllowsStyleProp()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using Demo = MyApp.Controls;

            component Element TodoBoard {
                render <Demo:StatusButton Style={TodoStyles.PrimaryButton} />;
            }
            """);
        var compilation = CreateCompilation(
            new Csxaml.ControlMetadata.ControlMetadata(
                "MyApp.Controls.StatusButton",
                "MyApp.Controls.StatusButton",
                "Microsoft.UI.Xaml.Controls.Button",
                ControlChildKind.Single,
                [
                    new PropertyMetadata("Style", "Microsoft.UI.Xaml.Style", true, true, false, true, ValueKindHint.Unknown)
                ],
                Array.Empty<EventMetadata>()));

        new MarkupValidator().Validate(
            component.Source,
            component,
            TestAstAssertions.RequireMarkup(component.Definition.Root),
            compilation);
    }

    [TestMethod]
    public void Validate_BareImportedExternalControl_AmbiguityFailsDeterministically()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using Alpha.Controls;
            using Beta.Controls;

            component Element TodoBoard {
                render <StatusButton />;
            }
            """);
        var compilation = CreateCompilation(
            new Csxaml.ControlMetadata.ControlMetadata(
                "Alpha.Controls.StatusButton",
                "Alpha.Controls.StatusButton",
                "Microsoft.UI.Xaml.Controls.Button",
                ControlChildKind.None,
                Array.Empty<PropertyMetadata>(),
                Array.Empty<EventMetadata>()),
            new Csxaml.ControlMetadata.ControlMetadata(
                "Beta.Controls.StatusButton",
                "Beta.Controls.StatusButton",
                "Microsoft.UI.Xaml.Controls.Button",
                ControlChildKind.None,
                Array.Empty<PropertyMetadata>(),
                Array.Empty<EventMetadata>()));

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => new MarkupValidator().Validate(
                component.Source,
                component,
                TestAstAssertions.RequireMarkup(component.Definition.Root),
                compilation));

        StringAssert.Contains(error.Message, "ambiguous tag 'StatusButton'");
    }

    [TestMethod]
    public void Validate_UnsupportedAliasQualifiedExternalControl_ReportsReason()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using Demo = MyApp.Controls;

            component Element TodoBoard {
                render <Demo:FancyControl />;
            }
            """);
        var compilation = new CompilationContext(
            new ComponentCatalog(Array.Empty<ComponentCatalogEntry>()),
            new NativeControlCatalog(
                Array.Empty<Csxaml.ControlMetadata.ControlMetadata>(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["MyApp.Controls.FancyControl"] = "type must have a public parameterless constructor"
                }));

        var error = Assert.ThrowsExactly<DiagnosticException>(
            () => new MarkupValidator().Validate(
                component.Source,
                component,
                TestAstAssertions.RequireMarkup(component.Definition.Root),
                compilation));

        StringAssert.Contains(error.Message, "imported control 'Demo:FancyControl' is not supported");
        StringAssert.Contains(error.Message, "parameterless constructor");
    }

    private static CompilationContext CreateCompilation(params Csxaml.ControlMetadata.ControlMetadata[] externalControls)
    {
        return new CompilationContext(
            new ComponentCatalog(Array.Empty<ComponentCatalogEntry>()),
            new NativeControlCatalog(externalControls));
    }
}
