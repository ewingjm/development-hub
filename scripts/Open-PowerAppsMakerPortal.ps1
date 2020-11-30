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