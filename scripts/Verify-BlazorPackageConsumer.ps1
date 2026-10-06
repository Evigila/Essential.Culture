param([string]$PackageDirectory, [string]$Version)
. (Join-Path $PSScriptRoot 'Release-Common.ps1')
if (!$Version) { $Version = Get-ReleaseVersion }
if (!$PackageDirectory) { $PackageDirectory = Join-Path $ReleaseRoot 'artifacts/packages' }
$PackageDirectory = [System.IO.Path]::GetFullPath($PackageDirectory)
$consumerRoot = Join-Path $ReleaseRoot ('artifacts/package-consumers/blazor-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $consumerRoot -Force | Out-Null
foreach ($file in 'Tests.Essential.Culture.Blazor.PackageConsumer.csproj', 'Program.cs', 'Culture.json') {
    Copy-Item -LiteralPath (Join-Path $ReleaseRoot "tests/package-consumers/Tests.Essential.Culture.Blazor.PackageConsumer/$file") -Destination $consumerRoot
}
$project = Join-Path $consumerRoot 'Tests.Essential.Culture.Blazor.PackageConsumer.csproj'
$packageCache = Join-Path $consumerRoot 'packages'
Invoke-ReleaseCommand dotnet @('restore', $project, '--source', $PackageDirectory, '--packages', $packageCache,
    "-p:CulturePackageVersion=$Version")
$assets = Get-Content -LiteralPath (Join-Path $consumerRoot 'obj/project.assets.json') -Raw | ConvertFrom-Json
foreach ($id in 'Arkheide.Essential.Culture.Blazor', 'Arkheide.Essential.Culture', 'Arkheide.Essential.Culture.Generator') {
    if ($assets.libraries.PSObject.Properties.Name -notcontains "$id/$Version") {
        throw "Blazor-only consumer is missing transitive package $id/$Version."
    }
}
Invoke-ReleaseCommand dotnet @('build', $project, '-c', 'Release', '--no-restore', '-p:TreatWarningsAsErrors=true',
    "-p:CulturePackageVersion=$Version")
Invoke-ReleaseCommand dotnet @('run', '--project', $project, '-c', 'Release', '--no-build', '--no-restore',
    "-p:CulturePackageVersion=$Version")
Write-Host "Verified isolated Blazor-only consumer at $consumerRoot"
