param([switch]$VerifyOnly)
. (Join-Path $PSScriptRoot 'Release-Common.ps1')
$version = Get-ReleaseVersion
if ($VerifyOnly) { & (Join-Path $PSScriptRoot 'Verify-PackageSet.ps1') -Version $version; return }
Push-Location $ReleaseRoot
try {
    $packages = [System.IO.Path]::GetFullPath((Join-Path $ReleaseRoot 'artifacts/packages'))
    $expectedDirectory = [System.IO.Path]::GetFullPath((Join-Path $ReleaseRoot 'artifacts/packages'))
    if ($packages -ne $expectedDirectory -or !$packages.StartsWith($ReleaseRoot + [System.IO.Path]::DirectorySeparatorChar)) { throw 'Package directory escaped the repository.' }
    New-Item -ItemType Directory -Path $packages -Force | Out-Null
    Get-ChildItem -LiteralPath $packages -File | Where-Object { $_.Extension -in '.nupkg', '.snupkg' } | ForEach-Object { Remove-Item -LiteralPath $_.FullName }
    Invoke-ReleaseCommand 'dotnet' @('restore', $ReleaseSettings.Solution)
    Invoke-ReleaseCommand 'dotnet' @('build', $ReleaseSettings.Solution, '-c', 'Release', '--no-restore', '--no-incremental', '-p:TreatWarningsAsErrors=true', '-p:ContinuousIntegrationBuild=true')
    Invoke-ReleaseCommand 'dotnet' @('test', $ReleaseSettings.Solution, '-c', 'Release', '--no-build', '--no-restore')
    foreach ($project in $ReleaseSettings.ConsoleTests) { Invoke-ReleaseCommand 'dotnet' @('run', '--project', $project, '-c', 'Release', '--no-build', '--no-restore') }
    if ($ReleaseSettings.JavaScriptTests.Count) { Invoke-ReleaseCommand 'node' (@('--test') + $ReleaseSettings.JavaScriptTests) }
    foreach ($check in $ReleaseSettings.CheckScripts) { Invoke-ReleaseCommand 'pwsh' @('-NoLogo', '-NoProfile', '-File', $check) }
    if ($ReleaseSettings.DemoSolution) {
        Invoke-ReleaseCommand 'dotnet' @('restore', $ReleaseSettings.DemoSolution, '-p:UseLocalCulture=true')
        Invoke-ReleaseCommand 'dotnet' @('build', $ReleaseSettings.DemoSolution, '-c', 'Release', '--no-restore', '--no-incremental', '-p:UseLocalCulture=true', '-p:TreatWarningsAsErrors=true', '-p:ContinuousIntegrationBuild=true')
    }
    foreach ($package in $ReleaseSettings.Packages) {
        Invoke-ReleaseCommand 'dotnet' @('pack', $package.Project, '-c', 'Release', '--no-build', '--no-restore', '-p:ContinuousIntegrationBuild=true')
    }
    & (Join-Path $PSScriptRoot 'Verify-PackageSet.ps1') -Version $version
    Write-Host "Release preparation completed for v$version. No Git or publishing changes were made."
} finally { Pop-Location }
