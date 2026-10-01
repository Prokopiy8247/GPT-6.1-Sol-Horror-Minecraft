# The Quiet Below / Низкий звон

Old surveyors listened below bedrock. Four resonant instruments now answer ordinary mining sounds.
The player investigates the answer, learns defensive rules, settles three local sources and calls the
physical author of the noise to the Open Bell. Killing it permanently ends the pursuit; Minecraft continues.

## Original flagship
The Unseam (`horror:unseam`) has three unequal stilt legs, an oblique folded rib mantle and two hooked
resonator arms. Its tall ivory head is a stack of split shutters around a recessed blue slit. No eyes,
grinning skull, dorsal spikes or quadruped silhouette. In pursuit it unfolds upward and strikes with one
pendulum arm; during search the head lags behind the body. Articulated Blender parts use controller-driven
procedural animation, never root motion. Narrow passages exclude its real AABB.

## Roster and learnable decisions
| ID | Name | Rule | Counter | Role |
|---|---|---|---|---|
| unseam | The Unseam | evidence-driven stalker; final lure/reveal/bind loop | break sight, sound lure, ward refuge | flagship |
| rattleblind | Rattleblind | hears game sounds, cannot see | crouch, throw clatter lure | teaches distraction |
| lintel | Lintel | folded overhead ambusher with falling wind-up | move sideways before drop; cover | teaches warning windows |
| hearthmimic | Hearth Mimic | imitates an in-game chest sound; reveals at close range | reveal dust at distance | teaches revealing |
| wickdrinker | Wick Drinker | advances in darkness, repelled by bright light | torch/light, ward lamp | teaches light defense |
| stillwright | Stillwright | moves when not watched | watch it, then escape behind cover | teaches line of sight |
| briarchoir | Briar Choir | territorial ranged resonant rings | leave marked boundary, move out of telegraph | guards binding material |

Every ID uses prefix horror:, every type has an existing-catalog egg. Creative eggs create explicit preview
actors. Preview victory never changes campaign milestones or rewards. Switching mode cancels previews.

## Progression and two learning paths
Mining answer or craft field journal -> Survey Cairn record -> craft locator and protective tools ->
Listening Well (sound lure rule), Shutter Chapel (reveal rule), Root Archive (binding rule) -> settle local
sources with tuning fork -> find Open Bell -> prepare all three tools -> summon -> lure strike to expose
shutters -> reveal with dust -> bind -> damage with ordinary weapons -> repeat through three tactics ->
real death -> one reward -> saved recovery/free play.

Every essential rule has site record plus repeatable creature experiment; journal stores observation,
hypothesis and confirmed counters independently of the physical book. All consumables have renewable
base-resource recipes. No required object drops only from the boss. Sites are recoverable overlays in
validated empty terrain; no base block or container is replaced.

Items: field journal, listening compass, tuning fork, ward lantern, clatter lure, reveal dust, binding spool,
hush balm, resonant splinter, shutter lens, bell key and quiet heart. Equipment and interactive site props
are authored through Blender. Functional uses and recipes are recorded in the guide as implemented.

## Memory, pacing and refuge contracts
One real flagship per world. Maximum 12 evidence records: actual sight, audible action/lure, observed
shelter and failed search; dimension + location + active gameplay clock + confidence + expiry.
No bed/home/current-position shortcut after loss of detection. Evidence expires in at most 90 seconds.
Attention rises only for disturbance, detection and progression; tension rises during pressure and suppresses
admission. Initial grace 90 seconds, recovery at least 45 seconds, max two scheduled threats (boss alone).
Timers use active ticks, not dayTime or offline time. Peaceful stops hostile admission.

Response demands: sound/quiet, overhead/move, mimic/reveal, dark/light, gaze/watch, choir/move.
Stillwright never overlaps an overhead/movement threat; boss excludes all other scheduled horror combat.
Nearby ordinary hostiles or active telegraphs postpone horror attacks. Recent family/place history bounded
to 16 entries; contextual mining, placement and door interaction cues have independent cooldowns.

Ward lantern protects radius 6 only in a real roofed, closed, dry room with solid horizontal boundaries.
Device must exist and have charge; 10-minute charge, renewable coal recharge, 15-second warning at low
charge. Closed barriers also block sight/strikes without wards. Opening a boundary invalidates protection
with a 5-second response window. No terrain breach or arbitrary wall phasing. Ward does not defeat a boss.

Sources have stable identities, radius 32, saved suppression and retained clue/replacement access.
Settled sources switch from dissonant to calm blue emission and stop local admission. Victory settles all
sources, ceases the pursuit motif and stops major escalation. Rematches require explicit Creative preview.
Optional perception distortion is deferred until the core route and tests are complete; it is not counted
as implemented. Settings provide horror enable, intensity and shake; no flashing effect is needed.

## Уточнения окончательной реализации

Survey Cairn содержит одноразовый тайник обычных материалов и еды. Журнал J показывает следующий
источник; камертон не нужен для чтения самого Cairn, но нужен для остальных трёх источников.
Архитектура различается: ступенчатый каменный Cairn с табличкой, колодец с подвесным резонатором,
часовня со ставнями/скатной крышей, корневой архив со стеллажами, открытый Колокол на воротах.
Сухие наклонные участки используют опоры. Коллизии стен/полок/перекладин принадлежат дополнению.
Размещение не редактирует исходные блоки; обязательные объекты находятся на поверхности.

Подавление источника также убирает его находящихся рядом нефинальных акторов. Флагман может
вновь исследовать мир при внимании от 28 после первого продвижения, с grace/recovery и проверками
безопасности; каждое нефинальное давление ограничено двумя минутами. Ни дом, ни кровать не
используются как скрытая цель. На потере контакта он пользуется только последними свидетельствами.
При финальном вызове эхо сменяется единственным настоящим телом. После сохранённого checkpoint
вызов не сбрасывает оставшееся здоровье. Победа не допускает невольного воскрешения.

Совместимость: Unseam исключает все пары; Stillwright допускается лишь с Wick Drinker;
Lintel не комбинируется с Briar Choir, одинаковые пары запрещены. Текущие телеграфы сериализуют
удары, обычные враги рядом откладывают horror-атаки. Повторная оценка учитывает геометрию:
нужна сухая доступная последовательность шагов длиной 3,6 блока хотя бы в одном направлении.
Это консервативная эвристика, не универсальный solver всех возможных ситуаций.

Правила расследования: отвлечение — запись Well / эксперимент с Rattleblind или Unseam;
раскрытие — запись Chapel / линза или Hearth Mimic; связывание — запись Archive / повторяемый
эксперимент на раскрытом существе. Все три используются в финальном бою. Фонарь возвращается
через Shift + камертон; Creative-фонари, наблюдения и подавление не засчитываются в кампанию.
Настройка shake включает только короткий наклон камеры до 0,6 градуса; flashing отсутствует.
