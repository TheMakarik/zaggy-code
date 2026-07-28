# AGENTS.md

## Что это за проект

**ZaggyCode** — desktop-приложение-аналог **КуМира** для обучения основам программирования.
Пользователь пишет код на выбранном языке (C#, Python и расширяемом наборе раннеров), запускает его и видит результат на игровом поле с исполнителем-роботом.

## Архитектура

Проект разбит на три слоя:

| Проект | Ответственность |
|--------|-----------------|
| `ZaggyCode.Core.Contracts` | Абстракции: интерфейсы, модели, enum, события, атрибуты. Не зависит ни от чего. |
| `ZaggyCode.Core` | Реализации модулей: `Data` (хранение), `Game` (исполнитель/робот), `Languages` (раннеры языков). Зависит только от `Core.Contracts`. |
| `ZaggyCode.Avalonia` | UI на Avalonia + композиция приложения. Зависит от `Core` и `Core.Contracts`, содержит `Bootstrapper` и регистрацию DI. |
| `ZaggyCode.Core.Tests` | Юнит-тесты на `Core`. |

### DI и композиция

Регистрация зависимостей находится в `ZaggyCode.Avalonia` (`Bootstrapper` + `DependencyInjection/ZaggyCodeAvaloniaServiceCollectionExtensions.cs`).
Используется **Scrutor** для сканирования сборок:

- `ViewModel`-ы (`AssignableTo<ViewModelBase>`) → `Singleton`, `AsSelf`.
- Типы, реализующие `IDisposable`/`IAsyncDisposable` → `Scoped`, `AsImplementedInterfaces`.
- Типы с `LanguageExtensionAttribute` → `Scoped`, `AsImplementedInterfaces`, keyed по расширению языка.
- Остальные сервисы → `Singleton`, `AsImplementedInterfaces`.

Настройки (`IOptions<T>`) биндятся из `appsettings.json` через `AddOptions<T>`.

## Стиль кода

### `var` по умолчанию

- В новых классах и при изменении существующих **всегда используй `var`**, если тип очевиден из правой части.
- Исключение — **collection expressions**: пиши явный тип коллекции, чтобы читатель видел, что это за коллекция.

  ```csharp
  // Хорошо
  var user = await storage.LoadAsync();
  var name = user.Name;

  int[] numbers = [1, 2, 3];
  List<string> items = [];
  Dictionary<int, string> map = [];
  ```

### Лаконичность и синтаксический сахар

- Код должен быть компактным: избегай лишней вложенности, ранних `return` там, где они упрощают чтение, и избыточных скобок.
- Используй современный C#:
  - primary constructors,
  - pattern matching,
  - switch expressions,
  - null-conditional operators,
  - collection expressions,
  - target-typed `new`,
  - is место == при проверке на null
  - file-scoped namespaces.

### Однострочные конструкции

- Если тело `if`/`else`/`for`/`while`/`foreach` состоит из одной строки, **не оборачивай его в фигурные скобки**.

  ```csharp
  if (count > 0)
      Process(count);

  foreach (var item in items)
      writer.Write(item);
  ```

### Исключения

- Если класс полностью написан без `var` (явные типы везде), при изменении такого класса **поддерживай его стиль**, не внедряй `var` насильно.
- Правила про `{}` и лаконичность применяются в разумных пределах: если класс использует исключительно блочный стиль, не ломай его ради единообразия.
