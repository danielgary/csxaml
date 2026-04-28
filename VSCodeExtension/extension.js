const vscode = require("vscode");
const {
    LanguageClient,
    RevealOutputChannelOn,
    State,
    Trace
} = require("vscode-languageclient/node");
const { resolveLanguageServerPath } = require("./src/languageServerPathResolver");

let client = null;
let outputChannel = null;
let restartRegistration = null;
let configurationRegistration = null;

async function activate(context) {
    outputChannel = vscode.window.createOutputChannel("CSXAML");
    context.subscriptions.push(outputChannel);

    restartRegistration = vscode.commands.registerCommand(
        "csxaml.restartLanguageServer",
        async () => {
            await restartLanguageServer(context, true);
        });
    configurationRegistration = vscode.workspace.onDidChangeConfiguration(async event => {
        if (!event.affectsConfiguration("csxaml.languageServer.path") &&
            !event.affectsConfiguration("csxaml.languageServer.trace")) {
            return;
        }

        await restartLanguageServer(context, false);
    });
    context.subscriptions.push(restartRegistration, configurationRegistration);

    await startLanguageServer(context);
}

async function deactivate() {
    await stopLanguageServer();
}

async function startLanguageServer(context) {
    const configuration = vscode.workspace.getConfiguration("csxaml.languageServer");
    const configuredPath = configuration.get("path", "");
    const traceLevel = configuration.get("trace", "off");
    const isDevelopmentMode = context.extensionMode === vscode.ExtensionMode.Development;
    const resolution = resolveLanguageServerPath(
        context.extensionPath,
        configuredPath,
        vscode.workspace.workspaceFolders,
        { isDevelopmentMode });

    if (!resolution.launchers || resolution.launchers.length === 0) {
        const recoveryText = isDevelopmentMode
            ? "Build Csxaml.LanguageServer or set 'csxaml.languageServer.path'."
            : "Reinstall the extension, ensure the .NET 10 Desktop Runtime is installed, or set 'csxaml.languageServer.path'.";
        const message = [
            "The CSXAML language server executable was not found.",
            "Looked in:",
            ...resolution.attemptedPaths.map(candidate => `- ${candidate}`),
            "",
            recoveryText
        ].join("\n");
        outputChannel.appendLine(message);
        const choice = await vscode.window.showErrorMessage(
            "CSXAML language server executable was not found.",
            "Open Output");
        if (choice === "Open Output") {
            outputChannel.show(true);
        }
        return;
    }

    for (const launcher of resolution.launchers) {
        const started = await tryStartLanguageServer(context, isDevelopmentMode, traceLevel, launcher);
        if (started) {
            return;
        }
    }

    const choice = await vscode.window.showErrorMessage(
        "CSXAML language server failed to start.",
        "Open Output");
    if (choice === "Open Output") {
        outputChannel.show(true);
    }
}

async function tryStartLanguageServer(context, isDevelopmentMode, traceLevel, launcher) {
    outputChannel.appendLine(
        `Starting CSXAML language server from '${launcher.resolvedPath}' using '${launcher.command}'.`);

    try {
        client = new LanguageClient(
            "csxaml-language-server",
            "CSXAML Language Server",
            {
                command: launcher.command,
                args: launcher.args
            },
            {
                documentSelector: [
                    { scheme: "file", language: "csxaml" },
                    { scheme: "untitled", language: "csxaml" }
                ],
                outputChannel,
                traceOutputChannel: outputChannel,
                revealOutputChannelOn: RevealOutputChannelOn.Never
            });
        client.onDidChangeState(event => {
            outputChannel.appendLine(
                `CSXAML language client state changed: ${formatClientState(event.oldState)} -> ${formatClientState(event.newState)}.`);
        });

        const disposable = client.start();
        if (disposable && typeof disposable.dispose === "function") {
            context.subscriptions.push(disposable);
        }

        if (typeof client.onReady === "function") {
            await client.onReady();
        }

        await applyTraceLevel(traceLevel, isDevelopmentMode);
    } catch (error) {
        const message = error && error.message ? error.message : String(error);
        outputChannel.appendLine(`Failed to start CSXAML language server from '${launcher.resolvedPath}': ${message}`);
        if (client) {
            try {
                await client.stop();
            } catch {
                // Ignore secondary shutdown failures while moving to the next candidate.
            }
        }
        client = null;
        return false;
    }

    outputChannel.appendLine("CSXAML language server is ready.");
    return true;
}

async function applyTraceLevel(traceLevel, isDevelopmentMode) {
    if (!client || typeof client.setTrace !== "function") {
        return;
    }

    switch (traceLevel) {
        case "messages":
            await client.setTrace(Trace.Messages);
            outputChannel.appendLine("CSXAML language client trace level: messages.");
            return;
        case "verbose":
            await client.setTrace(Trace.Verbose);
            outputChannel.appendLine("CSXAML language client trace level: verbose.");
            return;
        default:
            await client.setTrace(Trace.Off);
            if (isDevelopmentMode) {
                outputChannel.appendLine("CSXAML language client trace is off by default in extension development mode.");
            }
            return;
    }
}

function formatClientState(state) {
    switch (state) {
        case State.Starting:
            return "Starting";
        case State.Running:
            return "Running";
        case State.Stopped:
            return "Stopped";
        default:
            return String(state);
    }
}

async function restartLanguageServer(context, userInitiated) {
    outputChannel.appendLine("Restarting CSXAML language server.");
    await stopLanguageServer();
    await startLanguageServer(context);

    if (userInitiated) {
        vscode.window.setStatusBarMessage("CSXAML language server restarted.", 3000);
    }
}

async function stopLanguageServer() {
    if (!client) {
        return;
    }

    const currentClient = client;
    client = null;
    await currentClient.stop();
}

module.exports = { activate, deactivate };
