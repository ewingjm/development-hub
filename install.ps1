$ErrorActionPreference = 'Stop'

Write-Host "Checking for missing dependencies." -ForegroundColor Blue

if (!(Get-Module -ListAvailable -Name Microsoft.PowerApps.Administration.PowerShell)) {
    Install-Module -Name Microsoft.PowerApps.Administration.PowerShell -Scope CurrentUser -Force -AllowClobber
}
if (!(Get-Module -ListAvailable -Name Microsoft.Xrm.Data.Powershell)) {
    Install-Module -Name Microsoft.Xrm.Data.Powershell -Scope CurrentUser -Force -AllowClobber
}
if (!(Get-Module -ListAvailable -Name Microsoft.Graph)) {
    Install-Module -Name Microsoft.Graph -Scope CurrentUser -Force -AllowClobber
}

while ($true) {
    $url = Read-Host -Prompt "What is the URL of your developent environment? e.g. https://<environment>.<region>.dynamics.com"
    if ([uri]::IsWellFormedUriString($url, 'Absolute') -and ([uri] $url).Scheme -eq 'https') { break }

    Write-Host "Invalid environment URL." -ForegroundColor Red
}

Write-Host "`nPlease sign-in to the account you use to access this environment." -ForegroundColor Blue
Add-PowerAppsAccount

Write-Host "`nGetting environment details for $url."
$environment = Get-AdminPowerAppEnvironment | Where-Object { 
    $_.Internal.properties.linkedEnvironmentMetadata.instanceUrl -like "$url*" 
}

if (!$environment) {
    throw "Unable to find environment."
}

Write-Host "`nChecking $url for existing cloud flow connections."

$connections = $environment | Get-AdminPowerAppConnection

$approvalsConnections = $connections | Where-Object { $_.ConnectorName -eq "shared_approvals" }
$approvalsConnection = $null
foreach ($connection in $approvalsConnections) {
    $response = $Host.UI.PromptForChoice(
        "Existing connection found", 
        "Do you want to use existing Approvals connection?`nhttps://make.powerapps.com/environments/$($environment.EnvironmentName)/connections/shared_approvals/$($connection.ConnectionName)/details", 
        @("&Yes", "&No"), 
        0)

        
    if ($response -eq 0) {
        $approvalsConnection = $connection.ConnectionName
        break
    }
}

if (!$approvalsConnection) {
    Write-Host "`nPlease create an Approvals connection at https://make.powerapps.com/environments/$($Environment.EnvironmentName)/connections." -ForegroundColor Blue

    while ($true) {
        $approvalsUrl = Read-Host -Prompt  "What is the URL for your approvals connection? e.g. https://make.powerapps.com/environments/<environment>/connections/shared_approvals/<connectionname>/details.`nFind this by clicking on the details button for the connection."
        if ($approvalsUrl -match "connections\/shared_approvals\/(?<Connection>.+)\/details") {
            $approvalsConnection = $Matches.Connection
            break
        }

        Write-Host "Invalid Approvals connection URL." -ForegroundColor Red
    }
}

$azureDevOpsConnections = $connections | Where-Object { $_.ConnectorName -eq "shared_visualstudioteamservices" }
$azureDevOpsConnection = $null
foreach ($connection in $azureDevOpsConnections) {
    $response = $Host.UI.PromptForChoice(
        "Existing connection found", 
        "Do you want to use existing Azure DevOps connection?`nhttps://make.powerapps.com/environments/$($environment.EnvironmentName)/connections/shared_visualstudioteamservices/$($connection.ConnectionName)/details", 
        @("&Yes", "&No"), 
        0)

        
    if ($response -eq 0) {
        $azureDevOpsConnection = $connection.ConnectionName
        break
    }
}

if (!$azureDevOpsConnection) {
    Write-Host "`nPlease create an Azure DevOps connection at https://make.powerapps.com/environments/$($Environment.EnvironmentName)/connections." -ForegroundColor Blue

    while ($true) {
        $azureDevOpsUrl = Read-Host -Prompt  "What is the URL for your Azure DevOps connection? e.g. https://make.powerapps.com/environments/<environment>/connections/shared_visualstudioteamservices/<connectionname>/details"
        if ($azureDevOpsUrl -match "connections\/shared_visualstudioteamservices\/(?<Connection>.+)\/details") {
            $azureDevOpsConnection = $Matches.Connection
            break
        }

        Write-Host "Invalid Approvals connection URL." -ForegroundColor Red
    }
}

