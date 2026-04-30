namespace Csxaml.Generator;

internal sealed class ApplicationEntryPointEmitter
{
    private readonly IndentedCodeWriter _writer;

    public ApplicationEntryPointEmitter(IndentedCodeWriter writer)
    {
        _writer = writer;
    }

    public void Emit(CompilationContext compilation)
    {
        var application = FindApplication(compilation);
        var applicationType = $"global::{application.NamespaceName}.{application.Name}";

        _writer.WriteLine("#nullable enable");
        _writer.WriteLine();
        _writer.WriteLine("internal static class Program");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("[global::System.STAThread]");
        _writer.WriteLine("private static void Main(string[] args)");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("global::WinRT.ComWrappersSupport.InitializeComWrappers();");
        _writer.WriteLine("global::Microsoft.UI.Xaml.Application.Start(_ =>");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(");
        _writer.PushIndent();
        _writer.WriteLine("global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());");
        _writer.PopIndent();
        _writer.WriteLine("global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);");
        _writer.WriteLine($"new {applicationType}();");
        _writer.PopIndent();
        _writer.WriteLine("});");
        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.PopIndent();
        _writer.WriteLine("}");
    }

    private static ComponentCatalogEntry FindApplication(CompilationContext compilation)
    {
        return compilation.Components
            .FindLocalComponents()
            .Single(component => component.Kind == Csxaml.ControlMetadata.ComponentKind.Application);
    }
}
