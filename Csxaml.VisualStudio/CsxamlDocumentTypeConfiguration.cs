using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.LanguageServer;

namespace Csxaml.VisualStudio;

internal static class CsxamlDocumentTypeConfiguration
{
    [VisualStudioContribution]
    public static DocumentTypeConfiguration CsxamlDocumentType => new("csxaml")
    {
        FileExtensions = [".csxaml"],
        BaseDocumentType = LanguageServerProvider.LanguageServerBaseDocumentType,
    };
}
