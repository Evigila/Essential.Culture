using System.Diagnostics;
using System.Security;
using Xunit;

namespace ArkheideSystem.Essential.Culture.Generator.Test;

public sealed class ModuleBuildTests
{
    [Fact]
    public async Task Explicit_modules_copy_linked_paths_to_build_and_publish_and_remove_stale_files()
    {
        using var host = new Host();
        host.Write("Culture.json", """{"Greeting":{"en-US":"Hello"}}""");
        host.Write("Culture.Feature.json", """{"Feature":{"en-US":"Feature"}}""");
        host.Project("""<CultureModule Include="Culture.Feature.json" ModuleId="Feature" DeploymentPath="Features/Culture.Feature.json" />""");
        await host.Success("publish", "-o", host.Publish);
        Assert.True(System.IO.File.Exists(host.Output("Culture.json")));
        Assert.True(System.IO.File.Exists(host.Output("Features/Culture.Feature.json")));
        Assert.True(System.IO.File.Exists(Path.Combine(host.Publish, "Features/Culture.Feature.json")));
        var generated = host.Generated("CultureResources.g.cs");
        Assert.Contains("\"Culture.json\"", generated);
        Assert.Contains("\"Features/Culture.Feature.json\"", generated);

        host.Project("");
        await host.Success("publish", "-o", host.Publish);
        Assert.False(System.IO.File.Exists(host.Output("Features/Culture.Feature.json")));
        Assert.False(System.IO.File.Exists(Path.Combine(host.Publish, "Features/Culture.Feature.json")));
        Assert.True(System.IO.File.Exists(host.Output("Culture.json")));
        Assert.True(System.IO.File.Exists(Path.Combine(host.Publish, "Culture.json")));
        Assert.Empty(System.IO.Directory.GetFiles(Path.Combine(host.Directory, "obj"), "Essential.Culture.modules.*.list", SearchOption.AllDirectories));
    }

    [Theory]
    [InlineData("Content")]
    [InlineData("None")]
    public async Task Stale_cleanup_preserves_current_assigned_content_and_none_destinations(string itemKind)
    {
        using var host = new Host();
        host.Write("Culture.Feature.json", """{"Feature":{"en-US":"Feature"}}""");
        host.Project("""<CultureModule Include="Culture.Feature.json" ModuleId="Feature" DeploymentPath="Culture.Feature.json" />""");
        await host.Success("publish", "-o", host.Publish);
        var absolute = SecurityElement.Escape(Path.Combine(host.Directory, "Culture.Feature.json"));
        host.Project($"<{itemKind} Include=\"{absolute}\" CopyToOutputDirectory=\"Always\" CopyToPublishDirectory=\"Always\" />");
        await host.Success("publish", "-o", host.Publish);
        Assert.True(System.IO.File.Exists(host.Output("Culture.Feature.json")));
        Assert.True(System.IO.File.Exists(Path.Combine(host.Publish, "Culture.Feature.json")));
    }

