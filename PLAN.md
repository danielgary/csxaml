# DocFX GitHub Pages Documentation Site Plan

## Status

- Drafted: 2026-04-27
- Execution status: implemented locally; GitHub Pages repository setting still needs to be enabled after merge
- Roadmap target: documentation site and API reference enablement
- Replaces: the completed/previous Milestone 15 release-automation execution plan
- Primary goal: create a DocFX static documentation site for CSXAML, publish it with GitHub Pages, and make it useful to both new app authors and extension/tooling developers.

---

## 1. Outcome

At the end of this work, another engineer should be able to say all of the following with evidence:

- the repo contains a DocFX configuration that builds a static documentation website
- GitHub Pages publishes the generated site from GitHub Actions
- the site has a clear path from "what is CSXAML?" to "I built a Todo app"
- the site explains the CSXAML language in a human-readable overview and preserves the formal language specification
- the site has user-facing pages for the VS Code extension and the Visual Studio extension
- the site has an API overview plus generated API reference pages from XML documentation comments
- docs are built and validated in CI before deployment
- existing repo docs are organized into a coherent public documentation structure

This work is not done if DocFX technically builds but readers still have to guess which package to install, which editor extension to use, what language features are supported, or which APIs are public.

---

## 2. Official Tooling Facts

This plan depends on current DocFX and GitHub Pages behavior.

### 2.1 DocFX

DocFX can build a static site from Markdown and generated .NET API documentation.

Relevant official facts:

- DocFX is installed as a .NET tool with `dotnet tool update -g docfx`.
- `docfx init` creates an initial docset.
- `docfx docfx.json --serve` builds and serves the site locally.
- DocFX writes publishable static HTML to `_site`.
- DocFX converts XML documentation comments into rendered API documentation.
- DocFX API generation has two stages:
  - `docfx metadata` generates API YAML.
  - `docfx build` transforms Markdown and API YAML into HTML.
- The root `docfx` command can run both metadata and build.
- `docfx metadata` and `docfx build` support `--warningsAsErrors`.

Primary references:

- https://dotnet.github.io/docfx/
- https://dotnet.github.io/docfx/docs/dotnet-api-docs.html
- https://dotnet.github.io/docfx/reference/docfx-cli-reference/docfx-metadata.html
- https://dotnet.github.io/docfx/reference/docfx-cli-reference/docfx-build.html
- https://dotnet.github.io/docfx/docs/config.html

### 2.2 GitHub Pages

GitHub Pages should publish from a custom GitHub Actions workflow, not from committed generated files.

Relevant official facts:

- Repository settings must set Pages source to GitHub Actions.
- A custom Pages workflow should:
  - check out the repo
  - build the static site
  - upload the static site with `actions/upload-pages-artifact`
  - deploy with `actions/deploy-pages`
- The deploy job needs at least:
  - `contents: read`
  - `pages: write`
  - `id-token: write`
- The deployment environment should be named `github-pages`.
- Pull request builds should build the site but skip deployment.

Primary references:

- https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages
- https://docs.github.com/en/pages/getting-started-with-github-pages/configuring-a-publishing-source-for-your-github-pages-site

---

## 3. Current Repo State Snapshot

This plan is grounded in the current repo layout.

### 3.1 Existing source docs

The repo already has useful documentation that should be promoted into the site:

- [README.md](./README.md)
- [LANGUAGE-SPEC.md](./LANGUAGE-SPEC.md)
- [docs/compatibility-policy.md](./docs/compatibility-policy.md)
- [docs/component-testing.md](./docs/component-testing.md)
- [docs/debugging-and-diagnostics.md](./docs/debugging-and-diagnostics.md)
- [docs/external-control-interop.md](./docs/external-control-interop.md)
- [docs/lifecycle-and-async.md](./docs/lifecycle-and-async.md)
- [docs/native-props-and-events.md](./docs/native-props-and-events.md)
- [docs/package-installation.md](./docs/package-installation.md)
- [docs/performance-and-scale.md](./docs/performance-and-scale.md)
- [docs/release-and-versioning.md](./docs/release-and-versioning.md)
- [docs/supported-feature-matrix.md](./docs/supported-feature-matrix.md)
- [docs/visual-studio-bootstrap.md](./docs/visual-studio-bootstrap.md)
- [VSCodeExtension/README.md](./VSCodeExtension/README.md)

