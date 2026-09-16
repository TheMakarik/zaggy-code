namespace ZaggyCode.Core.Theming.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<ThemeCreationBehavior>))]
public enum ThemeCreationBehavior
{
    OpenEditorAndApply,
    ApplyOnly,
    Nothing
}
