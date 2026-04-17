# Milestone 15 Plan - Packaging, Release Automation, Templates, and V1 Ship

## Status

- Drafted: 2026-04-16
- Execution status: in progress
- Roadmap target: Milestone 15 in [ROADMAP.md](./ROADMAP.md)
- Replaces: the executed Milestone 14 performance plan
- Primary goal: make CSXAML consumable and releasable by outside users through stable packages, stable extensions, clear docs, and a reproducible GitHub Actions release path

---

## 1. Outcome

At the end of Milestone 15, another engineer should be able to say all of the following with evidence:

- a normal WinUI solution can consume CSXAML through published packages rather than only through this repo's local build wiring
- the NuGet package story is explicit, versioned, and documented
- the VS Code extension and the Visual Studio extension are built by GitHub Actions from the same repo version as the packages
- release notes are generated from Conventional Commit history through `git-cliff`
- semantic versioning is enforced by release policy rather than by guesswork
- release credentials and publishing steps are controlled through GitHub Actions environments rather than ad hoc local pushes
- the shipped behavior still matches [LANGUAGE-SPEC.md](./LANGUAGE-SPEC.md)

This milestone is not successful if the repo merely produces artifacts on one machine. It is successful only if the packaging surface is understandable, the release process is repeatable, and outside users can install the result without needing this repo checked out next to their app.

---

## 2. Current Repo State Snapshot

This plan is grounded in the repo as it exists now.

### 2.1 What already exists

- a working repo-local build flow through `build/Csxaml.props` and `build/Csxaml.targets`
- a generator executable project: `Csxaml.Generator`
- a runtime library: `Csxaml.Runtime`
- a testing helper library: `Csxaml.Testing`
- a Visual Studio extension project with real VSIX packaging: `Csxaml.VisualStudio`
- a VS Code extension scaffold under `VSCodeExtension/`
- milestone 15 roadmap targets in [ROADMAP.md](./ROADMAP.md)
- documentation already started in `docs/`, including:
  - `debugging-and-diagnostics.md`
  - `external-control-interop.md`
  - `component-testing.md`
  - `supported-feature-matrix.md`

### 2.2 What does not exist yet

- no `.github/` workflow directory
- no public-package metadata and pack flow for the core CSXAML product surface
- no public changelog or `git-cliff` configuration
- no Conventional Commit enforcement path
- no defined release workflow for NuGet, VS Code Marketplace, or Visual Studio Marketplace
- no finalized public package boundaries in source

### 2.3 Repo-specific packaging trap to solve

The current build targets invoke the generator through a repo-local project path:

- `build/Csxaml.props` defaults `CsxamlGeneratorProject` to `..\Csxaml.Generator\Csxaml.Generator.csproj`
- `build/Csxaml.targets` shells `dotnet run --project ...`

That is acceptable inside this repo. It is not an outside-user package story.

Milestone 15 must replace that repo-local assumption with a packaged build asset flow.

---

## 3. Non-Negotiable Constraints

Milestone 15 is a productization milestone, not permission to change the language contract casually.

### 3.1 Language and runtime constraints

- [LANGUAGE-SPEC.md](./LANGUAGE-SPEC.md) remains the source of truth
- packaging work must not quietly change parser, generator, runtime, or tooling semantics
- current `State<T>` equality semantics and `Touch()` behavior must remain intact
- release automation must run the existing correctness suites before publish steps
- generated code must remain deterministic and boring

### 3.2 Automation constraints

- GitHub Actions is the system of record for build, package, test, and publish automation
- local scripts may exist for developer convenience, but they must mirror the GitHub Actions flow rather than inventing a second release process
- publish workflows must be environment-gated and reviewable

### 3.3 Packaging constraints

- do not publish every internal project just because it can build
- do not leak repo-only wiring into public package APIs
- do not force downstream users to reference `Csxaml.Generator` or `Csxaml.ControlMetadata.Generator` directly
- do not ship a package structure that only works when the repo folder layout is present

### 3.4 Readability constraints

- keep packaging logic in small, obvious projects and targets
- prefer a dedicated packaging project over overloading unrelated runtime or generator projects with large amounts of release logic
- keep workflow files focused: CI, release prep, and publish concerns should not be collapsed into one giant YAML file

