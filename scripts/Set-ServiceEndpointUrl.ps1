param (
    [Parameter(Mandatory = $true)]
    [String]
    $Name,
    [Parameter(Mandatory = $true)]
    [String]
    $Url,
    [Parameter(Mandatory = $true)]
    [PSCustomObject]
    $AuthParameters
)

$ErrorActionPreference = 'Stop'

$serviceEndpoints = az devops service-endpoint list  | ConvertFrom-Json

$endpointToUpdate = $serviceEndpoints | Where-Object { $_.name -eq $Name }
if ($null -eq $endpointToUpdate) {
    throw "Unable to find a service endpoint named $Name."
}

$endpointToUpdate.url = $Url
$AuthParameters | Get-Member -Type NoteProperty | ForEach-Object {
    $endpointToUpdate.authorization.parameters | Add-Member -MemberType NoteProperty -Name $_.Name -Value $AuthParameters."$($_.Name)"
}

$tempFilePath = Join-Path -Path $env:TEMP -ChildPath "$(New-Guid).json"
$json = $endpointToUpdate | ConvertTo-Json -Depth 100 
[System.IO.File]::WriteAllLines($tempFilePath, $json)

az devops service-endpoint delete --id $endpointToUpdate.id -y
az devops service-endpoint create --service-endpoint-configuration $tempFilePath

Remove-Item -Path $tempFilePath
