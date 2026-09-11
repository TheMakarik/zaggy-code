
# Добавление цветов в Colors.axaml

## Главное
`src/desktop/ZaggyCode.Avalonia/Views/Styles/Colors.axaml` — это **клон темы Primus** (дефолтные значения до применения темы). Любой цвет, добавленный сюда, должен совпадать со значением Primus в `dev/scripts/generate-themes.cs`.

## Обязательный чек-лист
1. **Убедиться, что Colors.axaml соответствует Primus.** Сравнить значения ключей в `Colors.axaml` с аргументами темы `Primus` в `generate-themes.cs`. Расхождения — исправить.
2. **Добавить цвет.** Пара ресурсов, имя по конвенции:
   ```xml
   <Color x:Key="FooColor">#RRGGBB</Color>
   <SolidColorBrush Color="{DynamicResource FooColor}" x:Key="FooBrush" />
   ```
   Нейминг под рефлексию `ThemeApplier`: свойство `FooColor` в `Theme` → ресурсы `FooColor` + `FooBrush`; свойство без суффикса `Color` (напр. `GoalColor1`) → `GoalColor1Color` + `GoalColor1Brush`.
3. **Если Colors.axaml изменился — обновить и пересоздать темы:**
   - `ZaggyCode.Core/Theming/Model/Theme.cs` — добавить `required` свойство с `[XmlElement("...")]`.
   - `dev/scripts/generate-themes.cs` — параметр в `CreateTheme` + значение для **всех** тем (Primus обязательно = значению из Colors.axaml).
   - Пересоздать архивы:
     ```bash
     rm -f dev/themes/*.zct
     dotnet run dev/scripts/generate-themes.cs
     rm -f src/desktop/ZaggyCode.Avalonia/Themes/*.zct
     cp dev/themes/*.zct src/desktop/ZaggyCode.Avalonia/Themes/
     ```
   - Обновить тесты, где создаётся `Theme` (`ThemeCreatorTests.CreateTheme`).
4. **Никогда не хардкодить цвета** в контролах/XAML — только `DynamicResource`/ресурсы темы.
5. **Проверка:** `dotnet build` + профильные тесты (`ThemeCreatorTests`, `ThemeCopierTests`).

## Куда не лезть
- Файлы с `//#:NO_AI` — только чтение.
- `ThemeApplier`/`ThemeSetterProxy` менять не нужно: они сами разложат новое свойство `Theme` в ресурсы.

## Связанные скиллы
- `theming` — полный пайплайн тем, хранение/копирование/удаление `.zct`.
