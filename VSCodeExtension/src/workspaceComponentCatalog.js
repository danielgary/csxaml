const vscode = require("vscode");
const { findComponentDeclarations } = require("./componentSignatureParser");

class WorkspaceComponentCatalog {
    constructor() {
        this._cache = undefined;
        this._disposables = [
            vscode.workspace.onDidChangeTextDocument(() => this.invalidate()),
            vscode.workspace.onDidCreateFiles(() => this.invalidate()),
            vscode.workspace.onDidDeleteFiles(() => this.invalidate()),
            vscode.workspace.onDidRenameFiles(() => this.invalidate()),
            vscode.workspace.onDidSaveTextDocument(() => this.invalidate())
        ];
    }

    dispose() {
        for (const disposable of this._disposables) {
            disposable.dispose();
        }
    }

    async getCatalog() {
        if (!this._cache) {
            this._cache = this.loadCatalog();
        }

        return this._cache;
    }

    invalidate() {
        this._cache = undefined;
    }

    async loadCatalog() {
        const files = await vscode.workspace.findFiles("**/*.csxaml", "**/{bin,obj,node_modules}/**");
        const decoder = new TextDecoder("utf-8");
        const catalog = new Map();

        for (const file of files) {
            const bytes = await vscode.workspace.fs.readFile(file);
            const text = decoder.decode(bytes);
            const declaration = findComponentDeclarations(text)[0];
            if (!declaration) {
                continue;
            }

            catalog.set(declaration.name, {
                parameters: new Set(declaration.parameters.map(parameter => parameter.name)),
                uri: file
            });
        }

        return catalog;
    }
}

module.exports = {
    WorkspaceComponentCatalog
};