### 3.2 Existing workflows

The repo already has GitHub Actions under `.github/workflows/`:

- `.github/workflows/ci.yml`
- `.github/workflows/pr-title.yml`
- `.github/workflows/publish.yml`
- `.github/workflows/release-prep.yml`

The existing CI uses `windows-latest`, .NET 8 and .NET 10, and Node.js. The docs workflow should follow that shape unless implementation proves that a narrower runner is enough.

### 3.3 API documentation readiness

Developer-facing projects have XML documentation comments and documentation file generation enabled or in progress.

API docs should initially target:

- `Csxaml.Runtime`
- `Csxaml.Tooling.Core`
- `Csxaml.ControlMetadata`
- `Csxaml.Testing`
- `Csxaml.VisualStudio`

Treat these as article-first unless a stable public API reason appears:

- `Csxaml.Generator`
- `Csxaml.LanguageServer`

Do not include test, demo, benchmark, generated fixture, or sample app assemblies in the public API reference.

---

## 4. Non-Negotiable Constraints

### 4.1 Documentation constraints

- Documentation must be accurate about preview status and current limitations.
- Do not make the site look more stable than the product is.
- The language overview should be approachable; the formal spec should remain precise.
- Existing docs should be edited for public readers, not blindly dumped into navigation.
- Every page should answer a real developer question.
- Avoid repo-internal process details in first-contact user pages.

### 4.2 API constraints

- Generated API docs should expose intended developer surfaces only.
- Use DocFX filters to hide accidental implementation details.
- Keep API overview pages hand-written; generated reference alone is not enough.
- If a public type appears confusing in generated docs, improve the XML comments rather than hiding the problem.

### 4.3 Build and hosting constraints

- Do not commit `_site` output.
- Do not publish from pull request workflows.
- Build docs in CI before deploying.
- Use GitHub Actions Pages deployment instead of committing to `gh-pages` unless a blocker is discovered and recorded.
- Use `windows-latest` at first because the repo contains WinUI and Visual Studio extension projects.

### 4.4 Repo discipline

- Keep the docs site config small and obvious.
- Keep pages split by audience and task.
- Update [README.md](./README.md) and [ROADMAP.md](./ROADMAP.md) when the site exists or changes the documented project status.
- Do not refactor unrelated docs or code while implementing the site.

---

## 5. Target Repository Layout

Prefer this structure:

```text
docfx.json
filterConfig.yml
docs-site/
  index.md
  toc.yml
  articles/
    getting-started/
      index.md
      quick-start.md
      prerequisites.md
    tutorials/
      index.md
      todo-app.md
    language/
      index.md
      concepts.md
      syntax.md
      component-model.md
      state-and-events.md
      native-controls.md
      external-controls.md
      lifecycle.md
      specification.md
      supported-features.md
    guides/
      index.md
      package-installation.md
      native-props-and-events.md
      component-testing.md
      debugging-and-diagnostics.md
      performance-and-scale.md
      compatibility-policy.md
      release-and-versioning.md
    editors/
      index.md
      visual-studio-code.md
      visual-studio.md
      language-service-features.md
    api/
      index.md
      packages-and-namespaces.md
      runtime.md
      testing.md
      tooling.md
      metadata.md
    troubleshooting/
      index.md
      build-and-generation.md
      editor-extensions.md
      packages-and-versions.md
    contributing/
      docs.md
```

Generated paths:

```text
_site/
api/
```

