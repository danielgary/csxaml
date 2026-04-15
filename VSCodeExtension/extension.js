const vscode = require("vscode");
const {
    LanguageClient,
    RevealOutputChannelOn
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
        if (!event.affectsConfiguration("csxaml.languageServer.path")) {
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
    const resolution = resolveLanguageServerPath(
        context.extensionPath,
        configuredPath,
        vscode.workspace.workspaceFolders);

    if (!resolution.executablePath) {
        const message = [
            "The CSXAML language server executable was not found.",
            "Looked in:",
            ...resolution.attemptedPaths.map(candidate => `- ${candidate}`),
            "",
            "Build Csxaml.LanguageServer or set 'csxaml.languageServer.path'."
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

    outputChannel.appendLine(`Starting CSXAML language server from '${resolution.executablePath}'.`);

    try {
        client = new LanguageClient(
            "csxaml-language-server",
            "CSXAML Language Server",
            {
                command: resolution.executablePath
            },
            {
                documentSelector: [
                    { scheme: "file", language: "csxaml" },
                    { scheme: "untitled", language: "csxaml" }
                ],
                outputChannel,
                revealOutputChannelOn: RevealOutputChannelOn.Never
            });

        const started = client.start();
        if (started && typeof started.dispose === "function") {
            context.subscriptions.push(started);
        }

        if (typeof client.onReady === "function") {
            await client.onReady();
        }
    } catch (error) {
        const message = error && error.message ? error.message : String(error);
        outputChannel.appendLine(`Failed to start CSXAML language server: ${message}`);
        client = null;
        const choice = await vscode.window.showErrorMessage(
            "CSXAML language server failed to start.",
            "Open Output");
        if (choice === "Open Output") {
            outputChannel.show(true);
        }
        return;
    }

    outputChannel.appendLine("CSXAML language server is ready.");
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
