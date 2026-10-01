# Низкий звон — реализация и проверки

Статус реализации и результат проверки разделены. PASS относится к указанному объёму проверки,
а не ко всем возможным ситуациям. Доказательства: `.horror-sol-6.1-runs/20260930T151601Z/`.
Полное прохождение выполняет автоматический игрок через обычное движение, прицеливание,
крафт из полученных ингредиентов и PlayerInteraction. Предметы для Survival не выдаются отладкой.
Тест геометрии/яиц и live-checkpoint используют отдельно отмеченные искусственные площадки.

| Требование | Реализация | Проверка | Доказательство / предел |
|---|---|---|---|
| 1. Единая личность и ограниченная память | WORKING | PASS | 12 записей, срок 1800 активных тиков, источник/измерение/достоверность; persisted live-checkpoint, закрытая видимость |
| 2. Attention отдельно от tension | WORKING | PASS | Admission tests: внимание не отменяет grace/recovery/Peaceful/victory; legacy activation starts clean |
| 3. Совместимость и отсутствие частого повторения | WORKING | PASS | Симметричная таблица, босс один, текущие телеграфы, базовые враги, проверка сухого пути отхода; эвристика без полного решения навигационной задачи |
| 4. Связь с обычными действиями | WORKING | PASS | Узкие hooks успешной добычи, размещения/двери; отдельные звуковые/следовые/пещерные семейства; 900/1800-tick cooldowns |
| 5. Настоящее убежище | WORKING | PASS | Закрытая комната, угловой пролом, крыша, warning, изолированные Creative-фонари; заряд/рецепты проверены данными |
| 6. Расследование и подготовка | WORKING | PASS | J, записи отдельно от предмета; три источника + эксперименты/линза; настоящий финальный бой использует lure/reveal/bind |
| 7. Источники и восстановление | WORKING | PASS | Пять типов, saved lifecycle, локальное подавление, голубое свечение, cease escalation после смерти |
| 8. Необязательные искажения восприятия | NOT IMPLEMENTED | NOT TESTED | Осознанно не добавлены; нет ложных HUD/инвентаря, стробинга или смены управления |
| 9. Модели, движения и коллизии | WORKING | PASS | Blender MCP → 25 FBX → prefabs; исправлены оси/масштаб; day/night, семь игровых существ, реальные коллизии архитектуры |
| 10. Воспроизводимые тесты и Showcase | WORKING | PASS | 15 EditMode tests; Creative suite, автоматический Survival, отдельный preview, reset, контроль этапов |
| 7 видов / 5 локаций / 12 функциональных предметов | WORKING | PASS | Реестры + фактические prefabs/use/recipes; 19 найденных позиций Creative |
| Каждое новое существо имеет яйцо | WORKING | PASS | Все семь яиц реально создают существа с импортированными моделями; Creative invulnerability |
| Сохранения и отключение | WORKING | PASS | Atomic sidecar + backup recovery; original files unchanged, два legacy мира, victory/live boss reload, disable cleanup |
| Перемещение перекрытого источника | WORKING | NOT TESTED | Journal Settings; переносится только незавершённая принадлежащая дополнению запись, блоки игрока сохраняются |

| ID проверки MD | Результат | Реально выполненный объём |
|---|---|---|
| H01 Baseline / ID / saves | PASS | Компиляция/запуск, старые реестры, 1552 слоя сохранены, hash оригинальных миров, inventory/ender/seed копий |
| H02 Legacy activation | PASS | World 2/3 loaded without immediate pressure, initial grace independent of world age |
| H03 Attention / tension | PASS | State admission/recovery/defeat/Peaceful test; фактический маршрут источников |
| H04 Pacing / repetition | PASS | Бюджеты, cooldowns/history и реальный маршрут; многочасовая субъективная оценка ритма NOT TESTED |
| H05 Movement / presentation | PASS | Семь сущностей, потолочная траектория, сохранение шкалы, day/night, рельеф Survival; все варианты тоннелей/сложных лестниц NOT TESTED |
| H06 Memory / false leads | PASS | Бounded/expired/dimension memory, ложная приманка, настоящая стена закрывает sight, live evidence reload; долгий скрытый уход к дому NOT TESTED |
| H07 Encounter compatibility | PASS | Symmetric demands/boss exclusivity, windup deferral, geometry escape fixtures; exhaustive terrain combinations NOT TESTED |
| H08 Refuge | PASS | Closed room / off-axis gap / broken roof / grace; длительная ручная проверка Survival-подзарядки и всех door variants NOT TESTED |
| H09 Counterplay / cover | PASS | Lure/reveal/bind, gaze/light/mimic/ceiling/territory, actual closed-cover LOS and final sword reach |
| H10 Context events | PASS | Успешные action hooks и независимые cooldowns; все атмосферные семейства на видео NOT TESTED |
| H11 Investigation integrity | PASS | Три обязательных подтверждённых правила, renewable recipes, journal independent of physical item |
| H12 Sources | PASS | All five generated above ground without baseline block replacement, suppression/reward persisted; relocation NOT TESTED |
| H13 Save / stream / identity | PASS | Live actor/evidence checkpoint and process reload gives exactly one; victory reload gives zero; dimension round-trip, real route chunk streaming |
| H14 Creative | PASS | Search 19, real seven eggs, actual preview mechanics, no campaign progress/reward, preview defenses separated; drag every icon manually NOT TESTED |
| H15 Complete Survival | PASS | Fresh seed 931601, empty inventory, actual survey cache, normal walking/crafting/tools/attacks; three boss tactics, real death, one reward, saved free play |
| H16 Disable | PASS | Runtime actors/effects dismissed; definitions/data retained. Settings test and re-enable recovery |

## Регрессии базы

15 EditMode tests прошли (9 исходных + 6 дополнения). Два старых мира загружены без ошибок;
исходные файлы побайтово совпадают с резервной копией, инвентари, Ender Chest и seed копий сохранены.
Командный round-trip Overworld/Nether/End использует существующие portal-arrival правила, без ошибок.
Обычные блоки, реестры, текстуры и крафт сохранены. Полное повторное прохождение старых боссов,
редстоун-механизмов и всех портальных вариантов не выполнялось; старые известные пропуски не скрыты.
