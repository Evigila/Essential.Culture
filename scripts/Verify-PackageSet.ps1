param([string]$PackageDirectory, [string]$Version)
. (Join-Path $PSScriptRoot 'Release-Common.ps1')
if (!$Version) { $Version = Get-ReleaseVersion }
if (!$PackageDirectory) { $PackageDirectory = Join-Path $ReleaseRoot 'artifacts/packages' }
$expected = @($ReleaseSettings.Packages | ForEach-Object { "$($_.Id).$Version.nupkg" } | Sort-Object)
$actual = @(Get-ChildItem -LiteralPath $PackageDirectory -Filter '*.nupkg' -File | Select-Object -ExpandProperty Name | Sort-Object)
if (Compare-Object $expected $actual) { throw "Package set must be exactly: $($expected -join ', '). Found: $($actual -join ', ')." }
Add-Type -AssemblyName System.IO.Compression.FileSystem
foreach ($package in $ReleaseSettings.Packages) {
    $archive = [System.IO.Compression.ZipFile]::OpenRead((Join-Path $PackageDirectory "$($package.Id).$Version.nupkg"))
    try {
        $nuspec = @($archive.Entries | Where-Object { $_.FullName.EndsWith('.nuspec') })
        if ($nuspec.Count -ne 1) { throw "$($package.Id) must contain one nuspec." }
        $reader = [System.IO.StreamReader]::new($nuspec[0].Open())
        try { [xml]$xml = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $metadata = $xml.SelectSingleNode('/*[local-name()="package"]/*[local-name()="metadata"]')
        if ($metadata.id -ne $package.Id -or $metadata.version -ne $Version) { throw "Package metadata mismatch for $($package.Id)." }
        $dependencies = @($metadata.SelectNodes('.//*[local-name()="dependency"]'))
        $internal = @($dependencies | Where-Object { $_.id -like "$($ReleaseSettings.PackagePrefix)*" })
        foreach ($dependency in $internal) {
            $lowerBound = ($dependency.version.Trim('[]() ') -split ',')[0].Trim()
            if ($lowerBound -ne $Version) { throw "$($package.Id) depends on $($dependency.id) at '$($dependency.version)', expected $Version." }
            if ($dependency.id -notin $ReleaseSettings.Packages.Id) { throw "Unexpected internal dependency $($dependency.id)." }
        }
        foreach ($id in $package.Dependencies) {
            if ($id -notin @($dependencies | ForEach-Object { $_.id })) { throw "$($package.Id) is missing dependency $id." }
        }
        if ($ReleaseSettings.PackagePrefix -eq 'Arkheide.Flourish.' -and $package.Id -notlike 'Arkheide.Flourish.Extensions.*' -and @($dependencies | Where-Object { $_.id -like 'Arkheide.Essential.Culture*' }).Count) {
            throw "$($package.Id) must remain independent of optional Culture integration."
        }
        foreach ($dependency in $dependencies | Where-Object { $_.id -like 'Arkheide.Essential.Culture*' -and $ReleaseSettings.PackagePrefix -eq 'Arkheide.Flourish.' }) {
            $lowerBound = ($dependency.version.Trim('[]() ') -split ',')[0].Trim()
            if ($lowerBound -ne $ReleaseSettings.EssentialVersion) { throw "$($package.Id) must depend on Essential $($ReleaseSettings.EssentialVersion), found $($dependency.version)." }
        }
        foreach ($path in $package.Assets) {
            if (!$archive.GetEntry($path)) { throw "$($package.Id) is missing packaged asset '$path'." }
        }
        if ($package.Managed) {
            $assemblies = @($archive.Entries | Where-Object { $_.FullName -like 'lib/*.dll' })
            if (!$assemblies.Count) { throw "$($package.Id) contains no managed library." }
        }
        Write-Host "Verified $($package.Id) $Version"
    } finally { $archive.Dispose() }
}
Write-Host "Verified $($expected.Count) packages, dependencies, and required assets."
