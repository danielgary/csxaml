using System.Text;

namespace Csxaml.Generator;

internal sealed class IndentedCodeWriter
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
        if (string.IsNullOrEmpty(text))
        {
            _builder.AppendLine();
            return;
        }

        _builder.Append(' ', _indentLevel * 4);
        _builder.AppendLine(text);
    }
}
