# Retire stale restore caches after repository migration

This record follows merge-culture-extensions-and-essential-root without changing that append-only record.

Final root validation found 15 generated NuGet files under the old Essential.Culture/src/*/obj paths. Their timestamps match the first post-migration restore; there were no source files, project files or Git history in that residual directory. The migrated current project.assets.json files already point to the new Essential module paths.

The old cache-only root was verified by resolved absolute path and by every file's obj location/type, then moved to Essential/artifacts/solution-migration/old-root-cache. The earlier Gallery staticwebassets.publish.json still contained old source paths, so it was moved to Flourish/artifacts/solution-migration/gallery-staticwebassets.publish.previous.json. No maintained files or history were deleted. Both root solutions were then restored with --force --force-evaluate.

Final verification passed: neither old root exists, every project/file entry in the five Flourish and three Essential solution files resolves, Git diff --check passes, maintained Flourish README inventory is empty, old source/package names are absent from maintained build inputs, all six Culture plus two extension package identities/dependencies are correct, the Culture README bytes match the relocated module source, and all Generator analyzer/buildTransitive assets remain packed. Both temporary verification hosts are stopped. YAML parsing also passed for both migration workflows.

The previous passing build/test/browser results remain valid; this follow-up only invalidates generated restore/publish caches. Original caches remain in ignored rollback material. No commit, push or NuGet publication was performed.
