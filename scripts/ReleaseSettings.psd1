@{
    VersionProps = 'src/Essential.Culture/Directory.Build.props'
    Solution = 'Essential.slnx'
    DemoSolution = 'demo/Essential.Culture.Demo.slnx'
    ConsoleTests = @()
    JavaScriptTests = @()
    CheckScripts = @()
    PackagePrefix = 'Arkheide.Essential.Culture'
    EssentialVersion = '1.3.0'
    Packages = @(
        @{ Id = 'Arkheide.Essential.Culture.Generator'; Project = 'src/Essential.Culture/Essential.Culture.Generator/Essential.Culture.Generator.csproj'; Dependencies = @(); Assets = @('analyzers/dotnet/cs/Essential.Culture.Generator.dll', 'buildTransitive/Arkheide.Essential.Culture.Generator.props', 'buildTransitive/Arkheide.Essential.Culture.Generator.targets'); Managed = $false }
        @{ Id = 'Arkheide.Essential.Culture'; Project = 'src/Essential.Culture/Essential.Culture/Essential.Culture.csproj'; Dependencies = @('Arkheide.Essential.Culture.Generator'); Assets = @(); Managed = $true }
        @{ Id = 'Arkheide.Essential.Culture.Wpf'; Project = 'src/Essential.Culture/Essential.Culture.Wpf/Essential.Culture.Wpf.csproj'; Dependencies = @('Arkheide.Essential.Culture'); Assets = @(); Managed = $true }
        @{ Id = 'Arkheide.Essential.Culture.Avalonia'; Project = 'src/Essential.Culture/Essential.Culture.Avalonia/Essential.Culture.Avalonia.csproj'; Dependencies = @('Arkheide.Essential.Culture'); Assets = @(); Managed = $true }
        @{ Id = 'Arkheide.Essential.Culture.WinUI'; Project = 'src/Essential.Culture/Essential.Culture.WinUI/Essential.Culture.WinUI.csproj'; Dependencies = @('Arkheide.Essential.Culture'); Assets = @(); Managed = $true }
        @{ Id = 'Arkheide.Essential.Culture.Blazor'; Project = 'src/Essential.Culture/Essential.Culture.Blazor/Essential.Culture.Blazor.csproj'; Dependencies = @('Arkheide.Essential.Culture'); Assets = @(); Managed = $true }
    )
}
