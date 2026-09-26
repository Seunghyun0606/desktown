param(
  [Parameter(Mandatory = $true)]
  [ValidateSet('workshop-repairing', 'workshop-reveal-pending', 'railway-teaser')]
  [string] $Scenario,
  [Parameter(Mandatory = $true)] [string] $WindowsExport,
  [Parameter(Mandatory = $true)] [string] $DemoSaves
)

$ErrorActionPreference = 'Stop'
if (Get-Process -Name DeskTown -ErrorAction SilentlyContinue) {
  throw 'Close the existing DeskTown process before launching an isolated demo.'
}
$exe = (Resolve-Path (Join-Path $WindowsExport 'DeskTown.exe')).Path
$fixture = (Resolve-Path (Join-Path (Join-Path $DemoSaves $Scenario) 'save.json')).Path
$directory = Join-Path $env:TEMP ('desktown-demo-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $directory | Out-Null
$env:DESKTOWN_DEMO_SAVE = Join-Path $directory 'save.json'
Copy-Item $fixture $env:DESKTOWN_DEMO_SAVE
Write-Output "Isolated demo save: $env:DESKTOWN_DEMO_SAVE"
Write-Output 'This run does not use the personal DeskTown save. Close DeskTown to finish.'
$process = Start-Process -FilePath $exe -ArgumentList @('--', '--desktown-demo') -WorkingDirectory (Split-Path $exe) -PassThru -Wait
if ($process.ExitCode -ne 0) { throw "Demo exited with code $($process.ExitCode)." }
