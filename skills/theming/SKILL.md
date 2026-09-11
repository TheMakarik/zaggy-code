
# Редактирование тем

## Пайплайн цвета
Цвет темы проходит цепочку:
`Theme` (Core) → `theme.xml` (внутри .zct) → `ThemeApplier` (рефлексия по строковым свойствам `Theme`) → `ThemeSetterProxy.SetColor` → ресурсы `{имя}Color` + `{имя}Brush`.

`ThemeApplier` отражает `typeof(Theme).GetProperties()` где тип свойства — `string`, и вызывает:
- `SetColor(resourceName, hex)` где `resourceName` — имя свойства, у которого отрезан суффикс `Color`, если он есть.

> Следствие для нейминга: свойство `MapWallColor` даёт ресурсы `MapWallColor`/`MapWallBrush`. Свойство без суффикса `Color` (например `GoalColor1`) даёт ресурсы `GoalColor1Color`/`GoalColor1Brush`.

## Добавление нового цвета темы
1. **Модель** `src/desktop/ZaggyCode.Core/Theming/Model/Theme.cs` — объяви `required` строковое свойство и `[XmlElement("...")]`.
2. **Генератор** `dev/scripts/generate-themes.cs` — добавь параметр в `CreateTheme` и передай его в инициализатор `Theme`, затем задай значение для каждой из 4 тем (Primus, Tero, Viola, Brilla).
3. **Default-ресурсы** `src/desktop/ZaggyCode.Avalonia/Views/Styles/Colors.axaml` — добавь `<Color x:Key="XColor">` и `<SolidColorBrush x:Key="XBrush" Color="{DynamicResource XColor}"/>`. Это «клон Primus», значения должны совпадать с Primus.
4. **Регенерация архивов** — см. ниже.

## Регенерация тем (.zct)
Архивы лежат в двух местах: `dev/themes` (выход генератора) и `src/desktop/ZaggyCode.Avalonia/Themes` (runtime: `%ZAGGY_APP%/Themes`). При изменении скрипта — удали старые и создай заново:

```bash
rm -f dev/themes/*.zct
dotnet run dev/scripts/generate-themes.cs
rm -f src/desktop/ZaggyCode.Avalonia/Themes/*.zct
cp dev/themes/*.zct src/desktop/ZaggyCode.Avalonia/Themes/
```

Проверка содержимого: `tar -tjf src/desktop/ZaggyCode.Avalonia/Themes/Primus.zct` → внутри `meta.json` + `theme.xml`.

## Структура архива
- `theme.xml` — сериализованный `Theme` (`XmlSerializer<Theme>`, корневой элемент `<theme>`).
- `meta.json` — `ThemeMetadata` (используется для карточки темы в настройках; поля — превью-цвета).

## Хранение и доступность
- `SystemThemesFolder` (`%ZAGGY_APP%/Themes`) — системные темы из сборки, `IsSystemTheme == true`.
- `ExternThemesFolder` (`%ZAGGY_DATA%/themes`) — пользовательские (скопированные), `IsSystemTheme == false`. Их можно удалять/редактировать.
- `ThemeCatalog` — перечисление/загрузка; `ThemeCopier` — копирование; `ThemeDeleter` — удаление (запрещает системные темы).

## Регистрация в DI
Сервисы `ThemeCatalog`/`ThemeCopier`/`ThemeDeleter` (Modules) попадают в DI автоматически через `Scrutor`-скан в `Bootstrapper.cs` как **Singleton** (`AsImplementedInterfaces`). Интерфейсы расположены в `ZaggyCode.Core.Theming.Interfaces`; `IThemeDeleter` подключается в `AppearanceSettingsViewModel` через цепочку `MainWindowViewModel → SettingsViewModel → AppearanceSettingsViewModel`.

## Применение темы
`ThemeApplier.ApplyThemeAsync(name)` загружает `.zct`, читает `Theme` и проставляет цветовые ресурсы. Если темы нет — фолбэк на `Primus` (`ThemeApplier.FallbackThemeName`) и показывается `ThemeNotFoundWindow`.
