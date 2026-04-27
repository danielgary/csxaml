---
title: Contributing to Docs
description: How to build, preview, validate, and update the CSXAML DocFX documentation site.
---

# Contributing to Docs

The documentation site is built with DocFX and published with GitHub Pages.

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
2. restores API-bearing projects with failed local package sources ignored
3. builds XML-doc-enabled API projects in Release
4. runs `docfx metadata --noRestore`
5. runs `docfx build`

Generated API YAML is written under `obj\docfx\api`. Static site output is written under `_site`. Neither path should be committed.

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

## Add a page

1. Add the Markdown page under `docs-site/articles`.
2. Add front matter with `title` and `description`.
3. Add the page to `docs-site/toc.yml`.
4. Link to the next likely page for the reader.
5. Run the docs build.

## API docs

API docs are generated from C# XML documentation comments at build time. Improve XML comments in source when generated API pages are unclear.

Do not edit generated files under `obj\docfx\api`.
