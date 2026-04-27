using System.Text;

namespace Csxaml.Runtime;

/// <summary>
/// Represents a runtime failure enriched with CSXAML component and source frames.
/// </summary>
public sealed class CsxamlRuntimeException : InvalidOperationException
{
    internal CsxamlRuntimeException(
        IReadOnlyList<CsxamlRuntimeFrame> frames,
        Exception cause)
        : base(BuildMessage(frames, cause), cause)
    {
        Frames = frames;
    }

    /// <summary>
    /// Gets the ordered runtime frames that describe where the failure occurred.
    /// </summary>
    public IReadOnlyList<CsxamlRuntimeFrame> Frames { get; }

    internal CsxamlRuntimeException Prepend(CsxamlRuntimeFrame frame)
    {
        return new CsxamlRuntimeException([frame, .. Frames], InnerException ?? this);
    }

    private static string BuildMessage(IReadOnlyList<CsxamlRuntimeFrame> frames, Exception cause)
    {
        var builder = new StringBuilder();
        builder.AppendLine("CSXAML runtime failure.");
        foreach (var frame in frames)
        {
            builder.AppendLine($"- Stage: {frame.Stage}");
            if (!string.IsNullOrWhiteSpace(frame.ComponentName))
            {
                builder.AppendLine($"  Component: {frame.ComponentName}");
            }

            if (frame.SourceInfo is not null)
            {
                builder.AppendLine($"  File: {frame.SourceInfo.FilePath}");
                builder.AppendLine(
                    $"  Span: {frame.SourceInfo.StartLine}:{frame.SourceInfo.StartColumn}-{frame.SourceInfo.EndLine}:{frame.SourceInfo.EndColumn}");
                if (!string.IsNullOrWhiteSpace(frame.SourceInfo.TagName))
                {
                    builder.AppendLine($"  Tag: {frame.SourceInfo.TagName}");
                }

                if (!string.IsNullOrWhiteSpace(frame.SourceInfo.MemberName))
                {
                    builder.AppendLine($"  Member: {frame.SourceInfo.MemberName}");
                }
            }

            if (!string.IsNullOrWhiteSpace(frame.Detail))
            {
                builder.AppendLine($"  Detail: {frame.Detail}");
            }
        }

        builder.Append($"Cause: {cause.Message}");
        return builder.ToString();
    }
}
