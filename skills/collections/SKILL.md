# Коллекции

## Тип в сигнатурах: не отдавай лишнего

Выбирай тип параметра/возврата по тому, что реально нужно потребителю:

- **Только чтение** (перебрать, посчитать, прочитать по индексу) — `IReadOnlyCollection<T>`, а если нужен доступ по индексу — `IReadOnlyList<T>`.
- **Только LINQ-цепочка** — `IEnumerable<T>`, но только если перечисление гарантированно одноразовое.
- **Возможно повторное перечисление** (несколько LINQ-операторов, `Count()`/`Any()` + `foreach`, повторный `foreach`) — `ICollection<T>`/`IReadOnlyCollection<T>` (или один раз материализуй), чтобы не было двойного перечисления.
- **Изменение** (`Add`/`Remove`/`Clear`) — `ICollection<T>`/`IList<T>`/`IDictionary<TKey,TValue>` либо конкретный тип, если он нужен снаружи.

Не отдавай `List<T>`/`Dictionary<TKey,TValue>`, если потребителю достаточно интерфейса.

```csharp
// Хорошо — коллекция только читается
public IReadOnlyCollection<ThemeColorEntry> Colors { get; }

// Хорошо — одноразовое LINQ-перечисление
public IEnumerable<ThemeMetadata> Themes => _cache.Values;

// Хорошо — перечисление возможно несколько раз
public IReadOnlyCollection<ThemeMetadata> Themes { get; }

// Плохо — наружу отдаётся конкретный List без необходимости
public List<ThemeColorEntry> Colors { get; }
```

## Двойное перечисление

Если по `IEnumerable<T>` идёт больше одного прохода (несколько LINQ, `Any()` + `foreach`, `Count()` + доступ), материализуй один раз или прими `IReadOnlyCollection<T>`.

```csharp
// Плохо — source перечисляется дважды
if (source.Any())
    foreach (var item in source)
        Process(item);

// Хорошо
var items = source as IReadOnlyCollection<Item> ?? source.ToList();
if (items.Count > 0)
    foreach (var item in items)
        Process(item);
```

## Пустые коллекции и collection expressions

Для пустых коллекций используй `[]` вместо `new List<T>()` и `Array.Empty<T>()`.

Collection expressions (`[...]`) — исключение из правила `var`: указывай явный тип, чтобы читатель сразу видел вид коллекции.

```csharp
// Хорошо
int[] numbers = [1, 2, 3];
List<string> items = [];
Dictionary<int, string> map = [];
Span<char> buffer = ['a', 'b', 'c'];

// Плохо
var numbers = [1, 2, 3];
var items = new List<string>();
var map = new Dictionary<int, string>();
```

## Потокобезопасные коллекции

Обычные `List<T>`/`Dictionary<TKey,TValue>` не потокобезопасны. Если коллекция читается/пишется из нескольких потоков:

- `ConcurrentDictionary<TKey,TValue>`, `ConcurrentQueue<T>`, `ConcurrentBag<T>`, `ConcurrentStack<T>` — параллельный доступ без внешней блокировки.
- `ImmutableArray<T>`/`ImmutableDictionary<TKey,TValue>` — когда нужно отдавать неизменяемый снимок.
- `FrozenDictionary<TKey,TValue>`/`FrozenSet<T>` — справочники, созданные один раз и только читаемые (быстрее обычных).
- `BlockingCollection<T>`/`Channel<T>` — producer/consumer.

Не оборачивай обычную коллекцию в `lock`, если подходит concurrent/immutable.

```csharp
// Хорошо — параллельные записи
private readonly ConcurrentDictionary<string, ThemeMetadata> _themes = new(StringComparer.Ordinal);

// Плохо — lock поверх обычного словаря, когда есть ConcurrentDictionary
private readonly Dictionary<string, ThemeMetadata> _themes = [];
private readonly object _gate = new();
```

## LINQ

- Используй LINQ для декларативных преобразований, но не злоупотребляй: если запрос становится нечитаемым, разбей его или используй цикл.
- Материализуй (`ToList`/`ToArray`) осознанно: это один проход, но и аллокация.
- Не смешивай LINQ с побочными эффектами в `Select`.

## Чек-лист

- [ ] Тип параметра/возврата минимально достаточный: `IReadOnlyCollection`/`IReadOnlyList` для чтения, `ICollection`/`IList`/`IDictionary` для изменения.
- [ ] Нет двойного перечисления `IEnumerable<T>`.
- [ ] Пустые коллекции — `[]`, не `new List<T>()`/`Array.Empty<T>()`.
- [ ] Collection expressions — с явным типом.
- [ ] Для многопоточного доступа — concurrent/immutable/frozen коллекция, а не `lock` поверх `List<T>`.
