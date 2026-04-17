# PackageHello

This sample shows the intended outside-consumer package story for CSXAML.

It is not wired into the repo solution. It is a starter sample you can copy into a normal WinUI app once `Csxaml` is available from your feed of choice.

## What it demonstrates

- installing `Csxaml` as the author-facing package
- authoring a `.csxaml` component
- mounting the generated component into a WinUI window with `CsxamlHost`

## Before building

Update the package version in `PackageHello.csproj` to the version you want to consume.

If you are testing locally before a public publish, point NuGet restore at your local package feed.
