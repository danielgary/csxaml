# Plan: TextMate-Based CSXAML Highlighting for DocFX

## Status

Implemented locally on 2026-04-28. The final verification run passed with
`scripts/docs/Invoke-DocsBuild.ps1 -SkipProjectBuild`, including DocFX warnings
as errors, Shiki highlighting, code-fence validation, rendered include checks,
local link checks, and report-only external link checks.

## Goal

Improve CSXAML examples in the DocFX website by rendering fenced `csxaml`
code blocks with the same TextMate grammar used by the VS Code extension.

The docs should show CSXAML as its own language, not as approximate C#:

- `component Element`
- `render <Root />;`
- `State<T>` and `inject`
- native and component tags
- attributes and attached properties
- event attributes such as `OnClick`
- C# expression islands such as `{Count.Value}`

The intended audience is developers, so the result should be accurate enough
to help them read real `.csxaml` files and should stay aligned with editor
highlighting over time.

## Decision

Use Shiki at docs build time and load the existing VS Code TextMate grammars as
the source of truth:

- `VSCodeExtension/syntaxes/csxaml.tmLanguage.json`
- `VSCodeExtension/syntaxes/csxaml-embedded-csharp.tmLanguage.json`

Do not build a separate regex highlighter for DocFX. A separate highlighter
would immediately drift from the VS Code extension and would duplicate the
hardest part of the work: CSXAML plus embedded C# tokenization.

Shiki should run after `docfx build` and rewrite the built HTML files under
`_site`. This keeps DocFX responsible for Markdown, templates, search, links,
API pages, and copy-code structure while limiting the new behavior to
`csxaml` code blocks.

## References

- Shiki custom language loading:
  https://shiki.style/guide/load-lang
- Shiki light and dark dual themes:
  https://shiki.style/guide/dual-themes
- DocFX custom template `public/main.css` / `public/main.js` extension points:
  https://dotnet.github.io/docfx/docs/template.html

## Non-Goals

- Do not replace DocFX.
- Do not change the CSXAML language.
- Do not fork or duplicate the existing TextMate grammar.
- Do not add browser-side highlighting work for readers.
- Do not recolor plain C#, XML, PowerShell, or console snippets as CSXAML.
- Do not make semantic compiler decisions in the highlighter.
- Do not edit generated `_site` files by hand.

## Current State

Relevant files:

- `docfx.json`
- `docs-site/index.md`
- `docs-site/articles/**/*.md`
- `scripts/docs/Invoke-DocsBuild.ps1`
- `scripts/docs/Test-DocsLinks.ps1`
- `scripts/docs/Test-DocsRenderedIncludes.ps1`
- `scripts/docs/Test-DocsExternalLinks.ps1`
- `VSCodeExtension/package.json`
- `VSCodeExtension/syntaxes/csxaml.tmLanguage.json`
- `VSCodeExtension/syntaxes/csxaml-embedded-csharp.tmLanguage.json`
- `.github/workflows/docs.yml`

Confirmed observations:

- The VS Code extension already contributes the `csxaml` language id.
- The primary grammar has `scopeName: source.csxaml`.
- The embedded C# grammar has `scopeName: source.csxaml.embedded.csharp`.
- The extension maps CSXAML embedded scopes to `csharp`.
- The DocFX site currently emits fenced code as classes such as `lang-csharp`,
  `lang-xml`, `lang-powershell`, and `lang-text`.
- CSXAML examples in the docs are currently mostly fenced as `csharp`.
- DocFX modern template already links `public/main.css` and
  `public/main.js`; customizations should be supplied through a custom DocFX
  template folder.
- The docs GitHub Actions workflow does not currently install Node.js.
- `docs-site/node_modules/**` must stay excluded from DocFX content so package
  dependency README files do not become site pages.

## Architecture

The implementation has four small pieces:

1. A docs-local Node package that pins Shiki and the HTML parser.
2. A post-build Node script that rewrites only built `csxaml` code blocks.
3. A small DocFX template stylesheet for Shiki light/dark output.
4. Docs build and CI wiring that runs the highlighter before validation.

Keep the source of truth in this order:

