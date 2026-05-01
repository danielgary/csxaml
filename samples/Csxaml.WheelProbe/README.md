# WinUI Wheel Probe

This is a hand-written WinUI comparison app for mouse-wheel routing.

It does not use `Csxaml.Runtime`. The app hosts three ordinary WinUI `ListView`
controls:

- a bare `ListView`
- a `ListView` inside a `Border`
- a `ListView` inside `Csxaml.ExternalControls.FluentCard`

The status text records both XAML `PointerWheelChanged` routing and native
`WM_MOUSEWHEEL` / `WM_POINTERWHEEL` messages observed on the root WinUI HWND and
the `Microsoft.UI.Content.DesktopChildSiteBridge` child HWND.

Run it with:

```powershell
dotnet run --project .\samples\Csxaml.WheelProbe\Csxaml.WheelProbe.csproj
```
