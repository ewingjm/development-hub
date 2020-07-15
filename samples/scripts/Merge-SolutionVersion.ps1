param (
    [Parameter()]
    [String]
    $ClientId,
    [Parameter()]
    [String]
    $TenantId,
    [Parameter()]
    [SecureString]
    $ClientSecret,
    [Parameter()]
    [String]
    $SolutionVersionId,
    [Parameter()]
    [String]
    $Solution,
    [Parameter()]
    [String]
    $CommitUserEmailAddress,
    [Parameter()]
    [String]
    $CommitUserName,
    [Parameter()]
    [String]
    $CommitMessage,
    [Parameter()]
    [String]
    $SourceBranch,
    [Parameter()]
    [String]
    $WorkItemId
)

Install-Module ADAL.PS -Scope CurrentUser -Force

nuget install  Microsoft.CrmSdk.CoreTools
$solutionPackager = Get-ChildItem -Filter "SolutionPackager.exe" -Path packages -Recurse

$env:GIT_REDIRECT_STDERR = '2>&1'
git config --global user.email $CommitUserEmailAddress
git config --global user.name $CommitUserName
git checkout master
if ($SourceBranch)
{
  git merge origin/$SourceBranch --squash --no-commit;
}

$solutionFolder = Get-ChildItem -Filter $Solution -Path "./src/solutions" -Directory -Recurse
$solutionConfigurationPath = Join-Path -Path $solutionFolder.FullName -ChildPath "solution.json"
$solutionConfiguration = Get-Content -Raw -Path $solutionConfigurationPath | ConvertFrom-Json
$developmentUrl = $solutionConfiguration.environment

$tokenResponse = Get-AdalToken -Resource $developmentUrl -ClientId $ClientId -Authority "https://login.microsoftonline.com/$TenantId" -ClientSecret $ClientSecret -Verbose
$token = $tokenResponse.AccessToken
$webApiHeaders = @{
    "method"        = "GET"
    "authorization" = "Bearer $token"
}
$solutionVersionUrl = "$developmentUrl/api/data/v9.1/devhub_solutionversions($SolutionVersionId)"

$unmanagedSolutionResponse = Invoke-WebRequest -Uri "$solutionVersionUrl/devhub_unmanagedsolutionzip/`$value" -Headers $webApiHeaders
$managedSolutionResponse = Invoke-WebRequest -Uri "$solutionVersionUrl/devhub_managedsolutionzip/`$value" -Headers $webApiHeaders
$unmanagedZip = $unmanagedSolutionResponse.Content
$managedZip = $managedSolutionResponse.Content
$unmanagedZipFilePath = "$env:TEMP/$Solution.zip"
$managedZipFilePath = "$env:TEMP/$Solution" + "_managed.zip"

[IO.File]::WriteAllBytes($unmanagedZipFilePath, $unmanagedZip);
[IO.File]::WriteAllBytes($managedZipFilePath, $managedZip);

$extractFolder = Join-Path -Path $solutionFolder.FullName -ChildPath "extract"
$solutionPackagerPath = $solutionPackager.FullName
& $solutionPackagerPath /action:Extract /zipfile:$unmanagedZipFilePath /folder:$extractFolder /packagetype:Both /allowWrite:Yes /allowDelete:Yes

git add .;
git reset -- NuGet.config;
if ($WorkItemId) 
{
  $CommitMessage += " #$WorkItemId"
}
git commit -m $CommitMessage;
git push origin;