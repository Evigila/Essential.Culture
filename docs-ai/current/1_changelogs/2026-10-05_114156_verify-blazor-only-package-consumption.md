# Verify Blazor-only package consumption

Recorded on 2026-10-05 at 11:41:56 America/Sao_Paulo.

## Scope and reason

The user authorized local preparation of the six Culture 1.3.0 packages without committing, tagging, pushing or publishing. Existing demo projects explicitly reference Generator, so their successful builds alone did not prove that installing only the Blazor package brings Core and Generator automatically.

## Changes

- Added tests/package-consumers/BlazorOnly with a single direct PackageReference to Arkheide.Essential.Culture.Blazor and a small localization catalog.
- Added scripts/Verify-BlazorPackageConsumer.ps1, which copies the fixture into a new artifacts directory, restores exclusively from the prepared package directory into a separate cache, checks the three-package restore graph, then builds and runs the consumer.
- Included this verification after package inspection in full Test-Release.ps1 preparation. VerifyOnly retains its existing package-inspection behavior.
- Updated the active release verification and integration guides to distinguish the new local run from previous external observations.

## Verification

The final full publish-helper.bat -Mode Prepare completed with exit code 0: all 89 tests passed, all five demos compiled, six exact 1.3.0 packages passed inspection, and the isolated single-package consumer built with zero warnings/errors and ran successfully. The consumer verifies generated Key compilation, transitive Core access, two-scope language isolation, independent formatting culture and encoded LocalizedText output.

Prepared packages are at C:/Users/RC_Auditoria/source/Repos/Essential/artifacts/packages. The final isolated consumer is retained under artifacts/package-consumers/blazor-d9ebe1b4f154492b9bb39e99b55b0069. No runtime/platform API, package version or external dependency was changed.

## Limitations and follow-up

This establishes local package preparation and consumption, not public NuGet visibility or account publishing-policy completeness. No commit, tag, push or publication was performed. User-operated UI regression checks remain in release-verification.md. Review and commit the verification/documentation changes before the existing Publish clean-tree guard can succeed.
