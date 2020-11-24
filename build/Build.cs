using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("The solution that is the subject of the build target")]
    readonly string DataverseSolution;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    [PathExecutable]
    readonly Tool Pac;

    [PackageExecutable(
        "spkl",
        "spkl.exe"
    )]
    readonly Tool Spkl;

    [PackageExecutable(
        "Microsoft.CrmSdk.CoreTools",
        "SolutionPackager.exe")]
    readonly Tool SolutionPackager;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath SolutionsDirectory => SourceDirectory / "solutions";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target ExtractSolution => _ => _
        .Executes(() =>
       {
           var solutionConfig = GetSolutionConfig(DataverseSolution);

           SetActivePacProfile(solutionConfig.MasterProfile);

           var outputDirectory = SolutionsDirectory / DataverseSolution / ".tmp";
           EnsureExistingDirectory(outputDirectory);
           var managedSolutionPath = outputDirectory / $"{DataverseSolution}_managed.zip";
           var unmanagedSolutionPath = outputDirectory / $"{DataverseSolution}.zip";

           Pac($"solution export -p { managedSolutionPath } -n { DataverseSolution } -a -m");
           Pac($"solution export -p { unmanagedSolutionPath } -n { DataverseSolution } -a");

           var metadataFolder = SolutionsDirectory / DataverseSolution / "Extract";
           var mappingFilePath = SolutionsDirectory / DataverseSolution / "ExtractMappingFile.xml";
           SolutionPackager($"/action:Extract /zipfile:{unmanagedSolutionPath} /folder:{ metadataFolder } /packagetype:Both  /allowdelete:Yes /map:{ mappingFilePath }");

           DeleteDirectory(outputDirectory);
       });

    private void SetActivePacProfile(string profile)
    {
        Pac($"auth select -n { profile }");
    }

    private SolutionConfiguration GetSolutionConfig(string dataverseSolution)
    {
        return JsonSerializer.Deserialize<SolutionConfiguration>(
            File.ReadAllText(SolutionsDirectory / DataverseSolution / "solution.json"),
            new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
    }
}
