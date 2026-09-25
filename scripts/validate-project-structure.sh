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
  "src/DeskTown.Application/DeskTown.Application.csproj"
  "src/DeskTown.Platform.Windows/DeskTown.Platform.Windows.csproj"
  "tests/DeskTown.Foundation.Tests/DeskTown.Foundation.Tests.csproj"
  "tests/DeskTown.Domain.Tests/DeskTown.Domain.Tests.csproj"
  "tests/DeskTown.Application.Tests/DeskTown.Application.Tests.csproj"
)

for required_file in "${required_files[@]}"; do
  test -f "$required_file"
done

grep -Fq 'run/main_scene="res://scenes/app/AppRoot.tscn"' project.godot
grep -Fq '<Project Sdk="Godot.NET.Sdk/4.7.2">' DeskTown.csproj
grep -Fq '"version": "8.0.425"' global.json
grep -Fq '../DeskTown.Domain/DeskTown.Domain.csproj' \
  src/DeskTown.Application/DeskTown.Application.csproj

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

printf '%s\n' "DeskTown project structure validation passed."
