namespace Csxaml.Generator;

internal sealed record ComponentFileScanResult(
    TextSpan ComponentSpan,
    IReadOnlyList<FileHelperCodeBlock> HelperCodeBlocks);
