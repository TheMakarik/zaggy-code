namespace ZaggyCode.Core.BackgroundManagement.Enums;

public enum BackgroundTaskKind : byte
{
    GameEngineLoading,
    LanguageExecution,
    ThemingLoading,
    FlushingStateSettings,
    FlushingViewSettings,
    FlushingThemeSettings,
    FlushingPythonSettings,
    FlushingCSharpSettings,
    FlushingAllSettings,
    RevertingStateSettings,
    RevertingViewSettings,
    RevertingThemeSettings,
    RevertingPythonSettings,
    RevertingCSharpSettings,
    RevertingAllSettings,
    ToDefaultStateSettings,
    ToDefaultViewSettings,
    ToDefaultThemeSettings,
    ToDefaultPythonSettings,
    ToDefaultCSharpSettings,
    ToDefaultAllSettings,
}
