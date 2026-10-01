# Руководство разработчика

## Требования

- Unity Hub и Unity Editor 6000.6.0f1.
- Windows или macOS для открытия проекта.
- Для авторинга 3D-ассетов — Blender с подключением blender_unity, если оно используется в вашей локальной среде.

## Структура

- Assets/_Game/Scripts/ — базовые системы игры.
- Assets/_Game/Horror/Runtime/ — runtime хоррор-дополнения.
- Assets/_Game/Horror/Resources/Horror/ — материалы, FBX и prefabs дополнения.
- Tools/Horror/ — вспомогательные скрипты.
- ProjectSettings/ и Packages/ — настройки Unity и зависимости.

## Проверка

Откройте Assets/_Game/Scenes/Main.unity и нажмите Play. Для проверки сборки используйте Unity batchmode и Editor-методы из Tools/. Тестовые сохранения должны быть отдельными от пользовательских миров.

Не добавляйте в репозиторий Library, Temp, Logs, пользовательские сохранения, платформенные runtime-папки, IDE-файлы, локальные ключи или session logs. Для персональных тестов используйте локальные каталоги, исключённые .gitignore.

## Сборка

Собирайте Windows и macOS из Unity Editor с установленными соответствующими Build Support-модулями. Перед публикацией проверьте git status, отсутствие локальных путей и секретов, а также запуск launcher-файлов из чистой копии.
