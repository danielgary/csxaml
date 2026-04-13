function findMatchingDelimiter(text, startIndex, openingDelimiter, closingDelimiter) {
    const delimiters = [closingDelimiter];

    for (let index = startIndex + 1; index < text.length; index++) {
        const current = text[index];
        if (current === "\"" || current === "'") {
            index = skipQuotedLiteral(text, index, current);
            continue;
        }

        if (current === "(" || current === "[" || current === "{" || current === "<") {
            delimiters.push(getClosingDelimiter(current));
            continue;
        }

        if (current === delimiters[delimiters.length - 1]) {
            delimiters.pop();
            if (delimiters.length === 0) {
                return index;
            }
        }
    }

    return -1;
}

function findTagEnd(text, startIndex) {
    let braceDepth = 0;

    for (let index = startIndex; index < text.length; index++) {
        const current = text[index];
        if (current === "\"" || current === "'") {
            index = skipQuotedLiteral(text, index, current);
            continue;
        }

        if (current === "{") {
            braceDepth++;
            continue;
        }

        if (current === "}" && braceDepth > 0) {
            braceDepth--;
            continue;
        }

        if (current === ">" && braceDepth === 0) {
            return index;
        }
    }

    return -1;
}

function isIdentifierStart(character) {
    return /[A-Za-z_]/.test(character);
}

function readIdentifier(text, startIndex) {
    let index = startIndex;
    while (index < text.length && /[A-Za-z0-9_:.]/.test(text[index])) {
        index++;
    }

    return index;
}

function skipQuotedLiteral(text, startIndex, quoteCharacter) {
    for (let index = startIndex + 1; index < text.length; index++) {
        if (text[index] === "\\") {
            index++;
            continue;
        }

        if (text[index] === quoteCharacter) {
            return index;
        }
    }

    return text.length - 1;
}

function skipWhitespace(text, startIndex) {
    let index = startIndex;
    while (index < text.length && /\s/.test(text[index])) {
        index++;
    }

    return index;
}

function splitTopLevelRanges(text, offset, separator) {
    const segments = [];
    let start = 0;
    const delimiters = [];

    for (let index = 0; index < text.length; index++) {
        const current = text[index];
        if (current === "\"" || current === "'") {
            index = skipQuotedLiteral(text, index, current);
            continue;
        }

        if (current === "(" || current === "[" || current === "{" || current === "<") {
            delimiters.push(getClosingDelimiter(current));
            continue;
        }

        if (delimiters.length > 0 && current === delimiters[delimiters.length - 1]) {
            delimiters.pop();
            continue;
        }

        if (delimiters.length === 0 && current === separator) {
            segments.push({
                end: offset + index,
                start: offset + start,
                text: text.slice(start, index)
            });
            start = index + 1;
        }
    }

    segments.push({
        end: offset + text.length,
        start: offset + start,
        text: text.slice(start)
    });

    return segments;
}

function getClosingDelimiter(openingDelimiter) {
    switch (openingDelimiter) {
    case "(":
        return ")";
    case "[":
        return "]";
    case "{":
        return "}";
    case "<":
        return ">";
    default:
        throw new Error(`Unsupported delimiter '${openingDelimiter}'.`);
    }
}

module.exports = {
    findMatchingDelimiter,
    findTagEnd,
    isIdentifierStart,
    readIdentifier,
    skipWhitespace,
    splitTopLevelRanges
};
