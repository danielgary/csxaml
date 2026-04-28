const fs = require("fs");
const path = require("path");

function resolveLanguageServerPath(extensionPath, configuredPath, workspaceFolders, options) {
    const attemptedPaths = [];
    const launchers = [];
    const resolvedOptions = options || {};
    const candidates = buildCandidatePaths(extensionPath, configuredPath, workspaceFolders, resolvedOptions);

    for (const candidate of candidates) {
        const normalized = path.normalize(candidate);
        attemptedPaths.push(normalized);
        if (fs.existsSync(normalized)) {
            const launcher = createLauncher(normalized);
            launchers.push({
                command: launcher.command,
                args: launcher.args,
                resolvedPath: normalized
            });
        }
    }

    if (launchers.length > 0) {
        return {
            command: launchers[0].command,
            args: launchers[0].args,
            resolvedPath: launchers[0].resolvedPath,
            launchers,
            attemptedPaths
        };
    }

    return {
        command: null,
        args: [],
        resolvedPath: null,
        launchers,
        attemptedPaths
    };
}

function buildCandidatePaths(extensionPath, configuredPath, workspaceFolders, options) {
    const candidates = [];

    if (configuredPath && configuredPath.trim().length > 0) {
        candidates.push(...expandCandidatePath(resolveConfiguredPath(configuredPath.trim(), extensionPath, workspaceFolders)));
    }

    candidates.push(...expandCandidatePath(path.join(extensionPath, "LanguageServer", "Csxaml.LanguageServer")));

    if (options.isDevelopmentMode) {
        candidates.push(...expandCandidatePath(path.join(extensionPath, "..", "Csxaml.LanguageServer", "bin", "Debug", "net10.0", "Csxaml.LanguageServer")));
        candidates.push(...expandCandidatePath(path.join(extensionPath, "..", "Csxaml.LanguageServer", "bin", "Release", "net10.0", "Csxaml.LanguageServer")));
    }

    return candidates;
}

function expandCandidatePath(candidatePath) {
    const lowerPath = candidatePath.toLowerCase();
    if (lowerPath.endsWith(".exe") || lowerPath.endsWith(".dll")) {
        return [candidatePath];
    }

    return [
        candidatePath + ".exe",
        candidatePath + ".dll"
    ];
}

function createLauncher(candidatePath) {
    if (candidatePath.toLowerCase().endsWith(".dll")) {
        return {
            command: "dotnet",
            args: [candidatePath]
        };
    }

    return {
        command: candidatePath,
        args: []
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
