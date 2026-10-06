# Align the dependent Blazor release scope

The active release/integration guide now follows the approved Flourish first-release boundary: publish Essential's six Culture 1.3.0 packages first, then Flourish's six Core/Blazor 1.1.0 packages. Flourish Shared has been consolidated, its Blazor convenience package is dependency-only, and its WPF source is excluded from that release. The earlier fourteen-ID/eight-Flourish query is explicitly historical evidence rather than the current dependent manifest.

Essential's own six-package set and APIs are unchanged by this documentation correction. Its complete preparation remains verified with 89 passing tests, five demo builds and the clean Blazor-only NuGet consumer. No commit, tag, push or public package upload was performed. NuGet account ownership/trust configuration and public availability must be checked for the later authorized release.