Write-Host "`nGetting Dataverse environment variable values" -ForegroundColor Blue

$conn = Connect-CrmOnline -ServerUrl $url -ForceOAuth

try {
    $solutionPublisherPrefix = (Invoke-CrmAction -conn $conn -Name RetrieveEnvironmentVariableValue -Parameters @{ DefinitionSchemaName = "devhub_SolutionPublisher" }).Value

    if ($solutionPublisherPrefix) {
        $response = $Host.UI.PromptForChoice(
            "Existing environment variable found", 
            "Do you want to use existing solution publisher environment variable value of '$($solutionPublisherPrefix)'?", 
            @("&Yes", "&No"), 
            0)
        
        if ($response -eq 1) {
            throw "New value required"
        }
    }
}
catch {
    $solutionPublisherPrefix = Read-Host "`nWhat is you solution publisher prefix (exluding the trailing underscore)?`nThis can be updated later in the devhub_SolutionPublisher Dataverse environment variable"
}

try {
    $azureDevOpsOrg = (Invoke-CrmAction -conn $conn -Name RetrieveEnvironmentVariableValue -Parameters @{ DefinitionSchemaName = "devhub_AzureDevOpsOrganization" }).Value

    if ($azureDevOpsOrg) {
        $response = $Host.UI.PromptForChoice(
            "Existing environment variable found", 
            "Do you want to use existing Azure DevOps organisation environment variable value of '$($azureDevOpsOrg)'?", 
            @("&Yes", "&No"), 
            0)

        if ($response -eq 1) {
            throw "New value required"
        }
    }
}
catch {
    $azureDevOpsOrg = Read-Host -Prompt "`nWhat is the name of your Azure DevOps organisation?`nThis can be updated later in the devhub_AzureDevOpsOrganization Dataverse environment variable"
}

Write-Host "`nDownloading latest Development Hub release to temp directory." -ForegroundColor Blue

$tempPath = Join-Path $env:TEMP ([Guid]::NewGuid())
New-Item -ItemType Directory -Path $tempPath | Out-Null
$packageZipPath = Join-Path $tempPath DevelopmentHub.zip

Invoke-WebRequest "https://github.com/ewingjm/development-hub/releases/download/v0.2.30/Development.Hub.v0.2.30.zip" -OutFile $packageZipPath

Write-Host "`nExtracting package from release zip." -ForegroundColor Blue

$releasePath = Join-Path $tempPath Release
Expand-Archive $packageZipPath -DestinationPath $releasePath
Write-Host "`nDeploying package." -ForegroundColor Blue
$settings = @{
    "ConnRef:devhub_sharedapprovals_6d3fc"                = $approvalsConnection
    "ConnRef:devhub_sharedvisualstudioteamservices_d7fcb" = $azureDevOpsConnection
    "AzureDevOpsOrganisation"                             = $azureDevOpsOrg
    "SolutionPublisherPrefix"                             = $solutionPublisherPrefix
}
$settingsArray = $settings.Keys | ForEach-Object { "$_=$($settings[$_])" }
$runtimePackageSettings = [string]::Join("|", $settingsArray)

# Running the cmdlets in the Package Deployer PS module causes unrelated things to break and retains locks on the package template files. Quarantining inside a separate job.
$importJob = Start-Job { 
    if (!(Get-Module -ListAvailable -Name Microsoft.Xrm.Tooling.PackageDeployment.Powershell)) {
        Install-Module -Name Microsoft.Xrm.Tooling.PackageDeployment.Powershell -Scope CurrentUser -Force -AllowClobber
    }

    $conn = Connect-CrmOnline -ServerUrl $using:url -ForceOAuth
    Import-CrmPackage -PackageDirectory (Join-Path $using:releasePath "Development Hub") -PackageName DevelopmentHub.Deployment.dll -CrmConnection $conn -RuntimePackageSettings $using:runtimePackageSettings
}

$jobResult = Wait-Job $importJob
if ($jobResult.State -eq "Failed") {
    Receive-Job $importJob -Wait -AutoRemoveJob
    return;
}

Remove-Job $importJob

Remove-Item $tempPath -Recurse -Force

Write-Host "`nPackage deployment complete." -ForegroundColor Blue

Write-Host "`nChecking for existing app registration." -ForegroundColor Blue

