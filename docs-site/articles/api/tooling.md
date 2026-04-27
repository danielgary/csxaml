---
title: Tooling API
description: Overview of CSXAML APIs for editor and language-service integrations.
---

# Tooling API

`Csxaml.Tooling.Core` is the shared language-service library used by the VS Code and Visual Studio integrations.

Major services:

- completion
- diagnostics
- definitions
- formatting
- hover
- semantic tokens
- code actions
- C# projection
- workspace component discovery

Tooling APIs are intended for editor and language-server hosts. They should stay aligned with the language spec and the generator's parser/validation behavior.
