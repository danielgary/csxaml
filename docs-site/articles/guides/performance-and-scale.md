---
title: Performance and Scale
description: Current scale expectations and performance boundaries for CSXAML apps.
---

# Performance and Scale

CSXAML v1 optimizes for predictable retained rendering, not for hidden virtualization.

Current performance posture:

- generated code should remain deterministic and easy to inspect
- keyed child identity is retained where explicitly requested
- repeated visible children still represent real render work
- `foreach` is not a virtualization mechanism
- state equality suppresses no-op rerenders
- `Touch()` exists for deliberate in-place mutation cases

Use stable keys for repeated children and keep rendered lists within the scale that the current runtime supports. For very large visible lists, use host-level WinUI virtualization patterns rather than assuming CSXAML adds virtualization automatically.

Benchmark automation lives in the repo scripts and artifact baselines. Treat benchmark results as environment-sensitive unless the gate explicitly marks them as blocking.
