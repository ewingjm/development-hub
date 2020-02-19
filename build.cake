using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath.Extensions;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

#addin nuget:?package=Cake.Xrm.Sdk&version=0.1.9
#addin nuget:?package=Cake.Xrm.SolutionPackager&version=0.1.10
#addin nuget:?package=Cake.Xrm.Spkl&version=0.1.7
#addin nuget:?package=Cake.Npm&version=0.17.0
#addin nuget:?package=Cake.Json&version=3.0.0

const string SolutionsFolder = "./solutions";
const string PackagesFolder = "./packages";
const string DeployProjectFolder = "./deploy";
const string TestsFolder = "./tests";

var target = Argument("target", "Default");
var solution = Argument("solution", "");
var packedSolutions = new List<string>();

// Build package
Task("Default")
  .IsDependentOn("PackAll")
  .IsDependentOn("BuildDeploymentProject");

Task("BuildDeploymentProject")
  .Does(() => {
    BuildCSharpProject(
      File($"{DeployProjectFolder}/DevelopmentHub.Deployment.csproj"), 
      new NuGetRestoreSettings { ConfigFile = "NuGet.config" }, 
      new MSBuildSettings { Configuration = "Release" });
    DeleteFiles($"{DeployProjectFolder}/bin/Release/PkgFolder/*.zip");
    CopyFiles($"{SolutionsFolder}/**/*.zip", Directory("Deploy/bin/Release/PkgFolder"));
  });

Task("PackAll")
  .IsDependentOn("ValidatePackageDependencies")
  .Does(() => {
    foreach (var solutionDirectory in GetDirectories($"{SolutionsFolder}/*"))
    {
      solution = solutionDirectory.GetDirectoryName();
      if (!packedSolutions.Contains(solution)){
        RunTarget("PackSolution");
      }
    }
  });

Task("ValidatePackageDependencies")
  .Does(() => {
    var dependencies = new Dictionary<string, string>();
    foreach (var solutionDirectory in GetDirectories($"{SolutionsFolder}/*"))
    {
      var currentSolution = solutionDirectory.GetDirectoryName();
      var solutionDependencies = GetSolutionDependencies(currentSolution).Where(dep => !DirectoryExists($"{SolutionsFolder}/{dep.Key}"));
      var mismatch = solutionDependencies.FirstOrDefault(dependency => dependencies.ContainsKey(dependency.Key) && dependencies[dependency.Key] != dependency.Value);
      if (!mismatch.Equals(default(KeyValuePair<string,string>))) {
        throw new Exception($"Dependency mismatch detected. {currentSolution} is dependent on {mismatch.Key} {mismatch.Value} but existing dependency on {dependencies[mismatch.Key]}.");
      }
      dependencies = dependencies.Concat(solutionDependencies.Where(kvp => !dependencies.ContainsKey(kvp.Key))).ToDictionary(c => c.Key, c => c.Value);
    }
  });

Task("GenerateModel")
  .Does(() => {
    SpklGenerateEarlyBound(
      Directory($"{SolutionsFolder}/{solution}").Path.CombineWithFilePath("spkl.json"), 
      GetConnectionString(solution, false));
  });

Task("ExtractSolution")
  .Does(() => {
    ExtractSolution(
      GetConnectionString(solution, true), 
      solution, 
      Directory($"{SolutionsFolder}/{solution}").Path.Combine("Extract"));
  });

// build targets 
Task("BuildTestProjects")
  .Does(() => {
    var nugetSettings = new NuGetRestoreSettings { ConfigFile = "NuGet.config" };
    foreach (var testProject in GetFiles($"{TestsFolder}/**/*.csproj")) 
    {
      BuildCSharpProject(testProject.FullPath, nugetSettings, new MSBuildSettings { Configuration = "Debug" });
    }
  });

