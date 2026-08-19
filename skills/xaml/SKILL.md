# XAML разработка

## Структура UI-папок в ZaggyCode.Avalonia
```
Views/
├── Behaviors/          # Поведения для взаимодействия (TextEditorZoomBehavior, TerminalZoomBehavior)
├── Converters/         # Конвертеры как MarkupExtension
├── Controls/           # Кастомные контролы (MapView)
├── UserControls/       # Составные контролы
├── Styles/             # Стили и темы
│   ├── Colors.axaml    # Цветовые ресурсы
│   └── Controls.axaml  # Стили контролов
├── Dialogs/            # Диалоговые окна
├── Utils/              # UI-утилиты
└── TerminalEngine/     # Реализация xterm движка (VirtualTerminal.Avalonia)
```

## Форматирование по XAML Styler

### Атрибуты
- Каждый атрибут на новой строке
- Порядок: `x:Class`, `xmlns`, `x:DataType`, остальные атрибуты

```xml
<rxui:ReactiveWindow
    x:Class="ZaggyCode.Avalonia.Views.MainWindow"
    x:DataType="vm:MainWindowViewModel"
    xmlns="https://github.com/avaloniaui"
    xmlns:rxui="http://reactiveui.net"
    xmlns:vm="using:ZaggyCode.Avalonia.ViewModels"
    xmlns:converters="clr-namespace:ZaggyCode.Avalonia.Views.Converters"
    Background="{DynamicResource BackgroundBrush}"
    Foreground="{DynamicResource ForegroundBrush}"
    Title="Программирование с Загги">
```

### Grid с ColumnDefinitions и RowDefinitions
```xml
<Grid ColumnDefinitions="Auto,*,Auto" RowDefinitions="Auto,Auto,*">
```

### Элементы внутри Grid
```xml
<StackPanel
    Grid.Column="0"
    Grid.Row="0"
    Orientation="Horizontal"
    Spacing="12">
```

## Использование стилей
Выноси повторяющуюся логику в стили:

```xml
<!-- В ресурсах -->
<Style Selector="Button.icon-button">
    <Setter Property="Width" Value="32" />
    <Setter Property="Height" Value="32" />
    <Setter Property="Padding" Value="0" />
    <Setter Property="Background" Value="Transparent" />
</Style>

<Style Selector="Button.icon-button.window-control">
    <Setter Property="Width" Value="36" />
    <Setter Property="Height" Value="36" />
</Style>
```

### Использование
```xml
<Button Classes="icon-button run-button" Command="{Binding ExecuteCodeCommand}">
    <materialIcons:MaterialIcon Kind="Play" />
</Button>
```

## Behaviors для сложной логики
```xml
<avaloniaEdit:TextEditor>
    <Interaction.Behaviors>
        <behaviors:TextEditorZoomBehavior
            MaxFontSize="{Binding MaxFontSize}"
            MinFontSize="{Binding MinFontSize}"
            UpdateFontSizeCommand="{Binding UpdateFontSizeCommand}"
            ZoomStep="1" />
    </Interaction.Behaviors>
</avaloniaEdit:TextEditor>
```

## Адаптивность через конвертеры
```xml
<Menu IsVisible="{Binding Bounds.Width, Converter={converters:WidthToVisibilityConverter}, ConverterParameter='>=900'}">
    <!-- Полное меню -->
</Menu>

<Menu IsVisible="{Binding Bounds.Width, Converter={converters:WidthToVisibilityConverter}, ConverterParameter='<900'}">
    <!-- Компактное меню -->
</Menu>
```

## Конвертеры как MarkupExtension
Всегда используй `MarkupExtension` для конвертеров:

```xml
<!-- Хорошо -->
<TextBlock IsVisible="{Binding ShowSidebar, Converter={converters:InverseBooleanConverter}}" />

<!-- Плохо — через ресурсы -->
<TextBlock IsVisible="{Binding ShowSidebar, Converter={StaticResource InverseBooleanConverter}}" />
```
