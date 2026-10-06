# Portable AI work rules

This file supplements the mandatory root `AGENTS.md`; it does not override a harness's instruction hierarchy.

- Start by reading the root rules, monthly ensure record, and only the documents relevant to the task. Respect the human `docs/` versus AI `docs-ai/` ownership boundary. A one-task exception never becomes a lasting permission.
- When facts are uncertain, inspect code, tests, and authoritative project documents. Separate observed behavior, inference, intended behavior, and unverified assumptions in reports.
- Use bounded delegation for separable work when available; keep write scopes disjoint and review results before reporting them as complete.
- Keep changes proportional to the request. Ask before changing business behavior, security boundaries, external dependencies, or the placement of a project-specific rule into a cross-project standard.
- Preserve an append-only AI change history. Correct an old entry with a new timestamped entry rather than editing or deleting it. Historical bug reports must retain what was known at the time and identify later corrections.
- Verify changes with focused automated checks and provide a manual checklist for user-visible behavior. If Computer Use testing is prohibited by the project, do not use it.
- Finish with a detailed account of what changed or why it could not be completed, including limitations and outstanding decisions. Ask whether to commit only after functionality is complete; follow the repository's recent commit style and do not commit without consent.