1. CSXAML grammar: `VSCodeExtension/syntaxes/*.tmLanguage.json`
2. Markdown language choice: ` ```csxaml ` for authored CSXAML snippets
3. Build output: Shiki-highlighted static HTML in `_site`

## Phase 1: Add Docs Node Package

Create `docs-site/package.json`.

Recommended shape:

```json
{
  "name": "csxaml-docs-site",
  "private": true,
  "type": "module",
  "scripts": {
    "highlight:csxaml": "node ../scripts/docs/Highlight-DocsCsxamlCode.mjs --site-root ../_site",
    "check:csxaml-highlight": "node ../scripts/docs/Highlight-DocsCsxamlCode.mjs --site-root ../_site --check"
  },
  "devDependencies": {
    "cheerio": "<locked by npm install>",
    "shiki": "<locked by npm install>"
  }
}
```

Run this from `docs-site` so `package-lock.json` is generated and checked in:

```powershell
npm install --save-dev shiki cheerio
```

Notes:

- Use Node.js 22 because the repo workflows already use Node 22 elsewhere.
- Do not reuse `VSCodeExtension/package.json`; docs dependencies should not
  affect extension packaging.
- Keep `docs-site/node_modules` ignored by the existing `node_modules/`
  `.gitignore` rule.

## Phase 2: Add the Shiki Post-Build Script

Add `scripts/docs/Highlight-DocsCsxamlCode.mjs`.

Responsibilities:

- Parse command-line options:
  - `--site-root <path>`: required or default to `_site` relative to repo root.
  - `--repo-root <path>`: optional, default by walking up from the script.
  - `--check`: verify highlighted output is present without rewriting files.
- Load the existing TextMate grammars from `VSCodeExtension/syntaxes`.
- Create one Shiki highlighter for the whole run.
- Recursively scan `_site/**/*.html`.
- Find only code blocks with `code.lang-csxaml` or
  `code.language-csxaml`.
- Extract the code block text through the HTML parser so entities such as
  `&lt;` become real source text before Shiki sees them.
- Render each block with Shiki and replace the surrounding `<pre>` element.
- Mark rewritten blocks with `data-csxaml-highlighted="true"`.
- Print a short summary with scanned file count and highlighted block count.
- Exit non-zero when `--check` finds unhighlighted `csxaml` code blocks.

Use an HTML parser such as Cheerio. Do not regex-rewrite HTML.

Suggested Shiki setup:

```js
import { createHighlighter } from 'shiki'

const highlighter = await createHighlighter({
  themes: ['github-light', 'github-dark'],
  langs: [
    'csharp',
    {
      name: 'csxaml-embedded-csharp',
      scopeName: 'source.csxaml.embedded.csharp',
      embeddedLangs: ['csharp'],
      ...embeddedCSharpGrammar
    },
    {
      name: 'csxaml',
      scopeName: 'source.csxaml',
      embeddedLangs: ['csharp'],
      ...csxamlGrammar
    }
  ]
})
```

Suggested render call:

```js
const html = highlighter.codeToHtml(code, {
  lang: 'csxaml',
  themes: {
    light: 'github-light',
    dark: 'github-dark'
  },
  defaultColor: 'light'
})
```

After rendering, parse the returned Shiki fragment and add:

- `data-csxaml-highlighted="true"` on the Shiki `<pre>`.
- A stable class such as `csxaml-shiki` on the Shiki `<pre>`.

The script should preserve all non-CSXAML code blocks exactly as DocFX emitted
them.

Important Shiki detail:

- The CSXAML grammars include `source.cs` and
  `source.csxaml.embedded.csharp`.
- Load Shiki's built-in `csharp` language and both CSXAML grammar files.
- If Shiki reports an unresolved include, fix the grammar registration in the
  script first. Do not edit the grammar unless the grammar itself is wrong for
  VS Code too.
- The helper-code fallback in `csxaml.tmLanguage.json` must stop before
  `component` declarations; otherwise full file-shaped examples with
  `using`/`namespace` lines swallow the component declaration as plain C#.

## Phase 3: Add DocFX Template CSS

Add a custom DocFX template folder:

- `docs-site/templates/csxaml/public/main.css`

Update `docfx.json`:

```json
"template": [
  "default",
  "modern",
  "docs-site/templates/csxaml"
]
```

The CSS should style Shiki blocks without changing ordinary DocFX code blocks.

Required behavior:

- Preserve readable spacing, border radius, and horizontal scrolling.
- Use DocFX/Bootstrap variables where practical.
- Support DocFX light and dark theme switching.
- Do not make all code blocks look different; target only `.csxaml-shiki` or
  `.shiki[data-csxaml-highlighted="true"]`.

Minimum CSS shape:

```css
.csxaml-shiki {
  margin: 1rem 0;
  padding: 1rem;
  overflow-x: auto;
  border: 1px solid var(--bs-border-color);
  border-radius: 0.375rem;
}

