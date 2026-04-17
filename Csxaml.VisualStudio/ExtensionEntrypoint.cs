using Csxaml.Tooling.Core.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace Csxaml.VisualStudio;

[VisualStudioContribution]
public sealed class ExtensionEntrypoint : Extension
{
    public ExtensionEntrypoint()
    {
        CsxamlExtensionLog.Write("ExtensionEntrypoint constructed.");
    }

    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
            id: "Csxaml.VisualStudio",
            version: this.ExtensionAssemblyVersion,
            publisherName: "danielgarysoftware",
            displayName: "CSXAML Bootstrap",
            description: "Bootstraps the first Visual Studio checkpoint for CSXAML authoring."),
    };

    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);
        CsxamlExtensionLog.Write("ExtensionEntrypoint.InitializeServices called.");
        serviceCollection.AddSingleton<CsxamlBootstrapProbe>();
    }
}
