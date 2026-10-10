param([ValidateSet('Prepare', 'Publish')][string]$Mode = 'Prepare', [switch]$SkipBuild)
. (Join-Path $PSScriptRoot 'Release-Common.ps1')
Assert-ReleaseRepository
function Assert-CleanMaster {
    $status = @(& git -C $ReleaseRoot status --porcelain)
    if ($LASTEXITCODE -ne 0 -or $status.Count) { throw 'Release requires a clean working tree, including untracked files.' }
    $branch = & git -C $ReleaseRoot branch --show-current
    if ($LASTEXITCODE -ne 0 -or $branch -ne 'master') { throw 'Release requires the master branch.' }
}
if ($Mode -eq 'Publish') { Assert-CleanMaster }
& (Join-Path $PSScriptRoot 'Test-Release.ps1') -VerifyOnly:$SkipBuild
if ($Mode -eq 'Prepare') { return }
Assert-CleanMaster
Invoke-ReleaseCommand 'git' @('-C', $ReleaseRoot, 'fetch', 'origin', 'master', '--tags')
$head = & git -C $ReleaseRoot rev-parse HEAD
$remote = & git -C $ReleaseRoot rev-parse origin/master
if ($head -ne $remote) { throw 'Publish requires HEAD to equal origin/master. Commit and push reviewed changes first.' }
$tag = 'v' + (Get-ReleaseVersion)
Assert-ReleaseTag $tag
& git -C $ReleaseRoot rev-parse --verify "refs/tags/$tag" 2>$null | Out-Null
if ($LASTEXITCODE -eq 0) { throw "Tag '$tag' already exists. Choose a new version rather than replacing it." }
Write-Host "Repository: $ReleaseRoot"
Write-Host "Commit: $head"
Write-Host "This will create and push $tag. The GitHub nuget environment will control Trusted Publishing."
$answer = Read-Host "Type $tag to confirm"
if ($answer -ne $tag) { throw 'Release cancelled; no tag was created.' }
Invoke-ReleaseCommand 'git' @('-C', $ReleaseRoot, 'tag', '-a', $tag, '-m', "Release $tag")
Invoke-ReleaseCommand 'git' @('-C', $ReleaseRoot, 'push', 'origin', "refs/tags/$tag")
Write-Host "Pushed $tag. Monitor the Build, test, and publish workflow; no API key is used locally."
