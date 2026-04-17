[CmdletBinding()]
param(
    [Parameter()]
    [string]$VsixPath = "",

    [Parameter(Mandatory)]
    [string]$PersonalAccessToken,

    [Parameter()]
    [string]$PublishManifestPath = "Csxaml.VisualStudio/publishManifest.json",

    [Parameter()]
    [string]$VsixPublisherPath = ""
)

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")

if ([string]::IsNullOrWhiteSpace($VsixPath)) {
    $VsixPath = Get-ChildItem (Join-Path $repoRoot "artifacts/extensions/visualstudio") -Filter *.vsix |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1 -ExpandProperty FullName
}
elseif (-not [IO.Path]::IsPathRooted($VsixPath)) {
    $VsixPath = Join-Path $repoRoot $VsixPath
}

if (-not (Test-Path $VsixPath)) {
    throw "Visual Studio extension package '$VsixPath' does not exist."
}

if (-not [IO.Path]::IsPathRooted($PublishManifestPath)) {
    $PublishManifestPath = Join-Path $repoRoot $PublishManifestPath
}

if (-not (Test-Path $PublishManifestPath)) {
    throw "Publish manifest '$PublishManifestPath' does not exist."
}

if ([string]::IsNullOrWhiteSpace($VsixPublisherPath)) {
    $VsixPublisherPath = Get-ChildItem "C:\Program Files\Microsoft Visual Studio" -Recurse -Filter VsixPublisher.exe -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending |
        Select-Object -First 1 -ExpandProperty FullName
}

if (-not $VsixPublisherPath -or -not (Test-Path $VsixPublisherPath)) {
    throw "Could not find VsixPublisher.exe."
}

& $VsixPublisherPath publish -payload $VsixPath -publishManifest $PublishManifestPath -personalAccessToken $PersonalAccessToken
if ($LASTEXITCODE -ne 0) {
    throw "VsixPublisher.exe publish failed."
}
