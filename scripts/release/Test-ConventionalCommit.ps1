[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Message,

    [Parameter()]
    [string]$Context = "Commit message"
)

$normalizedMessage = $Message.Trim()
if ([string]::IsNullOrWhiteSpace($normalizedMessage)) {
    throw "$Context is empty. Expected Conventional Commit format."
}

$validTypes = @(
    "feat",
    "fix",
    "perf",
    "refactor",
    "docs",
    "test",
    "build",
    "ci",
    "chore",
    "revert"
)

$typePattern = ($validTypes -join "|")
$scopePattern = "(?:\([a-z0-9][a-z0-9._/-]*\))?"
$breakingPattern = "!?";
$descriptionPattern = ": .+"
$pattern = "^(?:$typePattern)$scopePattern$breakingPattern$descriptionPattern$"

if ($normalizedMessage -notmatch $pattern) {
    $examples = @(
        "feat(generator): support package-based generator execution",
        "fix(runtime): preserve TextBox selection during rerender",
        "docs(repo): publish release and versioning notes",
        "feat(vscode)!: bundle the language server in the extension"
    ) -join [Environment]::NewLine

    throw @"
$Context '$normalizedMessage' is not a valid Conventional Commit title.

Expected format:
  type(optional-scope)!: description

Allowed types:
  $($validTypes -join ", ")

Examples:
$examples
"@
}

Write-Host "$Context passed Conventional Commit validation."
