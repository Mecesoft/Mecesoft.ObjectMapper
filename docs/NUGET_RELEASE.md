# NuGet Release Runbook

This document prepares—but does not execute—the `1.0.0` release of:

- `Mecesoft.ObjectMapper`
- `Mecesoft.ObjectMapper.DependencyInjection`

The release workflow is `.github/workflows/nuget-publish.yml`. It is manual-only and uses
NuGet.org Trusted Publishing through GitHub Actions OIDC. No NuGet API key or long-lived
publishing secret is stored in the repository or in the workflow.

## 1. Prerequisites

- The release commit is on `main` and is the commit selected for the workflow run.
- CI (`ci.yml`) is green for that exact commit.
- The GitHub repository is `Mecesoft/Mecesoft.ObjectMapper`.
- The NuGet packages are owned by the `Mecesoft` account/organization.
- The NuGet.org Trusted Publishing policy and GitHub `nuget` environment are configured as
  described below.
- Both package versions are `1.0.0`.

## 2. NuGet.org Trusted Publisher setup

This is a one-time owner action on NuGet.org.

1. Sign in to https://www.nuget.org/.
2. Open the account menu and choose **Trusted Publishing**.
3. Create a policy with:
   - **Repository Owner:** `Mecesoft`
   - **Repository:** `Mecesoft.ObjectMapper`
   - **Workflow File:** `nuget-publish.yml`
   - **Environment:** `nuget`
4. Select the package owner/scope that covers both package IDs:
   - `Mecesoft.ObjectMapper`
   - `Mecesoft.ObjectMapper.DependencyInjection`
5. Save the policy.

The workflow filename is the file name only, not the
`.github/workflows/nuget-publish.yml` path.

NuGet temporary API keys are valid for **one hour**. The workflow requests the key only
after the build, duplicate check and environment approval, immediately before pushing.

For a private GitHub repository, a new policy can begin with a temporary activation window
(typically seven days) until a successful publish supplies the repository/owner IDs. Follow
the current NuGet.org Trusted Publishing UI guidance.

## 3. GitHub `nuget` environment setup

Create the environment in the GitHub repository:

1. **Settings → Environments → New environment**
2. Name: `nuget`
3. Configure protection:
   - Required reviewers: at least one trusted maintainer.
   - Prevent self-review if multiple maintainers are available.
   - Deployment branches: **`main` only**.
   - Optional: add a wait timer before deployment.
4. Add an environment secret:
   - Name: `NUGET_USER`
   - Value: the NuGet.org **username/profile name**, not the email address.
5. Do **not** add `NUGET_API_KEY` or any NuGet API-key secret.

The environment approval is the human gate between build verification and publication. A
workflow run cannot publish before the publish job is approved.

## 4. Workflow design

`nuget-publish.yml` has two jobs:

### Build job

- Triggered only by `workflow_dispatch`.
- Requires the confirmation text `publish-1.0.0`.
- Requires the selected ref to be `refs/heads/main`.
- Checks out `github.sha` explicitly with full history.
- Restores using the repository `NuGet.config` (nuget.org only).
- Builds Release and tests `net8.0` and `net10.0` separately.
- Packs core and DI packages.
- Verifies package IDs, version, metadata, README, LICENSE, XML docs, PDB symbol packages,
  core dependency isolation and DI dependency versions.
- Generates `SHA256SUMS` for all four package files.
- Uploads packages and test results as workflow artifacts.

### Publish job

- Runs only after the build job succeeds.
- Uses the protected GitHub environment `nuget`.
- Has only `contents: read` and `id-token: write`.
- Downloads the verified package artifact.
- Verifies checksums and package metadata again.
- Fails if either `1.0.0` package already exists on NuGet.org.
- Exchanges the GitHub OIDC token for a short-lived NuGet API key via `NuGet/login`.
- Publishes **core first**, then the DI package.
- Does not use `--skip-duplicate`.
- Does not create a tag, GitHub release or release note.

The primary `.nupkg` and its sibling `.snupkg` are in the same artifact directory. The NuGet
V3 push flow publishes the symbol package with the primary package; the workflow does not
silently skip symbols or duplicates.

## 5. Pre-release checklist

- [ ] Release commit is reviewed and on `main`.
- [ ] CI is green for that exact commit.
- [ ] Working tree is clean.
- [ ] `Mecesoft.ObjectMapper.csproj` and DI csproj versions are `1.0.0`.
- [ ] Release notes are reviewed.
- [ ] Both packages are absent from NuGet.org for `1.0.0`.
- [ ] Trusted Publishing policy filename/environment match the workflow.
- [ ] `NUGET_USER` is set as an environment secret and is the profile name.
- [ ] `nuget` environment has required reviewers and `main`-only deployment.
- [ ] Branch protection requires the CI check.
- [ ] SourceLink metadata is present in the packages.
- [ ] `dotnet list ... package --vulnerable --include-transitive` reports no known issues.
- [ ] SHA256 checksums from the build artifact are recorded.
- [ ] No API key, token or connection string is present in the repository.

