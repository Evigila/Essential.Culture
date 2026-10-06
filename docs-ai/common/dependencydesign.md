# Portable external dependency policy

Adding or updating any external library, package, SDK, service, hosted API, font, asset library, build tool, or runtime dependency **requires explicit user authorization first**. An existing transitive dependency is not automatic permission to add a direct reference or upgrade it. Do not install a plugin merely because it could be useful.

Before asking, state the problem, why existing code and dependencies are insufficient, the exact proposed dependency and version or service, affected projects, license and security considerations, runtime/operational cost, and a no-new-dependency alternative. After approval, use the repository's existing package-management method and pinned versions. Update only the authorized dependency scope, verify restore/build/tests, and document operational configuration without committing secrets. If approval is not given, continue with an in-scope alternative or report the limitation.