---

## 4. Release Model Decisions

This section defines the target release model for the implementation work.

### 4.1 Semantic versioning policy

CSXAML uses semantic versioning for all public deliverables.

- release versions are computed by automation from branch history and then recorded by workflow-created tags:
  - `v1.0.0`
  - `v1.0.1`
  - `v1.1.0`
  - `v1.1.0-preview.1`
- the repo ships one coordinated version line across:
  - public NuGet packages
  - the VS Code extension
  - the Visual Studio extension
- preview releases are required before the first stable `1.0.0`
- pushes to `develop` create preview releases automatically
- pushes to `master` create stable releases automatically
- manual tag creation should not be required

### 4.2 Conventional Commit policy

The repo uses Conventional Commits as the semantic input to versioning and changelog generation.

Expected types:

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

Expected scope examples:

- `generator`
- `runtime`
- `tooling`
- `vscode`
- `visualstudio`
- `build`
- `docs`
- `spec`
- `repo`

Breaking changes must use:

- `!` in the header, and
- a `BREAKING CHANGE:` footer with the human explanation

### 4.3 Changelog policy

`git-cliff` is the changelog and release-note engine.

- `CHANGELOG.md` should be generated from commit history
- GitHub Release notes should come from the same `git-cliff` config, not from hand-written summaries
- `git-cliff --bumped-version` is the default version recommendation mechanism
- docs-only or CI-only changes should not trigger public version bumps by themselves unless they intentionally change the release surface

### 4.4 Merge policy

To keep release history predictable:

- `develop` and `master` should prefer squash merges
- PR titles must follow Conventional Commits
- release notes should read merged changes from the resulting linear history, not from arbitrary local commit noise

### 4.5 GitHub Actions policy

GitHub Actions owns:

- CI validation
- package creation
- extension packaging
- changelog generation
- release-note generation
- NuGet publishing
- VS Code extension publishing
- Visual Studio extension publishing
- GitHub Release creation

No manual local publish step should remain required once the milestone is closed.

---

## 5. Public Package Boundary Target

The plan should optimize for a small public surface.

### 5.1 Required public packages

| Package | Purpose | Notes |
| --- | --- | --- |
| `Csxaml` | primary author-facing entry package | Carries `buildTransitive` assets, packaged generator payload, and the minimal install story for app authors. |
| `Csxaml.Runtime` | runtime library | Contains the runtime types that generated code depends on. |

### 5.2 Conditional public packages

| Package | Purpose | Ship rule |
| --- | --- | --- |
| `Csxaml.Testing` | test helpers for consumer apps | Ship only after reviewing the public API surface and docs. |
| `Csxaml.Templates` or equivalent template package | `dotnet new` install surface | Ship if the template experience is stable enough during this milestone; otherwise use a first-class sample app and keep the package as a follow-up. |

### 5.3 Internal-only projects by default

These should stay internal unless a concrete external-consumer reason appears during the milestone:

- `Csxaml.ControlMetadata`
- `Csxaml.ControlMetadata.Generator`
- `Csxaml.Generator`
- `Csxaml.Tooling.Core`
- `Csxaml.LanguageServer`
- `Csxaml.VisualStudio`
- demo and fixture projects

### 5.4 Packaging implementation preference

Prefer a dedicated package project for the author-facing package, expected name:

- `Csxaml\Csxaml.csproj`

This project should:

- own package metadata for the main package
- include `buildTransitive` assets
- include the packaged generator payload
- depend on `Csxaml.Runtime`
- avoid mixing runtime code with packaging-only concerns

---

## 6. Workstreams

Complete these in order unless a discovered blocker requires an explicit roadmap note.

### Workstream 1 - Release Governance and Version Contract

#### Goal

Establish the release rules before publishing anything.

#### Tasks

- add Conventional Commit policy to repo docs
- add PR-title enforcement in GitHub Actions
- add `git-cliff` configuration
- add a source-controlled `CHANGELOG.md`
- define version-bump rules for:
  - `feat`
  - `fix`
  - `perf`
  - breaking changes
