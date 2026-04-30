---
title: Contributing to Docs
description: How to build, preview, validate, and update the CSXAML DocFX documentation site.
---

# Contributing to Docs

The documentation site is built with DocFX and published with GitHub Pages.

The full docs build also runs a Node-based CSXAML code highlighter. It loads
the VS Code TextMate grammar from `VSCodeExtension/syntaxes`, so docs and editor
highlighting stay aligned. Install Node.js 20.18.1 or newer and make sure `npm`
is available on `PATH` before running `Invoke-DocsBuild.ps1`.

## Restore tools

```powershell
dotnet tool restore
```

DocFX is pinned in the repository tool manifest.

## Build locally

From the repo root:

```powershell
.\scripts\docs\Invoke-DocsBuild.ps1
```

The script:

1. restores local .NET tools
2. runs `npm ci` for the docs-site highlighter package
3. restores API-bearing projects with failed local package sources ignored
4. builds XML-doc-enabled API projects in Release
5. runs `docfx metadata --noRestore`
6. runs `docfx build`
7. applies CSXAML syntax highlighting to built code fences
8. checks that CSXAML code fences, the spec include, and the supported-feature
   include rendered into built HTML
9. checks built-site local links under `_site`
10. reports external-link failures without failing the build

Generated API YAML is written under `obj\docfx`. Static site output is written
under `_site`. Neither path should be committed.

## GitHub Pages publishing

The docs workflow runs for pull requests and pushes to `develop` and `master`.
Those builds validate the site and upload the `_site` artifact for inspection.

GitHub Pages deployment only runs for pushes to `master`. A successful `develop`
docs build proves the site can build, but it does not publish or update the
public Pages site.

## Preview locally

```powershell
.\scripts\docs\Invoke-DocsBuild.ps1 -Serve
```

If local execution policy blocks scripts, run the same command through PowerShell's bypass mode:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\docs\Invoke-DocsBuild.ps1 -Serve
```

## Build without treating warnings as errors

Use this only while repairing a warning baseline:

```powershell
.\scripts\docs\Invoke-DocsBuild.ps1 -AllowWarnings
```

## Build without link checking

Use this only while repairing the built output or investigating a link-checker
failure:

```powershell
.\scripts\docs\Invoke-DocsBuild.ps1 -SkipLinkCheck
```

## Build without external link reporting

External links are checked in report-only mode because public sites can fail
transiently. Skip that network check when working offline:

```powershell
.\scripts\docs\Invoke-DocsBuild.ps1 -SkipExternalLinkCheck
```

## Add a page

1. Add the Markdown page under `docs-site/articles`.
2. Add front matter with `title` and `description`.
3. Add the page to `docs-site/toc.yml`.
4. Link to the next likely page for the reader.
5. Run the docs build.

## Docs quality checklist

Before a docs change is done, check:

1. Does the page tell the reader what success looks like?
2. Are code examples complete enough to run or clearly marked as partial?
3. Do tutorial tests query elements that the tutorial code actually names?
4. Does the page link to the next task a reader is likely to need?
5. Are supported, preview, experimental, and deferred behaviors labeled honestly?
6. Did `Invoke-DocsBuild.ps1` pass without DocFX warnings or broken local links?
7. Did the external-link report look intentional?

## API docs

API docs are generated from C# XML documentation comments at build time. Improve XML comments in source when generated API pages are unclear.

Do not edit generated files under `obj\docfx\api`.
