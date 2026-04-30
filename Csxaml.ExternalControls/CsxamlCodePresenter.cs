using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.ExternalControls;

public sealed class CsxamlCodePresenter : ContentControl
{
    private string _code = string.Empty;
    private string _title = "CSXAML";

    public CsxamlCodePresenter()
    {
        Background = Brush(238, 22, 27, 34);
        BorderBrush = Brush(80, 255, 255, 255);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(8);
        Padding = new Thickness(14);
        RefreshContent();
    }

    public string Code
    {
        get => _code;
        set
        {
            _code = value ?? string.Empty;
            RefreshContent();
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            _title = string.IsNullOrWhiteSpace(value) ? "CSXAML" : value;
            RefreshContent();
        }
    }

    private void RefreshContent()
    {
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(new TextBlock
        {
            Text = Title,
            Foreground = Brush(255, 231, 237, 246),
            FontSize = 13
        });

        var richText = new RichTextBlock
        {
            FontFamily = new FontFamily("Cascadia Mono, Consolas"),
            FontSize = 12,
            Foreground = Brush(255, 231, 237, 246)
        };
        var paragraph = new Paragraph();
        foreach (var token in CsxamlCodeTokenizer.Tokenize(Code))
        {
            paragraph.Inlines.Add(new Run
            {
                Text = token.Text,
                Foreground = BrushFor(token.Kind)
            });
        }

        richText.Blocks.Add(paragraph);
        panel.Children.Add(new ScrollViewer
        {
            Height = 190,
            Content = richText
        });
        Content = panel;
    }

    private static SolidColorBrush BrushFor(CsxamlCodeTokenKind kind)
    {
        return kind switch
        {
            CsxamlCodeTokenKind.Keyword => Brush(255, 86, 156, 214),
            CsxamlCodeTokenKind.Markup => Brush(255, 78, 201, 176),
            CsxamlCodeTokenKind.String => Brush(255, 206, 145, 120),
            CsxamlCodeTokenKind.Expression => Brush(255, 220, 220, 170),
            _ => Brush(255, 231, 237, 246)
        };
    }

    private static SolidColorBrush Brush(byte alpha, byte red, byte green, byte blue)
    {
        return new SolidColorBrush(ColorHelper.FromArgb(alpha, red, green, blue));
    }
}
