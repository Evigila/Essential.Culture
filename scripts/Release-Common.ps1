Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ReleaseRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$ReleaseSettings = Import-PowerShellDataFile (Join-Path $PSScriptRoot 'ReleaseSettings.psd1')
function Get-ReleaseVersion {
    [xml]$props = Get-Content -LiteralPath (Join-Path $ReleaseRoot $ReleaseSettings.VersionProps)
    $value = [string]($props.Project.PropertyGroup | ForEach-Object { $_.VersionPrefix } | Where-Object { $_ } | Select-Object -First 1)
    if ($value -notmatch '^\d+\.\d+\.\d+$') { throw "VersionPrefix '$value' must be a stable three-part version." }
    if ($value -ne $ReleaseSettings.EssentialVersion) { throw "VersionPrefix '$value' must match release settings version '$($ReleaseSettings.EssentialVersion)'." }
    return $value
}
function Invoke-ReleaseCommand([string]$File, [string[]]$Arguments) {
    & $File @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$File failed with exit code $LASTEXITCODE." }
}
function Assert-ReleaseTag([string]$Tag, [string]$Commit = 'HEAD') {
    $version = Get-ReleaseVersion
    if ($Tag -ne "v$version") { throw "Tag '$Tag' must match VersionPrefix as 'v$version'." }
    & git -C $ReleaseRoot merge-base --is-ancestor $Commit origin/master
    if ($LASTEXITCODE -ne 0) { throw "Release commit '$Commit' must be contained in origin/master." }
}
function Assert-ReleaseRepository {
    $origin = & git -C $ReleaseRoot remote get-url origin
    if ($LASTEXITCODE -ne 0) { throw 'Release requires an origin remote.' }
    $match = [regex]::Match($origin.Trim(), '^(?:https?://github\.com/|git@github\.com:)(?<owner>[^/]+)/(?<repository>[^/]+?)(?:\.git)?/?$')
    $policy = $ReleaseSettings.TrustedPublishing
    if (!$match.Success -or $match.Groups['owner'].Value -ine $policy.RepositoryOwner -or $match.Groups['repository'].Value -ine $policy.RepositoryName) {
        throw "Release origin must identify $($policy.RepositoryOwner)/$($policy.RepositoryName), matching the Trusted Publishing policy."
    }
    $canonical = "https://github.com/$($policy.RepositoryOwner)/$($policy.RepositoryName)"
    if ($ReleaseSettings.RepositoryUrl -cne $canonical) { throw "Release RepositoryUrl must be '$canonical'." }
    [xml]$rootProps = Get-Content -LiteralPath (Join-Path $ReleaseRoot 'Directory.Build.props')
    [xml]$moduleProps = Get-Content -LiteralPath (Join-Path $ReleaseRoot $ReleaseSettings.VersionProps)
    if ($rootProps.Project.PropertyGroup.RepositoryUrl -cne $canonical -or $moduleProps.Project.PropertyGroup.PackageProjectUrl -cne $canonical) {
        throw "Package repository and project URLs must identify '$canonical'."
    }
    $workflowPath = Join-Path $ReleaseRoot ".github/workflows/$($policy.WorkflowFile)"
    if (!(Test-Path -LiteralPath $workflowPath)) { throw 'The Trusted Publishing workflow file does not exist.' }
    $workflowText = Get-Content -LiteralPath $workflowPath -Raw
    $publishJob = [regex]::Match($workflowText, '(?ms)^  publish:\r?\n(?<job>.*?)(?=^  [A-Za-z0-9_-]+:\r?\n|\z)')
    $environment = [regex]::Match($publishJob.Groups['job'].Value, '(?m)^    environment:\s*(?<name>[^\r\n]+)')
    if (!$publishJob.Success -or !$environment.Success -or $environment.Groups['name'].Value.Trim().Trim('"', "'") -ine $policy.Environment) {
        throw "The publish job in '$($policy.WorkflowFile)' must use GitHub environment '$($policy.Environment)'."
    }
}
function Assert-TrustedPublishingContext {
    param(
        [string]$Repository = $env:GITHUB_REPOSITORY,
        [string]$WorkflowRef = $env:GITHUB_WORKFLOW_REF,
        [string]$Environment = $env:TRUSTED_PUBLISHING_ENVIRONMENT,
        [string]$NuGetUser = $env:NUGET_USER,
        [switch]$RequireOidc
    )
    Assert-ReleaseRepository
    $policy = $ReleaseSettings.TrustedPublishing
    $expectedRepository = "$($policy.RepositoryOwner)/$($policy.RepositoryName)"
    if ($Repository -ine $expectedRepository) { throw "Trusted Publishing requires GitHub repository '$expectedRepository'." }
    $expectedWorkflow = "$expectedRepository/.github/workflows/$($policy.WorkflowFile)@"
    if ([string]::IsNullOrWhiteSpace($WorkflowRef) -or !$WorkflowRef.StartsWith($expectedWorkflow, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Trusted Publishing requires workflow filename '$($policy.WorkflowFile)' in '$expectedRepository'."
    }
    if ($Environment -ine $policy.Environment) { throw "Trusted Publishing requires GitHub environment '$($policy.Environment)'." }
    if ([string]::IsNullOrWhiteSpace($NuGetUser) -or $NuGetUser.Contains('@') -or $NuGetUser -match '\s') {
        throw "Set NUGET_USER to the NuGet.org profile username in GitHub environment '$($policy.Environment)' (Actions variable or secret), then match that account's publishing policy."
    }
    if ($env:GITHUB_EVENT_NAME -ne 'push' -or $env:GITHUB_REF -ne ('refs/tags/v' + (Get-ReleaseVersion))) {
        throw 'Trusted Publishing is allowed only for a push of the tag matching the package version.'
    }
    if ($RequireOidc -and ([string]::IsNullOrWhiteSpace($env:ACTIONS_ID_TOKEN_REQUEST_URL) -or [string]::IsNullOrWhiteSpace($env:ACTIONS_ID_TOKEN_REQUEST_TOKEN))) {
        throw 'The tag publish job must grant id-token: write for NuGet Trusted Publishing.'
    }
    Write-Host "Verified publishing context: $expectedRepository, $($policy.WorkflowFile), environment $($policy.Environment). NuGet/login will verify the remote trust policy."
}