- define preview tag naming and stable tag naming

#### Expected files

- `.github/workflows/pr-title.yml`
- `cliff.toml`
- `CHANGELOG.md`
- `README.md`
- release/versioning doc under `docs/`

#### Guardrails

- do not adopt a second versioning engine that conflicts with `git-cliff`
- do not depend on local git hooks as the only enforcement path
- do not hide breaking changes in vague commit messages

#### Done when

- `git-cliff` can compute a bumped version from the repo history
- PR titles are validated in CI
- the repo has a written version policy and changelog policy

### Workstream 2 - Finalize Package Boundaries and Asset Layout

#### Goal

Turn the current repo-local build story into a publishable package architecture.

#### Tasks

- create the primary packaging project for `Csxaml`
- move public package metadata out of ad hoc local assumptions
- decide exactly what the `Csxaml` package contains:
  - `buildTransitive/Csxaml.props`
  - `buildTransitive/Csxaml.targets`
  - packaged generator binaries
  - any required metadata payload
- define how packaged targets invoke the generator:
  - preferred approach: `dotnet exec` against packaged generator binaries
- remove repo-path assumptions from the public package flow
- keep repo-local development overrides available where useful, but make them opt-in rather than the public default

#### Expected files

- new package project, expected path: `Csxaml/Csxaml.csproj`
- packaging assets under that project or a nearby packaging folder
- updated `build/Csxaml.props`
- updated `build/Csxaml.targets`
- any supporting packaging script or target files

#### Guardrails

- do not make downstream users reference the generator project directly
- do not let the public package depend on this repo's folder structure
- do not collapse runtime and packaging logic into the same project unless there is a strong, documented reason

#### Done when

