# Portable code design standard

The repository's language tooling and `.editorconfig` govern formatting and language-specific syntax. These principles cover design decisions that formatting tools cannot decide.

## Naming and structure

- Name classes, methods, values, and modules by their domain meaning or responsibility. Do not add a brand or project prefix to class names for decoration. Keep names specific enough to reveal units, state, and scope.
- Keep public contracts small and explicit. Place behavior near the data and invariant it owns. Avoid ambiguous helpers that silently cross authentication, tenancy, or persistence boundaries.
- Use comments to explain *why* a non-obvious constraint exists, not to repeat what the code says. Keep security assumptions, fallback rules, and lifecycle constraints documented beside their implementation and in tests.

## Data structures and patterns

- Prefer the simplest structure matching the invariant: records or immutable values for snapshots, sets for membership, maps for keyed lookup, and sequences for ordered traversal. Do not introduce a cache, inheritance hierarchy, generic framework, or design pattern without a demonstrated need.
- Make absence, failure, and transitional states explicit. Avoid magic strings or silent catch-and-continue paths where a failure changes security or user-visible behavior.
- Use dependency inversion for external I/O and isolate side effects at boundaries. Keep retries, transactions, and idempotency semantics visible. Never equate an HTTP status from the wrong authentication scope with proof that the current session was revoked.

## Verification

Add focused regression tests for defects and boundary changes. Test the real authorization scheme or a faithful fake when the bug depends on authentication; a fake that always returns success can hide a cross-scope call. Preserve existing user data and unrelated working-tree changes.
