# Read Trusted Publishing environment username

Recorded at 2026-10-06T18:59:55-03:00.

The user clarified the existing nuget environment and completed NUGET_USER configuration. Both workflow login inputs now read vars.NUGET_USER || secrets.NUGET_USER, supporting Actions Variables and the already-established Essential Secret without hard-coding a profile. This corrects the preceding local direct-username decision, whose dated record remains unchanged. Temporary API keys still come only from NuGet/login@v1 via OIDC. The two newly created, unpushed agent commits are updated within the user's existing two-repository commit authorization; no third-party commit or published tag is replaced. Public release success must be established from workflow and NuGet indexing evidence.
