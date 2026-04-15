using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Csxaml.Tooling.Core.CSharp;

namespace Csxaml.Tooling.Core.SemanticTokens;

internal sealed class CsxamlCSharpSemanticTokenService
{
    private readonly CsxamlCSharpCompilationFactory _compilationFactory = new();
    private readonly CsxamlCSharpProjectionBuilder _projectionBuilder = new();

    public IReadOnlyList<CsxamlSemanticToken> GetTokens(string filePath, string text)
    {
        var projection = _projectionBuilder.Build(filePath, text);
        if (projection is null)
        {
            return Array.Empty<CsxamlSemanticToken>();
        }

        var compilation = _compilationFactory.Create(filePath, projection);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var semanticModel = compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: true);

        return syntaxTree
            .GetRoot()
            .DescendantTokens()
            .Select(token => CreateToken(token, projection, semanticModel))
            .Where(token => token is not null)
            .Cast<CsxamlSemanticToken>()
            .Distinct()
            .ToList();
    }

    private static CsxamlSemanticToken? CreateToken(
        SyntaxToken token,
        CsxamlProjectedDocument projection,
        SemanticModel semanticModel)
    {
        if (!TryMapToken(token, projection, out var start, out var length))
        {
            return null;
        }

        if (SyntaxFacts.IsKeywordKind(token.Kind()))
        {
            return new CsxamlSemanticToken(start, length, CsxamlSemanticTokenType.Keyword);
        }

        if (!token.IsKind(SyntaxKind.IdentifierToken))
        {
            return null;
        }

        var typeToken = CreateTypeToken(token.Parent, semanticModel, start, length);
        if (typeToken is not null)
        {
            return typeToken;
        }

        var declaredSymbol = TryGetDeclaredSymbol(token.Parent, semanticModel);
        if (declaredSymbol is not null)
        {
            return CreateSymbolToken(start, length, declaredSymbol, isDeclaration: true);
        }

        var referencedSymbol = token.Parent is null
            ? null
            : semanticModel.GetSymbolInfo(token.Parent).Symbol;
        return referencedSymbol is null
            ? null
            : CreateSymbolToken(start, length, referencedSymbol, isDeclaration: false);
    }

    private static bool TryMapToken(
        SyntaxToken token,
        CsxamlProjectedDocument projection,
        out int originalStart,
        out int originalLength)
    {
        if (!projection.TryMapProjectedRange(token.Span.Start, token.Span.End, out originalStart, out var originalEnd))
        {
            originalLength = 0;
            return false;
        }

        originalLength = originalEnd - originalStart;
        return originalLength > 0;
    }

    private static CsxamlSemanticToken? CreateTypeToken(
        SyntaxNode? node,
        SemanticModel semanticModel,
        int start,
        int length)
    {
        if (node is null)
        {
            return null;
        }

        var symbol = semanticModel.GetSymbolInfo(node).Symbol ?? semanticModel.GetTypeInfo(node).Type;
        if (symbol is not INamedTypeSymbol namedType)
        {
            return null;
        }

        return new CsxamlSemanticToken(start, length, MapTypeTokenType(namedType));
    }

    private static ISymbol? TryGetDeclaredSymbol(SyntaxNode? node, SemanticModel semanticModel)
    {
        return node switch
        {
            ClassDeclarationSyntax declaration => semanticModel.GetDeclaredSymbol(declaration),
            LocalFunctionStatementSyntax declaration => semanticModel.GetDeclaredSymbol(declaration),
            MethodDeclarationSyntax declaration => semanticModel.GetDeclaredSymbol(declaration),
            ParameterSyntax declaration => semanticModel.GetDeclaredSymbol(declaration),
            PropertyDeclarationSyntax declaration => semanticModel.GetDeclaredSymbol(declaration),
            SingleVariableDesignationSyntax declaration => semanticModel.GetDeclaredSymbol(declaration),
            VariableDeclaratorSyntax declaration => semanticModel.GetDeclaredSymbol(declaration),
            ForEachStatementSyntax declaration => semanticModel.GetDeclaredSymbol(declaration),
            _ => null,
        };
    }

    private static CsxamlSemanticToken? CreateSymbolToken(
        int start,
        int length,
        ISymbol symbol,
        bool isDeclaration)
    {
        if (symbol is IAliasSymbol alias)
        {
            symbol = alias.Target;
        }

        return symbol switch
        {
            INamedTypeSymbol namedType => new CsxamlSemanticToken(
                start,
                length,
                MapTypeTokenType(namedType),
                IsDeclaration: isDeclaration),
            IEventSymbol => new CsxamlSemanticToken(
                start,
                length,
                CsxamlSemanticTokenType.Event,
                IsDeclaration: isDeclaration),
            IFieldSymbol field => new CsxamlSemanticToken(
                start,
                length,
                CsxamlSemanticTokenType.Property,
                IsDeclaration: isDeclaration,
                IsReadOnly: field.IsReadOnly),
            ILocalSymbol => new CsxamlSemanticToken(
                start,
                length,
                CsxamlSemanticTokenType.Variable,
                IsDeclaration: isDeclaration),
            IMethodSymbol => new CsxamlSemanticToken(
                start,
                length,
                CsxamlSemanticTokenType.Method,
                IsDeclaration: isDeclaration),
            IParameterSymbol => new CsxamlSemanticToken(
                start,
                length,
                CsxamlSemanticTokenType.Parameter,
                IsDeclaration: isDeclaration),
            IPropertySymbol property => new CsxamlSemanticToken(
                start,
                length,
                CsxamlSemanticTokenType.Property,
                IsDeclaration: isDeclaration,
                IsReadOnly: property.IsReadOnly),
            _ => null,
        };
    }

    private static CsxamlSemanticTokenType MapTypeTokenType(INamedTypeSymbol namedType)
    {
        return namedType.TypeKind == TypeKind.Interface
            ? CsxamlSemanticTokenType.Interface
            : CsxamlSemanticTokenType.Class;
    }
}
