# Простые условия

## Правило

**Сложное условие обязано быть вынесено в метод с говорящим именем.** Условие в `if`/`while`/`?:`/pattern-выражении должно читаться как утверждение о предметной области, а не как разбор внутренностей.

Сложным считается условие, если выполнено хотя бы одно:

- содержит `&&` или `||`;
- содержит составной pattern matching (`is { A: ..., B: ... }`, `is X and Y`, `is X or Y`, список/свойства в паттерне);
- смысл нельзя понять, прочитав условие в контексте класса — нужны знания о деталях реализации;
- требует комментария, чтобы объяснить, что оно проверяет.

## Как выносить

Условие становится приватным методом (или локальной функцией) с именем-утверждением:

- `CanWrite(property)` → `property.CanWrite`
- `IsSameStorage(other)` → сравнение путей с нормализацией
- `ShouldFlush(version, flushedVersion)` → `version != flushedVersion`
- `HasUnsavedChanges()` → сравнение текущего и сохранённого состояния

```csharp
// Плохо — смысл нужно расшифровывать
if (property is { CanRead: true, CanWrite: true } &&
    property.GetIndexParameters().Length == 0)
    Copy(property);

// Хорошо — имя объясняет смысл
foreach (var property in Properties.GetOrAdd(typeof(T), GetCopyableProperties))
    CopyProperty(source, target, property);

private static PropertyInfo[] GetCopyableProperties(Type type)
    => type.GetProperties().Where(IsCopyable).ToArray();

private static bool IsCopyable(PropertyInfo property)
{
    if (property.GetIndexParameters().Length > 0)
        return false;

    return property is { CanRead: true, CanWrite: true };
}
```

```csharp
// Плохо
if (value is null || (!waitBeforeFlash && _pendingFlash is null))
    return;

// Хорошо
if (ShouldSkipFlash(waitBeforeFlash))
    return;

private bool ShouldSkipFlash(bool waitBeforeFlash)
{
    if (value is null)
        return true;

    return !waitBeforeFlash && _pendingFlash is null;
}
```

## Границы

- Одиночное сравнение (`size > 0`, `name is null`, `flag`) выносить не нужно: оно и так читается.
- Не размножай методы ради одной проверки, если условие действительно простое.
- Если условие повторяется в двух местах — выноси обязательно.
- Метод-предикат не должен иметь побочных эффектов и называться как вопрос: `Is...`, `Can...`, `Has...`, `Should...`.
- Проверки в LINQ (`Where(IsCopyable)`) выносятся в статические методы и переиспользуются.
- Правило распространяется на `switch`-ветки с `when` и на тернарные операторы с составным условием.

## Чек-лист

- [ ] В `if`/`while`/`?:`/`when` нет `&&` и `||`.
- [ ] Нет составного pattern matching в условии.
- [ ] Условие читается без комментария и без знания деталей реализации.
- [ ] Имя метода-предиката отвечает на вопрос, а не описывает механику.
