using Csxaml.Tooling.Core.Bootstrap;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.Extensibility.Editor;

namespace Csxaml.VisualStudio.Commands;

/// <summary>
/// Shows bootstrap details for the active CSXAML document in Visual Studio.
/// </summary>
[VisualStudioContribution]
public sealed class InspectActiveCsxamlDocumentCommand : Command
{
    private readonly CsxamlBootstrapProbe _probe;

    /// <summary>
    /// Initializes a new instance of the <see cref="InspectActiveCsxamlDocumentCommand"/> class.
    /// </summary>
    /// <param name="probe">The bootstrap probe used to inspect the active document.</param>
    public InspectActiveCsxamlDocumentCommand(CsxamlBootstrapProbe probe)
    {
        _probe = probe;
    }

    /// <summary>
    /// Gets the Visual Studio command registration and activation rules.
    /// </summary>
    public override CommandConfiguration CommandConfiguration => new("%CsxamlBootstrap.InspectActiveDocument.DisplayName%")
    {
        Placements = new[]
        {
            CommandPlacement.KnownPlacements.ExtensionsMenu,
        },
        Icon = new(ImageMoniker.KnownValues.OfficeWebExtension, IconSettings.IconAndText),
        VisibleWhen = ActivationConstraint.ClientContext(
            ClientContextKey.Shell.ActiveEditorFileName,
            @"(?i).*\.csxaml$"),
    };

    /// <summary>
    /// Executes the command against the active editor document.
    /// </summary>
    /// <param name="context">The Visual Studio client context.</param>
    /// <param name="cancellationToken">A token that cancels command execution.</param>
    /// <returns>A task that completes when the command has shown its prompt.</returns>
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var textView = await context.GetActiveTextViewAsync(cancellationToken);
        var fileName = textView?.FilePath ?? context[ClientContextKey.Shell.ActiveEditorFileName];
        var result = _probe.Probe(fileName);

        var message = string.Join(
            Environment.NewLine,
            result.Message,
            $"File: {result.NormalizedFileName}",
            $"Content type target: {CsxamlBootstrapProbe.ContentTypeName}");

        await this.Extensibility.Shell().ShowPromptAsync(message, PromptOptions.OK, cancellationToken);
    }
}
