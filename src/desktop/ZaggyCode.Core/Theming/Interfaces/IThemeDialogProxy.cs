namespace ZaggyCode.Core.Theming.Interfaces;

//#:NO_AI
public interface IThemeDialogProxy
{
    Task ShowThemeNotFoundDialogAsync(string missingThemeName);
}
