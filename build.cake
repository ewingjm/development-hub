using System.Text.RegularExpressions;

#addin nuget:?package=Cake.Xrm&version=0.6.0
#addin nuget:?package=Cake.Npm&version=0.16.0
#addin nuget:?package=Cake.Git&version=0.19.0

const string DataFolder = "./Data";
const string ModelProjectFolder = "Common\\Capgemini.DevelopmentHub.Model";
const string ModelNamespace = "Capgemini.DevelopmentHub.Model";

var target = Argument("target", "Default");
var solution = Argument<string>("solution", "");
var devConn = EnvironmentVariable("CAKE_CONN_DEV");
var stagingConn = EnvironmentVariable("CAKE_CONN_STAGING");

// Build package
Task("Default")
  .IsDependentOn("PackAll")
  .IsDependentOn("BuildTestProjects")
  .IsDependentOn("BuildDeploymentProject");

Task("BuildDeploymentProject")
  .Does(() => {
    BuildCSharpProject(
      File("Deploy/Capgemini.DevelopmentHub.Deployment.csproj"), 
      new NuGetRestoreSettings { ConfigFile = "NuGet.config" }, 
      new MSBuildSettings { Configuration = "Release" });
    DeleteFiles("Deploy/bin/Release/PkgFolder/Data/**/*");
    DeleteFiles("Deploy/bin/Release/PkgFolder/*.zip");
    EnsureDirectoryExists("Deploy/bin/Release/PkgFolder/Data");
    EnsureDirectoryExists("Deploy/bin/Release/PowerShell");
    CopyFiles("packages/Microsoft.CrmSdk.XrmTooling.PackageDeployment.Wpf.*/tools/**/*", Directory("Deploy/bin/Release"), true);
    CopyFiles("packages/Microsoft.CrmSdk.XrmTooling.PackageDeployment.PowerShell.*/tools/**/*", Directory("Deploy/bin/Release/PowerShell"));
    CopyFiles("Solutions/**/*.zip", Directory("Deploy/bin/Release/PkgFolder"));
    CopyDirectory(Directory("Data"), Directory("Deploy/bin/Release/PkgFolder/Data"));
  });

Task("PackAll")
  .Does(() => {
    var solutionDirectories = GetDirectories("Solutions/*");
    foreach (var solutionDirectory in solutionDirectories)
    {
      solution = solutionDirectory.GetDirectoryName();
      RunTarget("PackSolution");
    }
  });

Task("GenerateModel")
  .Does(() => {
    GenerateModel(Directory(ModelProjectFolder).Path.CombineWithFilePath("spkl.json"), devConn);
  });

Task("ExtractSolution")
  .Does(() => {
    ExtractSolution(stagingConn, solution, Directory($"Solutions/{solution}").Path.Combine("Extract"), SolutionPackageType.Both);
  });

// build targets 
Task("BuildTestProjects")
  .Does(() => {
    var folderPrefix = "Capgemini.DevelopmentHub.Tests.";
    var nugetSettings = new NuGetRestoreSettings { ConfigFile = "NuGet.config" };
    var testProjects = new string[] { "Unit", "Integration", "Ui" }.Select(
      testType => Directory($"Tests\\{folderPrefix}{testType}").Path.CombineWithFilePath($"{folderPrefix}{testType}.csproj"));

    foreach (var testProject in testProjects) 
    {
      BuildCSharpProject(testProject, nugetSettings, new MSBuildSettings { Configuration = "Debug" });
    }
  });

