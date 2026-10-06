# Project documentation and AI collaboration rules

This file belongs to the project, not to a particular AI agent, harness, device, or editor. Keep it in version control with its related `docs-ai/` content. Copying this file and that content to another project provides a portable starting point, but project-specific entries in `docs-ai/current/` must be reviewed and adapted there. This file does **not** claim the highest instruction priority in any AI system: a harness may merge its own `AGENTS.md`, `override.md`, system/developer rules, or other instructions. Follow the effective instruction hierarchy and report conflicts rather than silently overriding higher-priority instructions.

## Required documentation layout and ownership

The repository must contain all of the following paths. In this documentation system, machine-readable configuration is stored as `.json` and prose is stored as `.md`; do not create extensionless prose files. Toolchain-required formats outside the documentation system (for example project files and `.editorconfig`) keep their native formats.

```text
AGENTS.md
AGENTS.ensure.json
docs/
  .gitkeep
docs-ai/
  common/

    architecturedesign.md
    codedesign.md
    dependencydesign.md
    rules.md
    SKILLS/
  current/
    currentproject-architecture.md
    currentproject-changelogs/
      YYYY-MM-DD_HHmmss_<slug>.md
    bugfix-reports/
```

`docs/` and `docs-ai/` are distinct parts of one documentation system. `docs/` contains the existing human-maintained DocFX documentation and is reserved for third-party-facing material. Preserve its existing content; `.gitkeep` is only a version-control directory marker. The previous empty-directory rule was superseded for Flourish on 2026-10-03 by explicit user authorization. Do not create an AI-authored README or other placeholder prose there. AI may read, search, cite, and link future human content, but must not modify, move, or delete anything in `docs/` by default. If it appears wrong or stale, report the discrepancy to the user. An explicit, task-scoped user exception may authorize particular edits or a verified migration of AI-authored material; do not treat that exception as a permanent policy change. `docs-ai/` is the AI-writable area for durable working memory, standards, decisions, change records, and bug reports. Never move genuinely human-authored material into `docs-ai/` or silently replace it with an AI version. Link to existing material and record any unresolved discrepancy.

`docs-ai/common/` holds portable architecture, code, dependency and process standards intended to be copied between projects. Its four named Markdown files and `SKILLS/` directory are mandatory. `architecturedesign.md` covers solution/project responsibilities and dependency direction; `codedesign.md` covers naming, comments, data structures and patterns; `dependencydesign.md` covers external dependencies and approval; `rules.md` covers reusable process rules. `SKILLS/` stores project-owned skill packages, not installed harness skills. UI standards are not duplicated here: Flourish source and component implementations are the authority.

`docs-ai/current/` holds facts and decisions specific to this repository. Its `currentproject-architecture.md`, `currentproject-changelogs/`, and `bugfix-reports/` paths are mandatory. Keep active project-specific AI documents directly under `current/` alongside them. The user intentionally deleted the former `archive/` directory on 2026-09-21 and instructed that it must not be recreated unless explicitly requested later. Superseded decisions remain traceable through append-only change records and explicit supersession notes in active documents. When uncertain whether guidance is portable, place it in `current/`. If later ambiguity suggests moving it to `common/`, ask the user first. Base `currentproject-architecture.md` on `common/architecturedesign.md` and the actual project architecture, including its exceptions. Change records must be named `YYYY-MM-DD_HHmmss_<slug>.md`, using the project's local time and a descriptive lowercase slug. When splitting an older ledger whose entries have dates but no recorded times, `000000` is an explicit **unknown-time sentinel**, never a claim that an event occurred at midnight; retain the original entry ID in the slug and document the import in the current index. The `currentproject-changelogs/` directory is append-only: never edit or delete an existing record; append a new record for a correction. Keep historical bug reports under `bugfix-reports/`, including symptoms, cause, evidence, mitigation, limitations, and regression checks. Preserve history rather than silently erasing a prior diagnosis.

If `current/` guidance conflicts with `common/`, do not silently choose one or edit either to conceal the conflict. Ask whether the difference is intentional and obtain the user's confirmation and authorization. If approved, document the special mechanism explicitly in `current/` and follow that authorized exception. Until then, report the conflict and avoid the disputed change.

## Initial and monthly documentation check

At the start of every new project task, read `AGENTS.md` and `AGENTS.ensure.json`, then read the relevant project documents. `AGENTS.ensure.json` records `schemaVersion`, the last time missing material was asked about, and the last time the required structure was checked and repaired. A missing, invalid, or unsupported ensure file triggers an immediate full check. If one calendar month or more has elapsed since `lastCompletedAtUtc`, repeat the full check. Use a calendar-month comparison in UTC, not merely a fixed 30-day approximation. An obvious missing required path found before the next scheduled check also triggers a check.

