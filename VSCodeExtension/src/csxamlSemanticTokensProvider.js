const vscode = require("vscode");
const { findComponentDeclarations } = require("./componentSignatureParser");
const { getNativeControls } = require("./nativeControlCatalog");
const { findMatchingDelimiter, findTagEnd, isIdentifierStart, readIdentifier, skipWhitespace } = require("./textScanner");

const legend = new vscode.SemanticTokensLegend(
    ["class", "event", "keyword", "parameter", "property"],
    ["declaration", "defaultLibrary", "readonly"]);

class CsxamlSemanticTokensProvider {
    constructor(workspaceComponentCatalog) {
        this._workspaceComponentCatalog = workspaceComponentCatalog;
        this.legend = legend;
    }

    async provideDocumentSemanticTokens(document) {
        const builder = new vscode.SemanticTokensBuilder(legend);
        const text = document.getText();
        const components = await this.createComponentCatalog(text);

        emitKeywordTokens(builder, document, text);
        emitComponentDeclarationTokens(builder, document, text, components);
        emitMarkupTokens(builder, document, text, components);
        return builder.build();
    }

    async createComponentCatalog(text) {
        const components = new Map(await this._workspaceComponentCatalog.getCatalog());
        for (const declaration of findComponentDeclarations(text)) {
            components.set(declaration.name, {
                parameters: new Set(declaration.parameters.map(parameter => parameter.name))
            });
        }

        return components;
    }
}

function emitComponentDeclarationTokens(builder, document, text) {
    for (const declaration of findComponentDeclarations(text)) {
        pushToken(builder, document, declaration.nameStart, declaration.nameLength, "class", ["declaration"]);
        for (const parameter of declaration.parameters) {
            pushToken(builder, document, parameter.start, parameter.length, "parameter", ["declaration"]);
        }
    }
}

function emitKeywordTokens(builder, document, text) {
    const keywordPattern = /\b(component|Element|State|return|if|foreach|var|in)\b/g;
    for (const match of text.matchAll(keywordPattern)) {
        pushToken(builder, document, match.index, match[0].length, "keyword");
    }
}

function emitMarkupTokens(builder, document, text, components) {
    const nativeControls = getNativeControls();
    const tagPattern = /(^|[\s{;(])(<\/?)([A-Za-z_][A-Za-z0-9_:]*)(?=[\s/>])/gm;

    for (const match of text.matchAll(tagPattern)) {
        const tagName = match[3];
        const tagNameStart = match.index + match[1].length + match[2].length;
        pushTagToken(builder, document, tagNameStart, tagName, components, nativeControls);

        if (match[2] === "</") {
            continue;
        }

        const tagEnd = findTagEnd(text, tagNameStart + tagName.length);
        if (tagEnd < 0) {
            continue;
        }

        emitAttributeTokens(builder, document, text, tagName, tagNameStart + tagName.length, tagEnd, components, nativeControls);
    }
}

function emitAttributeTokens(builder, document, text, tagName, start, end, components, nativeControls) {
    let index = start;
    while (index < end) {
        index = skipWhitespace(text, index);
        if (index >= end || text[index] === "/" || text[index] === ">") {
            break;
        }

        if (!isIdentifierStart(text[index])) {
            index++;
            continue;
        }

        const nameStart = index;
        index = readIdentifier(text, index);
        const attributeName = text.slice(nameStart, index);
        pushAttributeToken(builder, document, tagName, attributeName, nameStart, components, nativeControls);

        index = skipWhitespace(text, index);
        if (text[index] !== "=") {
            continue;
        }

        index = skipWhitespace(text, index + 1);
        if (text[index] === "\"") {
            index = text.indexOf("\"", index + 1);
            index = index < 0 ? end : index + 1;
            continue;
        }

        if (text[index] === "{") {
            const expressionEnd = findMatchingDelimiter(text, index, "{", "}");
            index = expressionEnd < 0 ? end : expressionEnd + 1;
        }
    }
}

function pushAttributeToken(builder, document, tagName, attributeName, start, components, nativeControls) {
    if (attributeName === "Key") {
        pushToken(builder, document, start, attributeName.length, "property", ["readonly"]);
        return;
    }

    const nativeControl = nativeControls.get(tagName);
    if (nativeControl?.events.has(attributeName)) {
        pushToken(builder, document, start, attributeName.length, "event", ["defaultLibrary"]);
        return;
    }

    if (nativeControl?.properties.has(attributeName)) {
        pushToken(builder, document, start, attributeName.length, "property", ["defaultLibrary"]);
        return;
    }

    const component = components.get(tagName);
    if (component?.parameters.has(attributeName)) {
        pushToken(builder, document, start, attributeName.length, "parameter");
    }
}

function pushTagToken(builder, document, start, tagName, components, nativeControls) {
    if (nativeControls.has(tagName)) {
        pushToken(builder, document, start, tagName.length, "class", ["defaultLibrary"]);
        return;
    }

    if (components.has(tagName)) {
        pushToken(builder, document, start, tagName.length, "class");
    }
}

function pushToken(builder, document, start, length, tokenType, tokenModifiers = []) {
    const position = document.positionAt(start);
    const range = new vscode.Range(
        position.line,
        position.character,
        position.line,
        position.character + length);
    builder.push(range, tokenType, tokenModifiers);
}

module.exports = {
    CsxamlSemanticTokensProvider
};
