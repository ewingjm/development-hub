$resultArray = git show --name-only
$solutionList = ""

foreach ($_ in $resultArray) {
  if ($_.StartsWith("solutions") -and $_.Contains("Extract")) {
    $solutionName = $_.Split("/")[1]

    if (!$solutionList.Contains($solutionName)) {
      $solutionList += $solutionName + ";"
      Write-Host "##vso[build.addbuildtag]$solutionName" 
    }
  }
}

Write-Host "##vso[task.setvariable variable=solutionList;]$solutionList"