const { findMatchingDelimiter, splitTopLevelRanges } = require("./textScanner");

function findComponentDeclarations(text) {
    const declarations = [];
    const declarationPattern = /\bcomponent\s+Element\s+([A-Za-z_][A-Za-z0-9_]*)/g;

    for (const match of text.matchAll(declarationPattern)) {
        const name = match[1];
        const nameStart = match.index + match[0].length - name.length;
        let index = nameStart + name.length;

        while (index < text.length && /\s/.test(text[index])) {
            index++;
        }

        const parameters = text[index] === "("
            ? findParameters(text, index)
            : [];

        declarations.push({
            name,
            nameLength: name.length,
            nameStart,
            parameters
        });
    }

    return declarations;
}

function findParameters(text, openParenIndex) {
    const closeParenIndex = findMatchingDelimiter(text, openParenIndex, "(", ")");
    if (closeParenIndex < 0) {
        return [];
    }

    const parameterText = text.slice(openParenIndex + 1, closeParenIndex);
    return splitTopLevelRanges(parameterText, openParenIndex + 1, ",")
        .map(createParameter)
        .filter(Boolean);
}

function createParameter(segment) {
    const match = /([A-Za-z_][A-Za-z0-9_]*)\s*$/.exec(segment.text);
    if (!match) {
        return null;
    }

    const trimmedEnd = segment.text.length - segment.text.match(/\s*$/)[0].length;
    const name = match[1];
    return {
        length: name.length,
        name,
        start: segment.start + trimmedEnd - name.length
    };
}

module.exports = {
    findComponentDeclarations
};
