const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");

const grammarPath = path.join(__dirname, "..", "syntaxes", "csxaml.tmLanguage.json");
const grammar = JSON.parse(fs.readFileSync(grammarPath, "utf8"));

test("grammar recognizes parameterless component declarations", () => {
    const pattern = new RegExp(grammar.repository.parameterlessComponentDeclaration.match);

    assert.match("component Element TodoBoard {", pattern);
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
});
