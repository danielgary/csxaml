# CSXAML

`Csxaml` is the author-facing package for building WinUI applications with `.csxaml` files.

It provides:

- the MSBuild targets that discover and generate `.csxaml`
- the packaged CSXAML generator payload
- the dependency on `Csxaml.Runtime`

Most application projects should reference `Csxaml`, not `Csxaml.Runtime` directly.

See the repo documentation for the supported language surface and release notes:

- `LANGUAGE-SPEC.md`
- `docs/release-and-versioning.md`
