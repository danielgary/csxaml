namespace Csxaml.Runtime;

internal static class CsxamlRuntimeExceptionBuilder
{
    public static Exception Wrap(
        Exception exception,
        string stage,
        ComponentInstance? component = null,
        CsxamlSourceInfo? sourceInfo = null,
        string? detail = null)
    {
        var frame = new CsxamlRuntimeFrame(
            stage,
            component?.CsxamlComponentName,
            sourceInfo ?? component?.CsxamlSourceInfo,
            detail);

        return exception is CsxamlRuntimeException runtimeException
            ? runtimeException.Prepend(frame)
            : new CsxamlRuntimeException([frame], exception);
    }
}
