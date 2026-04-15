using Csxaml.Tooling.Core.Bootstrap;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.Extensibility.Editor;

namespace Csxaml.VisualStudio.Commands;

[VisualStudioContribution]
public sealed class InspectActiveCsxamlDocumentCommand : Command
{
    private readonly CsxamlBootstrapProbe _probe;

    public InspectActiveCsxamlDocumentCommand(CsxamlBootstrapProbe probe)
    {
        _probe = probe;
    }

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