The full check must explicitly verify **every** required path in the tree above, including both top-level directories, the blank `docs/.gitkeep` marker while the user wants `docs/` empty, both `docs-ai/` categories, all four common files, `SKILLS/`, the project architecture file, and both current history directories. The intentionally removed `docs-ai/current/archive/` must not be reported as missing or recreated. Also review whether the mandatory Markdown files contain their stated subject matter rather than being empty placeholders and follow the `.json`/`.md` documentation format rule; `.gitkeep` is only a version-control directory marker. If another required folder or file is missing, first ask the user once, in a consolidated question, whether they have related files or content to provide. If provided, incorporate it within its proper ownership boundary. If the user confirms none exists, create the missing directories and content in English from verified project sources, except that existing human-maintained `docs/` content remains unchanged unless the user authorizes a task-scoped edit. Do not repeatedly ask the same bootstrap question during one check. Update `AGENTS.ensure.json` only after the check, user confirmation where needed, and repairs are complete; record the latest ask time (if any), completion time, and schema version. Never use the ensure file as permission to skip a due audit or to modify `docs/`.

## Collaboration and implementation

- 禁止自行在docs-ai/目录外创建文档类文件，例如README.md等，除非获得授权。
- Do not create documentation files outside `docs-ai/` without explicit user authorization. This includes README files. The root `AGENTS.md` is a required exception; maintain it when the user requests rule changes. The project uses `docs-ai/`, not `doc-ai/`.

- Use a bounded subagent for a separable task when available, while preserving non-overlapping write scopes; inspect and integrate its output. Do not create another user-owned task merely for delegation.
- At task completion, explain the changes in detail or explain precisely why completion failed. Include relevant verification and a manual test checklist for functional changes.
- Do not use Computer Use to test this project. Use code inspection, builds, automated tests, and a manual checklist for the user.
- When functionality is complete, ask whether the user wants a Git commit. Match the style of recent commit subjects; do not commit without the user's answer.
- Keep class and type names semantic; do not prepend brand or project names merely for branding. Follow the solution/project and namespace naming rules below. Preserve established identifiers unless the user authorizes their migration; recording a rule is not authorization to rename existing code or packages.
- Do not turn a UI request into an unrelated business-rule change. When a UI task exposes an underlying behavioral defect, explain the boundary and obtain direction before a material expansion. Preserve existing access, domain, and session behavior unless the user requests or approves a change.
- Do not add or update an external package, service, SDK, font, icon library, or other dependency without the user's explicit authorization. Follow `docs-ai/common/dependencydesign.md`.
- Prefer existing project conventions, `.editorconfig`, central package management, and focused tests. Do not invent a rule that contradicts an existing human-maintained product specification; report it.

## Solution, project and namespace naming

The user approved these shared naming rules for Colligere, Flourish and Essential on 2026-10-05. They govern project-owned solution/project names and namespaces, not decorative prefixes on individual class/type names.

1. Ordinary solution and project names start directly with the product/project name, without an `ArkheideSystem.` or `Arkheide.` organization prefix: for example `Flourish.Blazor.Framework`, `Colligere.Web` and `Essential.Culture.Blazor`. Solution/project display names and file basenames follow the same convention.
2. Project-owned namespaces always start with `ArkheideSystem.` or `Arkheide.`; prefer the full `ArkheideSystem.` form, for example `ArkheideSystem.Flourish.Blazor`. An organization-free project name does not imply an organization-free namespace.
3. Test projects always use `Tests.<ProjectName>`, for example `Tests.Flourish.Blazor.Framework`, `Tests.Colligere.Web` and `Tests.Essential.Culture.Blazor`.
4. Gallery projects always use `Gallery.<ProjectName>`, for example `Gallery.Flourish.Blazor`. `Tests.` and `Gallery.` are explicit role-prefix exceptions to the ordinary product-first project-name rule; the product name immediately follows the role prefix.
5. Bridge/extension-layer projects always use `<ProjectName>.Extensions.<Integration>`, for example `Flourish.Extensions.Culture.Blazor`. Their test and Gallery projects combine the same role-prefix rules, such as `Tests.Flourish.Extensions.Culture.Blazor`.

Keep these identities distinct from explicit `RootNamespace`, `AssemblyName` and NuGet `PackageId` settings. This rule registration does not rename published or prepared NuGet IDs; existing `Arkheide.*` package IDs remain unchanged unless a package-specific migration is authorized. New or renamed solutions/projects must follow these rules. Existing nonconforming project names or project-owned namespaces are migration debt, not implicit exceptions; report compatibility implications before changing them. Third-party source retains its ownership and is not renamed as a side effect.

## UI authority

Use Flourish repository source and component implementations as the sole UI/UX authority. Use exclusively Flourish controls and styles; do not create host skins, cloned controls or a parallel visual specification. Consumers own business/backend behavior and approved brand/color-role configuration only. If a required control is missing, skip that work, record the gap and report it before custom implementation. Semantic HTML and native GET/POST may remain only at a host protocol boundary, not as replacement controls. Retired UI documents are historical evidence, not current standards.

## Project-specific reading order

For UI work, inspect actual Flourish component/style implementations and their public contracts; do not recreate a Markdown visual standard. For architecture work, read `docs-ai/current/currentproject-architecture.md`, then inspect affected project files. This directory map must explain maintained root directories/files; it may omit `docs/`, `docs-ai/`, version-control internals and generated caches when stated. Runtime/localization/package guides remain technical references, not a competing UI authority. Record new AI-authored history in `docs-ai/current/`, not human-only `docs/`, unless explicitly authorized.
