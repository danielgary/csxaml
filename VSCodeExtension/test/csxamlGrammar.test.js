const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");

const grammarPath = path.join(__dirname, "..", "syntaxes", "csxaml.tmLanguage.json");
const grammar = JSON.parse(fs.readFileSync(grammarPath, "utf8"));

test("grammar recognizes parameterless component declarations", () => {
    const pattern = new RegExp(grammar.repository.parameterlessComponentDeclaration.match);

    assert.match("component Element TodoBoard {", pattern);
    assert.match("component Page HomePage {", pattern);
    assert.match("component Window MainWindow {", pattern);
    assert.match("component Application App {", pattern);
    assert.match("component ResourceDictionary AppResources {", pattern);
});

test("grammar keeps parameterized component declarations", () => {
    const pattern = new RegExp(grammar.repository.componentDeclaration.begin);

    assert.match("component Element TodoCard(", pattern);
});

test("grammar includes a root C# fallback for helper code", () => {
    assert.ok(grammar.patterns.some(pattern => pattern.include === "#helperCode"));
    assert.equal(grammar.patterns.some(pattern => pattern.include === "source.cs"), false);
});

test("grammar helper-code region embeds C# without swallowing render markup", () => {
    assert.match(
        "    void UpdateItem(string itemId)",
        new RegExp(grammar.repository.helperCode.begin));
    assert.doesNotMatch(
        "    render <Grid>",
        new RegExp(grammar.repository.helperCode.begin));
    assert.doesNotMatch(
        "    startup MainWindow;",
        new RegExp(grammar.repository.helperCode.begin));
    assert.doesNotMatch(
        "    resources AppResources;",
        new RegExp(grammar.repository.helperCode.begin));
});

test("grammar recognizes app root keywords", () => {
    const keywordPattern = new RegExp(grammar.repository.keywords.patterns[0].match);

    assert.match("render <HomePage />;", keywordPattern);
    assert.match("startup MainWindow;", keywordPattern);
    assert.match("resources AppResources;", keywordPattern);
});

test("grammar recognizes property-content and ref syntax as markup", () => {
    const closingTagPattern = new RegExp(grammar.repository.markup.patterns[0].match);
    const openingTagPattern = new RegExp(grammar.repository.markup.patterns[1].begin);
    const attributePattern = new RegExp(grammar.repository.markup.patterns[1].patterns[1].match);

    assert.match("<Button.Flyout>", openingTagPattern);
    assert.match("</Button.Flyout>", closingTagPattern);
    assert.match(" Ref={SearchBox}", attributePattern);
    assert.match(" AutomationProperties.Name=\"Search\"", attributePattern);
});