Task("BuildSolution")
  .DoesForEach(
    GetFiles($"{Directory($"Solutions/{solution}")}/**/package.json",  new GlobberSettings { Predicate = (fileSystemInfo) => !fileSystemInfo.Path.FullPath.Contains("node_modules") }), 
    (packageFile) => {
      var directory = packageFile.GetDirectory();
      NpmInstall(new NpmInstallSettings { WorkingDirectory = directory });
      NpmRunScript(new NpmRunScriptSettings { ScriptName = "build", WorkingDirectory = directory, });
  })
  .DoesForEach(
    GetFiles($"{Directory($"Solutions/{solution}")}/**/*.csproj"),
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
  .Does(() => {
    PackSolution($"Solutions/{solution}", solution, Argument<string>("solutionVersion", ""));
  });

// data targets 
Task("ExportData")
  .Does(() => {
    var dataType = Argument<string>("dataType");
    ExportData(
      Directory(DataFolder).Path.Combine($"{dataType}\\Extract"),
      Directory(DataFolder).Path.MakeAbsolute(Context.Environment).CombineWithFilePath($"{dataType}\\{dataType}DataExport.json"));
  });

Task("StageData")
  .Does(() => {
    var dataType = Argument<string>("dataType");
    XrmImportData(
      new DataMigrationEngineImportSettings(
        stagingConn, 
        Directory(DataFolder).Path.MakeAbsolute(Context.Environment).CombineWithFilePath($"{dataType}\\{dataType}DataImport.json")));
  });
  
// deploy targets
Task("DeployPlugins")
  .DoesForEach(
    GetFiles($"{Directory($"Solutions/{solution}")}/**/*.csproj"),
    (msBuildProject) => {
      BuildCSharpProject(
        msBuildProject, 
        new NuGetRestoreSettings { ConfigFile = "NuGet.config" }, 
        new MSBuildSettings { Configuration = "Deploy" });
    }
  )
  .Does(() => {
    DeployPlugins(Directory($"Solutions/{solution}").Path.CombineWithFilePath("spkl.json"), devConn);
});

Task("DeployWorkflowActivities")
  .DoesForEach(
    GetFiles($"{Directory($"Solutions/{solution}")}/**/*.csproj"),
    (msBuildProject) => {
      BuildCSharpProject(
        msBuildProject, 
        new NuGetRestoreSettings { ConfigFile = "NuGet.config" }, 
        new MSBuildSettings { Configuration = "Deploy" });
    }
  )
  .Does(() => {
    DeployWorkflows(Directory($"Solutions/{solution}").Path.CombineWithFilePath("spkl.json"), devConn);
});

void BuildCSharpProject(FilePath projectPath, NuGetRestoreSettings nugetSettings, MSBuildSettings msBuildSettings = null) { 
    NuGetRestore(projectPath, nugetSettings);
    MSBuild(projectPath, msBuildSettings);
}

// Utilities
FilePath GetEarlyBoundGeneratorConfig() {
    var configurationFiles = GetFiles($"{ModelProjectFolder}\\DLab.EarlyBoundGenerator.*.xml");
    if(!configurationFiles.Any()){
      Error("No configuration file found in the model project directory.");
    }
    else if(configurationFiles.Count > 1){
      Warning("Multiple configuration files found in model project directory.");
    }
    return configurationFiles.First();
}

void ExportData(DirectoryPath extractFolder, FilePath exportConfigPath) {
  DeleteFiles($"{extractFolder}/**/*");
  XrmExportData(new DataMigrationEngineExportSettings(devConn, exportConfigPath));
}

void PackSolution(string projectFolder, string solutionName, string solutionVersion) {
  var changedSolutions = Argument<string>("solutions", "");
  if (!String.IsNullOrEmpty(solutionVersion) && !String.IsNullOrEmpty(changedSolutions) && changedSolutions.Contains(solutionName)) {
    var versionParts = solutionVersion.Split('.');
    versionParts[2] = (int.Parse(versionParts[2]) + 1).ToString();
    SetSolutionVersion(stagingConn, solutionName, String.Join(".", versionParts));
  }
    
  PackSolution(new SolutionPackagerPackSettings(
    Directory(projectFolder).Path.CombineWithFilePath($"bin\\Release\\{solutionName}.zip"),
    Directory(projectFolder).Path.Combine("Extract"),
    SolutionPackageType.Both,
    Directory(projectFolder).Path.CombineWithFilePath("MappingFile.xml")));
}

RunTarget(target);