## 6. Release procedure

1. Open **Actions → NuGet Publish**.
2. Choose **Run workflow**.
3. Select branch **`main`**.
4. Enter confirmation exactly `publish-1.0.0`.
5. Start the run.
6. Wait for the build/test/pack job to succeed.
7. Review the source commit and package checksums in the build summary.
8. Approve the `nuget` environment deployment as a required reviewer.
9. Monitor the publish job:
   - duplicate-version check
   - OIDC login
   - core package push
   - DI package push
10. Do not trigger a second release while this run is active or awaiting approval.

The workflow is intentionally **not** triggered by push, pull request or tag events.

## 7. Core → DI publication order

The fixed order is:

1. Verify both packages and checksums.
2. Verify core has no package dependencies.
3. Verify DI declares:
   - `Mecesoft.ObjectMapper` `1.0.0`
   - `Microsoft.Extensions.DependencyInjection.Abstractions` `8.0.2` for `net8.0`
   - `Microsoft.Extensions.DependencyInjection.Abstractions` `10.0.13` for `net10.0`
4. Confirm neither version exists on NuGet.org.
5. Publish `Mecesoft.ObjectMapper` `1.0.0`.
6. Publish `Mecesoft.ObjectMapper.DependencyInjection` `1.0.0`.

If core publication fails, the DI step is skipped. If DI publication fails after core has
been published, the release is partial; follow the recovery section below.

## 8. Failure and partial-publication handling

NuGet package versions are immutable. Never delete, overwrite, or republish an already
published `1.0.0` package.

- **Build/test/pack failure:** no package was published. Fix the issue and rerun the manual
  workflow.
- **Duplicate check failure:** no package was published by this run. Do not bypass it with
  `--skip-duplicate`; investigate the existing version and publish a new version if needed.
- **OIDC login failure:** no package was published. Verify the Trusted Publishing policy,
  workflow filename, environment and `NUGET_USER`.
- **Core push failure:** no DI package was published. Investigate and rerun only after the
  duplicate state is understood.
- **Core success, DI failure:** core `1.0.0` is already live; the workflow stopped before DI.
  Do not rerun the full workflow blindly because the duplicate guard will reject core.
  - Verify the core package and symbols on NuGet.org.
  - Publish only the already-verified DI `.nupkg` from the same workflow artifact, using a
    separately reviewed recovery procedure.
  - Do not publish a different build or a different commit under the same version.
- **Symbol-only failure:** the primary package may already be live. Verify the package and
  retry only symbol publication using the same verified `.snupkg`; do not re-push the
  primary package.

There is no automatic rollback in the workflow. A failed publish must leave the repository
and NuGet state observable and recoverable.

## 9. Post-publication verification

- [ ] Both packages are visible on NuGet.org with version `1.0.0`.
- [ ] Package metadata shows the correct repository URL and commit.
- [ ] Core lists no package dependencies.
- [ ] DI lists the core `1.0.0` dependency and the correct DI abstraction version per TFM.
- [ ] README and LICENSE are visible in the package details.
- [ ] XML documentation and symbol packages are available.
- [ ] SourceLink links resolve to the released commit.
- [ ] Install both packages into a clean consumer project on `net8.0` and `net10.0`.
- [ ] Verify the DI consumer resolves `IMapper` and maps both forward and reverse.
- [ ] Record the published package SHA256 checksums from the workflow artifact.
- [ ] Create the GitHub tag/release **separately** only after NuGet verification; the publish
  workflow does not create tags or releases.

## 10. Local verification commands

```zsh
cd /Users/macmecesoft/Projects/MecesoftNuget/Mecesoft.ObjectMapper

dotnet restore Mecesoft.ObjectMapper.sln --configfile NuGet.config
dotnet build Mecesoft.ObjectMapper.sln --configuration Release --no-restore
dotnet test Mecesoft.ObjectMapper.sln --configuration Release --no-build --framework net8.0
dotnet test Mecesoft.ObjectMapper.sln --configuration Release --no-build --framework net10.0

dotnet pack Mecesoft.ObjectMapper/Mecesoft.ObjectMapper.csproj \
  --configuration Release --no-build --output artifacts
dotnet pack Mecesoft.ObjectMapper.DependencyInjection/Mecesoft.ObjectMapper.DependencyInjection.csproj \
  --configuration Release --no-build --output artifacts

shasum -a 256 artifacts/*.nupkg artifacts/*.snupkg

dotnet list Mecesoft.ObjectMapper.sln package --vulnerable --include-transitive --configfile NuGet.config
```

Do not run the publish workflow or create a tag/release until the checklist and approvals are
complete.