The generated paths must be ignored by git if they are not already ignored.

---

## 6. Public Site Information Architecture

Use this as the target table of contents.

### 6.1 Home

Purpose: explain the product and route readers quickly.

Must cover:

- what CSXAML is
- what problem it solves
- current preview status
- supported platform/runtime expectations
- install path
- first links:
  - Quick Start
  - Todo Tutorial
  - Language Overview
  - Editor Extensions
  - API Reference

### 6.2 Getting Started

Purpose: get a new user to a working first component.

Pages:

- Prerequisites
- Quick Start
- Package installation
- First component
- Build and run
- Common first-build failures

Quick Start must include:

- required .NET SDK and Windows/WinUI assumptions
- package install commands
- minimal `.csxaml` component
- build command
- what generated output or runtime behavior to expect
- link to troubleshooting

### 6.3 Tutorial: Build a Todo App

Purpose: a guided end-to-end tutorial using realistic CSXAML patterns.

The Todo tutorial should be the primary learning path.

Required chapters:

1. Create or open a WinUI app.
2. Install the CSXAML package.
3. Add the first `TodoBoard.csxaml` component.
4. Render a list of native controls.
5. Extract a `TodoCard` component.
6. Add typed props.
7. Add component state.
8. Add event handlers.
9. Select a todo item.
10. Edit title and details.
11. Toggle completion state.
12. Add layout and styling.
13. Add tests with `Csxaml.Testing`.
14. Review the final component tree.
15. Next steps.

Use the current demo and runtime tests as source material. If the current demo has behavior that is not ready for documentation, either fix the demo first or write the tutorial around supported behavior.

### 6.4 Language

Purpose: explain CSXAML as a language before dropping readers into the full spec.

Pages:

- Language Overview
- Concepts
- Syntax
- Component Model
- Props
- State
- Expressions
- Native Controls
- Events
- Child Content
- Attached Properties
- External Controls
- Keys and Retained Identity
- Lifecycle and Async
- Supported Feature Matrix
- Formal Language Specification

The formal spec page may initially import or mirror [LANGUAGE-SPEC.md](./LANGUAGE-SPEC.md), but the overview pages must be shorter and task-oriented.

### 6.5 Guides

Purpose: keep practical deep dives discoverable.

Pages:

- Package Installation
- Native Props and Events
- External Control Interop
- Component Testing
- Debugging and Diagnostics
- Performance and Scale
- Compatibility Policy
- Release and Versioning

Most of these already exist under `docs/`. Implementation should copy or move them into the DocFX article tree, fix links, and edit headings/front matter for public navigation.

### 6.6 Editor Extensions

Purpose: document both editor experiences separately while explaining shared language-service behavior once.

Pages:

- Editor Extensions Overview
- VS Code Extension
- Visual Studio Extension
- Language Service Features
- Editor Troubleshooting

VS Code page must cover:

- installation path
- packaged VSIX or marketplace path
- runtime prerequisites
- language server resolution behavior
- supported features
- settings
- commands
- troubleshooting

Visual Studio page must cover:

- installation path
- VSIX or marketplace path
- supported Visual Studio version
- experimental instance workflow for contributors
- supported language-service features
- troubleshooting startup and marketplace issues

Shared language-service page must cover:

- diagnostics
- completion
- semantic tokens
- formatting
- go to definition
- hover, if supported
- known limitations

### 6.7 API Reference

Purpose: give developers a usable map before the generated reference.

Pages:

- API Overview
- Packages and Namespaces
- Runtime API
- Testing API
- Tooling API
- Control Metadata API
- Generated API Reference

The API overview must answer:

- which package a normal app author installs
- which namespaces generated code depends on
- which APIs are for tests
- which APIs are for tooling/editor integrations
- which APIs are advanced or subject to change

### 6.8 Troubleshooting

Purpose: centralize common failure paths.

Pages:

- Troubleshooting Overview
- Build and Generation
- Editor Extensions
- Packages and Versions
- Runtime Behavior
- Known Limitations

Seed this section from existing CI/release/package failures and current docs.

### 6.9 Contributing to Docs

Purpose: make docs maintenance repeatable.

Must cover:

- how to install or restore DocFX locally
- how to build the docs
- how to preview the site
- how to add a page
- how to update the TOC
- how API docs are generated
- how to fix DocFX warnings

---

## 7. DocFX Configuration Target

### 7.1 `docfx.json`

Add a root-level `docfx.json`.

Expected responsibilities:

- define API metadata generation
- include generated API YAML
- include article Markdown under `docs-site/`
- include static resources, if any
- set global metadata such as app title and GitHub repo URL
- configure sitemap base URL once the final Pages URL is known

Start with the default DocFX template. Do not customize visual design in the first pass unless a generated site is unusable.

### 7.2 API metadata

Prefer generating from built assemblies first if project-based DocFX metadata has trouble with WinUI workloads. Otherwise, generating from project files is acceptable.

Recommended first implementation path:

1. Restore and build API-bearing projects in Release.
2. Run DocFX metadata against built assemblies and side-by-side XML files.
3. Use `filterConfig.yml` to limit what appears.

Candidate projects:

- `Csxaml.Runtime/Csxaml.Runtime.csproj`
- `Csxaml.Testing/Csxaml.Testing.csproj`
- `Csxaml.Tooling.Core/Csxaml.Tooling.Core.csproj`
- `Csxaml.ControlMetadata/Csxaml.ControlMetadata.csproj`
- `Csxaml.VisualStudio/Csxaml.VisualStudio.csproj`

If Visual Studio extension metadata adds too much noise or requires Visual Studio-specific components that DocFX cannot resolve reliably, move it to a hand-written extension integration page and omit its generated API reference for the first published site.

### 7.3 API filter

Add `filterConfig.yml`.

Default posture:

- include public/protected APIs from intended assemblies
- exclude tests, demo, benchmark, generated fixture, sample, and package-internal namespaces
- exclude generated compiler artifacts if they leak into API metadata
- exclude implementation detail namespaces only after confirming they are not developer surfaces

Do not use filtering to hide missing XML docs that should be fixed.

### 7.4 Front matter

Each hand-written page should have front matter:

```yaml
---
title: Page Title
description: One sentence description for search results and previews.
---
```

Keep descriptions factual and short.

---

## 8. GitHub Pages Workflow Target

Add `.github/workflows/docs.yml`.

### 8.1 Triggers

```yaml
on:
  pull_request:
  push:
    branches:
      - develop
      - master
  workflow_dispatch:
```

Pull requests should build and upload a docs artifact if useful, but must not deploy.

Deployment should happen only on the chosen public branch. If public docs should track stable releases, deploy from `master`. If public docs should track active preview work, deploy from `develop`. Record the decision in the workflow and docs contributing page.

### 8.2 Runner

Use `windows-latest` initially.

Reasons:

- existing CI uses Windows
- runtime and tests target WinUI
- Visual Studio extension API metadata may need Windows-specific references
- this avoids debugging Linux workload gaps during the first docs implementation

If a later implementation proves docs can build reliably on Ubuntu, switching is acceptable in a separate cleanup.

### 8.3 Workflow steps

Required build job steps:

1. Check out the repository.
2. Set up .NET 8 and .NET 10.
3. Set up Node.js only if docs build depends on extension package metadata or assets.
4. Restore the solution.
5. Build API-bearing projects in Release.
6. Install or restore DocFX.
7. Run DocFX metadata with warnings as errors.
8. Run DocFX build with warnings as errors.
9. Upload `_site` with `actions/upload-pages-artifact`.

Required deploy job steps:

1. Run only for allowed branch pushes or manual workflow dispatch.
2. Use `actions/deploy-pages`.
3. Set environment to `github-pages`.

