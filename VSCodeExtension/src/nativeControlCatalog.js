const nativeControls = new Map([
    ["Border", {
        events: new Set(),
        properties: new Set(["Background", "BorderBrush", "BorderThickness", "Padding"])
    }],
    ["Button", {
        events: new Set(["OnClick"]),
        properties: new Set(["Background", "Content", "FontSize", "Foreground"])
    }],
    ["CheckBox", {
        events: new Set(["OnCheckedChanged"]),
        properties: new Set(["Content", "IsChecked"])
    }],
    ["StackPanel", {
        events: new Set(),
        properties: new Set(["Background", "Orientation", "Spacing"])
    }],
    ["TextBlock", {
        events: new Set(),
        properties: new Set(["FontSize", "Foreground", "Text"])
    }],
    ["TextBox", {
        events: new Set(["OnTextChanged"]),
        properties: new Set(["AcceptsReturn", "MinHeight", "PlaceholderText", "Text", "TextWrapping", "Width"])
    }]
]);

function getNativeControls() {
    return nativeControls;
}

module.exports = {
    getNativeControls
};
