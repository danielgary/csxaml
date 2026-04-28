const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");

test("bundle output exists and does not depend on vscode-languageclient at runtime", () => {
    const bundlePath = path.join(__dirname, "..", "dist", "extension.js");
    assert.equal(fs.existsSync(bundlePath), true);

    const bundleText = fs.readFileSync(bundlePath, "utf8");
    assert.equal(bundleText.includes('require("vscode-languageclient/node")'), false);
    assert.equal(bundleText.includes("require('vscode-languageclient/node')"), false);
});
