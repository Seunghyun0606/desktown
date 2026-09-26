param(
  [string] $KitDirectory = $PSScriptRoot,
  [string] $Scaling = 'record manually',
  [string] $OutputPath = (Join-Path $PSScriptRoot 'qa-results.md')
)

$ErrorActionPreference = 'Stop'
if (-not $IsWindows -and $PSVersionTable.PSVersion.Major -ge 6) {
  throw 'The QA evidence collector runs on Windows.'
}
$manifest = Get-Content (Join-Path $KitDirectory 'build-manifest.json') -Raw | ConvertFrom-Json
Add-Type -AssemblyName System.Windows.Forms
$screens = @([System.Windows.Forms.Screen]::AllScreens | ForEach-Object {
  "- $($_.Bounds.Width)x$($_.Bounds.Height), origin ($($_.Bounds.X), $($_.Bounds.Y)), primary=$($_.Primary)"
})
$lines = @(
  '# DeskTown Windows QA results',
  '',
  "- Build SHA: $($manifest.commit)",
  "- Tested at (UTC): $([DateTimeOffset]::UtcNow.ToString('u'))",
  "- Windows version: $([Environment]::OSVersion.Version)",
  "- Display scaling: $Scaling",
  '- Screens (resolution and origin only):'
) + $screens + @(
  '',
  '| Check | Result (Pass/Fail/Not tested) | Observed behavior / issue ID |',
  '| --- | --- | --- |',
  '| Extracted launch and onboarding | Not tested | |',
  '| Companion typing, click, drag, scroll | Not tested | |',
  '| Hidden window/resource behavior | Not tested | |',
  '| Completion notice or Tray-only and Open | Not tested | |',
  '| Save/restart/recovery, lock/sleep | Not tested | |',
  '| Ghost QA preview input matrix | Not tested | |',
  '| Town and Companion visual readability | Not tested | |',
  '',
  'Do not attach work documents, window titles, typed text, browser URLs, or personal save files.'
)
if (Test-Path $OutputPath) { throw "Result file already exists: $OutputPath" }
$lines | Set-Content $OutputPath -Encoding utf8
Write-Output "Results template created: $OutputPath"
