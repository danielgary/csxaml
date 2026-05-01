# CSXAML Todo App

This is the advanced generated-app sample. It demonstrates a realistic Todo app
without source XAML app-shell files.

It intentionally has no:

- `App.xaml`
- `App.xaml.cs`
- `MainWindow.xaml`
- `MainWindow.xaml.cs`

The app shell is authored through `component Application`, `component Window`,
`component Page`, and a limited `component ResourceDictionary` while
`CsxamlApplicationMode=Generated` asks the CSXAML generator to emit the WinUI
entry point and the hidden intermediate `App.xaml` needed for WinUI default
control resources.

`App.csxaml` is the app entry:

- `startup MainWindow;` selects the generated startup window.
- `resources AppResources;` declares the limited generated resource dictionary.
- `ConfigureServices()` registers the Todo service used by `inject ITodoService`.

The visible app includes component state, injected services, controlled inputs,
keyed repeated children, attached automation properties, external
`StatusButton` interop, and a generated-app proof panel.

Build it from the repository root:

```powershell
dotnet build .\samples\Csxaml.TodoApp\Csxaml.TodoApp.csproj --no-restore
```
