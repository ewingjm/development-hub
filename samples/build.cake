#addin nuget:?package=Cake.Xrm.Sdk&version=0.1.9
#addin nuget:?package=Cake.Xrm.SolutionPackager&version=0.1.9

const string SolutionsFolder = "./solutions";

var target = Argument("target", "Default");
var solution = Argument<string>("solution", "");

Task("ExtractSolutionFromDevelopmentHub")
  .Does(() => {
    var connectionString = GetConnectionString(solution, false);
    var tempDirectory = Directory(EnvironmentVariable("TEMP"));
    var unmanagedSolution = XrmDownloadAttachment(connectionString, Guid.Parse(Argument<string>("unmanagedNoteId")), tempDirectory);
    var managedSolution = XrmDownloadAttachment(connectionString, Guid.Parse(Argument<string>("managedNoteId")), tempDirectory);
    var outputPath = Directory($"{SolutionsFolder}/{solution}").Path.Combine("Extract");
    
    SolutionPackagerExtract(unmanagedSolution, outputPath, SolutionPackageType.Both);
  });

string GetConnectionString(string solution, bool stagingEnvironment) {
  var envConfig = ParseJsonFromFile(File($"{SolutionsFolder}/{solution}/env.json"));
  var targetEnvironment = stagingEnvironment && envConfig["stagingEnvironment"] != null ? "stagingEnvironment" : "environment";
  var url = envConfig[targetEnvironment].ToString();
  var username = envConfig["username"] ?? EnvironmentVariable("CAKE_DYNAMICS_USERNAME");
  var password = EnvironmentVariable("CAKE_DYNAMICS_PASSWORD");

  return $"Url={url}; Username={username}; Password={password}; AuthType=Office365;";
}

RunTarget(target);