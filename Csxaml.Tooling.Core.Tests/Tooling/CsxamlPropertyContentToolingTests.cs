using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.Definitions;
using Csxaml.Tooling.Core.Formatting;
using Csxaml.Tooling.Core.Hover;
using Csxaml.Tooling.Core.Markup;
using Csxaml.Tooling.Core.SemanticTokens;

namespace Csxaml.Tooling.Core.Tests.Tooling;

[TestClass]
public sealed class CsxamlPropertyContentToolingTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    [TestMethod]
    public void Scanner_identifies_property_content_tags()
    {
        const string text =
            """
            component Element ToolingProbe {
                render <Border>
                    <Border.Child />
                </Border>;
            }
            """;

        var scan = CsxamlMarkupScanner.Scan(text);
        var propertyContent = scan.Elements.Single(element => element.TagName == "Border.Child");

        Assert.AreEqual("Border", propertyContent.PropertyContentOwner);
        Assert.AreEqual("Child", propertyContent.PropertyContentName);
    }

    [TestMethod]
    public void Completion_suggests_native_property_content_names()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe {
                render <Border>
                    <Border.C />
                </Border>;
            }
            """);
        var position = tempFile.Text.IndexOf("Border.C", StringComparison.Ordinal) + "Border.C".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);

        Assert.IsTrue(items.Any(item => item.Label == "Child"), string.Join(", ", items.Select(item => item.Label)));
    }

    [TestMethod]
    public void Completion_suggests_component_named_slot_content_names()
    {
        using var cardFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element SlotProbeCard {
                render <Border>
                    <Slot Name="Header" />
                </Border>;
            }
            """);
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe {
                render <SlotProbeCard>
                    <SlotProbeCard.H />
                </SlotProbeCard>;
            }
            """);
        var position = tempFile.Text.IndexOf("SlotProbeCard.H", StringComparison.Ordinal) + "SlotProbeCard.H".Length;

        var items = new CsxamlCompletionService().GetCompletions(tempFile.FilePath, tempFile.Text, position);

        Assert.IsTrue(items.Any(item => item.Label == "Header"), string.Join(", ", items.Select(item => item.Label)));
    }

    [TestMethod]
    public void Hover_returns_native_property_content_details()
    {
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe {
                render <Border>
                    <Border.Child />
                </Border>;
            }
            """);
        var position = tempFile.Text.IndexOf("Child", StringComparison.Ordinal) + 1;

        var hover = new CsxamlHoverService().GetHover(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Markdown, "Native property content");
        StringAssert.Contains(hover.Markdown, "Border.Child");
    }

    [TestMethod]
    public void Definition_resolves_component_named_slot_content_to_slot_outlet()
    {
        using var cardFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element SlotProbeCard {
                render <Border>
                    <Slot Name="Header" />
                </Border>;
            }
            """);
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe {
                render <SlotProbeCard>
                    <SlotProbeCard.Header />
                </SlotProbeCard>;
            }
            """);
        var position = tempFile.Text.LastIndexOf("Header", StringComparison.Ordinal) + 1;

        var definition = new CsxamlDefinitionService().GetDefinition(tempFile.FilePath, tempFile.Text, position);

        Assert.IsNotNull(definition);
        Assert.AreEqual(cardFile.FilePath, definition.FilePath);
        Assert.AreEqual(cardFile.Text.IndexOf("Header", StringComparison.Ordinal), definition.Start);
    }

    [TestMethod]
    public void Semantic_tokens_split_property_content_owner_and_property()
    {
        const string text =
            """
            namespace Csxaml.Samples.TodoApp;

            component Element ToolingProbe {
                render <Border>
                    <Border.Child />
                </Border>;
            }
            """;
        using var tempFile = TemporaryCsxamlFile.Create(
            Path.Combine(RepoRoot, "samples", "Csxaml.TodoApp", "Components"),
            text);

        var tokens = new CsxamlSemanticTokenService().GetTokens(tempFile.FilePath, text);
        var ownerStart = text.IndexOf("Border.Child", StringComparison.Ordinal);
        var propertyStart = text.IndexOf("Child", ownerStart, StringComparison.Ordinal);

        Assert.IsTrue(tokens.Any(token => token.Start == ownerStart && token.Length == "Border".Length && token.Type == CsxamlSemanticTokenType.Class));
        Assert.IsTrue(tokens.Any(token => token.Start == propertyStart && token.Length == "Child".Length && token.Type == CsxamlSemanticTokenType.Property));
    }

    [TestMethod]
    public void Formatting_indents_property_content_blocks()
    {
        const string text =
            """
            component Element ToolingProbe {
            render <Border>
            <Border.Child>
            <TextBlock Text="Hello" />
            </Border.Child>
            </Border>;
            }
            """;

        var formatted = new CsxamlFormattingService().FormatDocument(text);

        StringAssert.Contains(
            formatted,
            """
                render <Border>
                    <Border.Child>
                        <TextBlock Text="Hello" />
                    </Border.Child>
                </Border>;
            """);
    }
}