Connect-MgGraph -Scopes "Application.ReadWrite.All"
$applications = Get-MgApplication | Where-Object { $_.Tags -contains "Development Hub" -or $_.DisplayName -like "*Development Hub*" }
$devHubApp = $null
foreach ($application in $applications) {
    $response = $Host.UI.PromptForChoice(
        "Existing application found", 
        "Do you want to use this application?`n$($application.DisplayName) - $($application.AppId)", 
        @("&Yes", "&No"), 
        0)
        
    if ($response -eq 0) {
        $devHubApp = $application
        break
    }
}

if (!$devHubApp) {
    Write-Host "`nCreating new app registration." -ForegroundColor Blue
    [Microsoft.Graph.PowerShell.Models.IMicrosoftGraphRequiredResourceAccess[]]$requiredResourceAccess = @(
        [Microsoft.Graph.PowerShell.Models.IMicrosoftGraphRequiredResourceAccess]@{
            ResourceAppId  = "00000007-0000-0000-c000-000000000000"
            ResourceAccess = [Microsoft.Graph.PowerShell.Models.MicrosoftGraphResourceAccess[]]@(
                [Microsoft.Graph.PowerShell.Models.MicrosoftGraphResourceAccess]@{
                    Id   = "78ce3f0f-a1ce-49c2-8cde-64b5c0896db4"
                    Type = "Scope"
                }
            )
        }
    )

    $devHubApp = New-MgApplication -DisplayName "Development Hub" -SignInAudience AzureADMyOrg -Tags "Development Hub" -RequiredResourceAccess $requiredResourceAccess
    New-MgServicePrincipal -BodyParameter @{ AppId = $devHubApp.AppId } | Out-Null
}

$devHubAppPwd = Add-MgApplicationPassword -ApplicationId $devHubApp.Id -PasswordCredential @{ displayName = "Created by Development Hub install script." }

Write-Host "`nUsing app registration '$($devHubApp.DisplayName)' with app ID '$($devHubApp.AppId)'." -ForegroundColor Blue

Write-Host "`nRegistering app registration as a management app." -ForegroundColor Blue
New-PowerAppManagementApp -ApplicationId $devHubApp.AppId | Out-Null

Write-Host "`nSetting up application user in development environment." -ForegroundColor Blue
$systemUser = (Get-CrmRecords -conn $conn -EntityLogicalName systemuser -FilterAttribute applicationid -FilterOperator eq -FilterValue $devHubApp.AppId -TopCount 1).CrmRecords[0]
if (!$systemUser) {
    Write-Host "`nExisting application user not found. Creating new application user in development environment." -ForegroundColor Blue
    $rootBusinessUnit = (Get-CrmRecords -conn $conn -EntityLogicalName businessunit -FilterAttribute parentbusinessunitid -FilterOperator null -Fields businessunitid -TopCount 1).CrmRecords[0]

    while (!$systemUser) {
        try {
            $systemUserId = New-CrmRecord -conn $conn -EntityLogicalName systemuser -Fields @{ applicationid = [Guid]::Parse($devHubApp.AppId); businessunitid = $rootBusinessUnit.EntityReference }
            $systemUser = Get-CrmRecord -conn $conn -EntityLogicalName systemuser -Id $systemUserId -Fields systemuserid
        }
        catch {
            if ($_.Exception.Message -notlike "*Make sure your application is registered in Azure AD*") {
                throw
            }
            Start-Sleep -Seconds 10
        }
    }
}
else {
    Write-Host "`nUsing existing application user in development environment." -ForegroundColor Blue
}

$systemAdmin = (Get-CrmRecords -conn $conn -EntityLogicalName role -FilterAttribute roletemplateid -FilterOperator eq -FilterValue "627090ff-40a3-4053-8790-584edc5be201" -Fields roleid -TopCount 1).CrmRecords[0]

try {
    Write-Host "`nAssigning system administrator role to application user in development environment." -ForegroundColor Blue
    Add-CrmSecurityRoleToUser -conn $conn -UserRecord $systemUser -SecurityRoleRecord $systemAdmin
}
catch {
    if ($_.Exception.Message -notlike "*duplicate key*") {
        throw
    }
}

Write-Host "`nConfiguring Azure DevOps." -ForegroundColor Blue

$orgUrl = "https://dev.azure.com/$azureDevOpsOrg"
$headers = @{
    "Accept"       = "application/json; api-version=7.0"
    "Content-Type" = "application/json"
}

