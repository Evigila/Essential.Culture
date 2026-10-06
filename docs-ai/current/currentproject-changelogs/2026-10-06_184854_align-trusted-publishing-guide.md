# Trusted Publishing release guide alignment

Recorded at 2026-10-06 18:48:54 America/Sao_Paulo after the user's correction selecting agent-executed NuGet Trusted Publishing.

## Correction

Both existing build.yml workflows already use the nuget environment, id-token: write and NuGet/login@v1 with NUGET_USER. The workflow obtains a temporary push credential; no local long-lived API key, extra nuget.exe or GitHub CLI is required. This supersedes the earlier prerequisite note without editing append-only history. NUGET_USER means the NuGet.org profile username.

The active release guide uses the verified current repository names Evigila/Essential.Culture (1327295083) and Evigila/Flourish (1246107902), workflow build.yml and environment nuget. It distinguishes exact package/new-ID/version permissions from environment existence, records current clean-master/tag checks and preserves October 5 configuration observations as historical. The umbrella automatically includes the Culture bridge; Framework-only installation can omit Design/Culture. Flourish's release order is Core, Abstract, bridge, Framework, Design and umbrella. The implicit sibling feed is removed in favor of explicit EssentialPackageDirectory; ArtifactsPath isolates build output and intermediates. Gallery's source bridge/package-consumption migration remains pending.

## Evidence and scope

The coordinating root inspected Essential v1.2.0 run 33030395528 (Trusted login/push succeeded), Essential master run 37291575812 (success without publication), Flourish run 37291596951 (older full-solution restore failure before publication), and both public nuget environment APIs (zero protection rules, null branch policy). Current secret presence/value and NuGet account policy are unverified. The successful old Essential scope does not establish permission for its new Blazor ID.

Updated active release/integration guidance and appended the current verification correction. Essential solution organization also now records the current remote and explicit local-feed boundary. Existing README/human docs, source, scripts and prior change records were not modified by this documentation subtask. No build, commit, tag, push or package publication was performed. New source commits still require the user's answer despite the existing publication authorization.
