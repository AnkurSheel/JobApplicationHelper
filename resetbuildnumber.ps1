$apiUrl = 'https://ci.appveyor.com/api'
$token = '265swe01gytyfhx7m2th'
$headers = @{
  "Authorization" = "Bearer $token"
  "Content-type" = "application/json"
  "Accept" = "application/json"
}
$accountName = $env:APPVEYOR_ACCOUNT_NAME
$projectSlug = $env:APPVEYOR_PROJECT_SLUG
$buildNumber = $env:APPVEYOR_BUILD_NUMBER
$buildVersionText = $env:APPVEYOR_BUILD_VERSION
$buildVersion = New-Object -TypeName PSObject -Property (@{
    'MajorVersion'=$buildVersionText.Split('.')[0];
    'MinorVersion'=$buildVersionText.Split('.')[1];
	'PatchVersion'=$buildVersionText.Split('.')[2]
    'BuildVersion'=$buildVersionText.Split('.')[3]
})

$response = Invoke-RestMethod -Method Get -Uri "$apiUrl/projects/$accountName/$projectSlug/history?recordsNumber=100" -Headers $headers
$lastBuildVersion = $response.builds | select -first 1 @{Label="MajorVersion"; Expression={$_.version.Split('.')[0]}}, @{Label="MinorVersion"; Expression={$_.version.Split('.')[1]}}, @{Label="BuildVersion"; Expression={$_.version.Split('.')[2]}}

if ($lastBuildVersion.MajorVersion -ne $buildVersion.MajorVersion -or ($lastBuildVersion.MinorVersion -ne $buildVersion.MinorVersion) -or ($lastBuildVersion.PatchVersion -ne $buildVersion.PatchVersion))
{
  $build = @{
      nextBuildNumber = 0
  }
  $json = $build | ConvertTo-Json

  Invoke-RestMethod -Method Put "$apiUrl/projects/$accountName/$projectSlug/settings/build-number" -Body $json -Headers $headers
}