Task("ResolveSolutionDependencies")
  .Does(() => {
    Information($"Resolving dependencies for the {solution} solution.");
    var solutionToResolve = solution;
    var dependencies = GetSolutionDependencies(solution);
    var resolvedSolutions = new List<string>();

    var localDependencies = dependencies.Where(dep => DirectoryExists($"{SolutionsFolder}/{dep.Key}"));
    foreach (var localDep in localDependencies)
    {
        if (!resolvedSolutions.Contains(localDep.Key))
        {
          Information($"Detected the {localDep.Key} solution in source. Building local dependency.");
          solution = localDep.Key;
          if (!packedSolutions.Contains(solution))
          {
            RunTarget("PackSolution");
          }
          EnsureDirectoryExists($"{SolutionsFolder}/{solutionToResolve}/bin/Release");
          CopyFiles($"{SolutionsFolder}/{solution}/bin/Release/**/*_managed.zip", Directory($"{SolutionsFolder}/{solutionToResolve}/bin/Release"));
          resolvedSolutions.Add(localDep.Key);
          solution = solutionToResolve;
        }
    }

    var externalDependencies = dependencies.Where(dep => !DirectoryExists($"{SolutionsFolder}/{dep.Key}"));
    var noResolveDependencies = GetSolutionConfig(solutionToResolve)["dependencies"]?["noResolve"].ToObject<List<string>>();
    foreach (var externalDep in externalDependencies)
    {
      if (!resolvedSolutions.Contains(externalDep.Key) && (noResolveDependencies == null || !noResolveDependencies.Contains(externalDep.Key)))
      {
        Information($"{externalDep.Key} solution not detected in source. Retrieving external dependency.");
        var packageId = GetNugetPackageIdForExternalSolution(solutionToResolve, externalDep.Key);
        if(string.IsNullOrEmpty(packageId))
        {
          throw new Exception($"An entry for {externalDep.Key} was not present in the externalDependencies configuration for the {solutionToResolve} solution.");
        }
        ResolveSolutionDependency(packageId, externalDep.Value, solutionToResolve);
        resolvedSolutions.Add(externalDep.Key);      
      }
    }
  });

Task("BuildSolution")
  .DoesForEach(
    GetFiles($"{Directory($"{SolutionsFolder}/{solution}")}/**/package.json",  new GlobberSettings { Predicate = (fileSystemInfo) => !fileSystemInfo.Path.FullPath.Contains("node_modules") }), 
    (packageFile) => {
      var directory = packageFile.GetDirectory();
      NpmInstall(new NpmInstallSettings { WorkingDirectory = directory });
      NpmRunScript(new NpmRunScriptSettings { ScriptName = "build", WorkingDirectory = directory, });
  })
  .DoesForEach(
    GetFiles($"{Directory($"{SolutionsFolder}/{solution}")}/**/*.csproj"),
    (msBuildProject) => {
      BuildCSharpProject(
        msBuildProject, 
        new NuGetRestoreSettings { ConfigFile = "NuGet.config" }, 
        new MSBuildSettings { Configuration = "Deploy" });
    }
  );

// pack targets
Task("PackSolution")
  .IsDependentOn("BuildSolution")
  .IsDependentOn("ResolveSolutionDependencies")
  .Does(() => {
    var solutionFolder = Directory($"{SolutionsFolder}/{solution}");
    
    SolutionPackagerPack(
      new SolutionPackagerPackSettings(
        solutionFolder.Path.CombineWithFilePath($"bin\\Release\\{solution}.zip"),
        solutionFolder.Path.Combine("Extract"),
        SolutionPackageType.Both,
        solutionFolder.Path.CombineWithFilePath("MappingFile.xml")));
    packedSolutions.Add(solution);
  });
  
// deploy targets
Task("DeployPlugins")
  .DoesForEach(
    GetFiles($"{Directory($"{SolutionsFolder}/{solution}")}/**/*.csproj"),
    (msBuildProject) => {
      BuildCSharpProject(
        msBuildProject, 
        new NuGetRestoreSettings { ConfigFile = "NuGet.config" }, 
        new MSBuildSettings { Configuration = "Deploy" });
    }
  )
  .Does(() => {
    SpklDeployPlugins(Directory($"{SolutionsFolder}/{solution}").Path.CombineWithFilePath("spkl.json"), GetConnectionString(solution, false));
});

Task("DeployWorkflowActivities")
  .DoesForEach(
    GetFiles($"{Directory($"{SolutionsFolder}/{solution}")}/**/*.csproj"),
    (msBuildProject) => {
      BuildCSharpProject(
        msBuildProject, 
        new NuGetRestoreSettings { ConfigFile = "NuGet.config" }, 
        new MSBuildSettings { Configuration = "Deploy" });
    }
  )
  .Does(() => {
    SpklDeployWorkflows(Directory($"{SolutionsFolder}/{solution}").Path.CombineWithFilePath("spkl.json"), GetConnectionString(solution, false));
});

// Environment targets

Task("BuildDevelopmentEnvironment")
  .Does(() => {
    Information($"Building development environment for {solution}.");
    RunTarget("ResolveSolutionDependencies");
    InstallSolution(GetConnectionString(solution, false), solution, solution, false, new List<string>());
  });

void BuildCSharpProject(FilePath projectPath, NuGetRestoreSettings nugetSettings, MSBuildSettings msBuildSettings = null) { 
    NuGetRestore(projectPath, nugetSettings);
    MSBuild(projectPath, msBuildSettings);
}

