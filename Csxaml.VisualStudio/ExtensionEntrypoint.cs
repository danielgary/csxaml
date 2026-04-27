using Csxaml.Tooling.Core.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace Csxaml.VisualStudio;

/// <summary>
/// Registers the CSXAML Visual Studio extension and its shared services.
/// </summary>
[VisualStudioContribution]
public sealed class ExtensionEntrypoint : Extension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionEntrypoint"/> class.
    /// </summary>
    public ExtensionEntrypoint()
    {
        CsxamlExtensionLog.Write("ExtensionEntrypoint constructed.");
    }

    /// <summary>
    /// Gets the Visual Studio extension metadata used for VSIX packaging and marketplace publication.
    /// </summary>
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
            id: "Csxaml.VisualStudio",
            version: this.ExtensionAssemblyVersion,
            publisherName: "Daniel Gary Software",
            displayName: "CSXAML Bootstrap",
            description: "Bootstraps the first Visual Studio checkpoint for CSXAML authoring."),
    };

    /// <summary>
    /// Registers services used by Visual Studio extension contributions.
    /// </summary>
    /// <param name="serviceCollection">The service collection provided by the Visual Studio extension host.</param>
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);
        CsxamlExtensionLog.Write("ExtensionEntrypoint.InitializeServices called.");
        serviceCollection.AddSingleton<CsxamlBootstrapProbe>();
    }
}
