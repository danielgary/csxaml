---
title: Packages and Versions Troubleshooting
description: How to diagnose CSXAML package version and artifact alignment issues.
---

# Packages and Versions Troubleshooting

Keep CSXAML packages on the same version line.

## Symptom: generation fails after changing package versions

Common snippets:

```text
unknown attribute 'Text' on native control 'Button'
Could not load file or assembly 'Csxaml.Runtime'
```

Likely causes:

1. The generator package is from one version and runtime/testing/metadata packages are from another.
2. Restore is using stale local-feed artifacts.
3. `obj` still contains generated files from an older package version.

How to verify:

```powershell
dotnet list package | Select-String Csxaml
```

Fix:

1. Put every `Csxaml*` package on the same version line.
2. Delete the project's `obj` directory.
3. Restore and build again.

## Symptom: clean project restores but runtime startup fails

Common snippets:

```text
NETSDK1045: The current .NET SDK does not support targeting .NET 10.0.
Could not load file or assembly 'Csxaml.Runtime'
```

Likely causes:

1. The app references generated components built against a different runtime.
2. The target framework does not match the supported WinUI target.
3. A local package feed is shadowing the intended package.

How to verify:

```powershell
dotnet list package
dotnet build
```

For local package validation, prefer an isolated local feed rather than project references.
