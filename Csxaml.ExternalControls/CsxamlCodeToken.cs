namespace Csxaml.ExternalControls;

internal enum CsxamlCodeTokenKind
{
    Text,
    Keyword,
    Markup,
    String,
    Expression
}

internal readonly record struct CsxamlCodeToken(string Text, CsxamlCodeTokenKind Kind);
