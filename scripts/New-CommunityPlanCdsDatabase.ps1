param (
    [Parameter(Mandatory = $true)]
    [String]
    $Username,
    [Parameter(Mandatory = $true)]
    [SecureString]
    $Password,
    [Parameter(Mandatory = $true)]
    $CurrencyName,
    [Parameter(Mandatory = $true)]
    $LanguageCode
)

$ErrorActionPreference = 'Stop'

Install-Module -Name Microsoft.PowerApps.Administration.PowerShell -Force
Install-Module -Name Microsoft.PowerApps.PowerShell -AllowClobber -Force 

Add-PowerAppsAccount -Username $Username -Password $Password
$environment = Get-AdminPowerAppEnvironment
$environment = New-AdminPowerAppCdsDatabase -EnvironmentName $environment.EnvironmentName -CurrencyName $CurrencyName -LanguageName $LanguageCode
$newOrganizationName = $environment.DisplayName.Substring($environment.DisplayName.IndexOf('(') + 1, 11)

Write-Output "##vso[task.setvariable variable=newOrganizationName]$newOrganizationName"