    [Fact]
    public async Task Explicit_modules_suppress_template_creation_and_disabled_generation_still_copies_resources()
    {
        using var host = new Host();
        host.Write("Feature.json", """{"Feature":{"en-US":"Feature"}}""");
        host.Project("""<CultureModule Include="Feature.json" ModuleId="Feature" DeploymentPath="Resources/Feature.json" />""",
            "<EssentialCultureGeneratorEnabled>false</EssentialCultureGeneratorEnabled><EssentialCultureAutoCreate>true</EssentialCultureAutoCreate>");
        await host.Success("build");
        Assert.False(System.IO.File.Exists(Path.Combine(host.Directory, "Culture.json")));
        Assert.True(System.IO.File.Exists(host.Output("Resources/Feature.json")));
        Assert.Empty(System.IO.Directory.GetFiles(Path.Combine(host.Directory, "obj", "generated"), "*.g.cs", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task Automatic_discovery_is_bounded_and_excludes_options()
    {
        using var host = new Host();
        host.Write("Culture.json", """{"Greeting":{"en-US":"Hello"}}""");
        host.Write("Culture.Feature.json", """{"Feature":{"en-US":"Feature"}}""");
        host.Write("Culture.options.json", "not translations");
        host.Write("Nested/Culture.Hidden.json", "not translations");
        host.Project("", "<EssentialCultureModulesEnabled>true</EssentialCultureModulesEnabled>");
        await host.Success("build");
        Assert.True(System.IO.File.Exists(host.Output("Culture.Feature.json")));
        Assert.False(System.IO.File.Exists(host.Output("Culture.options.json")));
        Assert.False(System.IO.File.Exists(host.Output("Nested/Culture.Hidden.json")));
    }

    [Fact]
    public async Task Auto_include_false_leaves_module_inclusion_and_copying_to_the_host()
    {
        using var host = new Host();
        host.Write("Culture.Feature.json", """{"Feature":{"en-US":"Feature"}}""");
        host.Project("""<CultureModule Include="Culture.Feature.json" />""",
            "<EssentialCultureModulesEnabled>true</EssentialCultureModulesEnabled><EssentialCultureAutoInclude>false</EssentialCultureAutoInclude><EssentialCultureAutoCreate>true</EssentialCultureAutoCreate>");
        await host.Success("build");
        Assert.False(System.IO.File.Exists(host.Output("Culture.Feature.json")));
        Assert.False(System.IO.File.Exists(Path.Combine(host.Directory, "Culture.json")));
        Assert.DoesNotContain("Culture.Feature.json", System.IO.File.ReadAllText(Path.Combine(host.Directory, "obj", "Debug", "net10.0", "Tests.Essential.Culture.Generator.Host.GeneratedMSBuildEditorConfig.editorconfig")));
        Assert.Empty(System.IO.Directory.GetFiles(Path.Combine(host.Directory, "obj"), "Essential.Culture.modules.*.list", SearchOption.AllDirectories));
    }

    [Theory]
    [InlineData("Culture.json", false)]
    [InlineData("Culture.Feature.json", true)]
    public async Task Relative_explicit_modules_do_not_duplicate_absolute_root_or_discovered_inputs(string name, bool discovery)
    {
        using var host = new Host();
        host.Write("Culture.json", """{"Greeting":{"en-US":"Hello"}}""");
        if (name != "Culture.json") host.Write(name, """{"Feature":{"en-US":"Feature"}}""");
        host.Project($"<CultureModule Include=\"{name}\" />", discovery ? "<EssentialCultureModulesEnabled>true</EssentialCultureModulesEnabled>" : "");
        await host.Success("build");
        var generated = host.Generated("CultureResources.g.cs");
        Assert.Equal(name == "Culture.json" ? 1 : 2, generated.Split("            \"").Length - 1);
    }

    [Fact]
    public async Task Unsafe_paths_fail_before_copying_even_when_the_generator_is_disabled()
    {
        using var host = new Host();
        host.Write("Feature.json", """{"Feature":{"en-US":"Feature"}}""");
        host.Project("""<CultureModule Include="Feature.json" DeploymentPath="../Outside.json" />""",
            "<EssentialCultureGeneratorEnabled>false</EssentialCultureGeneratorEnabled>");
        var result = await host.Run("build");
        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("AEC006", result.Output);
        Assert.False(System.IO.File.Exists(Path.Combine(host.Directory, "bin", "Debug", "Outside.json")));
    }

    [Fact]
    public async Task A_changed_output_directory_never_cleans_the_previous_destination()
    {
        using var host = new Host();
        host.Write("Feature.json", """{"Feature":{"en-US":"Feature"}}""");
        host.Project("""<CultureModule Include="Feature.json" DeploymentPath="Feature.json" />""");
        await host.Success("publish", "-o", host.Publish);
        host.Project("");
        await host.Success("publish", "-o", Path.Combine(host.Directory, "another-publish"));
        Assert.True(System.IO.File.Exists(Path.Combine(host.Publish, "Feature.json")));
    }

    [Fact]
    public async Task Stale_cleanup_never_deletes_outside_the_current_output_or_non_json_files()
    {
        using var host = new Host();
        host.Write("Feature.json", """{"Feature":{"en-US":"Feature"}}""");
        host.Project("""<CultureModule Include="Feature.json" />""");
        await host.Success("build");
        host.Write("bin/Debug/Outside.json", "keep");
        host.Write("bin/Debug/net10.0/Keep.dll", "keep");
        var root = Path.GetFullPath(host.Output("")).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        host.Write("obj/Debug/net10.0/Essential.Culture.modules.build.list", root + "\n../Outside.json\nKeep.dll\nFeature.json\n");
        host.Project("");
        await host.Success("build");
        Assert.True(System.IO.File.Exists(Path.Combine(host.Directory, "bin", "Debug", "Outside.json")));
        Assert.True(System.IO.File.Exists(host.Output("Keep.dll")));
        Assert.False(System.IO.File.Exists(host.Output("Feature.json")));
    }

    [Fact]
    public async Task Legacy_single_file_build_does_not_create_a_module_manifest_or_cleanup_state()
    {
        using var host = new Host();
        host.Write("Culture.json", """{"Greeting":{"en-US":"Hello"}}""");
        host.Project("");
        await host.Success("build");
        Assert.True(System.IO.File.Exists(host.Output("Culture.json")));
        Assert.Empty(System.IO.Directory.GetFiles(Path.Combine(host.Directory, "obj"), "Essential.Culture.modules.*.list", SearchOption.AllDirectories));
        Assert.Empty(System.IO.Directory.GetFiles(Path.Combine(host.Directory, "obj", "generated"), "CultureResources.g.cs", SearchOption.AllDirectories));
    }

    private sealed class Host : IDisposable
    {
        private readonly string ownedRoot = Path.Combine(AppContext.BaseDirectory, "module-build-tests");
        private readonly string assets;
        internal Host()
        {
            Directory = Path.Combine(ownedRoot, Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(Directory);
            var repository = new DirectoryInfo(AppContext.BaseDirectory);
            while (repository is not null && !System.IO.File.Exists(Path.Combine(repository.FullName, "Essential.slnx"))) repository = repository.Parent;
            assets = Path.Combine(repository!.FullName, "src", "Essential.Culture", "Essential.Culture.Generator", "buildTransitive");
            Write("Program.cs", "namespace ArkheideSystem.Essential.Culture.Generator.Test.Host; internal sealed class HostInput;");
        }

        internal string Directory { get; }
        internal string Publish => Path.Combine(Directory, "publish");
        internal string Output(string path) => Path.Combine(Directory, "bin", "Debug", "net10.0", path);
        internal string Generated(string hint) => System.IO.File.ReadAllText(Assert.Single(
            System.IO.Directory.GetFiles(Path.Combine(Directory, "obj", "generated"), hint, SearchOption.AllDirectories)));
        internal void Write(string path, string text)
        {
            var target = Path.Combine(Directory, path);
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            System.IO.File.WriteAllText(target, text);
        }

        internal void Project(string items, string properties = "") => Write("Tests.Essential.Culture.Generator.Host.csproj", $$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <Import Project="{{SecurityElement.Escape(Path.Combine(assets, "Arkheide.Essential.Culture.Generator.props"))}}" />
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework><OutputType>Library</OutputType><Nullable>enable</Nullable>
                <RootNamespace>ArkheideSystem.Essential.Culture.Generator.Test.Host</RootNamespace>
                <EnableDefaultCompileItems>false</EnableDefaultCompileItems><EnableDefaultNoneItems>false</EnableDefaultNoneItems>
                <EssentialCultureAutoCreate>false</EssentialCultureAutoCreate>
                <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles><CompilerGeneratedFilesOutputPath>obj/generated</CompilerGeneratedFilesOutputPath>
                <NuGetAudit>false</NuGetAudit>{{properties}}
              </PropertyGroup>
              <ItemGroup>
                <Compile Include="Program.cs" /><Analyzer Remove="@(Analyzer)" />
                <Analyzer Include="{{SecurityElement.Escape(typeof(CultureGenerator).Assembly.Location)}}" />
                {{items}}
              </ItemGroup>
              <Import Project="{{SecurityElement.Escape(Path.Combine(assets, "Arkheide.Essential.Culture.Generator.targets"))}}" />
            </Project>
            """);

        internal async Task Success(params string[] arguments)
        {
            var result = await Run(arguments);
            Assert.True(result.ExitCode == 0, result.Output);
        }

        internal async Task<(int ExitCode, string Output)> Run(params string[] arguments)
        {
            var start = new ProcessStartInfo("dotnet") { WorkingDirectory = Directory, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
            foreach (var argument in arguments) start.ArgumentList.Add(argument);
            start.ArgumentList.Add("--configuration"); start.ArgumentList.Add("Debug");
            start.ArgumentList.Add("--nologo");
            start.ArgumentList.Add("--verbosity"); start.ArgumentList.Add("minimal");
            using var process = Process.Start(start)!;
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { process.Kill(entireProcessTree: true); throw; }
            return (process.ExitCode, await output + await error);
        }

        public void Dispose()
        {
            var target = Path.GetFullPath(Directory);
            var allowed = Path.GetFullPath(ownedRoot) + Path.DirectorySeparatorChar;
            if (!target.StartsWith(allowed, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Unexpected test cleanup path.");
            if (System.IO.Directory.Exists(target)) System.IO.Directory.Delete(target, recursive: true);
        }
    }
}
