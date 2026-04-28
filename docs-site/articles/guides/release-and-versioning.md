---
title: Release and Versioning
description: CSXAML release posture, package version alignment, and extension versioning.
---

# Release and Versioning

CSXAML uses one coordinated version line across public packages and editor extensions.

Release governance:

- semantic versioning is the public version contract
- Conventional Commits drive release notes and version bumps
- `git-cliff` generates changelog and release notes
- GitHub Actions builds, packages, and publishes artifacts
- preview releases come from the preview release path
- stable releases come from the stable release path

Normal consumers should keep CSXAML packages on the same version line. Generator, runtime, testing, and metadata package version mixing is not part of the v1 promise.

For package install details, see [Package Installation](package-installation.md).

## App package example

A normal app references the author-facing package and keeps Windows App SDK in
the project:

```xml
<PackageReference Include="Csxaml" Version="0.1.0-preview.1" />
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260209005" />
```

If the app also has component tests, keep `Csxaml.Testing` on the same CSXAML
version line:

```xml
<PackageReference Include="Csxaml.Testing" Version="0.1.0-preview.1" />
```

Do not mix CSXAML package versions deliberately:

```xml
<!-- Avoid this kind of split. -->
<PackageReference Include="Csxaml" Version="0.1.0-preview.1" />
<PackageReference Include="Csxaml.Testing" Version="0.2.0-preview.1" />
```

When a release upgrades CSXAML, upgrade the package set together, delete stale
`obj` output if generated files look old, and rebuild before investigating
source-level diagnostics.

## Branch examples

- A docs-only pull request can pass the docs workflow without publishing the
  public site.
- A push to `develop` follows the preview release path.
- A push to `master` follows the stable release path.
