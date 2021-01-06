using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using Nuke.Common.Utilities.Collections;
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

    [Parameter("The type of solution")]
    readonly SolutionType SolutionType;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    [PathExecutable]
    readonly Tool Pac;

    [PackageExecutable(
        "Microsoft.CrmSdk.CoreTools",
        "SolutionPackager.exe")]
    readonly Tool SolutionPackager;

    // using local executable rather than package due to an issue with spkl location CrmSvcUtil within SDK-style package cache
    [LocalExecutable(".tmp/spkl/spkl.exe")]
    readonly Tool Spkl;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath SolutionsDirectory => SourceDirectory / "solutions";
    AbsolutePath SolutionDirectory => SolutionsDirectory / DataverseSolution;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj", "**/dist", "**/out", "**/node_modules").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
            {
                SourceDirectory.GlobFiles("**/WebResources/*/package.json").ForEach(packageFile =>
                {
                    NpmTasks.NpmInstall(s => s.SetProcessWorkingDirectory(packageFile.Parent));
                });

                DotNetRestore(s => s
                    .SetProjectFile(Solution));
            });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobFiles("**/WebResources/*/package.json").ForEach(packageFile =>
            {
                NpmTasks.NpmRun(s => s
                    .SetProcessWorkingDirectory(packageFile.Parent)
                    .SetCommand("build"));
            });

            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(SolutionType == SolutionType.Unmanaged ? Configuration.Debug : Configuration.Release)
                .SetDisableParallel(true)
                .EnableNoRestore());
        });

    Target CompileTests => _ => _
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration.Test));
        });

    Target ExtractSolution => _ => _
        .Executes(() =>
       {
           var solutionConfig = GetSolutionConfig(DataverseSolution);

           SetActivePacProfile(solutionConfig.MasterProfile);

           var outputDirectory = SolutionDirectory / ".tmp";
           EnsureExistingDirectory(outputDirectory);
           var managedSolutionPath = outputDirectory / $"{DataverseSolution}_managed.zip";
           var unmanagedSolutionPath = outputDirectory / $"{DataverseSolution}.zip";

           Pac($"solution export -p { managedSolutionPath } -n { DataverseSolution } -a -m");
           Pac($"solution export -p { unmanagedSolutionPath } -n { DataverseSolution } -a");

           var metadataFolder = SolutionDirectory / "Extract";
           var mappingFilePath = SolutionDirectory / "ExtractMappingFile.xml";
           SolutionPackager($"/action:Extract /zipfile:{unmanagedSolutionPath} /folder:{ metadataFolder } /packagetype:Both  /allowdelete:Yes /map:{ mappingFilePath }");

           DeleteDirectory(outputDirectory);
       });

    Target PackSolution => _ => _
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(SolutionDirectory / $"{DataverseSolution}.cdsproj")
                .SetConfiguration(SolutionType == SolutionType.Unmanaged ? Configuration.Debug : Configuration.Release)
                .SetDisableParallel(true));
        });

    Target PrepareDevelopmentEnvironment => _ => _
        .Executes(() =>
        {
            InstallSolutionAndDependencies(DataverseSolution, SolutionType.Unmanaged);
        });

    Target CopySpklToTempFolder => _ => _
        .Executes(() =>
        {
            var spklDirectory = ((AbsolutePath)ToolPathResolver.GetPackageExecutable("spkl", "spkl.exe")).Parent;
            var targetSpklDirectory = TemporaryDirectory / "spkl";
            DeleteDirectory(targetSpklDirectory);
            CopyDirectoryRecursively(spklDirectory, targetSpklDirectory);

            var coreToolsDirectory = ((AbsolutePath)ToolPathResolver.GetPackageExecutable("Microsoft.CrmSdk.CoreTools", "CrmSvcUtil.exe")).Parent;
            var targetCoreToolsDirectory = TemporaryDirectory / "coretools";
            DeleteDirectory(targetCoreToolsDirectory);
            CopyDirectoryRecursively(coreToolsDirectory, targetCoreToolsDirectory);
        });

    Target DeployPlugins => _ => _
        .DependsOn(CopySpklToTempFolder)
        .Executes(() =>
        {
            Spkl($"plugins { SolutionDirectory / "spkl.json" }");
        });

    Target DeployWorkflowActivities => _ => _
        .DependsOn(CopySpklToTempFolder)
        .Executes(() =>
        {
            Spkl($"workflow { SolutionDirectory / "spkl.json" }");
        });

    Target GenerateModel => _ => _
        .DependsOn(CopySpklToTempFolder)
        .Executes(() =>
        {
            Spkl($"earlybound { SolutionDirectory / "spkl.json" }");
        });

    void SetActivePacProfile(string profile)
    {
        Pac($"auth select -n { profile }");
    }

    SolutionConfiguration GetSolutionConfig(string dataverseSolution)
    {
        return JsonSerializer.Deserialize<SolutionConfiguration>(
            File.ReadAllText(SolutionsDirectory / dataverseSolution / "solution.json"),
            new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
    }

    Dictionary<string, string> GetSolutionDependencies(string solution)
    {
        var dependencyAttributes = (IEnumerable)XDocument.Load(SolutionsDirectory / solution / "Extract" / "Other" / "Solution.xml")
          .XPathEvaluate("/ImportExportXml/SolutionManifest/MissingDependencies/MissingDependency/Required/@solution");

        var dependencies = dependencyAttributes
          .Cast<XAttribute>()
          .Select(solutionAttribute => solutionAttribute.Value)
          .Where(sol => sol != "Active")
          .Distinct()
          .ToDictionary(sol => sol.Split(' ')[0], sol => sol.Split('(', ')')[1]);

        return dependencies;
    }

    void InstallSolutionAndDependencies(string solution, SolutionType solutionType, IList<string> installedSolutions = null)
    {
        if (installedSolutions == null)
        {
            installedSolutions = new List<string>();
            SetActivePacProfile(GetSolutionConfig(solution).DevelopmentProfile);
        }

        foreach (var dependency in GetSolutionDependencies(solution).Where(dep => DirectoryExists(SolutionsDirectory / dep.Key)))
        {
            if (!installedSolutions.Contains(dependency.Key))
            {
                InstallSolutionAndDependencies(dependency.Key, SolutionType.Managed, installedSolutions);
            }
        }

        var buildConfiguration = solutionType == SolutionType.Managed ? "Release" : "Debug";
        DotNetBuild(s => s
            .SetProjectFile(SolutionsDirectory / solution / $"{solution}.cdsproj")
            .SetConfiguration(buildConfiguration)
            .SetDisableParallel(true));

        Pac($"solution import --path \"{ SolutionsDirectory / solution / "bin" / buildConfiguration / $"{solution}.zip" }\" -ap -pc -a");
        installedSolutions.Add(solution);
    }
}
