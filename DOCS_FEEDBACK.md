## Bottom line

The docs are structurally organized, but they read more like an internal release contract than a public developer guide. The biggest problems are not 404-style dead links. The bigger problems are **dead-end pages**, **thin placeholder pages**, **tutorial code that appears inconsistent with its own tests**, and **missing “here is what success looks like” guidance**.

The current homepage promises a quick start, Todo tutorial, language overview, editor setup, and API reference. That is the right top-level structure. But several of those paths do not actually get a reader to a working mental model or a working app. ([danielgary.github.io][1])

## Highest-priority fixes

### 1. Fix the Quick Start. It currently ends before the user has actually used anything.

The Quick Start shows package references, a `HelloCard.csxaml`, and `dotnet build`, then says generated components are normal C# types and can be used through the runtime host or from other generated CSXAML components. It does **not** show the runtime host code, the WinUI `MainWindow` code, or a second component using `HelloCard`. That makes step 4 a dead end. ([danielgary.github.io][2])

What to change:

````md
## 4. Render the component in a WinUI app

In MainWindow.xaml.cs:

```csharp
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var host = new CsxamlHost(RootPanel, new HelloCard("Hello from CSXAML"));
        host.Render();
    }
}
```
````

Then show the minimal `MainWindow.xaml` with `RootPanel`.

Also add:

```md
Expected result: the app window shows a StackPanel with the title and a Tap button.
```

The first tutorial should not require the reader to infer the host integration layer.

### 2. Fix the Todo tutorial test inconsistency.

The Todo tutorial’s `TodoEditor` renders two `TextBox` controls and a `CheckBox`, but the shown component code does not assign automation IDs. Later, the test calls `FindByAutomationId("SelectedTodoTitle")`, and the component testing guide also uses `SelectedTodoTitle` and `SelectedTodoDone`. As written, those examples appear inconsistent. ([danielgary.github.io][3]) ([danielgary.github.io][4])

Add automation IDs directly to the tutorial code:

```csharp
<TextBox
    AutomationProperties.AutomationId="SelectedTodoTitle"
    Text={Title}
    OnTextChanged={value => OnTitleChanged(ItemId, value)} />

<TextBox
    AutomationProperties.AutomationId="SelectedTodoNotes"
    Text={Notes}
    OnTextChanged={value => OnNotesChanged(ItemId, value)} />

<CheckBox
    AutomationProperties.AutomationId="SelectedTodoDone"
    Content="Done"
    IsChecked={IsDone}
    OnCheckedChanged={value => OnDoneChanged(ItemId, value)} />
```

This is a high-confidence bug because the docs explicitly recommend semantic queries by automation ID, but the tutorial component does not set the queried IDs.

### 3. Replace the Language Specification page with real content or a real link.

The current public spec page says the formal spec lives at the repository root in `LANGUAGE-SPEC.md`, says this page is the public entry point, then says a future cleanup can split the spec into smaller DocFX pages. That is a placeholder, not useful documentation. ([danielgary.github.io][5])

The repository spec is substantial and useful. It defines the status, design goals, parsing strategy, file shape, render syntax, state semantics, DI, lifecycle, and non-goals. Hiding that behind a placeholder page wastes one of the strongest pieces of the project.

Best fix:

- Render `LANGUAGE-SPEC.md` into the DocFX site directly.
- Add section-level pages later only after the full spec is visible.
- At minimum, make `LANGUAGE-SPEC.md` a clickable “Read the full spec” link.

### 4. Resolve the version mismatch.

The Quick Start and package install docs use `Csxaml` version `0.1.0-preview.1`. ([danielgary.github.io][2])

But the supported feature matrix says package consumption has clean validation from locally packed `1.0.0` artifacts, and the VS Code extension packaging produces an installable `1.0.0` VSIX.

That mismatch will confuse readers. Pick one public posture:

- If this is pre-release public documentation, use the latest actual published preview package everywhere.
- If this is preparing for v1, use `1.0.0-preview.*` consistently.
- If `1.0.0` is not public yet, do not mention it in user-facing docs except under contributor/release notes.