string GetNugetPackageIdForExternalSolution(string dependentSolution, string dependencySolution) 
{
  Information($"Retrieving NuGet package ID for external dependency {dependencySolution}");
  return GetSolutionConfig(dependentSolution)["dependencies"]?["nuget"]?[dependencySolution]?.Value<string>();
}

Dictionary<string, string> GetSolutionDependencies(string solution) 
{
  var dependencyAttributes = (IEnumerable)XDocument.Load($"{SolutionsFolder}/{solution}/Extract/Other/Solution.Xml")
    .XPathEvaluate("/ImportExportXml/SolutionManifest/MissingDependencies/MissingDependency/Required/@solution");
    
  var dependencies = dependencyAttributes
    .Cast<XAttribute>()
    .Select(solutionAttribute => solutionAttribute.Value)
    .Where(sol => sol != "Active")
    .Distinct()
    .ToDictionary(sol => sol.Split(' ')[0], sol => sol.Split('(', ')')[1]);
  
  return dependencies;
}

void InstallSolution(string connectionString, string solutionToBuild, string solutionToInstall, bool managed, List<string> installedSolutions) {
  var dependencies = GetSolutionDependencies(solutionToInstall);

  var localDependencies = dependencies.Where(dep => DirectoryExists($"{SolutionsFolder}/{dep.Key}"));
  foreach (var localDep in localDependencies)
  {
      if (!installedSolutions.Contains(localDep.Key))
      {
        InstallSolution(connectionString, solutionToBuild, localDep.Key, true, installedSolutions);
        installedSolutions.Add(localDep.Key);
      }
  }

  var externalDependencies = dependencies.Where(dep => !DirectoryExists($"{SolutionsFolder}/{dep.Key}"));
  var noResolveDependencies = GetSolutionConfig(solutionToInstall)["dependencies"]?["noResolve"].ToObject<List<string>>();
  foreach (var externalDep in externalDependencies)
  {
    if (!installedSolutions.Contains(externalDep.Key)  && (noResolveDependencies == null || !noResolveDependencies.Contains(externalDep.Key)))
    {
      var solutionZip = GetFiles($"{SolutionsFolder}/{solutionToBuild}/bin/Release/**/{externalDep.Key}{(managed ? "_managed" : "")}.zip").First();
      XrmImportSolution(connectionString, solutionZip, new SolutionImportSettings { ActivePlugins = true });
      installedSolutions.Add(externalDep.Key);
    }
  }
  
  XrmImportSolution(connectionString, File($"{SolutionsFolder}/{solutionToBuild}/bin/Release/{solutionToInstall}{(managed ? "_managed" : "")}.zip"), new SolutionImportSettings { ActivePlugins = true });
  installedSolutions.Add(solutionToInstall);
}

void ResolveSolutionDependency(string packageId, string packageVersion, string solution)
{
  Information($"Resolving NuGet solution dependency {packageId} {packageVersion} for {solution}.");
  NuGetInstall(packageId, new NuGetInstallSettings { Version = packageVersion });
  EnsureDirectoryExists($"{SolutionsFolder}/{solution}/bin/Release");
  CopyFiles($"{PackagesFolder}/{packageId}.{packageVersion}/**/*_managed.zip", Directory($"{SolutionsFolder}/{solution}/bin/Release"));
}

// Utilities

JObject GetSolutionConfig(string solution) {
  return ParseJsonFromFile(File($"{SolutionsFolder}/{solution}/solution.json"));
}

string GetConnectionString(string solution, bool stagingEnvironment) {
  var config = GetSolutionConfig(solution);
  var targetEnvironment = stagingEnvironment && config["environments"]?["staging"] != null ? "staging" : "development";
  
  var url = config["environments"][targetEnvironment]["url"].ToString();
  var username = config["environments"][targetEnvironment]["username"]?.ToString() ?? EnvironmentVariable("CAKE_DYNAMICS_USERNAME_DEVELOPMENT_HUB");
  var password = EnvironmentVariable("CAKE_DYNAMICS_PASSWORD_DEVELOPMENT_HUB");

  return $"Url={url}; Username={username}; Password={password}; AuthType=Office365;";
}

void ExtractSolution(string connectionString, string solutionName, DirectoryPath outputPath) {
  var tempDirectory = DirectoryPath.FromString(EnvironmentVariable("TEMP"));
  XrmExportSolution(connectionString, solutionName, tempDirectory, isManaged: false);
  XrmExportSolution(connectionString, solutionName, tempDirectory, isManaged: true);
  SolutionPackagerExtract(tempDirectory.CombineWithFilePath($"{solutionName}.zip"), outputPath, SolutionPackageType.Both);
}

RunTarget(target);