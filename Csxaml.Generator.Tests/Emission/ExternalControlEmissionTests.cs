namespace Csxaml.Generator.Tests.Emission;

[TestClass]
public sealed class ExternalControlEmissionTests
{
    [TestMethod]
    public void Emit_ExternalControl_UsesResolvedRuntimeKeyAndGeneratedRegistration()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            using Demo = MyApp.Controls;

            component Element TodoBoard {
                return <Demo:StatusButton BadgeText="todo-1" Style={TodoStyles.PrimaryButton} OnClick={() => { }}>
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
                    new PropertyMetadata("BadgeText", "System.String", true, true, false, true, ValueKindHint.String),
                    new PropertyMetadata("Style", "Microsoft.UI.Xaml.Style", true, true, false, true, ValueKindHint.Unknown)
                ],
                [
                    new EventMetadata("Click", "OnClick", "System.Action", true, ValueKindHint.Unknown, EventBindingKind.Direct)
                ]));

        var emitted = new CodeEmitter().Emit(component, compilation);

        StringAssert.Contains(emitted, "using Demo = MyApp.Controls;");
        StringAssert.Contains(emitted, "GeneratedExternalControlRegistration.EnsureRegistered();");
        StringAssert.Contains(emitted, "new NativeElementNode(");
        StringAssert.Contains(emitted, "\"MyApp.Controls.StatusButton\"");
        StringAssert.Contains(emitted, "new NativePropertyValue(\"BadgeText\", \"todo-1\"");
        StringAssert.Contains(emitted, "\"Style\"");
        StringAssert.Contains(emitted, "TodoStyles.PrimaryButton");
        StringAssert.Contains(emitted, "new NativeEventValue(");
        StringAssert.Contains(emitted, "\"OnClick\"");
        StringAssert.Contains(emitted, "(global::System.Action)(");
    }

    [TestMethod]
    public void Emit_ExternalRegistrationSupportFile_RegistersDiscoveredControls()
    {
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
        var writer = new IndentedCodeWriter();

        new GeneratedExternalControlRegistrationEmitter(writer).Emit(compilation);

        var emitted = writer.ToString();
        StringAssert.Contains(emitted, "global::Csxaml.Runtime.ExternalControlRegistry.Register(");
        StringAssert.Contains(emitted, "typeof(global::MyApp.Controls.StatusButton)");
        StringAssert.Contains(emitted, "new global::Csxaml.ControlMetadata.ControlMetadata(");
        StringAssert.Contains(emitted, "new global::Csxaml.ControlMetadata.PropertyMetadata(\"BadgeText\"");
        StringAssert.Contains(emitted, "new global::Csxaml.ControlMetadata.EventMetadata(\"Click\", \"OnClick\", \"System.Action\"");
    }

    private static CompilationContext CreateCompilation(params Csxaml.ControlMetadata.ControlMetadata[] externalControls)
    {
        return new CompilationContext(
            new ComponentCatalog(Array.Empty<ComponentCatalogEntry>()),
            new NativeControlCatalog(externalControls));
    }
}