html[data-bs-theme="dark"] .csxaml-shiki,
html[data-bs-theme="dark"] .csxaml-shiki span {
  color: var(--shiki-dark) !important;
  background-color: var(--shiki-dark-bg) !important;
  font-style: var(--shiki-dark-font-style) !important;
  font-weight: var(--shiki-dark-font-weight) !important;
  text-decoration: var(--shiki-dark-text-decoration) !important;
}
```

Check whether DocFX's copy-code behavior still sees the Shiki
`<pre><code>...</code></pre>` shape. If copy buttons stop working, adjust the
postprocessor to preserve the class or attribute DocFX expects on the new
`<pre>` or `<code>`.

## Phase 4: Convert Markdown Fences

Convert authored CSXAML examples from `csharp` to `csxaml`.

Review these files first:

- `docs-site/index.md`
- `docs-site/articles/getting-started/*.md`
- `docs-site/articles/language/*.md`
- `docs-site/articles/guides/*.md`
- `docs-site/articles/tutorials/*.md`
- `docs-site/articles/troubleshooting/*.md`

Use `csxaml` fences for:

- Full `.csxaml` component examples.
- Snippets containing `component Element`.
- Snippets containing a `render <... />;` statement.
- Standalone CSXAML markup examples used to explain tags or attributes.
- CSXAML state, event, `inject`, or helper-code snippets shown as source from
  a `.csxaml` file.

Keep existing languages for:

- `.csproj` XML.
- NuGet or MSBuild XML.
- Generated C#.
- Pure C# tests or runtime examples.
- PowerShell commands.
- Console or diagnostic output.
- JSON or YAML.

Use this search to find likely candidates:

```powershell
rg -n "```csharp|component Element|render <|State<|inject |<TextBlock|<StackPanel|<Button|OnClick" docs-site docs LANGUAGE-SPEC.md README.md
```

Do not bulk-replace every `csharp` fence. Review each block in context.

## Phase 5: Wire the Docs Build

Update `scripts/docs/Invoke-DocsBuild.ps1`.

Required changes:

- Resolve `npm.cmd` / `npm` before the docs build when
  `docs-site/package.json` exists.
- Run `npm ci` with `.\docs-site` as the working directory.
- Run DocFX build normally.
- Run `npm run highlight:csxaml` with `.\docs-site` as the working directory
  after DocFX writes
  `_site`.
- Run the rendered include, local link, and external link checks after the
  highlighter so validation sees the final published HTML.

Serve mode needs special handling:

- Do not use `dotnet docfx build ... --serve`, because that serves before the
  post-build highlighter can rewrite `_site`.
- Instead, for `-Serve`:
  1. Run `dotnet docfx build .\docfx.json`.
  2. Run the Shiki highlighter.
  3. Run `dotnet docfx serve .\_site`.

Keep existing switches working:

- `-SkipProjectBuild`
- `-AllowWarnings`
- `-SkipLinkCheck`
- `-SkipExternalLinkCheck`
- `-Serve`

## Phase 6: Add Validation

Add `scripts/docs/Test-DocsCodeFences.ps1`.

Checks:

- Fail if a Markdown code fence in `docs-site/**/*.md` appears to contain
  CSXAML source but is not fenced as `csxaml`.
- Fail if built HTML still contains `code.lang-csxaml` or
  `code.language-csxaml` after the highlighter ran.
- Fail if built HTML contains no `data-csxaml-highlighted="true"` blocks once
  the docs have been converted.

Suggested heuristic for authored Markdown:

- Track fenced code blocks and their declared language.
- Treat a block as likely CSXAML if it contains any of:
  - `component Element`
  - `render <`
  - `inject `
  - `State<`
  - a line starting with `<TextBlock`, `<StackPanel`, `<Button`, `<Border`,
    `<Grid`, or `<ListView`
- Ignore blocks declared as `xml`, `powershell`, `text`, `json`, or `yaml`
  unless they contain `component Element` or `render <`.

Wire this test into `Invoke-DocsBuild.ps1` after the Shiki highlighter and
before link checks.

## Phase 7: Update CI

Update `.github/workflows/docs.yml`.

Add Node setup before the docs build:

```yaml
- name: Setup Node.js
  uses: actions/setup-node@v4
  with:
    node-version: 22
    cache: npm
    cache-dependency-path: docs-site/package-lock.json
