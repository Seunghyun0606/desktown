param(
  [string] $KitDirectory = $PSScriptRoot,
  [switch] $RunSmoke
)

$ErrorActionPreference = 'Stop'
if (-not $IsWindows -and $PSVersionTable.PSVersion.Major -ge 6) {
  throw 'The QA kit must be checked on Windows.'
}
$kit = (Resolve-Path $KitDirectory).Path
$zipFiles = @(Get-ChildItem $kit -Filter 'DeskTown-v0.1-*-win-x64.zip' -File)
if ($zipFiles.Count -ne 1) { throw 'Expected exactly one versioned DeskTown ZIP in the QA kit.' }
$zip = $zipFiles[0]
$checksum = (Get-Content (Join-Path $kit 'SHA256SUMS.txt') -Raw).Trim()
if ($checksum -notmatch '^([a-fA-F0-9]{64})  (DeskTown-v0\.1-[a-fA-F0-9]{8}-win-x64\.zip)$' -or
    $Matches[2] -ne $zip.Name -or
    (Get-FileHash $zip.FullName -Algorithm SHA256).Hash -ne $Matches[1]) {
  throw 'The versioned ZIP SHA-256 does not match SHA256SUMS.txt.'
}
$manifest = Get-Content (Join-Path $kit 'build-manifest.json') -Raw | ConvertFrom-Json
if ($manifest.commit -notmatch '^[a-fA-F0-9]{40}$' -or
    $zip.Name -ne "DeskTown-v0.1-$($manifest.commit.Substring(0, 8))-win-x64.zip" -or
    $manifest.signed -ne $false) { throw 'The package manifest is incomplete or mismatched.' }
$appDirectory = Join-Path $kit 'app'
if (-not (Test-Path $appDirectory)) {
  Expand-Archive $zip.FullName -DestinationPath $appDirectory
}
$seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($entry in $manifest.files) {
  $relative = [string]$entry.path
  if ($relative -match '(^/|^\\|:|(^|[/\\])\.\.([/\\]|$))' -or -not $seen.Add($relative)) {
    throw "Unsafe or duplicate package path: $relative"
  }
  $file = Join-Path $appDirectory $relative
  if (-not (Test-Path $file -PathType Leaf) -or
      (Get-Item $file).Length -ne $entry.bytes -or
      (Get-FileHash $file -Algorithm SHA256).Hash -ne $entry.sha256) {
    throw "Export file does not match the manifest: $relative"
  }
}
$actual = @(Get-ChildItem $appDirectory -Recurse -File)
if ($actual.Count -ne $seen.Count -or -not (Test-Path (Join-Path $appDirectory 'DeskTown.exe'))) {
  throw 'The extracted export has missing or extra files.'
}
Write-Output "Package verified: $($manifest.commit)"
Write-Output "Extracted app: $appDirectory"
if (-not $RunSmoke) { return }
if (Get-Process -Name DeskTown -ErrorAction SilentlyContinue) {
  throw 'Close DeskTown before running the isolated preflight.'
}
$smokeDirectory = Join-Path $env:TEMP ('desktown-qa-smoke-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $smokeDirectory | Out-Null
$priorSmoke = [Environment]::GetEnvironmentVariable('DESKTOWN_CI_SMOKE')
$priorSave = [Environment]::GetEnvironmentVariable('DESKTOWN_CI_SMOKE_SAVE')
$succeeded = $false
try {
  $env:DESKTOWN_CI_SMOKE = '1'
  $env:DESKTOWN_CI_SMOKE_SAVE = Join-Path $smokeDirectory 'save.json'
  foreach ($phase in @('write', 'recover', 'check')) {
    $stdout = Join-Path $smokeDirectory "$phase.stdout.log"
    $stderr = Join-Path $smokeDirectory "$phase.stderr.log"
    $process = Start-Process -FilePath (Join-Path $appDirectory 'DeskTown.exe') `
      -ArgumentList @('--headless', '--', "--desktown-ci-save-smoke=$phase") `
      -WorkingDirectory $appDirectory -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    if (-not $process.WaitForExit(45000)) {
      $process.Kill()
      throw "Isolated $phase phase timed out. Logs: $smokeDirectory"
    }
    $log = (Get-Content $stdout -Raw) + "`n" + (Get-Content $stderr -Raw)
    if ($process.ExitCode -ne 0 -or $log -match '(?m)^(ERROR:|SCRIPT ERROR:)' -or
        $log -notmatch "DESKTOWN_CI_SAVE_SMOKE_OK:$phase") {
      throw "Isolated $phase phase failed. Logs: $smokeDirectory"
    }
  }
  $succeeded = $true
  Write-Output 'Isolated save/restart/recovery smoke passed. Personal save was not used.'
}
finally {
  $env:DESKTOWN_CI_SMOKE = $priorSmoke
  $env:DESKTOWN_CI_SMOKE_SAVE = $priorSave
  if ($succeeded) { Remove-Item $smokeDirectory -Recurse -Force }
}
