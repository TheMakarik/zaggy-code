namespace ZaggyCode.Core.Theming.Interfaces;

public interface IThemeEditorService : INotifyPropertyChanged
{
    string? ThemeName { get; }

    bool IsSystemTheme { get; }

    bool IsDirty { get; }

    IReadOnlyList<ThemeColorEntry> Colors { get; }

    Task BeginEditAsync(string themeName, CancellationToken token = default);

    void SetColor(string propertyName, string hex);

    void ResetColor(string propertyName);

    void Revert();

    Task SaveAsync(CancellationToken token = default);
}
