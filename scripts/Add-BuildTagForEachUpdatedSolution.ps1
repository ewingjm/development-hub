$resultArray = git show --name-only
[System.Collections.ArrayList]$changedSolutions = @()

foreach ($_ in $resultArray) {
  if ($_.StartsWith("src/solutions")) {
    $solutionName = $_.Split("/")[2]

    if (!$changedSolutions.Contains($solutionName)) {
      $changedSolutions.Add($solutionName)
      Write-Host "##vso[build.addbuildtag]$solutionName" 
    }
  }
}
$output = $changedSolutions -Join ','  

Write-Host "##vso[task.setvariable variable=solutionList]$output"