### 5. Promote the feature matrix into the public site.

The Supported Features page says the source feature matrix remains in `docs/supported-feature-matrix.md` while the DocFX site is being established. ([danielgary.github.io][6])

That matrix is more useful than the public Supported Features page. It contains concrete statuses for component parameters, state, inject, conditional markup, repeated markup, slots, attached properties, external controls, root hosting, DI activation, lifecycle, testing, editor features, package consumption, templates, VSIX workflow, and benchmarks.

Move it into DocFX as the real Supported Features page. The current page is too vague.

## Link and navigation audit

| Area               | Issue                                                                                                                                                                                                                                         | Fix                                                                                                                                     |
| ------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| Homepage           | Good start links, but it hides many important docs that are present in the full TOC, including package installation, lifecycle, diagnostics, performance, compatibility, release/versioning, and troubleshooting. ([danielgary.github.io][1]) | Add “Common tasks” cards: Install, Render first component, Debug build failure, Set up editor, Test component, Read supported features. |
| Language spec      | Placeholder page points away from the real spec instead of rendering it. ([danielgary.github.io][5])                                                                                                                                          | Render the spec or link directly to it.                                                                                                 |
| Todo tutorial      | Mentions `Csxaml.Demo` as the reference implementation but does not link to the demo source. ([danielgary.github.io][3])                                                                                                                      | Link directly to `Csxaml.Demo` and each relevant component file.                                                                        |
| Supported Features | Says the real feature matrix remains in `docs/supported-feature-matrix.md`. ([danielgary.github.io][6])                                                                                                                                       | Publish the matrix in DocFX.                                                                                                            |
| API Reference      | API overview says generated pages are produced from XML comments and published under `api/`, but the conceptual API pages mostly summarize package areas rather than linking readers into actual generated types. ([danielgary.github.io][7]) | Add links to the actual generated namespace/type pages and a “most useful types” list.                                                  |
| Editor docs        | VS Code and Visual Studio pages are mostly contributor packaging/debugging instructions, not user installation instructions. ([danielgary.github.io][8]) ([danielgary.github.io][9])                                                          | Split into “Install extension” and “Build extension from source.”                                                                       |

## Code formatting verdict

The Markdown source is mostly doing the right thing: code examples are fenced with language identifiers such as `xml`, `csharp`, `powershell`, and `text`. The Quick Start source is properly fenced.

So I would not describe the source Markdown as having unformatted code. The problem is different: the examples need more surrounding explanation, expected output, “why this matters” notes, and failure checks. Code blocks alone are not a tutorial.

## Page-by-page notes

### Homepage

The homepage is clear but too passive. It says CSXAML is experimental and source-generated, then points to five starting pages. That is fine, but it should also answer these immediately:

- What does CSXAML look like?
- Why would I use it instead of XAML/code-behind?
- What works today?
- What should I not rely on?
- Can I build and run a real app in five minutes?

Right now it describes the site more than it sells the workflow. ([danielgary.github.io][1])

### Prerequisites

The prerequisites page lists Windows, .NET SDK 10, .NET SDK 8 for the VS extension, Windows App SDK, `UseWinUI`, and nullable reference types. It also shows a typical project file. ([danielgary.github.io][10])

Missing:

- Exact Visual Studio workload requirements.
- Whether command-line only usage is supported.
- Whether `dotnet new winui` or a template is expected.
- How to verify the Windows App SDK version.
- What error appears if `.NET 10` is missing.
- Whether `net10.0-windows10.0.19041.0` is required or just the current tested target.

### Quick Start

The quick start should be the highest-polish page in the docs. It is currently too short and stops at build output. It should produce a visible app result. ([danielgary.github.io][2])

Recommended replacement structure:

1. Create or open a WinUI app.
2. Add package references.
3. Add `HelloCard.csxaml`.
4. Render it in `MainWindow`.
5. Build.
6. Run.
7. Expected output.
8. Common failures.

### Todo tutorial

The tutorial covers the right concepts: components, typed props, state, native controls, events, keyed children, controlled inputs, and testing. ([danielgary.github.io][3])

Problems:

- It is dry. A Todo app is acceptable, but this one reads like a test fixture.
- It does not show the UI outcome.
- It has the automation ID mismatch described above.
- It mentions the demo reference implementation without linking it.
- It introduces testing after a large amount of code but does not clearly say what the test proves.
- It should have “Checkpoint” sections after each component.

The tutorial would be much better if it had small visible milestones:

```md
Checkpoint: after this step, you should see two task cards.
Checkpoint: after this step, selecting a task should update the editor.
Checkpoint: after this step, editing the title should update the card.
```

### Language overview, concepts, syntax

These pages are useful but repetitive. The overview says a component file contains using directives, namespace, component declarations, optional `inject`, `State<T>`, helper code, and one final `render <Root />;` statement. ([danielgary.github.io][11])

The Syntax page has one of the most important rules: `render <Root />;` is required, and `return` markup is invalid. ([danielgary.github.io][12])

Suggested changes:

- Merge “Language Overview” and “Concepts” unless each gets a distinct job.
- Keep “Syntax” as the practical cheat sheet.
- Add “Common invalid syntax” examples:

```csharp
// Invalid
return <StackPanel />;

// Valid
render <StackPanel />;
```

```csharp
// Invalid if state is a State<int>
<TextBlock Text={Count} />

// Usually intended
<TextBlock Text={Count.Value} />
```

### State and events

The page is concise and useful. It explains `Value`, invalidation, reference mutation, `Touch()`, event delegates, lambdas, and controlled inputs. ([danielgary.github.io][13])

Needed improvements:

- Add a “wrong vs right” mutation example.
- Explain equality behavior in plain language.
- Explain whether `Count.Value++` is valid and recommended.
- Link to runtime troubleshooting directly from the `Touch()` section.

### Native props and events

This page is too shallow. It lists literals, expressions, normalized event names, controlled input, supported projected events, and attached properties. ([danielgary.github.io][14])

Needed additions:

- A table of supported native controls.
- A table of supported events with delegate shapes.
- An example of a validation failure and the corrected code.
- Link to generated metadata if available.

### Editor docs

The VS Code page describes contributed language ID, file association, grammar, snippets, language server client, prerequisites, packaging from repo root, and language server resolution. ([danielgary.github.io][8])

The Visual Studio page targets Visual Studio 2026/version 18, gives build output paths, bootstrap commands, and verification steps. ([danielgary.github.io][9])

These are contributor docs, not user docs. Split them:

- `Install VS Code extension`
- `Install Visual Studio extension`
- `Develop the VS Code extension`
- `Develop the Visual Studio extension`
- `Troubleshoot language server`

### API docs

The API pages are placeholders. The overview says API reference is generated from XML comments and lists runtime, testing, tooling, and metadata projects. ([danielgary.github.io][7])

The Runtime, Testing, Tooling, and Metadata pages mostly summarize areas. That is not enough for reference docs. ([danielgary.github.io][15]) ([danielgary.github.io][16]) ([danielgary.github.io][17]) ([danielgary.github.io][18])

Add a “most useful types” section:

```md
## Most app authors need

- `CsxamlHost`
- `State<T>`
- `ComponentInstance`
- `CsxamlTestHost`
- `RenderedComponent`

## Most tooling authors need

- parser entry point
- diagnostics service
- completion service
- workspace loader
```

Then link each item to generated API output.

### Troubleshooting

The troubleshooting section has the right categories: build/generation, editor extensions, packages/versions, runtime behavior.

But the individual pages are too generic. For example, build troubleshooting says to confirm package references, check generated output under `obj`, inspect namespace imports, and inspect generated files/source maps.

Make this much more concrete:

````md
Symptom: dotnet build succeeds but no generated file appears
Likely causes:

1. The file extension is not `.csxaml`.
2. The `Csxaml` package is not referenced in this project.
3. The item is excluded by MSBuild.

How to verify:

```powershell
dotnet build /bl
dir obj\Debug\net10.0-windows10.0.19041.0\Csxaml
```
````

````

Troubleshooting docs should be symptom-first, not architecture-first.

## Recommended revised docs structure

Use this structure:

```text
Home
Getting Started
  - What CSXAML is
  - Prerequisites
  - Quick Start: render your first component
  - From blank WinUI app
Tutorials
  - Counter component
  - Todo board
  - Component testing
Language
  - Syntax cheat sheet
  - Components and props
  - State and events
  - Control flow
  - Native controls
  - External controls
  - Dependency injection
  - Lifecycle
Reference
  - Supported feature matrix
  - Compatibility policy
  - Runtime API
  - Testing API
  - Tooling API
  - Metadata API
Troubleshooting
  - Build/generation failures
  - Editor/language server failures
  - Runtime behavior
  - Package version mismatch
Contributing
  - Build docs
  - Build packages
  - Build editor extensions
  - Release process
````

## Concrete improvement backlog

### P0

1. Fix Todo tutorial automation IDs.
2. Add actual host/render code to Quick Start.
3. Render or link the full language spec.
4. Resolve `0.1.0-preview.1` vs `1.0.0` documentation mismatch.
5. Publish the full supported feature matrix inside DocFX.

### P1

1. Split editor docs into user install docs and contributor build docs.
2. Turn troubleshooting into symptom-first pages.
3. Add expected output/checkpoints to Quick Start and Todo tutorial.
4. Add direct links to demo source files.
5. Add direct links from API overview to generated type pages.

### P2

1. Add screenshots or short GIFs for the Todo tutorial and editor features.
2. Add “wrong vs right” examples throughout syntax/state/native controls.
3. Add a docs quality checklist to contributing docs.
4. Add link checking to the docs build.
5. Add a small “Why CSXAML?” page comparing CSXAML to XAML/code-behind and handwritten C# UI trees.

## The most important rewrite target

Rewrite the Quick Start first. It is the page that decides whether the docs feel serious. The current version proves that files can build. A good quick start proves that the reader can create, render, run, and understand a CSXAML component.

[1]: https://danielgary.github.io/csxaml "CSXAML Documentation | CSXAML Documentation "
[2]: https://danielgary.github.io/csxaml/articles/getting-started/quick-start.html "Quick Start | CSXAML Documentation "
[3]: https://danielgary.github.io/csxaml/articles/tutorials/todo-app.html "Build a Todo App | CSXAML Documentation "
[4]: https://danielgary.github.io/csxaml/articles/guides/component-testing.html "Component Testing | CSXAML Documentation "
[5]: https://danielgary.github.io/csxaml/articles/language/specification.html "Language Specification | CSXAML Documentation "
[6]: https://danielgary.github.io/csxaml/articles/language/supported-features.html "Supported Features | CSXAML Documentation "
[7]: https://danielgary.github.io/csxaml/articles/api/index.html "API Overview | CSXAML Documentation "
[8]: https://danielgary.github.io/csxaml/articles/editors/visual-studio-code.html "VS Code Extension | CSXAML Documentation "
[9]: https://danielgary.github.io/csxaml/articles/editors/visual-studio.html "Visual Studio Extension | CSXAML Documentation "
[10]: https://danielgary.github.io/csxaml/articles/getting-started/prerequisites.html "Prerequisites | CSXAML Documentation "
[11]: https://danielgary.github.io/csxaml/articles/language/index.html "Language Overview | CSXAML Documentation "
[12]: https://danielgary.github.io/csxaml/articles/language/syntax.html "Syntax | CSXAML Documentation "
[13]: https://danielgary.github.io/csxaml/articles/language/state-and-events.html "State and Events | CSXAML Documentation "
[14]: https://danielgary.github.io/csxaml/articles/guides/native-props-and-events.html "Native Props and Events | CSXAML Documentation "
[15]: https://danielgary.github.io/csxaml/articles/api/runtime.html "Runtime API | CSXAML Documentation "
[16]: https://danielgary.github.io/csxaml/articles/api/testing.html "Testing API | CSXAML Documentation "
[17]: https://danielgary.github.io/csxaml/articles/api/tooling.html "Tooling API | CSXAML Documentation "
[18]: https://danielgary.github.io/csxaml/articles/api/metadata.html "Metadata API | CSXAML Documentation "