Required permissions:

```yaml
permissions:
  contents: read
  pages: write
  id-token: write
```

Add concurrency:

```yaml
concurrency:
  group: pages
  cancel-in-progress: false
```

---

## 9. Local Author Commands

These should work by the end of the implementation.

### 9.1 Install or restore DocFX

Prefer a local tool manifest if the repo does not already have one:

```powershell
dotnet new tool-manifest
dotnet tool install docfx
dotnet tool restore
```

If using a global tool instead:

```powershell
dotnet tool update -g docfx
```

The implementation must choose one path and document it. Prefer local tool manifest for reproducibility.

### 9.2 Build API-bearing projects

```powershell
dotnet restore .\Csxaml.sln
dotnet build .\Csxaml.Runtime\Csxaml.Runtime.csproj -c Release
dotnet build .\Csxaml.Testing\Csxaml.Testing.csproj -c Release
dotnet build .\Csxaml.ControlMetadata\Csxaml.ControlMetadata.csproj -c Release
dotnet build .\Csxaml.Tooling.Core\Csxaml.Tooling.Core.csproj -c Release
dotnet build .\Csxaml.VisualStudio\Csxaml.VisualStudio.csproj -c Release
```

If Visual Studio metadata is omitted from generated API docs, remove that build from the docs command list.

### 9.3 Build and preview docs

If using local tools:

```powershell
dotnet tool restore
dotnet docfx docfx.json --serve
```

If using global DocFX:

```powershell
docfx docfx.json --serve
```

For stricter validation:

```powershell
dotnet docfx metadata docfx.json --warningsAsErrors
dotnet docfx build docfx.json --warningsAsErrors
```

Adjust commands if the implementation pins scripts under `scripts/docs/`.

---

## 10. Implementation Workstreams

Complete these in order unless a blocker requires a documented change.

### Workstream 1 - Scaffold DocFX Site

#### Goal

Create a minimal DocFX site that builds locally.

#### Tasks

- add `docfx.json`
- add `filterConfig.yml`
- add `docs-site/index.md`
- add `docs-site/toc.yml`
- add initial article folders
- add generated output paths to `.gitignore` if needed
- add a docs contributing page with local build commands

#### Expected files

- `docfx.json`
- `filterConfig.yml`
- `docs-site/index.md`
- `docs-site/toc.yml`
- `docs-site/contributing/docs.md`
- `.gitignore`

#### Done when

- `docfx docfx.json --serve` or `dotnet docfx docfx.json --serve` starts a local site
- the home page renders
- the TOC has the target top-level sections

### Workstream 2 - Generate API Reference

#### Goal

Generate usable API docs for intended developer-facing assemblies.

#### Tasks

- build XML-doc-enabled projects in Release
- configure DocFX metadata generation
- add API output to the DocFX build content
- add API filters
- add hand-written API landing pages
- verify API nav is understandable

#### Expected files

- `docfx.json`
- `filterConfig.yml`
- `docs-site/api/index.md`
- `docs-site/api/packages-and-namespaces.md`
- `docs-site/api/runtime.md`
- `docs-site/api/testing.md`
- `docs-site/api/tooling.md`
- `docs-site/api/metadata.md`

#### Done when

- API pages generate
- API pages do not include test/demo/sample projects
- normal app authors can tell which package and namespace to use
- tooling authors can find advanced surfaces

### Workstream 3 - Write Getting Started and Todo Tutorial

#### Goal

Give new users a clear path from zero to a working Todo app.

#### Tasks

- write prerequisites page
- write quick start page
- write Todo tutorial
- use examples that compile against current CSXAML behavior
- link from home page and README
- include troubleshooting links at likely failure points

#### Expected files

- `docs-site/articles/getting-started/index.md`
- `docs-site/articles/getting-started/prerequisites.md`
- `docs-site/articles/getting-started/quick-start.md`
- `docs-site/articles/tutorials/index.md`
- `docs-site/articles/tutorials/todo-app.md`

