param(
  [string] $KitDirectory = $PSScriptRoot,
  [string] $Scaling = 'record manually',
  [ValidateSet('CurrentPC', 'CleanWindows11', 'CI')]
  [string] $EnvironmentKind = 'CurrentPC'
)
$ErrorActionPreference = 'Stop'
$kit = (Resolve-Path -LiteralPath $KitDirectory).Path
& (Join-Path $PSScriptRoot 'preflight.ps1') -KitDirectory $kit -RunSmoke
$reports = Join-Path $kit 'qa-reports'
[IO.Directory]::CreateDirectory($reports) | Out-Null
$name = 'qa-results-' + [DateTimeOffset]::UtcNow.ToString('yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N').Substring(0, 8) + '.md'
& (Join-Path $PSScriptRoot 'collect-evidence.ps1') -KitDirectory $kit -Scaling $Scaling `
  -EnvironmentKind $EnvironmentKind -OutputPath (Join-Path $reports $name)
Write-Output 'Preflight passed. Manual checks remain Not tested; fill in actual scaling and results.'
