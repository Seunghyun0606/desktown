$ErrorActionPreference = 'Stop'
$collector = Join-Path $PSScriptRoot 'qa/collect-evidence.ps1'
$directory = Join-Path ([IO.Path]::GetTempPath()) ('desktown QA evidence ' + [guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($directory) | Out-Null
$encoding = [Text.UTF8Encoding]::new($false)
$commit = 'a' * 40
$manifest = Join-Path $directory 'build-manifest.json'
[IO.File]::WriteAllText($manifest, ('{"commit":"' + $commit + '"}'), $encoding)
$checks = 0
function Assert-Check([bool] $Condition, [string] $Message) {
  if (-not $Condition) { throw $Message }
  $script:checks++
}
function Assert-Rejected([scriptblock] $Action, [string] $Message) {
  $failed = $false
  try { & $Action | Out-Null } catch { $failed = $true }
  Assert-Check $failed $Message
}
$report = Join-Path $directory 'legacy report.md'
& $collector -KitDirectory $directory -Scaling "125%|150%`nmanual" -EnvironmentKind CI -OutputPath $report
$content = [IO.File]::ReadAllText($report, [Text.Encoding]::UTF8)
Assert-Check ($content.Contains("- Build SHA: $commit")) 'Legacy CI manifest lost its build identity.'
Assert-Check ($content.Contains('125%\|150% manual')) 'Free-form scaling split the report structure.'
Assert-Check ($content.Contains('declared, not verified): CI')) 'Environment was incorrectly certified.'
$cases = @($content -split '\r?\n' | Where-Object { $_ -match '^\| (BOOT|ART|DAILY|WIN|AUDIO|SAVE|CLEAN|GHOST)-\d+ \|' })
Assert-Check ($cases.Count -eq 26) 'Missing current MVP manual checks.'
Assert-Check (@($cases | Where-Object { $_ -notmatch '\| Not tested \|' }).Count -eq 0) 'A manual case was auto-passed.'
Assert-Check (-not $content.Contains('Runtime source SHA:')) 'Runtime source was invented for legacy manifests.'

# A filled-in report, including non-ASCII notes, must survive a repeated invocation.
$notes = 'existing ' + [char]0xAC80 + [char]0xC0AC
[IO.File]::WriteAllText($report, $notes, $encoding)
$before = (Get-FileHash -LiteralPath $report).Hash
Assert-Rejected { & $collector -KitDirectory $directory -OutputPath $report } 'Existing report was overwritten.'
Assert-Check ((Get-FileHash -LiteralPath $report).Hash -eq $before) 'Rejected output changed report bytes.'

$handoff = Join-Path $directory 'qa-handoff.json'
[IO.File]::WriteAllText($handoff, (@{ packageCommit = $commit; runtimeSourceCommit = ('b' * 40); qaToolingCommit = ('c' * 40) } | ConvertTo-Json), $encoding)
$newReport = Join-Path $directory 'handoff report.md'
& $collector -KitDirectory $directory -EnvironmentKind CleanWindows11 -OutputPath $newReport
$content = [IO.File]::ReadAllText($newReport, [Text.Encoding]::UTF8)
Assert-Check ($content.Contains('Runtime source SHA: ' + ('b' * 40)) -and $content.Contains('QA tooling SHA: ' + ('c' * 40))) 'Runtime and QA identities were conflated.'
Assert-Check ($content.Contains('declared, not verified): CleanWindows11')) 'Clean-PC declaration became a pass.'

$invalidReport = Join-Path $directory 'invalid.md'
[IO.File]::WriteAllText($handoff, (@{ packageCommit = ('d' * 40); runtimeSourceCommit = ('b' * 40); qaToolingCommit = ('c' * 40) } | ConvertTo-Json), $encoding)
Assert-Rejected { & $collector -KitDirectory $directory -OutputPath $invalidReport } 'Mismatched handoff was accepted.'
Assert-Check (-not (Test-Path -LiteralPath $invalidReport)) 'Invalid provenance created a report.'
[IO.File]::WriteAllText($manifest, '{"commit":"not-a-build"}', $encoding)
Assert-Rejected { & $collector -KitDirectory $directory -OutputPath $invalidReport } 'Invalid build identity was accepted.'
Assert-Check (-not (Test-Path -LiteralPath $invalidReport)) 'Invalid manifest created a report.'
Write-Output "QA evidence checks passed: $checks (PowerShell $($PSVersionTable.PSVersion)). Evidence: $directory"