#### Done when

- a reader can install packages and create a first component
- the Todo tutorial is complete enough to teach props, state, events, layout, and testing
- all code snippets are verified manually or by a script/sample

### Workstream 4 - Build Language Documentation

#### Goal

Create approachable language docs and preserve the formal spec.

#### Tasks

- write the language overview
- write concepts page
- split key language topics into short pages
- include or mirror the formal spec
- surface supported feature matrix
- cross-link language pages to tutorial and API pages

#### Expected files

- `docs-site/articles/language/index.md`
- `docs-site/articles/language/concepts.md`
- `docs-site/articles/language/syntax.md`
- `docs-site/articles/language/component-model.md`
- `docs-site/articles/language/state-and-events.md`
- `docs-site/articles/language/native-controls.md`
- `docs-site/articles/language/external-controls.md`
- `docs-site/articles/language/lifecycle.md`
- `docs-site/articles/language/specification.md`
- `docs-site/articles/language/supported-features.md`

#### Done when

- readers can learn concepts without reading the full spec first
- the formal spec is discoverable
- current feature support is explicit

### Workstream 5 - Promote Existing Guides

#### Goal

Move existing repo docs into public documentation shape.

#### Tasks

- migrate or copy existing guide docs into `docs-site/articles/guides/`
- fix headings and front matter
- fix relative links
- remove repo-internal phrasing from public pages
- preserve useful technical detail

#### Expected files

- `docs-site/articles/guides/index.md`
- `docs-site/articles/guides/package-installation.md`
- `docs-site/articles/guides/native-props-and-events.md`
- `docs-site/articles/guides/external-control-interop.md`
- `docs-site/articles/guides/component-testing.md`
- `docs-site/articles/guides/debugging-and-diagnostics.md`
- `docs-site/articles/guides/performance-and-scale.md`
- `docs-site/articles/guides/compatibility-policy.md`
- `docs-site/articles/guides/release-and-versioning.md`

#### Done when

- all existing public-relevant docs are reachable from the site TOC
- old docs are not duplicated in a way that creates obvious drift
- links from README and ROADMAP point to the right canonical docs location

### Workstream 6 - Document Editor Extensions

#### Goal

Add first-class docs for both editor extensions.

#### Tasks

- create editor extension overview page
- convert VS Code README content into a user-facing page
- convert Visual Studio bootstrap content into a user-facing page
- add shared language-service feature page
- add editor troubleshooting page
- separate contributor/dev-host instructions from normal installation instructions

#### Expected files

- `docs-site/articles/editors/index.md`
- `docs-site/articles/editors/visual-studio-code.md`
- `docs-site/articles/editors/visual-studio.md`
- `docs-site/articles/editors/language-service-features.md`
- `docs-site/articles/troubleshooting/editor-extensions.md`

#### Done when

- VS Code users know how to install and configure the extension
- Visual Studio users know how to install and troubleshoot the extension
- shared language-service capabilities are documented once

### Workstream 7 - Add Troubleshooting Hub

#### Goal

Make common failure modes easy to resolve.

#### Tasks

- add troubleshooting overview
- add build and generation troubleshooting
- add package/version troubleshooting
- add editor troubleshooting
- link troubleshooting from Quick Start, Tutorial, Guides, and Editors pages

#### Expected files

- `docs-site/articles/troubleshooting/index.md`
- `docs-site/articles/troubleshooting/build-and-generation.md`
- `docs-site/articles/troubleshooting/packages-and-versions.md`
- `docs-site/articles/troubleshooting/editor-extensions.md`
- `docs-site/articles/troubleshooting/runtime-behavior.md`

#### Done when

- common package, generator, editor, and runtime failures have a documented starting point
- users can report issues with enough diagnostic context

### Workstream 8 - Add GitHub Pages Deployment

#### Goal

Publish the site from GitHub Actions.