while ($true) {
    $pat = Read-Host -Prompt "Please provide a PAT token for Azure DevOps with the following scopes:`n`tVariable Groups: Read, create, & manage`n`tService Connections: Read, query, & manage`n`tExtensions: Read & manage`n`tCode: Read, write & manage`n`tBuild: Read & execute`n" -AsSecureString
    
    $token = [System.Convert]::ToBase64String(
        [System.Text.Encoding]::ASCII.GetBytes(
            ":$([Runtime.InteropServices.Marshal]::PtrToStringAuto(
                [Runtime.InteropServices.Marshal]::SecureStringToBSTR($pat)))"))
    $headers.authorization = "Basic $token"
    
    $response = Invoke-WebRequest -Headers $headers -Uri "$orgUrl/_apis/projects"
    if ($response.StatusCode -eq 200) {
        $projects = $response.Content | ConvertFrom-Json | Select-Object -ExpandProperty value
        break
    }

    Write-Host "Invalid PAT." -ForegroundColor Red
}

$extMgmtUrl = "https://extmgmt.dev.azure.com/$azureDevOpsOrg"
$extMgmtHeaders = $headers.Clone()
$extMgmtHeaders.accept = "application/json; api-version=7.0-preview"

Write-Host "`nChecking for Power Platform Build Tools extension." -ForegroundColor Blue
try { 
    Invoke-RestMethod -Headers $extMgmtHeaders -Uri "$extMgmtUrl/_apis/extensionmanagement/installedextensionsbyname/microsoft-IsvExpTools/PowerPlatform-BuildTools" | Out-Null
}
catch [System.Net.WebException] { 
    if (!$_.Exception.Response.StatusCode.Value__ -eq 404) {
        throw
    }
    Write-Host "`nNot installed. Installing Power Platform Build Tools extension." -ForegroundColor Blue
    Invoke-RestMethod -Method POST -Headers $extMgmtHeaders -Uri "$extMgmtUrl/_apis/extensionmanagement/installedextensionsbyname/microsoft-IsvExpTools/PowerPlatform-BuildTools"
} 

while ($true) {
    $project = Read-Host -Prompt "What Azure DevOps project do you want to use?"
    
    if ($project -in $projects.name) {
        break
    }

    Write-Host "Project not found." -ForegroundColor Red
}

Write-Host "`nChecking for service connection." -ForegroundColor Blue

$projectReference = @{
    name = $project
    id   = ($projects | Where-Object name -eq $project | Select-Object -First 1 -ExpandProperty id)
}
$serviceConnection = Invoke-RestMethod -Headers $headers -Uri "$orgUrl/$project/_apis/serviceendpoint/endpoints?endpointNames=Development Hub" | Select-Object -ExpandProperty value
if (!$serviceConnection) {
    Write-Host "`nService connection not found. Creating service connection." -ForegroundColor Blue
    $serviceConnection = Invoke-RestMethod -Method POST -Headers $headers -Uri "$orgUrl/_apis/serviceendpoint/endpoints" -Body (ConvertTo-Json -Depth 5 @{
            name                             = "Development Hub"
            type                             = "powerplatform-spn"
            url                              = "https://placeholder.crm.dynamics.com"
            authorization                    = @{
                parameters = @{
                    tenantId      = $conn.TenantId.ToString()
                    applicationId = $devHubApp.AppId
                    clientSecret  = $devHubAppPwd.SecretText
                }
                scheme     = "None"
            }
            isShared                         = $false
            serviceEndpointProjectReferences = @(
                @{ 
                    name             = "Development Hub" 
                    projectReference = $projectReference
                }
            )
        })
}
else {
    Write-Host "`nUsing existing service connection. Updating secret." -ForegroundColor Blue
    $serviceConnection.authorization.parameters | Add-Member -MemberType NoteProperty -Name clientSecret -Value $devHubAppPwd.SecretText
    Invoke-RestMethod -Method PUT -Headers $headers -Uri "$orgUrl/_apis/serviceendpoint/endpoints/$($serviceConnection.id)" -Body (ConvertTo-Json -Depth 5 $serviceConnection) | Out-Null
}

Write-Host "`nChecking for variable group." -ForegroundColor Blue

