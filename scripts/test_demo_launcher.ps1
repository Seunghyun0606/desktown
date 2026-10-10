param(
  [Parameter(Mandatory = $true)] [string] $WindowsExport,
  [Parameter(Mandatory = $true)] [string] $DemoSaves
)
$ErrorActionPreference = 'Stop'
$previousDemo = $env:DESKTOWN_DEMO_SAVE
$previousSmoke = $env:DESKTOWN_CI_SMOKE
$previousArt = $env:DESKTOWN_CI_ART_SMOKE
try {
  $env:DESKTOWN_DEMO_SAVE = 'caller-path-must-not-be-used'
  $env:DESKTOWN_CI_SMOKE = '1'
  $env:DESKTOWN_CI_ART_SMOKE = '1'
  foreach ($scenario in @('workshop-repairing', 'workshop-reveal-pending', 'railway-teaser')) {
    $fixture = Join-Path (Join-Path $DemoSaves $scenario) 'save.json'
    $before = (Get-FileHash -LiteralPath $fixture -Algorithm SHA256).Hash
    $output = @(& (Join-Path $PSScriptRoot 'qa/launch-demo.ps1') -Scenario $scenario `
      -WindowsExport $WindowsExport -DemoSaves $DemoSaves -Headless)
    $saveLine = @($output | Where-Object { $_ -like 'Isolated demo save: *' })
    if ($saveLine.Count -ne 1) { throw 'Launcher did not report exactly one isolated save.' }
    $savePath = $saveLine[0].Substring('Isolated demo save: '.Length)
    $log = Get-Content -LiteralPath (Join-Path (Split-Path $savePath) 'stdout.log') -Raw -Encoding UTF8
    if ($log -notmatch 'application_started') { throw "Demo did not reach normal initialization: $scenario" }
    if ($env:DESKTOWN_DEMO_SAVE -ne 'caller-path-must-not-be-used' -or
        $env:DESKTOWN_CI_SMOKE -ne '1' -or $env:DESKTOWN_CI_ART_SMOKE -ne '1') {
      throw 'Launcher changed the caller environment.'
    }
    if ((Get-FileHash -LiteralPath $fixture -Algorithm SHA256).Hash -ne $before) {
      throw "Demo changed its source fixture: $scenario"
    }
    Write-Output "Launcher passed: $scenario ($($PSVersionTable.PSVersion))"
  }
}
finally {
  $env:DESKTOWN_DEMO_SAVE = $previousDemo
  $env:DESKTOWN_CI_SMOKE = $previousSmoke
  $env:DESKTOWN_CI_ART_SMOKE = $previousArt
}
