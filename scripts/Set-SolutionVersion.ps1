[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    [String[]]
    $Solutions = "devhub_DevelopmentHub_Issues",
    [Parameter(Mandatory = $false)]
    [String]
    $Version = "0.1.6.2"
)

$Solutions | ForEach-Object {
    $solutionPath = Join-Path -Path .\src\solutions -ChildPath $_
    $solutionXmlPath = Join-Path -Path $solutionPath -ChildPath '.\Extract\Other\Solution.xml'
    
    [xml]$solutionXml = Get-Content -Path $solutionXmlPath

    $solutionXml.ImportExportXml.SolutionManifest.Version = $Version

    $solutionXml.Save($solutionXmlPath)
}