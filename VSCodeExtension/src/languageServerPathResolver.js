const fs = require("fs");
const path = require("path");

function resolveLanguageServerPath(extensionPath, configuredPath, workspaceFolders) {
    const attemptedPaths = [];
    const candidates = [];

    if (configuredPath && configuredPath.trim().length > 0) {
        candidates.push(resolveConfiguredPath(configuredPath.trim(), extensionPath, workspaceFolders));
    }

    candidates.push(path.join(extensionPath, "LanguageServer", "Csxaml.LanguageServer.exe"));
    candidates.push(path.join(extensionPath, "..", "Csxaml.LanguageServer", "bin", "Debug", "net10.0", "Csxaml.LanguageServer.exe"));
    candidates.push(path.join(extensionPath, "..", "Csxaml.LanguageServer", "bin", "Release", "net10.0", "Csxaml.LanguageServer.exe"));

    for (const candidate of candidates) {
        const normalized = path.normalize(candidate);
        attemptedPaths.push(normalized);
        if (fs.existsSync(normalized)) {
            return {
                executablePath: normalized,
                attemptedPaths
            };
        }
    }

    return {
        executablePath: null,
        attemptedPaths
    };
}

function resolveConfiguredPath(configuredPath, extensionPath, workspaceFolders) {
    if (path.isAbsolute(configuredPath)) {
        return configuredPath;
    }

    const workspaceFolder = workspaceFolders && workspaceFolders.length > 0
        ? workspaceFolders[0].uri.fsPath
        : extensionPath;
    return path.join(workspaceFolder, configuredPath);
}

module.exports = {
    resolveLanguageServerPath
};
