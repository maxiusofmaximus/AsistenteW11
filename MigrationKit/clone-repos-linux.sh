#!/usr/bin/env bash
set -euo pipefail

TARGET_DIR="${1:-$HOME/Projects}"
mkdir -p "$TARGET_DIR"
cd "$TARGET_DIR"

repos=(
  "https://github.com/maxiusofmaximus/AsistenteW11.git"
  "https://github.com/maxiusofmaximus/AsistHub.git"
  "https://github.com/maxiusofmaximus/Components.git"
  "https://github.com/maxiusofmaximus/trono-2415-maximus-2026.git"
  "https://github.com/maxiusofmaximus/Claudia.git"
  "https://github.com/maxiusofmaximus/DEV-OS.git"
  "https://github.com/maxiusofmaximus/DevSecurityGuard.git"
  "https://github.com/maxiusofmaximus/notejob.git"
  "https://github.com/maxiusofmaximus/ProtoAscend.git"
)

for repo in "${repos[@]}"; do
  name="$(basename "$repo" .git)"
  if [[ -d "$name/.git" ]]; then
    echo "Skip (already exists): $name"
    continue
  fi
  echo "Cloning: $repo"
  git clone "$repo"
done

echo "Done. Repositories available at: $TARGET_DIR"
