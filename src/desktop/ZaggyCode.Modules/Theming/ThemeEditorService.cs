namespace ZaggyCode.Modules.Theming;

public sealed class ThemeEditorService(
    ILogger<ThemeEditorService> logger,
    IThemeCatalog catalog,
    IThemeCreator creator,
    IThemeSetterProxy setter,
    IOptions<ThemeOptions> themeOptions,
    IOptions<ThemeColorDisplayNameOptions> displayNames)
    : IThemeEditorService
{
    private const string ColorSuffix = "Color";

    private static readonly PropertyInfo[] ColorProperties = typeof(Theme)
        .GetProperties()
        .Where(property => property.PropertyType == typeof(string))
        .ToArray();

    private readonly ThemeOptions _themeOptions = themeOptions.Value;
    private readonly Dictionary<string, string> _displayNames = displayNames.Value.Names;

    private readonly List<ThemeColorEntry> _colors = [];
    private Theme? _theme;
    private ThemeMetadata? _metadata;

    public string? ThemeName => _metadata?.Name;

    public bool IsSystemTheme => _metadata?.IsSystemTheme ?? false;

    public bool IsDirty => _colors.Any(color => color.IsChanged);

    public IReadOnlyList<ThemeColorEntry> Colors => _colors;

    public async Task BeginEditAsync(string themeName, CancellationToken token = default)
    {
        var metadata = (await catalog.GetAvailableThemesAsync(token))
            .FirstOrDefault(theme => theme.Name == themeName)
            ?? throw new InvalidOperationException($"Theme '{themeName}' was not found");

        if (metadata.IsSystemTheme)
            throw new InvalidOperationException($"System theme '{themeName}' cannot be edited");

        var theme = await catalog.LoadThemeAsync(themeName, token)
            ?? throw new InvalidOperationException($"Theme '{themeName}' could not be loaded");

        _metadata = metadata;
        _theme = theme;
        _colors.Clear();

        foreach (var property in ColorProperties)
        {
            var value = (string?)property.GetValue(theme) ?? string.Empty;
            var displayName = _displayNames.GetValueOrDefault(property.Name, property.Name);
            var resourceName = property.Name.EndsWith(ColorSuffix, StringComparison.Ordinal)
                ? property.Name[..^ColorSuffix.Length]
                : property.Name;

            _colors.Add(new ThemeColorEntry(property.Name, resourceName, displayName, value));
        }

        ApplyAll();
        Notify(nameof(ThemeName));
        Notify(nameof(IsSystemTheme));
        Notify(nameof(Colors));
        Notify(nameof(IsDirty));
    }

    public void SetColor(string propertyName, string hex)
    {
        var entry = _colors.FirstOrDefault(color => color.PropertyName == propertyName);
        if (entry is null || _theme is null)
            return;

        entry.Value = hex;
        typeof(Theme).GetProperty(propertyName)?.SetValue(_theme, hex);
        setter.SetColor(entry.ResourceName, hex);
        Notify(nameof(IsDirty));
    }

    public void ResetColor(string propertyName)
    {
        var entry = _colors.FirstOrDefault(color => color.PropertyName == propertyName);
        if (entry is null)
            return;

        SetColor(propertyName, entry.OriginalValue);
    }

    public void Revert()
    {
        foreach (var entry in _colors)
        {
            entry.Value = entry.OriginalValue;
            typeof(Theme).GetProperty(entry.PropertyName)?.SetValue(_theme, entry.OriginalValue);
            setter.SetColor(entry.ResourceName, entry.OriginalValue);
        }

        Notify(nameof(IsDirty));
    }

    public async Task SaveAsync(CancellationToken token = default)
    {
        if (_theme is null || _metadata is null)
            return;

        Directory.CreateDirectory(_themeOptions.ExternThemesFolder);
        await creator.CreateAsync(_theme, BuildMetadata(), _themeOptions.ExternThemesFolder);

        foreach (var entry in _colors)
            entry.MarkSaved();

        logger.LogInformation("Сохранена изменённая тема «{name}»", _metadata.Name);
        Notify(nameof(IsDirty));
    }

    private void ApplyAll()
    {
        foreach (var entry in _colors)
            setter.SetColor(entry.ResourceName, entry.Value);
    }

    private ThemeMetadata BuildMetadata()
    {
        var original = _metadata!;
        var theme = _theme!;

        return new ThemeMetadata
        {
            Name = original.Name,
            Description = original.Description,
            CreatedAtVersion = original.CreatedAtVersion,
            Author = original.Author,
            WindowBackgroundColor = theme.WindowBackgroundColor,
            SidebarBackgroundColor = theme.SidebarBackgroundColor,
            EditorBackgroundColor = theme.EditorBackgroundColor,
            TerminalBackgroundColor = theme.TerminalBackgroundColor,
            AccentColor = theme.AccentColor,
            BorderColor = theme.BorderColor,
            TextPrimaryColor = theme.TextPrimaryColor
        };
    }

    private void Notify([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
