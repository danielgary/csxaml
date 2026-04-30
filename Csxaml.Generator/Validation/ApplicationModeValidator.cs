namespace Csxaml.Generator;

internal sealed class ApplicationModeValidator
{
    public void Validate(
        IReadOnlyList<ParsedComponent> components,
        CompilationContext compilation)
    {
        var applicationRoots = components
            .Where(component => component.Definition.Kind == ComponentKind.Application)
            .ToList();

        if (compilation.Project.ApplicationMode == CsxamlApplicationMode.Hybrid)
        {
            ValidateHybridMode(applicationRoots);
            return;
        }

        ValidateGeneratedMode(components, compilation, applicationRoots);
    }

    private static void ValidateHybridMode(IReadOnlyList<ParsedComponent> applicationRoots)
    {
        if (applicationRoots.Count == 0)
        {
            return;
        }

        var application = applicationRoots[0];
        throw DiagnosticFactory.FromSpan(
            application.Source,
            application.Definition.Span,
            "component Application requires CsxamlApplicationMode=Generated; keep App.xaml/App.xaml.cs or enable generated application mode");
    }

    private static void ValidateGeneratedMode(
        IReadOnlyList<ParsedComponent> components,
        CompilationContext compilation,
        IReadOnlyList<ParsedComponent> applicationRoots)
    {
        if (applicationRoots.Count == 0)
        {
            throw DiagnosticFactory.FromPosition(
                components[0].Source,
                0,
                "CsxamlApplicationMode=Generated requires exactly one component Application declaration");
        }

        if (applicationRoots.Count > 1)
        {
            var duplicate = applicationRoots[1];
            throw DiagnosticFactory.FromSpan(
                duplicate.Source,
                duplicate.Definition.Span,
                "CsxamlApplicationMode=Generated supports only one component Application declaration");
        }

        ValidateApplicationReferences(applicationRoots[0], compilation);
    }

    private static void ValidateApplicationReferences(
        ParsedComponent application,
        CompilationContext compilation)
    {
        var declaration = application.Definition.Application
            ?? throw DiagnosticFactory.FromSpan(
                application.Source,
                application.Definition.Span,
                "component Application is missing startup metadata");
        ValidateReference(
            application,
            compilation,
            declaration.StartupTypeName,
            declaration.StartupSpan,
            Csxaml.ControlMetadata.ComponentKind.Window,
            "startup");

        if (declaration.ResourcesTypeName is null || declaration.ResourcesSpan is null)
        {
            return;
        }

        ValidateReference(
            application,
            compilation,
            declaration.ResourcesTypeName,
            declaration.ResourcesSpan.Value,
            Csxaml.ControlMetadata.ComponentKind.ResourceDictionary,
            "resources");
    }

    private static void ValidateReference(
        ParsedComponent application,
        CompilationContext compilation,
        string typeName,
        TextSpan span,
        Csxaml.ControlMetadata.ComponentKind expectedKind,
        string statementName)
    {
        var matches = compilation.Components.FindByName(typeName);
        if (matches.Any(component => component.Kind == expectedKind))
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            application.Source,
            span,
            $"Application {statementName} target '{typeName}' must reference a generated {expectedKind} root");
    }
}
