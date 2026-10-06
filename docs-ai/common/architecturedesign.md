# Portable architecture design standard

Use this standard to describe and review a project's solution, not to impose a universal folder structure. The project-specific architecture belongs in `docs-ai/current/currentproject-architecture.md` and must be checked against the actual code and current project specifications.

## Boundaries and responsibilities

- Identify user-facing surfaces, application services, domain rules, persistence, external systems, and deployment orchestration. Give each component a single accountable responsibility and a documented owner for its data.
- Keep business policy independent from UI frameworks, database providers, and hosting details. Have outer adapters depend on stable inner contracts; do not make the domain depend on an HTTP endpoint, screen, or database implementation.
- Document the actual solution/project reference graph. If a dependency points against the intended direction, record it as an explicit exception or a refactoring need, not as an assumed clean boundary.
- Separate authentication, authorization, routing, and data ownership. A route or domain name identifies a destination; it is not proof of permission. Scope credentials and tokens to the surface and resource they authorize.
- Define failure behavior for partial availability, provisioning, retries, session expiry, and cross-system operations. Prefer idempotent operations and fail-closed access checks where security is involved.

## Change control

Before changing a boundary, list affected producers, consumers, data contracts, and tests. Distinguish implemented behavior from target architecture and temporary mitigations. Explain compatibility and migration consequences. Record project-specific choices and approved deviations in `current/`; ask before promoting a local exception into this portable standard.
