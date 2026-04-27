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
