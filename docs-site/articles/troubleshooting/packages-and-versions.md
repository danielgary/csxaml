---
title: Packages and Versions Troubleshooting
description: How to diagnose CSXAML package version and artifact alignment issues.
---

# Packages and Versions Troubleshooting

Keep CSXAML packages on the same version line.

Do not mix:

- generator package from one version
- runtime package from another version
- testing package from another version
- metadata package from another version

If a clean project restores but fails during generation or runtime startup:

1. Check all `Csxaml*` package versions.
2. Clear local package caches only if restore is clearly using stale local artifacts.
3. Rebuild with a clean `obj` directory.
4. Confirm the app target framework matches the supported WinUI target.

For local package validation, prefer an isolated local feed rather than project references.
