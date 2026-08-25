namespace ZaggyCode.Avalonia.Theming;

public interface IThemeApplier
{
    Task ApplySavedThemeAsync();

    Task ApplyThemeAsync(string name);
}
