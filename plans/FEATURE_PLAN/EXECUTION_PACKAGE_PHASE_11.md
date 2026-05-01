# Execution Package Phase 11: CSXAML Highlighting In Sample Presenters And Fixtures

## Purpose

Ensure CSXAML source is highlighted consistently wherever developers see it:

- VS Code
- DocFX
- Visual Studio language service surfaces
- app-hosted sample presenters
- tests and fixtures

The existing VS Code TextMate grammar should remain the authoritative source
where practical.

## Non-Goals

- Do not fork a second full CSXAML grammar without tests.
- Do not hand-edit generated `_site` output.
- Do not make semantic compiler decisions inside a simple sample highlighter.
- Do not regress existing DocFX Shiki highlighting.

## Files To Review First

- `VSCodeExtension/syntaxes/csxaml.tmLanguage.json`
- `VSCodeExtension/syntaxes/csxaml-embedded-csharp.tmLanguage.json`
- `VSCodeExtension/test/csxamlGrammar.test.js`
- `scripts/docs/Highlight-DocsCsxamlCode.mjs`
- `docs-site/package.json`
- `docs-site/templates/csxaml/public/main.css`
- `Csxaml.Tooling.Core/Net10/SemanticTokens`
- `Csxaml.LanguageServer/Server/CsxamlLspServer.SemanticTokens.cs`
- any sample presenter or Gallery-style code presenter introduced by the app
- docs-render checks

## Implementation Steps

### 1. Update Grammar For New Syntax

Add TextMate support for:

- `component Application`
- `component Window`
- `component Page`
- `component ResourceDictionary`
- `startup`
- `resources`
- `Ref`
- property-content tags such as `<Button.Flyout>`
- named slot syntax
- new event examples only if grammar needs special handling

Keep embedded C# highlighting working inside:

- attribute expressions
- component parameters
- state initializers
- helper code
- application service helper code

### 2. Update Grammar Tests

Add examples that cover:

- generated application roots
- generated window roots
- property-content blocks
- named slots
- refs
- typed event args
- broader attached properties
- resource dictionary roots

Tests should verify important scopes rather than snapshotting enormous token
streams.

### 3. DocFX Shiki Path

- Ensure DocFX still loads the same grammar.
- Add docs-render checks for new `csxaml` examples.
- Ensure docs examples are fenced as `csxaml`.
- Keep `scripts/docs/Invoke-DocsBuild.ps1` validation strict.

### 4. App-Hosted Sample Presenter

If the repo has or adds a `SampleCodePresenter`:

- create a tiny highlighting abstraction.
- prefer TextMate/Shiki-generated spans if practical.
- if runtime TextMate is too heavy, create a small fallback classifier covering:
  - comments
  - component/render/startup/resources keywords
  - tags
  - property-content names
  - attributes
  - string literals
  - C# expression islands
- document that fallback classifier is not the language authority.
- add tests for representative snippets.

### 5. Tooling Semantic Tokens

- Update semantic token service for new root kinds and property-content nodes.
- Ensure semantic coloring does not fight TextMate scopes.
- Add tests for token classification.

### 6. Snippets

Add snippets for:

- `component Element`
- `component Page`
- `component Window`
- `component Application`
- `component ResourceDictionary`
- `Ref`
- property-content block
- named slot declaration and usage

### 7. Documentation Updates

Update:

- `FEATURE_PLAN.md` status table.
- `ROADMAP.md`.
- `docs/supported-feature-matrix.md`.
- `docs-site/articles/editors/visual-studio-code.md`.
- `docs-site/articles/editors/language-service-features.md`.
- `VSCodeExtension/README.md`.
- any sample presenter docs.

## Validation Checks

VS Code extension:

```powershell
Push-Location VSCodeExtension
npm test
Pop-Location
```

Tooling:

```powershell
dotnet test .\Csxaml.Tooling.Core.Tests\Csxaml.Tooling.Core.Tests.csproj --no-restore -m:1 -p:UseAppHost=false
```

Docs:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/docs/Invoke-DocsBuild.ps1 -SkipProjectBuild
```

Full verification if semantic tokens or shared tooling changed:

```powershell
dotnet test .\Csxaml.sln --no-restore -m:1 -nr:false
```

## Completion Criteria

This phase is complete only when:

- new syntax highlights in VS Code.
- DocFX renders new `csxaml` fences through Shiki.
- semantic tokens cover new syntax where applicable.
- snippets help authors create new constructs.
- sample presenters color CSXAML or clearly use the documented fallback.
- grammar, docs, fixtures, and status surfaces agree.

