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
  $SolutionMergeId,
  [Parameter()]
  [String]
  $DevEnvironmentUrl
)

Install-Module ADAL.PS -Scope CurrentUser -Force

Write-Host "Installing solution packager."

$coreToolsPath = nuget install  Microsoft.CrmSdk.CoreTools -o (Join-Path $env:TEMP -ChildPath packages) | Where-Object { $_ -like "*Installing package *'Microsoft.CrmSdk.CoreTools' to '*'." } | Select-String -Pattern "to '(.*)'" | ForEach-Object { $_.Matches[0].Groups[1].Value } 
$solutionPackager = Get-ChildItem -Filter "SolutionPackager.exe" -Path $coreToolsPath -Recurse

function Get-WebApiHeaders ($url, $clientId, $tenantId, $clientSecret) {
  Write-Host "Getting access token for $url for client ID $clientId."
  $tokenResponse = Get-AdalToken -Resource $url -ClientId $clientId -Authority "https://login.microsoftonline.com/$tenantId" -ClientSecret $clientSecret -Verbose
  $token = $tokenResponse.AccessToken
  
  return @{
    "method"        = "GET"
    "authorization" = "Bearer $token"
    "content-type"  = "application/json"
  }
}

Write-Host "Authenticating to development environment."
$devWebApiHeaders = Get-WebApiHeaders -url $DevEnvironmentUrl -clientId $ClientId -tenantId $TenantId -clientSecret $ClientSecret
$devWebApiUrl = "$DevEnvironmentUrl/api/data/v9.1"

Write-Host "Getting solution merge $SolutionMergeId."
$select = 'devhub_sourcebranch,statuscode'
$expand = 'createdby($select=fullname,internalemailaddress),devhub_TargetSolution($select=devhub_uniquename;$expand=devhub_StagingEnvironment($select=devhub_url),devhub_Repository($select=devhub_sourcecontrolstrategy,devhub_extractbuilddefinitionid,devhub_targetbranch)),devhub_Issue($select=devhub_type,devhub_name,devhub_azuredevopsworkitemid,devhub_developmentsolution)'
$solutionMerge = Invoke-RestMethod -Uri "$devWebApiUrl/devhub_solutionmerges($SolutionMergeId)?`$select=$select&`$expand=$expand" -Headers $devWebApiHeaders

Write-Host "Setting git user configuration to solution merge creator: $($solutionMerge.createdby.internalemailaddress)."
git config --global user.email $solutionMerge.createdby.internalemailaddress
git config --global user.name $solutionMerge.createdby.fullname

Write-Host "Checking source control strategy."
if ($solutionMerge.devhub_TargetSolution.devhub_Repository.devhub_sourcecontrolstrategy -eq 353400000) {
  Write-Host "Source control strategy is pull request."
  if ($solutionMerge.devhub_sourcebranch) {
    Write-Host "Source branch provided. Checking out $($solutionMerge.devhub_sourcebranch)."
    $updateExistingBranch = $true
    git checkout "$($solutionMerge.devhub_sourcebranch)"
  }
  else {
    Write-Host "No source branch provided. Calculating branch name from solution merge issue."
    $branchPrefix = if ($solutionMerge.devhub_Issue.devhub_type -eq 353400001) { "feature/" } else { "bugfix/" } 
    $branchName = $solutionMerge.devhub_Issue.devhub_name.ToLower().Replace(' ', '-') -replace "[^a-zA-Z0-9\s-]"
    $calculatedBranch = "$branchPrefix$branchName"

    Write-Host "Checking if $calculatedBranch exists."
    $updateExistingBranch = $null -ne (git rev-parse --verify --quiet "origin/$calculatedBranch")
    if ($updateExistingBranch) {
      Write-Host "Branch already exists. Updating existing branch at $calculatedBranch."
      git checkout "$calculatedBranch" 
    }
    else {
      Write-Host "Branch not found. Creating branch $calculatedBranch."
      git checkout -b "$calculatedBranch" 
    }
  }
}
else {
  Write-Host "Source control strategy is push. Checking out $($solutionMerge.devhub_TargetSolution.devhub_Repository.devhub_targetbranch)"
  git checkout $solutionMerge.devhub_TargetSolution.devhub_Repository.devhub_targetbranch
  if ($SourceBranch) {
    Write-Host "Source branch provided. Squashing $SourceBranch."
    git merge origin/$SourceBranch --squash --no-commit;
  }
  $result = git merge HEAD
  if ($result[0] -like "*error*") {
    Write-Error "Unable to automatically merge the source branch due to a conflict."
  }
}

