# Release And Versioning

CSXAML uses semantic versioning for its public release surface and Conventional Commits for the semantic input to versioning and changelog generation.

## Version Source Of Truth

A shipped version is selected by the publish workflow from branch history and then recorded by an automation-created release tag.

Examples:

- `v1.0.0`
- `v1.0.1`
- `v1.1.0`
- `v1.0.0-preview.1`

The workflow-created release tag remains the durable record of a shipped version, but it is not the trigger for publishing.

The repo should stamp one coordinated version line across:

- NuGet packages
- the VS Code extension
- the Visual Studio extension

That coordination uses artifact-specific mappings where a marketplace requires a different version shape:

- NuGet packages use the semantic release version directly, for example `1.0.0-preview.1`
- the VS Code extension uses the semantic core version in `package.json`, for example `1.0.0`, and uses `--pre-release` when the repo release version is prerelease
- the Visual Studio extension uses a four-part numeric VSIX version derived from the semantic version, for example:
  - `1.0.0` -> `1.0.0.0`
  - `1.0.0-preview.2` -> `1.0.0.2`

The repo release version remains the single input; the extension-specific stamps are deterministic derivations of that input.

## Public Identities

Current public release identities are:

- NuGet package ids:
  - `Csxaml`
  - `Csxaml.Runtime`
- VS Code Marketplace publisher id:
  - `danielgarysoftware`
- Visual Studio Marketplace publisher id:
  - `danielgarysoftware`
- repository and package license:
  - `Apache-2.0`

## Commit Policy

Changes merged to `develop` or `master` should use squash merge, and the pull request title should follow Conventional Commit format:

```text
type(optional-scope)!: description
```

Allowed commit types:

- `feat`
- `fix`
- `perf`
- `refactor`
- `docs`
- `test`
- `build`
- `ci`
- `chore`
- `revert`

Examples:

- `feat(generator): support package-based generator execution`
- `fix(runtime): preserve TextBox selection during rerender`
- `docs(repo): publish release notes guide`
- `feat(vscode)!: bundle the language server in the extension`

Breaking changes should use `!` in the header and include a `BREAKING CHANGE:` footer in the body when the merge commit or squash commit is created.

## SemVer Mapping

The default release-bump policy is:

- `fix` -> patch
- `perf` -> patch
- `feat` -> minor
- `!` or `BREAKING CHANGE` -> major

By default, `docs`, `test`, `build`, `ci`, `chore`, and `refactor` do not trigger a public version bump by themselves.

## Changelog Generation

`git-cliff` is the changelog engine.

Expected commands:

```powershell
git cliff --bumped-version
git cliff --output CHANGELOG.md
```

The generated changelog should be the source for GitHub Release notes rather than a separate hand-maintained summary.

## GitHub Actions Workflows

The repo currently defines these release-facing workflows:

- `pr-title.yml`
  - validates Conventional Commit pull request titles
- `ci.yml`
  - restores, tests, packs public packages, validates local package install, and builds both extensions as artifacts
- `release-prep.yml`
  - resolves a release version, generates release notes with `git-cliff`, and uploads a reviewable changelog/release-note bundle
- `publish.yml`
  - runs from pushes to `develop` and `master`, computes the release version automatically, reruns validation, packs versioned artifacts, publishes packages/extensions, creates the release tag, and creates the GitHub Release

## Branch Release Flow

CSXAML now uses a branch-driven release flow:

- pushes to `develop` create preview releases automatically
- pushes to `master` create stable releases automatically
- manual tag creation is not required

The publish workflow computes the next release version from Conventional Commit history and the existing release tags:

- on `develop`, the workflow creates `vX.Y.Z-preview.N`
- on `master`, the workflow creates `vX.Y.Z`

Preview releases are tied to the next inferred stable version. Once that stable release is published from `master`, the next preview line starts from the next inferred stable version.

If no releasable commit type is present since the latest stable release, the publish workflow exits without publishing.

## Preview Releases

Preview releases are required before the first stable `1.0.0`.

NuGet and Git tags use semantic prerelease tags such as `1.0.0-preview.1`.

The current first public preview target for CSXAML is `v0.1.0-preview.1`.

The VS Code Marketplace has a different prerelease model than NuGet. The extension release flow must respect that marketplace's versioning constraints while still remaining traceable to the repo release version.

## Publish Environments And Secrets

The publish workflow expects separate protected GitHub Actions environments:

- `nuget`
- `vscode-marketplace`
- `visualstudio-marketplace`

Current secrets/inputs expected by the workflows:

- `NUGET_USER`
  - nuget.org profile name used with `NuGet/login@v1` trusted publishing
- `VSCODE_MARKETPLACE_PAT`
  - Azure DevOps personal access token used by `vsce publish`
- `VS_MARKETPLACE_PAT`
  - Visual Studio Marketplace personal access token used by `VsixPublisher.exe`

The Visual Studio publish path also depends on:

- `Csxaml.VisualStudio/publishManifest.json`
- `Csxaml.VisualStudio/overview.md`

Those files are checked into the repo so the marketplace metadata stays reviewable.

## Publish Authority

GitHub Actions is the system of record for release automation.

Publish workflows should:

- run the required tests before publish steps
- use protected environments for secrets and approvals
- generate release notes from `git-cliff`
- stamp all published artifacts from the same release version input
- create the release tag automatically after publish succeeds

The release tag is the published receipt, not a manual prerequisite for publish.

Manual local publishing should not remain part of the long-term supported release path.
