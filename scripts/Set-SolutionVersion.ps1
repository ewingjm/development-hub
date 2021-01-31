[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [String[]]
    $Solutions,
    [Parameter(Mandatory = $true)]
    [String]
    $Version
)

$Solutions | ForEach-Object {
    $solutionPath = Join-Path -Path .\src\solutions -ChildPath $_
    $solutionXmlPath = Join-Path -Path $solutionPath -ChildPath '.\Extract\Other\Solution.xml'
    
    [xml]$solutionXml = Get-Content -Path $solutionXmlPath

    $solutionXml.ImportExportXml.SolutionManifest.Version = $Version

    $solutionXml.Save($solutionXmlPath)
}