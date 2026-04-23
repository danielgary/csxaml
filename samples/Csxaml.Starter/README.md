# CSXAML Starter Sample

This sample is the smallest WinUI app in the repository that hosts generated CSXAML components.

It demonstrates the supported v1 slice:

- local `Csxaml.Runtime` project reference
- repository `build/Csxaml.props` and `build/Csxaml.targets` imports through `Directory.Build.*`
- one root component mounted through `CsxamlHost`
- one child component
- `State<T>` invalidation
- a `Button` `OnClick` event
- a controlled `TextBox`
- an `AutomationProperties.Name` attached property

Build it from the repository root:

```powershell
dotnet build .\samples\Csxaml.Starter\Csxaml.Starter.csproj --no-restore
```
