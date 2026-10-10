param([string]$PackageDirectory, [string]$Version)
. (Join-Path $PSScriptRoot 'Release-Common.ps1')
Assert-ReleaseRepository
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
        $repository = $metadata.SelectSingleNode('*[local-name()="repository"]')
        $projectUrl = $metadata.SelectSingleNode('*[local-name()="projectUrl"]')
        if ($null -eq $repository -or $repository.GetAttribute('type') -ne 'git' -or $repository.GetAttribute('url') -cne $ReleaseSettings.RepositoryUrl -or $null -eq $projectUrl -or $projectUrl.InnerText -cne $ReleaseSettings.RepositoryUrl) {
            throw "$($package.Id) must identify repository/project URL '$($ReleaseSettings.RepositoryUrl)'."
        }
        $readme = $metadata.SelectSingleNode('*[local-name()="readme"]')
        $readmeAsset = $archive.GetEntry('README.md')
        if ($null -eq $readme -or $readme.InnerText -cne 'README.md' -or $null -eq $readmeAsset -or $readmeAsset.Length -eq 0) {
            throw "$($package.Id) must declare and contain a nonempty README.md."
        }
        $readmeStream = $readmeAsset.Open()
        try { $packagedReadmeHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($readmeStream)) } finally { $readmeStream.Dispose() }
        $sourceReadmeHash = (Get-FileHash -LiteralPath (Join-Path $ReleaseRoot $ReleaseSettings.PackageReadme) -Algorithm SHA256).Hash
        if ($packagedReadmeHash -ne $sourceReadmeHash) { throw "$($package.Id) contains a README that differs from the current module README." }
        $license = $metadata.SelectSingleNode('*[local-name()="license"]')
        if ($null -eq $license -or $license.GetAttribute('type') -ne 'expression' -or $license.InnerText -ne 'MIT') { throw "$($package.Id) must declare the MIT license expression." }
        $dependencies = @($metadata.SelectNodes('.//*[local-name()="dependency"]'))
        $internal = @($dependencies | Where-Object { $_.id -like "$($ReleaseSettings.PackagePrefix)*" })
        $expectedInternal = @($package.Dependencies | Sort-Object)
        $actualInternal = @($internal | ForEach-Object { $_.id } | Sort-Object -Unique)
        if ($expectedInternal.Count -ne $actualInternal.Count -or ($expectedInternal.Count -gt 0 -and (Compare-Object $expectedInternal $actualInternal))) { throw "$($package.Id) internal dependency graph must match the release manifest." }
        foreach ($dependency in $internal) {
            $lowerBound = ($dependency.version.Trim('[]() ') -split ',')[0].Trim()
            if ($lowerBound -ne $Version) { throw "$($package.Id) depends on $($dependency.id) at '$($dependency.version)', expected $Version." }
            if ($dependency.id -notin $ReleaseSettings.Packages.Id) { throw "Unexpected internal dependency $($dependency.id)." }
        }
        foreach ($id in $package.Dependencies) {
            if ($id -notin @($dependencies | ForEach-Object { $_.id })) { throw "$($package.Id) is missing dependency $id." }
        }
        if (!$package.Managed -and $dependencies.Count) { throw "$($package.Id) analyzer package must not publish compiler/runtime dependencies." }
        foreach ($path in $package.Assets) {
            $asset = $archive.GetEntry($path)
            if ($null -eq $asset -or $asset.Length -eq 0) { throw "$($package.Id) is missing or has an empty packaged asset '$path'." }
            if ($path.StartsWith('buildTransitive/', [StringComparison]::Ordinal)) {
                $sourceAsset = Join-Path (Join-Path $ReleaseRoot ([IO.Path]::GetDirectoryName($package.Project))) $path
                $assetStream = $asset.Open()
                try { $packagedAssetHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($assetStream)) } finally { $assetStream.Dispose() }
                if ($packagedAssetHash -ne (Get-FileHash -LiteralPath $sourceAsset -Algorithm SHA256).Hash) {
                    throw "$($package.Id) contains stale build asset '$path'; repack the current source."
                }
            }
        }
        if ($package.Managed) {
            $assemblyName = $package.Id.Substring('Arkheide.'.Length)
            $assemblies = @($archive.Entries | Where-Object { $_.FullName -like "lib/*/$assemblyName.dll" -and $_.Length -gt 0 })
            if ($assemblies.Count -ne 1) { throw "$($package.Id) must contain its one managed library '$assemblyName.dll'." }
            $xmlPath = [IO.Path]::ChangeExtension($assemblies[0].FullName, '.xml').Replace('\', '/')
            $documentation = $archive.GetEntry($xmlPath)
            if ($null -eq $documentation -or $documentation.Length -eq 0) { throw "$($package.Id) is missing public API XML documentation." }
        }
        Write-Host "Verified $($package.Id) $Version"
    } finally { $archive.Dispose() }
}
Write-Host "Verified $($expected.Count) packages, versions, repository metadata, README, licenses, dependencies, API documentation and required assets."
