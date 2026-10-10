param([string]$PackageDirectory, [string]$Version)
. (Join-Path $PSScriptRoot 'Release-Common.ps1')
if (!$Version) { $Version = Get-ReleaseVersion }
if (!$PackageDirectory) { $PackageDirectory = Join-Path $ReleaseRoot 'artifacts/packages' }
$PackageDirectory = [IO.Path]::GetFullPath($PackageDirectory)
$consumerRoot = Join-Path $ReleaseRoot ('artifacts/package-consumers/modules-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $consumerRoot -Force | Out-Null
$project = Join-Path $consumerRoot 'Tests.Essential.Culture.Modules.PackageConsumer.csproj'
$projectContent = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <RootNamespace>ArkheideSystem.Essential.Culture.PackageConsumer</RootNamespace>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <EssentialCultureNamespace>ArkheideSystem.Essential.Culture.PackageConsumer.Texts</EssentialCultureNamespace>
    <EssentialCultureGeneratorEnabled>true</EssentialCultureGeneratorEnabled>
    <EssentialCultureXamlFramework>none</EssentialCultureXamlFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Arkheide.Essential.Culture" Version="$(CulturePackageVersion)" />
    <CultureModule Include="Culture.Feature.json" ModuleId="Feature" DeploymentPath="Features/Culture.Feature.json" />
  </ItemGroup>
</Project>
'@
Set-Content -LiteralPath $project -Value $projectContent -Encoding utf8
Set-Content -LiteralPath (Join-Path $consumerRoot 'Culture.json') -Value '{"Greeting":{"en-US":"Hello","zh-CN":"你好"}}' -Encoding utf8
Set-Content -LiteralPath (Join-Path $consumerRoot 'Culture.Feature.json') -Value '{"Feature_Message":{"en-US":"Feature {0}","zh-CN":"功能 {0}"}}' -Encoding utf8
$program = @'
using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Essential.Culture.PackageConsumer.Texts;

namespace ArkheideSystem.Essential.Culture.PackageConsumer;

internal static class Program
{
    private static void Main()
    {
        var files = CultureResources.Files.Select(path => Path.Combine(AppContext.BaseDirectory, path)).ToArray();
        Assert(CultureResources.Files.SequenceEqual(new[] { "Culture.json", "Features/Culture.Feature.json" }),
            "Generated resource manifest must preserve the root catalog and explicit deployment path.");
        Assert(files.All(File.Exists), "Every catalog must be deployed to its generated relative path.");
        var catalog = LocalizationCatalog.FromFiles(files, CultureResources.FallbackCulture,
            new CatalogLoadOptions(disabledCultures: ["zh-CN"]));
        Assert(catalog.AvailableCultures.SequenceEqual(new[] { "en-US" }), "Disabled language must be unavailable.");
        Localizer.Configure(catalog, "en-US");
        Assert(Localizer.Parse(Key.Greeting) == "Hello", "Root generated key must use the composed catalog.");
        Assert(Localizer.Parse(Key.Feature_Message, "package") == "Feature package", "Module generated key must resolve and format.");
        Console.WriteLine("Module-only NuGet consumer passed: transitive Generator, root/module keys, deployment manifest and language policy.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
'@
Set-Content -LiteralPath (Join-Path $consumerRoot 'Program.cs') -Value $program -Encoding utf8
$packageCache = Join-Path $consumerRoot 'packages'
Invoke-ReleaseCommand dotnet @('restore', $project, '--source', $PackageDirectory, '--packages', $packageCache,
    "-p:CulturePackageVersion=$Version")
$assets = Get-Content -LiteralPath (Join-Path $consumerRoot 'obj/project.assets.json') -Raw | ConvertFrom-Json
foreach ($id in 'Arkheide.Essential.Culture', 'Arkheide.Essential.Culture.Generator') {
    if ($assets.libraries.PSObject.Properties.Name -notcontains "$id/$Version") {
        throw "Module-only consumer is missing transitive package $id/$Version."
    }
}
Invoke-ReleaseCommand dotnet @('build', $project, '-c', 'Release', '--no-restore', '-p:TreatWarningsAsErrors=true',
    "-p:CulturePackageVersion=$Version")
Invoke-ReleaseCommand dotnet @('run', '--project', $project, '-c', 'Release', '--no-build', '--no-restore',
    "-p:CulturePackageVersion=$Version")
$publishDirectory = Join-Path $consumerRoot 'published'
Invoke-ReleaseCommand dotnet @('publish', $project, '-c', 'Release', '--no-build', '--no-restore', '--output', $publishDirectory,
    "-p:CulturePackageVersion=$Version")
Invoke-ReleaseCommand dotnet @((Join-Path $publishDirectory 'Tests.Essential.Culture.Modules.PackageConsumer.dll'))
Write-Host "Verified isolated module consumer build and publish at $consumerRoot"