```

Keep the existing .NET setup and docs artifact upload unchanged.

The regular CI workflow already installs Node for extension packaging, but the
docs workflow is separate and must be self-contained.

## Phase 8: Update Roadmap and Developer Notes

Update `ROADMAP.md` only if this work materially changes project status. A
short current-position bullet is enough if the docs site gains CSXAML-specific
highlighting.

Possible wording:

- DocFX docs render `csxaml` code fences with the same TextMate grammar used by
  the VS Code extension.

If a troubleshooting or editor docs page mentions syntax highlighting, update
it only if the new docs-site behavior is relevant to developers.

## Verification

For an isolated highlighter check, build `_site` and then run the highlighter:

```powershell
Push-Location .\docs-site
npm ci
Pop-Location

dotnet docfx build .\docfx.json

Push-Location .\docs-site
npm run highlight:csxaml
npm run check:csxaml-highlight
Pop-Location
```

For the integrated check, run the full docs build from the repo root:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\docs\Invoke-DocsBuild.ps1 -SkipProjectBuild
```

Manual smoke checks:

- Open `_site/index.html`.
- Open `_site/articles/language/syntax.html`.
- Open `_site/articles/getting-started/quick-start.html`.
- Confirm CSXAML examples are highlighted differently from ordinary C#.
- Toggle DocFX light/dark theme and confirm tokens remain readable.
- Confirm non-CSXAML C#, XML, PowerShell, and text blocks still look normal.
- Confirm the copy-code behavior still copies the original source text.

HTML checks:

```powershell
rg -n "data-csxaml-highlighted" .\_site
rg -n "code class=\"lang-csxaml\"|code class=\"language-csxaml\"" .\_site
```

The first command should find highlighted blocks. The second command should
find no unprocessed CSXAML code blocks.

## Acceptance Criteria

- `csxaml` Markdown fences are used for authored CSXAML examples.
- The existing VS Code TextMate grammar is used by the docs highlighter.
- No separate DocFX-only CSXAML grammar exists.
- The generated site contains statically highlighted CSXAML HTML.
- Light and dark DocFX themes keep CSXAML blocks readable.
- Non-CSXAML fences are untouched.
- Docs build script runs the highlighter automatically.
- Docs CI installs Node and runs the highlighter automatically.
- Validation fails if future CSXAML examples are accidentally fenced as C#.
- Full docs build passes.

## Rollback Plan

If Shiki integration blocks the docs build:

1. Revert the `docfx.json` custom template entry.
2. Stop invoking `npm run highlight:csxaml` from `Invoke-DocsBuild.ps1`.
3. Keep the `csxaml` Markdown fences if possible; they are still semantically
   correct even without custom rendering.
4. Do not delete the VS Code grammar. It remains the editor source of truth.

## Implementation Order

Follow this order and do not move forward until the current step passes:

1. Add the docs Node package and lock file.
2. Build a tiny local HTML fixture and prove the Shiki script can highlight one
   `lang-csxaml` block using the existing grammar.
3. Run DocFX once and prove the script rewrites real `_site` pages.
4. Add the custom DocFX template CSS and confirm light/dark readability.
5. Convert Markdown fences in a small docs slice, then run the docs build.
6. Convert the remaining docs fences.
7. Add the code-fence validation script.
8. Wire the highlighter and validation into `Invoke-DocsBuild.ps1`.
9. Update the docs workflow with Node setup.
10. Run the full verification commands.