#### Tasks

- add `.github/workflows/docs.yml`
- build docs on PRs and pushes
- deploy only from the selected public branch
- upload `_site` as the GitHub Pages artifact
- configure environment and permissions
- document required GitHub repository setting

#### Expected files

- `.github/workflows/docs.yml`
- `docs-site/contributing/docs.md`
- optionally `scripts/docs/Invoke-DocsBuild.ps1`

#### Done when

- PR workflow builds docs
- branch push publishes to GitHub Pages
- Pages deployment URL is visible in the workflow output
- generated site output is not committed

### Workstream 9 - Polish Navigation, Links, and Search

#### Goal

Make the site feel coherent and reliable.

#### Tasks

- verify TOC order
- verify every top-level landing page exists
- fix broken links
- ensure page titles are unique and useful
- configure sitemap base URL after the public URL is known
- add logo/favicon only if assets already exist or are easy to produce
- add README link to live docs after deployment

#### Expected files

- `docfx.json`
- `docs-site/toc.yml`
- `README.md`
- `ROADMAP.md`

#### Done when

- no broken internal links remain
- the site has a readable public navigation path
- README points to the docs site
- ROADMAP reflects the documentation site status

---

## 11. Content Quality Checklist

Every public page should pass this checklist:

- clear title
- one-sentence description
- states audience or use case quickly
- links to the next likely page
- avoids stale release claims
- avoids undocumented commands
- code snippets are copyable and current
- internal repo details are moved below user-facing setup
- limitations are explicit where behavior is preview or incomplete

---

## 12. API Documentation Checklist

Generated API docs are acceptable only when:

- XML docs exist for public/protected APIs
- public namespace list is intentional
- accidental generated types are filtered out
- package ownership is explained by hand-written API pages
- advanced tooling APIs are labeled as advanced
- normal app authors can avoid internal/tooling pages unless needed
- DocFX warnings are either fixed or explicitly recorded as temporary debt

---

## 13. Validation Commands

These commands should exist by the end of the work, either directly or through a script.

### 13.1 Restore and build

```powershell
dotnet restore .\Csxaml.sln
dotnet build .\Csxaml.Runtime\Csxaml.Runtime.csproj -c Release
dotnet build .\Csxaml.Testing\Csxaml.Testing.csproj -c Release
dotnet build .\Csxaml.ControlMetadata\Csxaml.ControlMetadata.csproj -c Release
dotnet build .\Csxaml.Tooling.Core\Csxaml.Tooling.Core.csproj -c Release
dotnet build .\Csxaml.VisualStudio\Csxaml.VisualStudio.csproj -c Release
```

### 13.2 Build docs

```powershell
dotnet tool restore
dotnet docfx metadata .\docfx.json --warningsAsErrors
dotnet docfx build .\docfx.json --warningsAsErrors
```

If the implementation uses global DocFX instead of a local tool:

```powershell
docfx metadata .\docfx.json --warningsAsErrors
docfx build .\docfx.json --warningsAsErrors
```

### 13.3 Preview docs

```powershell
dotnet docfx .\docfx.json --serve
```

or:

```powershell
docfx .\docfx.json --serve
```

### 13.4 Optional link check

If a link checker is added:

```powershell
lychee .\_site
```

Do not make external-link failures block the first implementation unless the baseline is clean and reliable. Internal broken links should block.

---

## 14. GitHub Pages Repository Settings

After `.github/workflows/docs.yml` is merged:

1. Open the repository settings on GitHub.
2. Go to Pages.
3. Under Build and deployment, set Source to GitHub Actions.
4. Confirm the `github-pages` environment exists.
5. Add a deployment protection rule if public docs should deploy only from the selected branch.
6. Record the public docs URL in [README.md](./README.md).

If a custom domain is used later, configure it in GitHub settings. Do not rely only on a committed `CNAME` file.

---

## 15. Risks and Decisions

