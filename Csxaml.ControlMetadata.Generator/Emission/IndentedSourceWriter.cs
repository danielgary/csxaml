using System.Text;

namespace Csxaml.ControlMetadata.Generator;

internal sealed class IndentedSourceWriter
{
    private readonly StringBuilder _builder = new();
    private int _indentLevel;

    public void PopIndent()
    {
        _indentLevel--;
    }

    public void PushIndent()
    {
        _indentLevel++;
    }

    public override string ToString()
    {
        return _builder.ToString();
    }

    public void WriteLine(string text = "")
    {
        _builder.Append(' ', _indentLevel * 4);
        _builder.AppendLine(text);
    }
}