- the package boundary is explicit in source
- local `dotnet pack` produces an installable author-facing package
- the author-facing package no longer assumes `..\Csxaml.Generator\`

### Workstream 3 - Package Metadata and Local Install Validation

#### Goal

Make the produced packages credible before CI publishes anything.

#### Tasks

- add package metadata for all public packages:
  - package id
  - description
  - authors/owners
  - repository URL
  - package tags
  - readme
  - license metadata
- decide whether symbol packages are produced
- add package validation commands
- create a local-feed validation loop
- prove package consumption in a clean sample or fixture solution

#### Repo-specific checks

- package and marketplace metadata should stay aligned with the confirmed public release identities:
  - NuGet packages: `Csxaml`, `Csxaml.Runtime`
  - marketplace publisher id: `danielgarysoftware`
  - public license: `Apache-2.0`
- package metadata should not be copied inconsistently across many unrelated projects

#### Expected files

- package project files
- shared packaging props if needed
- package README assets
- validation script under `scripts/`

#### Done when

- public packages pack cleanly in `Release`
- a local feed install works in a clean solution
- package metadata is complete enough for publication

### Workstream 4 - Artifact-Only GitHub Actions CI

#### Goal

Create a fully automated build-and-package path before enabling any publish step.

#### Tasks

- add CI workflow on PRs and pushes to `develop` and `master`
- run restore, build, and tests
- pack NuGet artifacts
- package the VS Code extension
- build the VSIX
- upload all artifacts

#### Expected workflows

- `.github/workflows/ci.yml`

#### Expected artifacts

- `.nupkg`
- `.snupkg` if enabled
- VS Code `.vsix`
- Visual Studio `.vsix`
- generated changelog preview or release-note preview if useful

#### Guardrails

- do not publish from PR workflows
- keep the workflow readable; split composite concerns if the file becomes too large
- use `windows-latest` where WinUI and VSIX validation require it

#### Done when

- every release deliverable can be built in GitHub Actions without local intervention
- the repo has downloadable build artifacts for packages and extensions

### Workstream 5 - Release Prep Automation with `git-cliff`

#### Goal

Make version calculation and release-note generation reproducible.

#### Tasks

- add a release-prep workflow or script that:
  - computes the next version with `git-cliff`
  - generates release notes
  - updates `CHANGELOG.md`
  - stages any versioned source files that must match the release artifact version
- decide whether release prep:
  - remains a manual review helper, or
  - is bypassed for routine branch-driven publishes in favor of the publish workflow's computed release plan
- standardize one source of truth for release version variables consumed by:
  - `dotnet pack`
  - VS Code extension packaging
  - VSIX packaging

#### Preferred approach

Use a release-prep workflow as an optional review tool, while the real publish path computes the release plan directly from `develop` or `master` and creates the release tag automatically after publish succeeds.

#### Expected files

- `.github/workflows/release-prep.yml`
- release/versioning doc under `docs/`
- any helper script under `scripts/release/`

#### Done when

- the next version is computed by tooling, not by hand
- release notes are generated by `git-cliff`
- a reviewer can inspect the proposed version and notes before publish

### Workstream 6 - Publish Workflows and Credentials

#### Goal

Turn artifact builds into real releases with protected publish steps.

#### Tasks

- configure NuGet trusted publishing as the preferred publish path
- configure GitHub Actions environments for:
  - `nuget`
  - `vscode-marketplace`
  - `visualstudio-marketplace`
- add branch-driven publish workflows for `develop` preview releases and `master` stable releases
- create GitHub Releases from the same artifacts and notes
- define fallback behavior if a downstream marketplace publish fails after package creation

#### NuGet policy

- prefer trusted publishing over long-lived API keys
- if trusted publishing cannot be established immediately, use a temporary secret-based fallback and record the debt explicitly

#### Expected workflows

- `.github/workflows/publish.yml`
- or a small set of focused workflows such as:
  - `publish-nuget.yml`
  - `publish-vscode.yml`
  - `publish-vsix.yml`

#### Done when

- a protected GitHub Actions run can publish public packages
- a protected GitHub Actions run can create the GitHub Release
- required secrets and environment reviews are documented clearly

### Workstream 7 - Extension Packaging and Marketplace Readiness

#### Goal

Make both editor extensions releasable from the same repo version.

#### Tasks

- for VS Code:
  - add packaging command
  - add publishing command
  - ensure version stamping comes from the release version
  - decide whether the language server is bundled by default
  - keep `csxaml.languageServer.path` as an override, not as the normal install requirement
- for Visual Studio:
  - ensure VSIX version stamping is driven from the same release version
  - verify the existing VSIX packaging tests still pass
  - add marketplace publish automation or an explicit first-release manual gate if automation needs one extra pass

#### Repo-specific expectation

The public VS Code extension should ideally "just work" without asking the user to point to a separate language server binary.

#### Expected files

- `VSCodeExtension/package.json`
- `VSCodeExtension/README.md`
- any VS Code packaging scripts
- `Csxaml.VisualStudio/Csxaml.VisualStudio.csproj`
- Visual Studio publish manifest or helper scripts if needed

#### Done when

- both extensions build from CI artifacts
- both extensions consume the same release version
- marketplace-specific prerequisites are documented and tested

### Workstream 8 - Templates, Samples, and Consumer Docs

#### Goal

Close the adoption gap for a new outside user.

#### Tasks

- decide whether the milestone ships:
  - a `dotnet new` template package, or
  - a polished starter sample first
- ensure one sample path demonstrates package consumption rather than project-reference consumption only
- write or finish the remaining product docs:
  - native props/events guide
  - component testing guide review and completion
  - install/use package guide
  - release/versioning notes
- update README with:
  - what to install
  - where the docs live
  - what is stable versus internal

#### Guardrails

- do not claim template polish that the repo cannot maintain
- if the template package is not ready, ship a clear starter sample and record the template package as follow-up work
- do not leave roadmap checkboxes stale if docs already exist but need review rather than creation

#### Done when

- an outside user has a documented path from zero to first working CSXAML app
- milestone docs cover the supported v1 surface honestly
- README and docs tell the same install story as the packages

### Workstream 9 - Preview Release, Verification, and V1 Closeout

#### Goal

Ship a preview first, validate the whole release path, then close the milestone.

#### Tasks

- cut at least one preview release:
  - example: `v1.0.0-preview.1`
- validate:
  - NuGet install
  - VS Code extension install
  - Visual Studio extension install
  - sample or template flow
  - changelog and GitHub Release formatting
- fix any packaging or documentation gaps found in preview
- only then tag the stable `1.0.0`

#### Done when

- preview artifacts are installable and believable
- the release pipeline has been exercised end to end
- milestone 15 roadmap items can be closed honestly

---

## 7. Workflow Matrix

| Workflow | Trigger | Purpose | Publish allowed |
| --- | --- | --- | --- |
| `pr-title.yml` | PR opened/edited/synchronize | enforce Conventional Commit PR titles | No |
| `ci.yml` | PRs, pushes to `develop` and `master` | restore, build, test, pack, build extensions, upload artifacts | No |
| `release-prep.yml` | manual run | compute version, generate changelog, stage release notes | No |
| `publish.yml` | pushes to `develop` and `master` | compute release plan, validate, publish NuGet, publish extensions, create release tag, create GitHub Release | Yes |

Keep each workflow focused. If one file becomes hard to scan, split it before proceeding.

---

## 8. Codebase Tracking Map

Use this map to keep milestone edits anchored to the real repo.

### 8.1 Packaging and build assets

- `build/Csxaml.props`
- `build/Csxaml.targets`
- `Directory.Build.props`
- `Directory.Build.targets`
- new author-facing package project, expected `Csxaml/Csxaml.csproj`
- `Csxaml.Runtime/Csxaml.Runtime.csproj`
- `Csxaml.Testing/Csxaml.Testing.csproj`

### 8.2 Generator payload and metadata payload

- `Csxaml.Generator/*`
- `Csxaml.ControlMetadata/*`
- any packaging-time target or script that stages generator output

### 8.3 GitHub Actions and release config

- `.github/workflows/*`
- `cliff.toml`
- `CHANGELOG.md`
- release helper scripts under `scripts/release/`

### 8.4 VS Code extension

- `VSCodeExtension/package.json`
- `VSCodeExtension/README.md`
- `VSCodeExtension/src/*`
- any extension packaging scripts

### 8.5 Visual Studio extension

- `Csxaml.VisualStudio/Csxaml.VisualStudio.csproj`
- `Csxaml.VisualStudio/.vsextension/*`
- any marketplace publish manifest or helper script

### 8.6 Docs and roadmap touchpoints

- `README.md`
- `ROADMAP.md`
- `docs/component-testing.md`
- `docs/debugging-and-diagnostics.md`
- `docs/external-control-interop.md`
- new docs for:
  - native props/events
  - package install story
  - release/versioning notes

---

## 9. Validation Commands

These command shapes should exist by the end of the milestone.

### 9.1 Correctness

```powershell
dotnet test Csxaml.Generator.Tests\Csxaml.Generator.Tests.csproj -m:1 /p:UseSharedCompilation=false
dotnet test Csxaml.Runtime.Tests\Csxaml.Runtime.Tests.csproj -m:1 /p:UseSharedCompilation=false
dotnet test Csxaml.ControlMetadata.Generator.Tests\Csxaml.ControlMetadata.Generator.Tests.csproj -m:1 /p:UseSharedCompilation=false
dotnet test Csxaml.ProjectSystem.Tests\Csxaml.ProjectSystem.Tests.csproj -m:1 /p:UseSharedCompilation=false
dotnet test Csxaml.Tooling.Core.Tests\Csxaml.Tooling.Core.Tests.csproj -m:1 /p:UseSharedCompilation=false
dotnet test Csxaml.VisualStudio.Tests\Csxaml.VisualStudio.Tests.csproj -m:1 /p:UseSharedCompilation=false
```

### 9.2 Pack and extension build

```powershell
dotnet pack .\Csxaml\Csxaml.csproj -c Release
dotnet pack .\Csxaml.Runtime\Csxaml.Runtime.csproj -c Release
dotnet pack .\Csxaml.Testing\Csxaml.Testing.csproj -c Release
dotnet build .\Csxaml.VisualStudio\Csxaml.VisualStudio.csproj -c Release
cd VSCodeExtension; npm ci; npx vsce package
```

### 9.3 Release tooling

```powershell
git cliff --bumped-version
git cliff --output CHANGELOG.md
```

If the final commands differ, update this plan in the same change that introduces the real command surface.

---

## 10. Review Gates After Each Workstream

Each workstream closes only after these checks are done:

1. the changed files are still easy to navigate
2. related tests or validation commands were run
3. the result was compared against [ROADMAP.md](./ROADMAP.md)
4. the install/publish story is more explicit than before, not less
5. no public-facing versioning or changelog behavior depends on tribal knowledge

If a workstream changes packaging structure, the implementing agent must also confirm that an outside consumer can still understand:

- what package to install
- what version they are getting
- what runtime/tooling payload is bundled

---

## 11. Risks and Decision Traps

Track these explicitly while implementing.

### 11.1 Package boundary sprawl

Risk: too many public packages make the product hard to adopt.

Response: default to `Csxaml` plus `Csxaml.Runtime`, and justify any additional public package in source and docs.

### 11.2 Repo-local assumptions leaking into packages

Risk: packaged targets still reference repo paths or repo-only generator behavior.

Response: local-feed install validation is mandatory before any publish workflow is enabled.

### 11.3 VS Code extension not being self-sufficient

Risk: the marketplace extension technically publishes but still depends on a separately located language server binary.

Response: bundle the language server for the normal path and keep the current path setting as an override only.

### 11.4 Marketplace publish asymmetry

Risk: NuGet publish is automated but one extension still requires manual local steps.

Response: record any temporary manual gate explicitly in roadmap/docs and keep it as a short-lived milestone subtask, not a hidden exception.

### 11.5 Version drift across artifacts

Risk: NuGet packages, VS Code extension, and VSIX use different versions.

Response: one release version variable must feed all artifact stamps.

---

## 12. Implementation Issue Log

Fill this in as work lands.

| ID | Workstream | File(s) | Problem | Resolution | Status |
| --- | --- | --- | --- | --- | --- |
| REL-1 | 2 | `build/Csxaml.props`, `build/Csxaml.targets`, package project | Public package still assumed repo-local generator paths. | Added packaged generator payload, packaged `dotnet exec` path, and local-feed validation outside the repo layout. | Closed |
| REL-2 | 4 | `.github/workflows/ci.yml` | CI produced artifacts inconsistently across .NET and Node surfaces. | Added `Invoke-CiValidation.ps1` plus artifact CI workflow and validated the full local path sequentially to avoid WinAppSDK file-lock collisions. | Closed |
| REL-3 | 7 | `VSCodeExtension/package.json`, extension packaging path | Marketplace packaging did not bundle the normal language server path. | Added extension packaging script, bundled `LanguageServer/` payload into the packaged VSIX, and tightened manifest/license metadata. | Closed |
| REL-4 | 6 | publish workflow and environment config | NuGet or marketplace credentials were incomplete or not review-gated. | Added branch-driven publish workflow with protected environment boundaries and automatic post-publish tag creation; real publish still depends on maintainer-owned accounts and secrets. | Mitigated |

---

## 13. Execution Order

1. establish release governance and commit/version rules
2. finalize package boundaries
3. implement the author-facing package and local pack/install flow
4. add artifact-only CI
5. add release-prep automation with `git-cliff`
6. add protected publish workflows
7. finish extension packaging and version stamping
8. finish templates, samples, and remaining docs
9. cut a preview release from `develop`
10. fix preview gaps
11. publish the stable v1 release from `master`

Do not publish anything public before steps 1 through 5 are complete.

---

## 14. Definition of Done

Milestone 15 is complete only when all of the following are true:

- public package boundaries are defined in source and docs
- a local-feed package install works outside this repo's folder layout
- GitHub Actions builds all release artifacts
- GitHub Actions can publish the release artifacts through protected workflows
- Conventional Commit enforcement exists
- `git-cliff` generates the changelog and release notes
- semantic versioning is applied consistently across packages and extensions
- at least one preview release has been published and validated
- README and docs explain the install and release story clearly
- [ROADMAP.md](./ROADMAP.md) reflects the milestone's real state
- the shipped product still aligns with [LANGUAGE-SPEC.md](./LANGUAGE-SPEC.md)

If the repo can produce packages but a new user still has to guess what to install, how versions are chosen, or how releases are cut, the milestone is not done.
