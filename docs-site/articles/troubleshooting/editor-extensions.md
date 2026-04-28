---
title: Editor Extension Troubleshooting
description: How to diagnose CSXAML VS Code and Visual Studio editor startup and language-server issues.
---

# Editor Extension Troubleshooting

## Symptom: VS Code has highlighting but no semantic features

Common snippets:

```text
Could not load Csxaml.LanguageServer
Failed to start CSXAML language server
```

Likely causes:

1. The .NET 10 Desktop Runtime is missing.
2. The packaged extension does not contain a `LanguageServer` folder.
3. `csxaml.languageServer.path` points to a missing server.
4. The server was rebuilt but VS Code is still running the old process.

How to verify:

1. Open the CSXAML output channel.
2. Confirm the server path exists.
3. Run **CSXAML: Restart Language Server**.
4. Leave `csxaml.languageServer.path` empty when validating the bundled server.

## Symptom: Visual Studio opens `.csxaml` without CSXAML features

Common snippets:

```text
Could not load Csxaml.LanguageServer
The CSXAML language server exited before initialization
```

Likely causes:

1. The extension is not installed or enabled.
2. The Visual Studio version is not compatible with the current extension.
3. The machine-wide .NET 10 runtime is missing.
4. The language server failed during startup.

How to verify:

1. Check `%TEMP%\csxaml-visualstudio.log`.
2. Check the Visual Studio activity log under `%LOCALAPPDATA%`.
3. Confirm the installed extension targets Visual Studio 2026 / version 18.

For contributor testing, rerun:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Run-CsxamlVisualStudioBootstrap.ps1
```

Close the experimental instance before rerunning the bootstrap.
