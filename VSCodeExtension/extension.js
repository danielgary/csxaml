const vscode = require("vscode");
const { CsxamlSemanticTokensProvider } = require("./src/csxamlSemanticTokensProvider");
const { WorkspaceComponentCatalog } = require("./src/workspaceComponentCatalog");

function activate(context) {
    const componentCatalog = new WorkspaceComponentCatalog();
    const semanticTokensProvider = new CsxamlSemanticTokensProvider(componentCatalog);

    context.subscriptions.push(componentCatalog);
    context.subscriptions.push(
        vscode.languages.registerDocumentSemanticTokensProvider(
            { language: "csxaml" },
            semanticTokensProvider,
            semanticTokensProvider.legend));
}

function deactivate() {
}

module.exports = {
    activate,
    deactivate
};
