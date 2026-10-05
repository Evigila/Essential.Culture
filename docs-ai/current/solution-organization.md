# Essential solution organization

Essential is the repository and root solution. Essential.Culture is its first independent module. The physical checkout is now C:/Users/Evigila/source/repos/Essential; the previous Essential.Culture root was renamed with Git history and uncommitted Web adaptation work intact.

Open Essential.slnx for all six Culture libraries and four test projects. Open src/Essential.Culture/Essential.Culture.slnx for the focused module, or demo/Essential.Culture.Demo.slnx for its five runnable examples. The full file tree is maintained in currentproject-architecture.md.

Root Directory.Build.props owns common organization, MIT license, unchanged Git remote metadata, deterministic builds and artifacts/packages output. Module Directory.Build.props explicitly imports the root and owns Culture VersionPrefix=1.3.0 and the existing README packaging. Current tests/Directory.Build.props and demo/Directory.Build.props explicitly import the Culture module. When adding another module, give its own source/test/demo groups explicit module settings; do not put a module's version into root props.

All Culture package IDs, assembly names, namespaces and APIs remain unchanged. Core and Generator remain framework-independent. Wpf, Avalonia and WinUI keep their existing translation behavior; Blazor remains scoped for SSR and Interactive Server. Essential does not reference Flourish. The optional UI adapters belong to Flourish/src/Flourish.Extensions, not to Essential.

The Git remote is still https://github.com/Evigila/Arkheide.Essential.Culture.git. A remote repository rename was not performed. The current vX.Y.Z CI release convention still releases Culture only, reads its module props and verifies its six packages. Future modules need their own release/version configuration.

Existing human module README and docs were relocated into src/Essential.Culture, with only Demo/license links adjusted. Existing demo READMEs stayed with their demos. Generator analyzer files and buildTransitive paths were preserved. The Blazor guide's source links now target the module directory.

Verification on 2026-10-05: root and module builds, 89 tests, five demos and all six Culture 1.3.0 packages passed. Flourish integration also passed after the path changes. No commit, push or NuGet publication was performed.

Manual acceptance: open the new root/module/demo solutions, run the existing desktop demos to exercise real XAML culture switching, run the Blazor demo on localhost:5196 and check two independent browser profiles plus cookie reload persistence. For Flourish development, prepare Essential's 1.3.0 packages first. A sibling Essential/artifacts/packages folder becomes a local NuGet feed in Flourish; it does not select Culture source projects. The old EssentialCultureRoot mechanism is retired. See [NuGet release and integration](nuget-release-integration.md) for current preparation, package-consumer validation and separately authorized publishing.
