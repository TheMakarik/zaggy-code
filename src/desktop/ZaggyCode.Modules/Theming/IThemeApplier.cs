namespace ZaggyCode.Modules.Theming;

public interface IThemeApplier
{
    Task ApplySavedThemeAsync();
    Task ApplyThemeAsync(string name);
}
