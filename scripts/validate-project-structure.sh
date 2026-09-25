#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

required_files=(
  "global.json"
  "DeskTown.sln"
  "DeskTown.csproj"
  "project.godot"
  "export_presets.cfg"
  "scenes/app/AppRoot.tscn"
  "src/DeskTown.Domain/DeskTown.Domain.csproj"
  "src/DeskTown.Domain/Focus/FocusEnergy.cs"
  "src/DeskTown.Domain/Focus/SessionLedger.cs"
  "src/DeskTown.Domain/Focus/IEnergyPolicy.cs"
  "src/DeskTown.Domain/Focus/ElapsedTimeEnergyPolicy.cs"
  "src/DeskTown.Domain/Projects/ProjectSystem.cs"
  "src/DeskTown.Domain/Projects/ProjectCompleted.cs"
  "src/DeskTown.Domain/Projects/WorkshopState.cs"
  "src/DeskTown.Domain/Simulation/MinaStateMachine.cs"
  "src/DeskTown.Domain/Simulation/MinaSimulationState.cs"
  "src/DeskTown.Domain/Simulation/MinaStateCheckpoint.cs"
  "src/DeskTown.Application/DeskTown.Application.csproj"
  "src/DeskTown.Application/Persistence/SaveEnvelopeV1.cs"
  "src/DeskTown.Application/Persistence/SaveStateMapper.cs"
  "src/DeskTown.Application/Ports/IGameStateStore.cs"
  "src/DeskTown.Persistence/DeskTown.Persistence.csproj"
  "src/DeskTown.Persistence/JsonGameStateStore.cs"
  "src/DeskTown.Godot/Bootstrap/GameStatePathResolver.cs"
  "src/DeskTown.Application/Ports/IProcessCatalog.cs"
  "src/DeskTown.Application/Sessions/FocusSessionCoordinator.cs"
  "src/DeskTown.Platform.Windows/DeskTown.Platform.Windows.csproj"
  "src/DeskTown.Platform.Windows/Processes/WindowsProcessCatalog.cs"
  "tests/DeskTown.Foundation.Tests/DeskTown.Foundation.Tests.csproj"
  "tests/DeskTown.Domain.Tests/DeskTown.Domain.Tests.csproj"
  "tests/DeskTown.Domain.Tests/FocusEnergyTests.cs"
  "tests/DeskTown.Domain.Tests/SessionLedgerEnergyPolicyTests.cs"
  "tests/DeskTown.Domain.Tests/ProjectSystemTests.cs"
  "tests/DeskTown.Domain.Tests/MinaStateMachineTests.cs"
  "tests/DeskTown.Application.Tests/DeskTown.Application.Tests.csproj"
  "tests/DeskTown.Application.Tests/FocusSessionCoordinatorTests.cs"
  "tests/DeskTown.Application.Tests/FocusProjectIntegrationTests.cs"
  "tests/DeskTown.Application.Tests/JsonGameStateStoreTests.cs"
  "tests/DeskTown.Platform.Windows.Tests/DeskTown.Platform.Windows.Tests.csproj"
)

for required_file in "${required_files[@]}"; do
  test -f "$required_file"
done

grep -Fq 'run/main_scene="res://scenes/app/AppRoot.tscn"' project.godot
grep -Fq '<Project Sdk="Godot.NET.Sdk/4.7.2">' DeskTown.csproj
grep -Fq '"version": "8.0.425"' global.json
grep -Fq '../DeskTown.Domain/DeskTown.Domain.csproj' \
  src/DeskTown.Application/DeskTown.Application.csproj
grep -Fq 'tests\DeskTown.Domain.Tests\DeskTown.Domain.Tests.csproj' DeskTown.sln

if command -v rg >/dev/null 2>&1; then
  forbidden_references="$(
    rg -n 'Godot|DllImport|LibraryImport' src/DeskTown.Domain src/DeskTown.Application \
      --glob '*.cs' \
      --glob '!**/PrivacyFieldPolicy.cs' || true
  )"
else
  forbidden_references="$(
    find src/DeskTown.Domain src/DeskTown.Application -type f -name '*.cs' \
      ! -name 'PrivacyFieldPolicy.cs' -print0 \
      | xargs -0 grep -En 'Godot|DllImport|LibraryImport' || true
  )"
fi

if [[ -n "$forbidden_references" ]]; then
  printf '%s\n' "$forbidden_references"
  printf '%s\n' "Forbidden framework/native reference found in inner layers." >&2
  exit 1
fi

if command -v rg >/dev/null 2>&1; then
  activity_native_leaks="$(
    rg -n 'DllImport|LibraryImport' src/DeskTown.Platform.Windows/Activity \
      --glob '*.cs' \
      --glob '!NativeMethods.cs' || true
  )"
else
  activity_native_leaks="$(
    find src/DeskTown.Platform.Windows/Activity -type f -name '*.cs' \
      ! -name 'NativeMethods.cs' -print0 \
      | xargs -0 grep -En 'DllImport|LibraryImport' || true
  )"
fi

if [[ -n "$activity_native_leaks" ]]; then
  printf '%s\n' "$activity_native_leaks"
  printf '%s\n' "Activity P/Invoke must remain in Activity/NativeMethods.cs." >&2
  exit 1
fi

grep -Fq 'GetForegroundWindow' \
  src/DeskTown.Platform.Windows/Activity/NativeMethods.cs
grep -Fq 'GetWindowThreadProcessId' \
  src/DeskTown.Platform.Windows/Activity/NativeMethods.cs
grep -Fq 'GetLastInputInfo' \
  src/DeskTown.Platform.Windows/Activity/NativeMethods.cs

printf '%s\n' "DeskTown project structure validation passed."