$variableGroupVariables = @{
    "DevelopmentHub.Application.TenantId"     = @{
        value    = $conn.TenantId.ToString()
        isSecret = $false
    }
    "DevelopmentHub.Application.ClientId"     = @{
        value    = $devHubApp.AppId
        isSecret = $false
    }
    "DevelopmentHub.Application.ClientSecret" = @{
        value    = $devHubAppPwd.SecretText
        isSecret = $true
    }
}
$variableGroup = Invoke-RestMethod -Headers $headers -Uri "$orgUrl/$project/_apis/distributedtask/variablegroups?groupName=Development Hub" | Select-Object -ExpandProperty value -First 1
if (!$variableGroup) {
    Write-Host "`nVariable group not found. Creating variable group." -ForegroundColor Blue
    $variableGroup = Invoke-RestMethod -Method Post -Headers $headers -Uri "$orgUrl/$project/_apis/distributedtask/variablegroups" -Body (ConvertTo-Json -Depth 5 @{
            name                           = "Development Hub"    
            description                    = "Variables that support the Development Hub pipelines"
            type                           = "Vsts"
            variableGroupProjectReferences = @(
                @{
                    name             = "Development Hub"
                    projectReference = $projectReference
                }
            )
            variables                      = $variableGroupVariables
        })
}
else {
    Write-Host "`nUsing existing variable group. Updating values." -ForegroundColor Blue
    $variableGroup.variableGroupProjectReferences = @(
        @{
            name             = "Development Hub"
            projectReference = $projectReference
        }
    )
    $variableGroup.variables = $variableGroupVariables
    Invoke-RestMethod -Method Put -Headers $headers -Uri "$orgUrl/$project/_apis/distributedtask/variablegroups/$($variableGroup.id)" -Body (ConvertTo-Json -Depth 5 $variableGroup) | Out-Null
}

Write-Host "`nChecking for development-hub-pipelines repository." -ForegroundColor Blue
try {
    $pipelinesRepo = Invoke-RestMethod -Headers $headers -Uri "$orgUrl/$project/_apis/git/repositories/development-hub-pipelines"
    Write-Host "`nExisting repository found." -ForegroundColor Blue
}
catch [System.Net.WebException] { 
    if (!$_.Exception.Response.StatusCode.Value__ -eq 404) {
        throw
    }

    Write-Host "`nRepository not found. Creating development-hub-pipelines repository." -ForegroundColor Blue
    $pipelinesRepo = Invoke-RestMethod -Method Post -Headers $headers -Uri "$orgUrl/$project/_apis/git/repositories" -Body (ConvertTo-Json -Depth 5 @{
            name    = "development-hub-pipelines"
            project = $projectReference
        })

    Write-Host "`nImporting repository from github.com/ewingjm/development-hub-pipelines." -ForegroundColor Blue
    $importRequest = Invoke-RestMethod -Method Post -Headers $headers -Uri "$orgUrl/$project/_apis/git/repositories/development-hub-pipelines/importRequests" -Body (ConvertTo-Json -Depth 5 @{
            parameters = @{
                gitSource = @{
                    url = "https://github.com/ewingjm/development-hub-pipelines.git"
                }
            }
        })

    while ($true) {
        Write-Host "`nWaiting for repository import to complete..." -ForegroundColor Blue
        $importRequest = Invoke-RestMethod -Headers $headers -Uri "$orgUrl/$project/_apis/git/repositories/development-hub-pipelines/importRequests/$($importRequest.importRequestId)"

        if ($importRequest.status -eq "completed") {
            Write-Host "`nRepository import complete." -ForegroundColor Blue
            break
        }
        else {
            Start-Sleep -Seconds 10
        }
    }
}

Write-Host "`nChecking for existing Development Hub project configuration for '$project'." -ForegroundColor Blue
$devHubProject = (Get-CrmRecords -conn $conn -EntityLogicalName devhub_project -FilterAttribute devhub_name -FilterOperator eq -FilterValue $project -Fields devhub_deleteenvironmentpipelineid, devhub_mergepipelineid -TopCount 1).CrmRecords[0]
if (!$devHubProject) {
    Write-Host "`nDevelopment Hub project not found. Creating project record." -ForegroundColor Blue
    $devHubProjectId = New-CrmRecord -conn $conn -EntityLogicalName devhub_project -Fields @{
        devhub_name = $project
    }
    $devHubProject = Get-CrmRecord -conn $conn -EntityLogicalName devhub_project -Id $devHubProjectId -Fields devhub_name, devhub_deleteenvironmentpipelineid, devhub_mergepipelineid
}
else {
    Write-Host "`nUsing existing project." -ForegroundColor Blue
}