Write-Host "Authenticating to extract environment."
$extractUrl = $solutionMerge.devhub_TargetSolution.devhub_StagingEnvironment.devhub_url
$extractWebApiHeaders = Get-WebApiHeaders -url $extractUrl -clientId $ClientId -tenantId $TenantId -clientSecret $ClientSecret
$extractWebApiUrl = "$($extractUrl)/api/data/v9.1"

Write-Host "Exporting $($solutionMerge.devhub_TargetSolution.devhub_uniquename) as unmanaged."
$unmanagedZipResponse = Invoke-RestMethod -Uri "$($extractWebApiUrl)/ExportSolution" `
  -Method POST `
  -Headers $extractWebApiHeaders `
  -UseBasicParsing `
  -Body (ConvertTo-Json @{ SolutionName = $solutionMerge.devhub_TargetSolution.devhub_uniquename; Managed = $false }) 

$unmanagedZipFilePath = Join-Path -Path $env:TEMP -ChildPath "$($solutionMerge.devhub_TargetSolution.devhub_uniquename).zip"
Write-Host "Writing unmanaged solution to $unmanagedZipFilePath."
[IO.File]::WriteAllBytes($unmanagedZipFilePath, [Convert]::FromBase64String($unmanagedZipResponse.ExportSolutionFile));

