Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ReleaseRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$ReleaseSettings = Import-PowerShellDataFile (Join-Path $PSScriptRoot 'ReleaseSettings.psd1')
function Get-ReleaseVersion {
    [xml]$props = Get-Content -LiteralPath (Join-Path $ReleaseRoot $ReleaseSettings.VersionProps)
    $value = [string]($props.Project.PropertyGroup | ForEach-Object { $_.VersionPrefix } | Where-Object { $_ } | Select-Object -First 1)
    if ($value -notmatch '^\d+\.\d+\.\d+$') { throw "VersionPrefix '$value' must be a stable three-part version." }
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
