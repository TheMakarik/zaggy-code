# Обновление библиотек

## Что обновлять
Версии зависимостей — в `PackageReference` внутри `*.csproj`:

- `src/desktop/ZaggyCode.Avalonia/ZaggyCode.Avalonia.csproj`
- `src/desktop/ZaggyCode.Modules/ZaggyCode.Modules.csproj`
- `src/desktop/ZaggyCode.Core/ZaggyCode.Core.csproj`
- `tests/ZaggyCode.Tests/ZaggyCode.Tests.csproj`

## Обязательное правило
**После любого обновления пакета обнови его версию в списке компонентов окна «О приложении».**

Версии компонентов хранятся не в ассембли и не в коде, а в `appsettings.json`:

```
src/desktop/ZaggyCode.Avalonia/appsettings.json → "AboutOptions" → "Components"
```

У каждого компонента есть поля:

| Поле | Назначение |
|------|-----------|
| `Name` | Отображаемое название |
| `Version` | Версия, совпадающая с `PackageReference` |
| `Description` | Краткое описание компонента |
| `WebsiteUrl` | Необязательный сайт |
| `GitHubUrl` | Необязательный репозиторий |
| `IconFileName` | Имя файла иконки в папке `ComponentsIconsFolder` |

Иконки лежат в `src/desktop/ZaggyCode.Avalonia/Assets/ComponentsIcons`.
Если у пакета появилось/сменилось лого — положи его туда и обнови `IconFileName`.
Для пакетов Microsoft используй `dotnet.svg`, для пакетов без собственного лого — `nuget.svg`.

## Порядок действий
1. Обнови `PackageReference` в нужном `.csproj`.
2. Найди этот пакет в секции `AboutOptions.Components` в `appsettings.json` и обнови `Version`.
3. Если пакета в списке ещё нет — добавь объект компонента и иконку; ставь его в списке по важности, а не по алфавиту.
4. Выполни `dotnet build ZaggyCode.slnx`.
5. Проверь окно «О приложении» → вкладка «Компоненты»: название, версия и кнопки должны отображаться.

## Если иконки нет
`AboutViewModel` проверяет существование файла иконки через `File.Exists`:

- при отсутствии файла компонент логируется как ошибка и **пропускается** (окно не падает);
- поэтому для нового компонента обязательно добавь файл иконки, иначе он не появится в списке.

## Текущие компоненты
Порядок в `appsettings.json` — это порядок отображения (по важности), а не алфавитный.

| Компонент | Пакет | Иконка |
|-----------|-------|--------|
| .NET | платформа | `dotnet.svg` |
| AvaloniaUI | `Avalonia` | `avalonia.png` |
| ReactiveUI | `ReactiveUI.Avalonia` | `reactiveui.png` |
| IronPython | `IronPython` | `ironpython.png` |
| Microsoft.Extensions.Hosting | `Microsoft.Extensions.Hosting` | `dotnet.svg` |
| Serilog | `Serilog` | `serilog.png` |
| Microsoft.CodeAnalysis.Scripting | `Microsoft.CodeAnalysis.Scripting` | `dotnet.svg` |
| Scrutor | `Scrutor` | `nuget.svg` |
| AvalonEdit | `Avalonia.AvaloniaEdit` | `avaloniaedit.png` |
| VirtualTerminal | `VirtualTerminal.Avalonia` | `nuget.svg` |
| Material.Icons.Avalonia | `Material.Icons.Avalonia` | `materialdesignicons.svg` |