Write-Host "`nChecking for existing Development Hub pipeline configuration." -ForegroundColor Blue
$pipelines = Invoke-RestMethod -Headers $headers -Uri "$orgUrl/$project/_apis/pipelines" | Select-Object -ExpandProperty value
if (!$devHubProject.devhub_mergepipelineid) {
    Write-Host "`nExisting Development Hub project solution merge pipeline not configured. Checking for existing Azure DevOps pipeline." -ForegroundColor Blue
    $mergePipeline = $pipelines | Where-Object name -eq "development-hub.merge" | Select-Object -First 1
    
    if ($mergePipeline) {
        Write-Host "`nUpdating project using existing Azure DevOps pipeline '$($mergePipeline.name)'." -ForegroundColor Blue
    }
    else {
        Write-Host "`nExisting Azure DevOps pipeline not found. Creating new solution merge pipeline." -ForegroundColor Blue
        $mergePipeline = Invoke-RestMethod -Method Post -Headers $headers -Uri "$orgUrl/$project/_apis/pipelines" -Body (ConvertTo-Json -Depth 5 @{
                folder        = "development-hub"
                name          = "development-hub.merge"
                configuration = @{
                    type       = "yaml"
                    path       = "/azure-pipelines-merge.yml"
                    repository = @{
                        id   = $pipelinesRepo.id
                        name = "development-hub-pipelines"
                        type = "azureReposGit"
                    }
                }
            })
    }

    $devHubProject | Add-Member -MemberType NoteProperty -Name devhub_mergepipelineid -Value $mergePipeline.id.ToString()
    Update-CrmRecord -conn $conn -CrmRecord $devHubProject
}
else {
    Write-Host "`nUsing existing solution merge pipeline configured for project." -ForegroundColor Blue
}

if (!$devHubProject.devhub_deleteenvironmentpipelineid) {
    Write-Host "`nExisting Development Hub project environment deletion pipeline not configured. Checking for existing Azure DevOps pipeline." -ForegroundColor Blue
    $deleteEnvironmentPipeline = $pipelines | Where-Object name -eq "development-hub.delete-environment" | Select-Object -First 1
    
    if ($deleteEnvironmentPipeline) {
        Write-Host "`nUpdating project using existing Azure DevOps pipeline '$($deleteEnvironmentPipeline.name)'." -ForegroundColor Blue
    }
    else {
        Write-Host "`nExisting Azure DevOps pipeline not found. Creating new environment deletion pipeline." -ForegroundColor Blue
        $deleteEnvironmentPipeline = Invoke-RestMethod -Method Post -Headers $headers -Uri "$orgUrl/$project/_apis/pipelines" -Body (ConvertTo-Json -Depth 5 @{
                folder        = "development-hub"
                name          = "development-hub.delete-environment"
                configuration = @{
                    type       = "yaml"
                    path       = "/azure-pipelines-environment-delete.yml"
                    repository = @{
                        id   = $pipelinesRepo.id
                        name = "development-hub-pipelines"
                        type = "azureReposGit"
                    }
                }
            })
    }

    $devHubProject | Add-Member -MemberType NoteProperty -Name devhub_deleteenvironmentpipelineid -Value $deleteEnvironmentPipeline.id.ToString()
    Update-CrmRecord -conn $conn -CrmRecord $devHubProject
}
else {
    Write-Host "`nUsing existing environment deletion pipeline configured for project." -ForegroundColor Blue
}

Write-Host "`nGranting Azure DevOps pipelines permission to resources." -ForegroundColor Blue
$headers.Accept = $headers.Accept + "-preview"
$pipelines = @( 
    @{
        id         = $deleteEnvironmentPipeline.id
        authorized = $true
    },
    @{
        id         = $mergePipeline.id
        authorized = $true
    })
Invoke-RestMethod -Method Patch -Headers $headers -Uri "$orgUrl/$project/_apis/pipelines/pipelinepermissions" -Body (ConvertTo-json -Depth 5 @(
        @{
            resource  = @{
                type = "variablegroup"
                id   = $variableGroup.id
            }
            pipelines = $pipelines
        },
        @{
            resource  = @{
                type = "endpoint"
                id   = $serviceConnection.id
            }
            pipelines = $pipelines
        }, 
        @{
            resource  = @{
                type = "repository"
                id   = "$($projectReference.id).$($pipelinesRepo.id)"
            }
            pipelines = $pipelines
        }
    )) | Out-Null

Write-Host "`nDone." -ForegroundColor Green