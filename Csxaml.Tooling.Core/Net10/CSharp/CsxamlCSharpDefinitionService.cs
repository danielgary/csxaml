using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Csxaml.Tooling.Core.CSharp;

internal sealed class CsxamlCSharpDefinitionService
{
    private readonly CsxamlCSharpCompilationFactory _compilationFactory = new();
    private readonly CsxamlCSharpProjectionBuilder _projectionBuilder = new();

    public (string FilePath, int Start, int Length)? GetDefinition(string filePath, string text, int position)
    {
        var projection = _projectionBuilder.Build(filePath, text);
        if (projection is null || !projection.TryMapOriginalToProjected(position, out var projectedPosition))
        {
            return null;
        }

        var compilation = _compilationFactory.CreateWithProjectSources(filePath, projection);
        var projectionTree = compilation.SyntaxTrees.Single(tree =>
            string.Equals(tree.FilePath, filePath + ".projection.cs", StringComparison.OrdinalIgnoreCase));
        var semanticModel = compilation.GetSemanticModel(projectionTree, ignoreAccessibility: true);
        var root = projectionTree.GetRoot();
        var token = root.FindToken(projectedPosition);
        var node = token.Parent;
        if (node is null)
        {
            return null;
        }

        var symbol = FindSymbol(node, semanticModel);
        return symbol is null
            ? null
            : MapLocation(filePath, projection, symbol);
    }

    private static ISymbol? FindSymbol(SyntaxNode node, SemanticModel semanticModel)
    {
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
                return UnwrapAlias(symbol);
            }

            if (semanticModel.GetTypeInfo(current).Type is ISymbol typeSymbol)
            {
                return UnwrapAlias(typeSymbol);
            }
        }

        return null;
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

    private static (string FilePath, int Start, int Length)? MapLocation(
        string filePath,
        CsxamlProjectedDocument projection,
        ISymbol symbol)
    {
        foreach (var location in symbol.Locations.Where(location => location.IsInSource))
        {
            var sourceTree = location.SourceTree;
            if (sourceTree is null)
            {
                continue;
            }

            if (string.Equals(sourceTree.FilePath, filePath + ".projection.cs", StringComparison.OrdinalIgnoreCase))
            {
                var projectedStart = location.SourceSpan.Start;
                var projectedEnd = projectedStart + location.SourceSpan.Length;
                if (!projection.TryMapProjectedRange(projectedStart, projectedEnd, out var originalStart, out var originalEnd))
                {
                    continue;
                }

                return (filePath, originalStart, originalEnd - originalStart);
            }

            return (sourceTree.FilePath, location.SourceSpan.Start, location.SourceSpan.Length);
        }

        return null;
    }
}
