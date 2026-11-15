namespace ZaggyCode.Core.Theming.Interfaces;

public interface IThemeDialogProxy
{
    Task ShowThemeNotFoundDialogAsync(string missingThemeName);
}
