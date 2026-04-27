namespace Csxaml.Runtime;

/// <summary>
/// Represents a color using alpha, red, green, and blue byte channels.
/// </summary>
/// <param name="A">The alpha channel.</param>
/// <param name="R">The red channel.</param>
/// <param name="G">The green channel.</param>
/// <param name="B">The blue channel.</param>
public readonly record struct ArgbColor(byte A, byte R, byte G, byte B);