Write-Host "Exporting $($solutionMerge.devhub_TargetSolution.devhub_uniquename) as managed."
$managedZipResponse = Invoke-RestMethod `
  -Uri "$extractWebApiUrl/ExportSolution" `
  -Method POST `
  -Headers $extractWebApiHeaders `
  -UseBasicParsing `
  -Body (ConvertTo-Json @{ SolutionName = $solutionMerge.devhub_TargetSolution.devhub_uniquename; Managed = $true })

$managedZipFilePath = Join-Path -Path $env:TEMP -ChildPath "$($solutionMerge.devhub_TargetSolution.devhub_uniquename)_managed.zip"
Write-Host "Writing managed solution to $managedZipFilePath."
[IO.File]::WriteAllBytes($managedZipFilePath, [Convert]::FromBase64String($managedZipResponse.ExportSolutionFile));

$solutionFolder = Get-ChildItem -Filter $solutionMerge.devhub_TargetSolution.devhub_uniquename -Path "./src/solutions" -Directory
$extractFolder = Join-Path -Path $solutionFolder.FullName -ChildPath "extract"
Write-Host "Extracting solutions with the Solution Packager to $extractFolder."
$solutionPackagerPath = $solutionPackager.FullName
& $solutionPackagerPath /action:Extract /zipfile:$unmanagedZipFilePath /folder:$extractFolder /packagetype:Both /allowWrite:Yes /allowDelete:Yes

git add .
git reset -- NuGet.config

Write-Host "Calculating commit message from solution merge issue."
$commitPrefix = if ($solutionMerge.devhub_Issue.devhub_type -eq 353400001) { "feat: " } else { "fix: " }
$commitMessage = $solutionMerge.devhub_Issue.devhub_name
$buildNumber = $commitMessage -replace "[^a-zA-Z0-9\s]"
Write-Host "##vso[build.updatebuildnumber]$buildNumber"

$commitTrailers = @"
Solution-merge-id: $SolutionMergeId
Solution-merge-creator: $($solutionMerge.createdby.fullname) <$($solutionMerge.createdby.internalemailaddress)>
"@

if ($solutionMerge.devhub_Issue.devhub_azuredevopsworkitemid) {
  Write-Host "Committing '$commitPrefix$commitMessage' with work item $($solutionMerge.devhub_Issue.devhub_azuredevopsworkitemid)."
  git commit -m "$commitPrefix$commitMessage" -m "#$($solutionMerge.devhub_Issue.devhub_azuredevopsworkitemid)" -m "$commitTrailers";
}
else {
  Write-Host "Committing '$commitPrefix$commitMessage'."
  git commit -m "$commitPrefix$commitMessage" -m "$commitTrailers"
}

if ($solutionMerge.devhub_TargetSolution.devhub_Repository.devhub_sourcecontrolstrategy -eq 353400000) {
  $remoteOrigin = [Uri]::new((git config --get remote.origin.url))
  if ($remoteOrigin.Host -eq 'dev.azure.com') {
    $org = $remoteOrigin.UserInfo
    $project = $remoteOrigin.Segments[2].Replace('/', '')
    $repository = $remoteOrigin.Segments[4]  
  }
  else {
    $org = $remoteOrigin.Host.Split('.')[0]
    $project = $remoteOrigin.Segments[1].Replace('/', '')
    $repository = $remoteOrigin.Segments[3]  
  }

  $sourceBranch = if ($solutionMerge.devhub_sourcebranch) { $solutionMerge.devhub_sourcebranch } else { $calculatedBranch }
  Write-Host "Checking for existing pull request"
  $result = Invoke-RestMethod `
    -Uri "https://dev.azure.com/$org/$project/_apis/git/repositories/$repository/pullRequests?searchCriteria.sourceRefName=refs/heads/$sourceBranch&searchCriteria.targetRefName=refs/heads/$($solutionMerge.devhub_TargetSolution.devhub_Repository.devhub_targetbranch)&api-version=6.0" `
    -Headers @{ 'authorization' = "Bearer $env:SYSTEM_ACCESSTOKEN"; 'content-type' = 'application/json' } 

  if ($result.value.Count -eq 0) {
    Write-Host "Publishing pull request branch."
    git push -u origin HEAD
    
    Write-Host "Creating pull request from refs/heads/$sourceBranch into refs/heads/$($solutionMerge.devhub_TargetSolution.devhub_Repository.devhub_targetbranch)."
    $result = Invoke-RestMethod `
      -Uri "https://dev.azure.com/$org/$project/_apis/git/repositories/$repository/pullRequests?api-version=6.0" `
      -Method POST `
      -Headers @{ 'authorization' = "Bearer $env:SYSTEM_ACCESSTOKEN"; 'content-type' = 'application/json' } `
      -UseBasicParsing `
      -Body (ConvertTo-Json `
      @{ 
        title         = "$commitPrefix$commitMessage"; 
        sourceRefName = "refs/heads/$sourceBranch"; 
        targetRefName = "refs/heads/$($solutionMerge.devhub_TargetSolution.devhub_Repository.devhub_targetbranch)";
        description   = @"

$commitTrailers
"@
      })

    if ($solutionMerge.devhub_Issue.devhub_azuredevopsworkitemid) {
      Write-Host "Linking pull request to work item $($solutionMerge.devhub_Issue.devhub_azuredevopsworkitemid)"
      $result = Invoke-RestMethod `
        -Uri "https://dev.azure.com/$org/$project/_apis/wit/workItems/$($solutionMerge.devhub_Issue.devhub_azuredevopsworkitemid)?api-version=4.0-preview" `
        -Headers @{ 'authorization' = "Bearer $env:SYSTEM_ACCESSTOKEN"; 'content-type' = 'application/json-patch+json' } `
        -Method PATCH `
        -Body (ConvertTo-Json -Depth 100 @(
          @{
            op    = 'add';
            path  = '/relations/-';
            value = 
            @{
              rel        = "ArtifactLink";
              url        = $($result.artifactId)
              attributes = @{
                name = "Pull Request"
              }
            }
          }
        )
      )
    }
  }

  Write-Host "Updating solution merge status to 'Awaiting PR Approval'."
  $solutionMerge = Invoke-RestMethod `
    -Method PATCH `
    -Uri "$devWebApiUrl/devhub_solutionmerges($SolutionMergeId)" `
    -Headers $devWebApiHeaders `
    -Body (ConvertTo-Json @{
      statuscode = 353400007
    })
}

Write-Host "Pushing new commit."
git push origin