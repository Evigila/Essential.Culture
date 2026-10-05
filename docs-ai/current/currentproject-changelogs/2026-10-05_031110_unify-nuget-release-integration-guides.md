# Culture package release documentation

Essential.Culture 1.3.0 remains independently versioned under src/Essential.Culture and releases all six module packages. Its current remote repository remains Evigila/Arkheide.Essential.Culture, despite the local checkout name Essential.

## Changes

- Added nuget-release-integration.md for the actual Prepare/Publish helper parameters, package verification, clean master requirements, exact v1.3.0 confirmation and Trusted Publishing workflow.
- Documented the Blazor service's scoped culture/formatting state, catalog-specific and formatted lookup methods, refresh event and startup builder.
- Updated the existing currentproject-architecture.md directory map to cover 107 maintained files and 31 directories, including scripts, the batch entry and Culture.Blazor responsibilities.
- Corrected the active solution/demo guidance: demo source mode is explicitly UseLocalCulture=true, while Flourish consumes Culture packages and a sibling artifacts folder supplies a NuGet feed.
- Kept existing module/demo README packaging and human documentation intact. No duplicate module/root documentation system was created.

## Evidence and limits

ReleaseSettings, Test-Release, Publish-Helper, package verification, the workflow, version props, Blazor API and actual origin were inspected. The coordinating implementation task reports successful release and package verification. No public release, commit, tag creation, push or build was performed by this documentation task.

The root currently lacks AGENTS.md and AGENTS.ensure.json. This was reported to the coordinating agent; the existing docs-ai/current system was used without bootstrapping a second documentation system.

## Acceptance

Prepare and inspect all six 1.3.0 packages, validate generator build assets and package consumers, then let the user confirm any publishing action. Confirm publicly indexed Essential packages before a dependent Flourish 1.1.0 release. Keep the desktop demos and scoped Web regression checks part of release acceptance.
