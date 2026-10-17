param(
  [string] $KitDirectory = $PSScriptRoot,
  [string] $Scaling = 'record manually',
  [ValidateSet('CurrentPC', 'CleanWindows11', 'CI')]
  [string] $EnvironmentKind = 'CurrentPC',
  [string] $OutputPath = (Join-Path $PSScriptRoot 'qa-results.md')
)

$ErrorActionPreference = 'Stop'
if (-not $IsWindows -and $PSVersionTable.PSVersion.Major -ge 6) {
  throw 'The QA evidence collector runs on Windows.'
}
$output = [IO.Path]::GetFullPath($OutputPath)
if (Test-Path -LiteralPath $output) { throw "Result file already exists: $output" }
$kit = (Resolve-Path -LiteralPath $KitDirectory).Path
$manifest = Get-Content -LiteralPath (Join-Path $kit 'build-manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if ($manifest.commit -notmatch '^[a-fA-F0-9]{40}$') { throw 'Missing or invalid build SHA.' }
$handoff = $null
$handoffPath = Join-Path $kit 'qa-handoff.json'
if (Test-Path -LiteralPath $handoffPath) {
  $handoff = Get-Content -LiteralPath $handoffPath -Raw -Encoding UTF8 | ConvertFrom-Json
  if ($handoff.packageCommit -ne $manifest.commit -or
      $handoff.runtimeSourceCommit -notmatch '^[a-fA-F0-9]{40}$' -or
      $handoff.qaToolingCommit -notmatch '^[a-fA-F0-9]{40}$') {
    throw 'QA handoff provenance is incomplete or mismatched.'
  }
}
# Keep free-form display notes on one Markdown line.
$scalingNote = ($Scaling -replace '[\r\n]', ' ' -replace '\|', '\|')
Add-Type -AssemblyName System.Windows.Forms
$screens = @([System.Windows.Forms.Screen]::AllScreens | ForEach-Object {
  "- $($_.Bounds.Width)x$($_.Bounds.Height), origin ($($_.Bounds.X), $($_.Bounds.Y)), primary=$($_.Primary)"
})
$lines = @(
  '# DeskTown Windows QA results',
  '',
  "- Build SHA: $($manifest.commit)",
  "- Template created (UTC): $([DateTimeOffset]::UtcNow.ToString('u'))",
  "- Environment kind (declared, not verified): $EnvironmentKind",
  "- Windows version: $([Environment]::OSVersion.Version)",
  "- PowerShell version: $($PSVersionTable.PSVersion)",
  "- Display scaling (record actual Windows value): $scalingNote",
  '- Storage under test: isolated demo / separate-account default save (fill in)',
  '- Tested app and window state: fill in per input/DPI run; no document titles',
  '- Screens (resolution and origin only):'
) + $screens
if ($handoff) {
  $lines += @(
    "- Runtime source SHA: $($handoff.runtimeSourceCommit)",
    "- QA tooling SHA: $($handoff.qaToolingCommit)"
  )
}
$lines += @(
  '',
  '## Manual results',
  '',
  'Creating this template does not run or pass these checks. Record actual test time and evidence per row.',
  'Use a new report for each app / DPI / monitor / window-state combination. Leave untested rows as Not tested.',
  'See QA-README.md and WINDOWS_GATE.md for steps and acceptance criteria.',
  '',
  '| ID | Check / expected behavior | Result (Pass/Fail/Not tested) | Actual time / evidence / issue ID |',
  '| --- | --- | --- | --- |',
  '| BOOT-01 | Full ZIP extraction and preflight integrity + isolated three-process save smoke | Not tested | |',
  '| BOOT-02 | Start-Play: onboarding, no accumulation before Start, persisted progress on reopening | Not tested | |',
  '| ART-01 | 38 visual assets: Town states, NPC routes, Companion contact, effects, logo and map readable | Not tested | |',
  '| ART-02 | Reduced motion stops optional movement/effects and persists after restart | Not tested | |',
  '| DAILY-01 | Start-Daily-Focus opens completed Town; Focus with Mina offers 25/45/60 minutes | Not tested | |',
  '| DAILY-02 | Real second Focus increases today/total; Workshop stays restored; no repeated reward/map | Not tested | |',
  '| DAILY-03 | play and daily progress remain separate; same launcher resumes its own save | Not tested | |',
  '| DAILY-04 | Pending reward/map/recovery blocks new Focus; duplicate Start does not replace the session | Not tested | |',
  '| WIN-01 | PiP only, no Town window/taskbar entry; typing/click/drag/scroll reaches the work app | Not tested | |',
  '| WIN-02 | PiP 75/100/125/150%, header drag, saved position and missing-monitor correction | Not tested | |',
  '| WIN-03 | PiP -> Minimized -> Hidden -> PiP preserves one session; each mode has expected windows | Not tested | |',
  '| WIN-04 | Minimized leaves Town taskbar entry; Hidden leaves Tray only; record idle CPU/GPU | Not tested | |',
  '| WIN-05 | Taskbar/Tray Open restores existing Town during/after Focus; no duplicate process/session | Not tested | |',
  '| WIN-06 | Completion hides PiP, marks Tray and does not open Town or take work-app focus | Not tested | |',
  '| WIN-07 | Completion notice on/off and explicit Open work; preference persists | Not tested | |',
  '| AUDIO-01 | Town ambience/birds/steps and Companion hammer follow visible scenes/actions; device volume acceptable | Not tested | |',
  '| AUDIO-02 | Tray Audio off silences immediately; restart preserves off; on restores sound | Not tested | |',
  '| AUDIO-03 | Hidden/minimized/locked scenes stop sound; reopening has no catch-up burst | Not tested | |',
  '| AUDIO-04 | Button, Workshop completion and map cues occur once in their visible flow | Not tested | |',
  '| SAVE-01 | Normal exit/restart retains Focus progress, Audio, Reduced motion and display settings | Not tested | |',
  '| SAVE-02 | After checkpoint + forced exit, Resume/End work in all three modes; downtime excluded | Not tested | |',
  '| SAVE-03 | Real Windows lock/sleep/overlapping pause reasons exclude downtime and resume correctly | Not tested | |',
  '| CLEAN-01 | Clean Windows 11 account without editor/SDK: extract, launch, exit and relaunch | Not tested | |',
  '| CLEAN-02 | Separate-account default save path outside app folder, permissions and backup restore verified | Not tested | |',
  '| CLEAN-03 | Side-by-side ZIP update/rollback retains progress; no overwrite of occupied target save | Not tested | |',
  '| GHOST-01 | Ordinary Ghost Focus stays disabled; optional F10 preview needs full WINDOWS_GATE matrix | Not tested | |',
  '',
  '## Issues',
  '',
  '| Issue ID / severity | Case ID / configuration | Steps | Expected / actual | Fix / retest evidence |',
  '| --- | --- | --- | --- | --- |',
  '| | | | | |',
  '',
  'Progress loss or blocked input: P0. Repeated focus stealing, off-screen windows or work interruption: P1. Visual/text polish: P2.',
  'Automated checks and template generation do not certify manual Windows or physical-speaker results.',
  '',
  'Do not attach work documents, window titles, typed text, browser URLs, or personal save files.'
)
# CreateNew also protects an existing report if two collectors race.
$stream = [IO.File]::Open($output, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
try {
  $text = ($lines -join [Environment]::NewLine) + [Environment]::NewLine
  $bytes = [Text.UTF8Encoding]::new($true).GetPreamble() + [Text.Encoding]::UTF8.GetBytes($text)
  $stream.Write($bytes, 0, $bytes.Length)
}
finally { $stream.Dispose() }
Write-Output "Results template created: $output"
