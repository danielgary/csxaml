const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const os = require("node:os");
const path = require("node:path");

const { resolveLanguageServerPath } = require("../src/languageServerPathResolver");

function createTempDirectory() {
    return fs.mkdtempSync(path.join(os.tmpdir(), "csxaml-vscode-tests-"));
}

function ensureFile(filePath) {
    fs.mkdirSync(path.dirname(filePath), { recursive: true });
    fs.writeFileSync(filePath, "");
}

test("resolver prefers a packaged executable", () => {
    const extensionRoot = createTempDirectory();
    try {
        const executablePath = path.join(extensionRoot, "LanguageServer", "Csxaml.LanguageServer.exe");
        ensureFile(executablePath);

        const resolution = resolveLanguageServerPath(extensionRoot, "", [], { isDevelopmentMode: false });

        assert.equal(resolution.command, executablePath);
        assert.deepEqual(resolution.args, []);
        assert.equal(resolution.resolvedPath, executablePath);
        assert.equal(resolution.launchers.length, 1);
    } finally {
        fs.rmSync(extensionRoot, { recursive: true, force: true });
    }
});

test("resolver falls back to dotnet for a packaged dll", () => {
    const extensionRoot = createTempDirectory();
    try {
        const dllPath = path.join(extensionRoot, "LanguageServer", "Csxaml.LanguageServer.dll");
        ensureFile(dllPath);

        const resolution = resolveLanguageServerPath(extensionRoot, "", [], { isDevelopmentMode: false });

        assert.equal(resolution.command, "dotnet");
        assert.deepEqual(resolution.args, [dllPath]);
        assert.equal(resolution.resolvedPath, dllPath);
        assert.equal(resolution.launchers.length, 1);
    } finally {
        fs.rmSync(extensionRoot, { recursive: true, force: true });
    }
});

test("resolver supports relative configured dll paths from the workspace root", () => {
    const extensionRoot = createTempDirectory();
    const workspaceRoot = createTempDirectory();
    try {
        const dllPath = path.join(workspaceRoot, "tools", "Csxaml.LanguageServer.dll");
        ensureFile(dllPath);

        const resolution = resolveLanguageServerPath(
            extensionRoot,
            path.join("tools", "Csxaml.LanguageServer.dll"),
            [{ uri: { fsPath: workspaceRoot } }],
            { isDevelopmentMode: false });

        assert.equal(resolution.command, "dotnet");
        assert.deepEqual(resolution.args, [dllPath]);
        assert.equal(resolution.resolvedPath, dllPath);
    } finally {
        fs.rmSync(extensionRoot, { recursive: true, force: true });
        fs.rmSync(workspaceRoot, { recursive: true, force: true });
    }
});

test("resolver returns both executable and dll launchers when both exist", () => {
    const extensionRoot = createTempDirectory();
    try {
        const executablePath = path.join(extensionRoot, "LanguageServer", "Csxaml.LanguageServer.exe");
        const dllPath = path.join(extensionRoot, "LanguageServer", "Csxaml.LanguageServer.dll");
        ensureFile(executablePath);
        ensureFile(dllPath);

        const resolution = resolveLanguageServerPath(extensionRoot, "", [], { isDevelopmentMode: false });

        assert.equal(resolution.launchers.length, 2);
        assert.deepEqual(
            resolution.launchers.map(launcher => launcher.resolvedPath),
            [executablePath, dllPath]);
    } finally {
        fs.rmSync(extensionRoot, { recursive: true, force: true });
    }
});
