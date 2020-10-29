param (
    [Parameter(Mandatory=$true)]
    [String]
    $Username,
    [Parameter(Mandatory=$true)]
    [SecureString]
    $Password,
    [Parameter(Mandatory=$true)]
    [String]
    $ClientId,
    [Parameter(Mandatory=$true)]
    [String]
    $FirstName,
    [Parameter(Mandatory=$true)]
    [String]
    $LastName,
    [Parameter(Mandatory=$true)]
    [String]
    $Domain,
    [Parameter(Mandatory=$true)]
    [String]
    $Url,
    [Parameter(Mandatory=$true)]
    [String]
    $OrganizationName
)

$ErrorActionPreference = 'Stop'

[PSCredential]$cred = New-Object System.Management.Automation.PSCredential ($Username, $Password);
$conn = Get-CrmConnection -OrganizationName $OrganizationName -OnLineType Office365 -Credential $cred

$webApiHeaders = @{
    "Authorization" = "Bearer $($conn.CurrentAccessToken)"
    "Accept" = "application/json"
    "OData-MaxVersion" = "4.0"  
    "OData-Version" = "4.0"
    "Prefer" = "return=representation"
}
$apiUrl = "$Url/api/data/v9.1/"

$res = Invoke-WebRequest -Uri "$apiUrl/businessunits?`$select=businessunitid&`$orderby=createdon asc&`$top=1" -Headers $webApiHeaders -Method 'GET'
$rootBusinessUnitId = (ConvertFrom-Json -InputObject $res.content).value.businessunitid

$email = "$FirstName$LastName".ToLower()
$azureDevOpsApplicationUser = @{
    "accessmode"                = 4
    "firstname"                 = $FirstName
    "internalemailaddress"      = "$email@$Domain"
    "domainname"                = "$email@$Domain"
    "windowsliveid"             = "$email@$Domain"
    "lastname"                  = $LastName
    "userlicensetype"           = 3
    "isdisabled"                = $false
    "islicensed"                = $false
    "applicationid"             = "$ClientId"
    "applicationiduri"          = "$ClientId"
    "businessunitid@odata.bind" = "businessunits($rootBusinessUnitId)"
}
$body = ConvertTo-Json -InputObject $azureDevOpsApplicationUser

$res = Invoke-WebRequest -Uri "$apiUrl/systemusers" -Headers $webApiHeaders -Body $body -ContentType 'application/json' -Method 'POST'
$applicationUserId = (ConvertFrom-Json -InputObject $res.Content).systemuserid

$res = Invoke-WebRequest -Uri "$apiUrl/roles?`$filter=name eq 'System Administrator'&`$select=roleid" -Headers $webApiHeaders -Method 'GET'
$systemAdminRoleId = (ConvertFrom-Json -InputObject $res.content).value.roleid

$assignRoleRequest = @{
    "@odata.id" = "$apiUrl/roles($systemAdminRoleId)"
}
$body = ConvertTo-Json -InputObject $assignRoleRequest
Invoke-WebRequest -Uri "$apiUrl/systemusers($applicationUserId)/systemuserroles_association/`$ref" -Method 'POST' -Body $body -Headers $webApiHeaders -ContentType 'application/json'