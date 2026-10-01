#!/bin/sh
set -e
SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
if [ -d "$SCRIPT_DIR/MinecraftHorror.app" ]; then
  open "$SCRIPT_DIR/MinecraftHorror.app"
elif [ -d "$SCRIPT_DIR/Builds/HorrorRelease/MinecraftHorror.app" ]; then
  open "$SCRIPT_DIR/Builds/HorrorRelease/MinecraftHorror.app"
elif [ -d "/Applications/Unity Hub.app" ]; then
  open -a "Unity Hub" "$SCRIPT_DIR"
else
  echo "Install Unity Hub and Unity 6000.6.0f1, then open: $SCRIPT_DIR"
  exit 1
fi
