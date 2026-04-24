using Csxaml.Tooling.Core.Hover;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Csxaml.Tooling.Core.CSharp;

internal sealed class CsxamlCSharpHoverService
{
    private readonly CsxamlCSharpCompilationFactory _compilationFactory = new();
    private readonly CsxamlCSharpProjectionBuilder _projectionBuilder = new();

    public CsxamlHoverInfo? GetHover(string filePath, string text, int position)
    {
        var projection = _projectionBuilder.Build(filePath, text);
        if (projection is null || !projection.TryMapOriginalToProjected(position, out var projectedPosition))
        {
            return null;
        }

        var compilation = _compilationFactory.CreateWithProjectSources(filePath, projection);
        var projectionTree = compilation.SyntaxTrees.Single(
            tree => string.Equals(tree.FilePath, filePath + ".projection.cs", StringComparison.OrdinalIgnoreCase));
        var semanticModel = compilation.GetSemanticModel(projectionTree, ignoreAccessibility: true);
        var token = projectionTree.GetRoot().FindToken(projectedPosition);
        if (!TryMapToken(token, projection, out var start, out var length))
        {
            return null;
        }

        var symbol = FindSymbol(token.Parent, semanticModel);
        return symbol is null
            ? null
            : new CsxamlHoverInfo(start, length, CsxamlHoverFormatter.FormatCSharpSymbol(UnwrapAlias(symbol)));
    }

    private static ISymbol? FindSymbol(SyntaxNode? node, SemanticModel semanticModel)
    {
        if (node is null)
        {
            return null;
        }

        foreach (var current in node.AncestorsAndSelf())
        {
            var declared = TryGetDeclaredSymbol(current, semanticModel);
            if (declared is not null)
            {
                return declared;
            }

            var symbol = semanticModel.GetSymbolInfo(current).Symbol
                ?? semanticModel.GetSymbolInfo(current).CandidateSymbols.FirstOrDefault();
            if (symbol is not null)
            {
                return symbol;
            }

            if (semanticModel.GetTypeInfo(current).Type is ISymbol typeSymbol)
            {
                return typeSymbol;
            }
        }

        return null;
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

    private static ISymbol? TryGetDeclaredSymbol(SyntaxNode node, SemanticModel semanticModel)
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

    private static ISymbol UnwrapAlias(ISymbol symbol)
    {
        return symbol is IAliasSymbol alias ? alias.Target : symbol;
    }
}
