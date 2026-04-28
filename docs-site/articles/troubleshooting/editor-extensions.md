---
title: Editor Extension Troubleshooting
description: How to diagnose CSXAML VS Code and Visual Studio editor startup and language-server issues.
---

# Editor Extension Troubleshooting

## VS Code has highlighting but no semantic features

Check:

- the .NET 10 Desktop Runtime is installed
- the packaged extension contains a `LanguageServer` folder
- `csxaml.languageServer.path` is either unset or points to a valid server
- the `CSXAML: Restart Language Server` command has been run after server rebuilds

## Visual Studio opens `.csxaml` without CSXAML features

Check:

- the extension is installed and enabled
- the Visual Studio version is compatible with the current extension
- the machine-wide .NET 10 runtime is installed
- `%TEMP%\csxaml-visualstudio.log`
- the Visual Studio activity log under `%LOCALAPPDATA%`

For contributor testing, rerun:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Run-CsxamlVisualStudioBootstrap.ps1
```

Close the experimental instance before rerunning the bootstrap.
