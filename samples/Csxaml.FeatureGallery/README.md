# CSXAML Feature Gallery

This is the broad generated-app sample for the experimental CSXAML feature
track. It is intentionally separate from the Todo app: Todo stays realistic,
while this app demonstrates one feature surface at a time.

It demonstrates:

- generated `Application`, `Window`, `Page`, and `ResourceDictionary` roots
- default slots and named slots
- native and external property-content syntax
- external `[ContentProperty]` interop
- `ElementRef<T>` focus and `Frame` navigation handles
- typed event-argument handlers for loaded, key, pointer, selection, item
  click, autosuggest, slider value, and frame navigation events
- expanded attached-property metadata for `Canvas`, `RelativePanel`,
  `ToolTipService`, `VariableSizedWrapGrid`, and automation properties
- generated resource guidance and C# style helpers
- `foreach` retained rendering contrasted with native `ListView`
- app-hosted CSXAML sample code presentation with a small fallback classifier
- WinUI Fluent resources and controls, translucent cards, reveal-style hover
  states, animated card lift, Mica window backdrop, and native button hover
  motion
- package-provided Fluent control interop through `Microsoft.UI.Xaml.Controls.InfoBar`

The sample intentionally has no source XAML shell files:

- `App.xaml`
- `App.xaml.cs`
- `MainWindow.xaml`
- `MainWindow.xaml.cs`

Build it from the repository root:

```powershell
dotnet build .\samples\Csxaml.FeatureGallery\Csxaml.FeatureGallery.csproj --no-restore
```

Launch it from VS Code with `Sample: Feature Gallery (Generated App)`.
