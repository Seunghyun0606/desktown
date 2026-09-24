#!/usr/bin/env bash
set -euo pipefail

godot_version="${GODOT_VERSION:-4.7.2}"
tools_root="${RUNNER_TEMP:-/tmp}/desktown-tools"
engine_archive="$tools_root/godot-mono.zip"
templates_archive="$tools_root/godot-templates.tpz"
engine_directory="$tools_root/Godot_v${godot_version}-stable_mono_linux_x86_64"
engine_binary="$engine_directory/Godot_v${godot_version}-stable_mono_linux.x86_64"
template_directory="$HOME/.local/share/godot/export_templates/${godot_version}.stable.mono"

mkdir -p "$tools_root" "$template_directory"

curl --fail --location --retry 3 \
  "https://github.com/godotengine/godot-builds/releases/download/${godot_version}-stable/Godot_v${godot_version}-stable_mono_linux_x86_64.zip" \
  --output "$engine_archive"
unzip -q "$engine_archive" -d "$tools_root"
chmod +x "$engine_binary"
ln -sfn "$engine_binary" "$tools_root/godot"

curl --fail --location --retry 3 \
  "https://github.com/godotengine/godot-builds/releases/download/${godot_version}-stable/Godot_v${godot_version}-stable_mono_export_templates.tpz" \
  --output "$templates_archive"
unzip -q "$templates_archive" -d "$tools_root/export-templates"
cp -R "$tools_root/export-templates/templates/." "$template_directory/"

if [[ -n "${GITHUB_PATH:-}" ]]; then
  printf '%s\n' "$tools_root" >> "$GITHUB_PATH"
else
  printf '%s\n' "$tools_root/godot"
fi
