const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");

const snippetsPath = path.join(__dirname, "..", "snippets", "csxaml.code-snippets");
const snippets = JSON.parse(fs.readFileSync(snippetsPath, "utf8"));

test("snippets cover generated roots, refs, property content, and named slots", () => {
    const prefixes = Object.values(snippets).map(snippet => snippet.prefix);

    assert.ok(prefixes.includes("csxaml-component"));
    assert.ok(prefixes.includes("csxaml-page"));
    assert.ok(prefixes.includes("csxaml-window"));
    assert.ok(prefixes.includes("csxaml-app"));
    assert.ok(prefixes.includes("csxaml-resources"));
    assert.ok(prefixes.includes("csxaml-ref"));
    assert.ok(prefixes.includes("csxaml-property-content"));
    assert.ok(prefixes.includes("csxaml-named-slot"));
    assert.ok(prefixes.includes("csxaml-named-slot-usage"));
});
