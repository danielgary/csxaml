namespace Csxaml.Generator.Tests.Emission;

[TestClass]
public sealed class InjectEmissionTests
{
    [TestMethod]
    public void Emit_ComponentWithInject_GeneratesResolutionHookAndKeepsPropsSeparate()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard(string Title) {
                inject ITodoService todoService;

                render <TextBlock Text={Title} />;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);

        StringAssert.Contains(emitted, "public sealed record TodoBoardProps(string Title);");
        StringAssert.Contains(emitted, "private ITodoService? _todoService;");
        StringAssert.Contains(emitted, "private ITodoService todoService =>");
        StringAssert.Contains(emitted, "protected override void ResolveInjectedServices(IServiceProvider services)");
        StringAssert.Contains(emitted, "InjectedServiceResolver.ResolveRequired<ITodoService>");
        Assert.IsFalse(
            emitted.Contains("public sealed record TodoBoardProps(string Title, ITodoService todoService);", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Emit_StateInitialization_UsesLazyRenderBootstrap()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                State<int> Count = new State<int>(1);
                render <TextBlock Text={Count.Value} />;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);

        StringAssert.Contains(emitted, "Count ??= CreateCountState();");
        StringAssert.Contains(emitted, "State<int> CreateCountState()");
        StringAssert.Contains(emitted, "InvalidateState,");
        StringAssert.Contains(emitted, "ValidateStateWrite");
    }

    [TestMethod]
    public void Compile_InjectAndStateInitializerUsingInjectedHelper_CompilesCleanly()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            interface ICounterService
            {
                int GetInitial();
            }

            component Element TodoBoard {
                inject ICounterService counterService;
                State<int> Count = new State<int>(BuildInitialCount());

                int BuildInitialCount()
                {
                    return counterService.GetInitial();
                }

                render <TextBlock Text={Count.Value} />;
            }
            """);

        var diagnostics = GeneratedCompilationTestHarness.Compile(GeneratorTestHarness.Emit(component));
        var errors = diagnostics
            .Where(diagnostic => diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .Select(diagnostic => diagnostic.ToString())
            .ToArray();

        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
    }
}