### 15.1 API metadata from WinUI/VSIX projects may be noisy

Risk: DocFX metadata generation may fail or produce too much irrelevant API content from Windows-specific projects.

Response: start on `windows-latest`, generate from built assemblies, and omit Visual Studio generated API docs from the first release if needed. Keep the Visual Studio extension documented as articles either way.

### 15.2 Existing docs may duplicate new public pages

Risk: docs drift if `docs/` and `docs-site/articles/` both contain canonical content.

Response: pick one canonical location during implementation. The preferred end state is public docs under `docs-site/articles/`, with old docs moved or replaced by redirecting/linking stubs if needed.

### 15.3 The tutorial may expose unsupported behavior

Risk: the Todo tutorial teaches behavior that works only in the demo or tests.

Response: base the tutorial on verified supported behavior. If the desired tutorial requires a missing capability, record the gap and adjust the tutorial rather than overstating support.

### 15.4 Docs workflow could duplicate CI work

Risk: docs CI becomes slow by rebuilding too much.

Response: first make it correct and reliable. After the site builds, optimize by building only API-bearing projects or using compiled artifacts if appropriate.

### 15.5 Public branch decision affects reader expectations

Risk: deploying from `develop` shows unreleased docs; deploying from `master` may lag preview packages.

Response: choose explicitly:

- Deploy from `master` if docs should represent stable/public releases.
- Deploy from `develop` if docs should represent current preview work.

Record the decision in `docs-site/contributing/docs.md`.

---

## 16. Implementation Issue Log

Fill this in as work lands.

| ID | Workstream | File(s) | Problem | Resolution | Status |
| --- | --- | --- | --- | --- | --- |
| DOCS-1 | 1 | `docfx.json`, `docs-site/*` | DocFX site scaffold needed. | Added DocFX config, site content tree, TOC, local tool manifest, and docs build script. | Closed |
| DOCS-2 | 2 | `filterConfig.yml`, API projects | API reference needs intentional public surface. | Added XML-doc metadata generation into `obj/docfx/api`, API filter, API overview pages, and generated-reference entry points. | Closed |
| DOCS-3 | 3 | Tutorial docs | Todo app tutorial needs verified snippets. | Added quick start and Todo tutorial based on the current demo shape and testing API names. | Closed |
| DOCS-4 | 8 | `.github/workflows/docs.yml` | GitHub Pages publish path needed. | Added docs workflow that builds on PRs and publishes `_site` to GitHub Pages from `master`. Repository Pages source still must be set to GitHub Actions after merge. | Mitigated |

---

## 17. Execution Order

1. scaffold DocFX config and minimal site
2. add local DocFX tool restore or document global tool usage
3. generate API reference for the safest API-bearing projects
4. add API overview pages
5. write quick start
6. write Todo tutorial
7. build language overview pages
8. migrate existing guides
9. write extension docs
10. add troubleshooting hub
11. add GitHub Pages workflow
12. verify local docs build with warnings as errors
13. verify docs workflow on PR
14. deploy from the selected public branch
15. update README and ROADMAP with the live docs status

Do not spend time on custom visual design before the content structure, API generation, and deployment path work.

---

## 18. Definition of Done

The DocFX documentation site is complete only when all of the following are true:

- `docfx.json` builds the site locally
- the site includes hand-written conceptual docs and generated API reference
- quick start exists
- Todo tutorial exists
- language overview exists
- formal language spec is reachable
- VS Code extension page exists
- Visual Studio extension page exists
- API overview explains packages and namespaces
- troubleshooting hub exists
- `.github/workflows/docs.yml` builds on PRs
- GitHub Pages deploys from the selected branch
- `_site` and generated API output are not committed
- README links to the docs site after deployment
- ROADMAP reflects the real documentation-site status
- DocFX warnings are fixed or tracked
- internal links are clean

If the site exists but a new user still has to read source files to build their first CSXAML component, this work is not done.
