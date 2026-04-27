---
title: Quick Start
description: Install CSXAML and create a first generated WinUI component.
---

# Quick Start

This quick start assumes an existing WinUI app or library project.

## 1. Install the package

Add the author-facing package:

```xml
<PackageReference Include="Csxaml" Version="0.1.0-preview.1" />
```

Keep the Windows App SDK package in the app project:

```xml
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260209005" />
```

## 2. Add a component

Create `HelloCard.csxaml`:

```csharp
using Microsoft.UI.Xaml.Controls;

namespace MyApp.Components;

component Element HelloCard(string Title) {
    render <StackPanel Spacing={8}>
        <TextBlock Text={Title} />
        <Button Content="Tap" />
    </StackPanel>;
}
```

## 3. Build

Build the project:

```powershell
dotnet build
```

The package-provided targets discover `.csxaml` files, run the packaged generator, and write generated C# under `obj\<configuration>\<tfm>\Csxaml\Generated\`.

## 4. Use the generated component

Generated components are normal C# types in the namespace declared by the `.csxaml` file. Use them through the CSXAML runtime host or from other generated CSXAML components.

## 5. Next

Build the [Todo tutorial](../tutorials/todo-app.md) to learn state, props, events, child components, and testing.
