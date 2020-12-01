# Opens the Power Apps Maker Portal as the given user. Recreates the user's Power Apps Community Plan environment if deleted
param (
    [Parameter(Mandatory = $true)]
    [String]
    $Username,
    [Parameter(Mandatory = $true)]
    [SecureString]
    $Password
)

$ErrorActionPreference = 'Stop'

Install-Module -Name Microsoft.PowerApps.Administration.PowerShell -Force
Install-Module -Name Microsoft.PowerApps.PowerShell -AllowClobber -Force 

$ie = New-Object -Com 'InternetExplorer.Application'
$ie.visible = $true
$ie.Navigate('https://make.powerapps.com')

function Get-ElementByIdWhenAvailable ($id, $timeout = 5000) {
    $interval = 1000
    while ($null -eq $ie.Document -or $ie.Busy) {
        Start-Sleep -Milliseconds $interval
    }

    $element = $ie.Document.getElementById($id)
    $time = 0
    while (([DBNull]::Value) -eq $element -and $timeout -gt $time) {
        Start-Sleep -Milliseconds $interval
        $time += $interval
        $element = $ie.Document.getElementById($id)
    }

    $element
}

$otherTile = Get-ElementByIdWhenAvailable('otherTileText')
if ($otherTile) {
    $otherTile.click()
}

$usernameInput = Get-ElementByIdWhenAvailable('i0116')
$usernameInput.value = $Username

$nextButton = Get-ElementByIdWhenAvailable('idSIButton9')
$nextButton.focus()
$nextButton.click()

do {
    $passwordInput = Get-ElementByIdWhenAvailable('i0118')
    Start-Sleep -Seconds 1
} while ($passwordInput.attributes.getNamedItem('aria-hidden').value -eq $true)
$passwordInput.value = $Password

$nextButton = Get-ElementByIdWhenAvailable('idSIButton9')
$nextButton.focus()
$nextButton.click()

$backButton = Get-ElementByIdWhenAvailable('idBtn_Back')
$backButton.click()

Add-PowerAppsAccount -Username $Username -Password $Password

$time = 0
$interval = 5000
$timeout = 120000
do {
    $env = Get-AdminPowerAppEnvironment
    if ($null -eq $env) {
        Start-Sleep -Milliseconds $interval
        $time += $interval
    }
} while ($null -eq $env -and $time -lt $timeout)

if ($null -eq $env) {
    throw "Environment wasn't provisioned within $($timeout)ms timeout"
}