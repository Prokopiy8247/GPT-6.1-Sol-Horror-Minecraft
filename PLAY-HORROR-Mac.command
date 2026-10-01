#!/bin/bash
set -e

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"

if [ -d "$PROJECT_DIR/MinecraftHorror.app" ]; then
  open "$PROJECT_DIR/MinecraftHorror.app"
  exit 0
fi

if [ -d "$PROJECT_DIR/Builds/HorrorRelease/MinecraftHorror.app" ]; then
  open "$PROJECT_DIR/Builds/HorrorRelease/MinecraftHorror.app"
  exit 0
fi

if [ -d "/Applications/Unity Hub.app" ]; then
  open -a "Unity Hub" "$PROJECT_DIR"
  exit 0
fi

echo "Unity Hub не найден. Установите Unity Hub и Unity 6000.6.0f1, затем откройте проект вручную:"
echo "$PROJECT_DIR"
read -r -p "Нажмите Enter для закрытия..."
