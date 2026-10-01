@echo off
setlocal
cd /d "%~dp0"

if exist "%~dp0MinecraftHorror.exe" (
  start "The Quiet Below" "%~dp0MinecraftHorror.exe"
  exit /b 0
)

if exist "%~dp0Builds\HorrorRelease\MinecraftHorror.exe" (
  start "The Quiet Below" "%~dp0Builds\HorrorRelease\MinecraftHorror.exe"
  exit /b 0
)

if exist "%ProgramFiles%\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" (
  start "Unity project" "%ProgramFiles%\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -projectPath "%~dp0"
  exit /b 0
)

if exist "%ProgramFiles%\Unity Hub\Unity Hub.exe" (
  start "Unity Hub" "%ProgramFiles%\Unity Hub\Unity Hub.exe" "%~dp0"
  exit /b 0
)

echo Готовая сборка и Unity 6000.6.0f1 не найдены.
echo Установите Unity Hub и Unity 6000.6.0f1, затем запустите этот файл снова.
pause
exit /b 1
