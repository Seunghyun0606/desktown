param(
  [Parameter(Mandatory = $true)]
  [ValidateSet('workshop-repairing', 'workshop-reveal-pending', 'railway-teaser')]
  [string] $Scenario,
  [Parameter(Mandatory = $true)] [string] $WindowsExport,
  [Parameter(Mandatory = $true)] [string] $DemoSaves,
  [switch] $Headless,
  [ValidateRange(1, 600)] [int] $HeadlessTimeoutSeconds = 45
)

$ErrorActionPreference = 'Stop'
if (Get-Process -Name DeskTown -ErrorAction SilentlyContinue) {
  throw 'Close the existing DeskTown process before launching an isolated demo.'
}
$exe = (Resolve-Path (Join-Path $WindowsExport 'DeskTown.exe')).Path
$fixture = (Resolve-Path (Join-Path (Join-Path $DemoSaves $Scenario) 'save.json')).Path
$directory = Join-Path $env:TEMP ('desktown-demo-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $directory | Out-Null
$demoSave = Join-Path $directory 'save.json'
Copy-Item -LiteralPath $fixture -Destination $demoSave
Write-Output "Isolated demo save: $demoSave"
Write-Output 'This run does not use the personal DeskTown save. Close DeskTown to finish.'

# Use Process directly, as in preflight.ps1, to obtain the actual GUI executable
# exit code in Windows PowerShell 5.1. Give only the child an isolated save path.
$startInfo = [Diagnostics.ProcessStartInfo]::new()
$startInfo.FileName = $exe
$startInfo.Arguments = '-- --desktown-demo'
if ($Headless) { $startInfo.Arguments = '--headless --quit-after 120 -- --desktown-demo' }
$startInfo.WorkingDirectory = Split-Path $exe
$startInfo.UseShellExecute = $false
$startInfo.CreateNoWindow = $true
$startInfo.RedirectStandardOutput = $true
$startInfo.RedirectStandardError = $true
$startInfo.StandardOutputEncoding = [Text.Encoding]::UTF8
$startInfo.StandardErrorEncoding = [Text.Encoding]::UTF8
$startInfo.EnvironmentVariables['DESKTOWN_DEMO_SAVE'] = $demoSave
# An inherited smoke flag would bypass normal demo initialization.
$startInfo.EnvironmentVariables.Remove('DESKTOWN_CI_SMOKE')
$startInfo.EnvironmentVariables.Remove('DESKTOWN_CI_ART_SMOKE')
$process = [Diagnostics.Process]::new()
$process.StartInfo = $startInfo
try {
  if (-not $process.Start()) { throw 'Demo did not start.' }
  $stdoutTask = $process.StandardOutput.ReadToEndAsync()
  $stderrTask = $process.StandardError.ReadToEndAsync()
  $timedOut = $Headless -and -not $process.WaitForExit($HeadlessTimeoutSeconds * 1000)
  if ($timedOut) { $process.Kill() }
  $process.WaitForExit()
  $stdoutText = $stdoutTask.Result
  $stderrText = $stderrTask.Result
  $exitCode = $process.ExitCode
}
finally { $process.Dispose() }
[IO.File]::WriteAllText((Join-Path $directory 'stdout.log'), $stdoutText, [Text.Encoding]::UTF8)
[IO.File]::WriteAllText((Join-Path $directory 'stderr.log'), $stderrText, [Text.Encoding]::UTF8)
if ($timedOut) { throw "Demo timed out. Logs and isolated save: $directory" }
$log = $stdoutText + "`n" + $stderrText
if ($exitCode -ne 0 -or $log -match '(?m)^(ERROR:|SCRIPT ERROR:|DeskTown command failed:)') {
  throw "Demo failed with exit code $exitCode. Logs and isolated save: $directory"
}
Write-Output "Demo completed with exit code $exitCode. Logs: $directory"
