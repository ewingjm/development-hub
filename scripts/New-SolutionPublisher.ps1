param (
    [Parameter(Mandatory = $true)]
    [String]
    $Username,
    [Parameter(Mandatory = $true)]
    [SecureString]
    $Password,
    [Parameter(Mandatory = $true)]
    [String]
    $DisplayName,
    [Parameter(Mandatory = $true)]
    [String]
    $UniqueName,
    [Parameter(Mandatory = $true)]
    [String]
    $Prefix
)

$ErrorActionPreference = 'Stop'

[PSCredential]$cred = New-Object System.Management.Automation.PSCredential ($Username, $Password);
$conn = Get-CrmConnection -OrganizationName $OrganizationName -OnLineType Office365 -Credential $cred

$publisher = New-Object -TypeName Microsoft.Xrm.Sdk.Entity -ArgumentList  "publisher"
$publisher.Attributes.Add("uniquename", $UniqueName)
$publisher.Attributes.Add("friendlyname", $DisplayName)
$publisher.Attributes.Add("customizationprefix", $Prefix)

$conn.Create($publisher)