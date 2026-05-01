# Existing WinUI + CSXAML

This sample shows how CSXAML fits into an app that already has the normal WinUI
`App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, and `MainWindow.xaml.cs` shell.

It demonstrates the supported v1 slice:

- local `Csxaml.Runtime` project reference
- repository `build/Csxaml.props` and `build/Csxaml.targets` imports through `Directory.Build.*`
- one root component mounted into a named XAML host with `CsxamlHost`
- one child component
- `State<T>` invalidation
- a `Button` `OnClick` event
- a controlled `TextBox`
- an `AutomationProperties.Name` attached property

This sample intentionally keeps the WinUI shell. Use `samples/Csxaml.HelloWorld`
or `samples/Csxaml.TodoApp` when you want generated `App.csxaml` startup.

Build it from the repository root:

```powershell
dotnet build .\samples\Csxaml.ExistingWinUI\Csxaml.ExistingWinUI.csproj --no-restore
```
