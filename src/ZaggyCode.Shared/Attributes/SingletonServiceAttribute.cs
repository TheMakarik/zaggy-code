namespace ZaggyCode.Shared.Attributes;

[Obsolete("Аттрибуты для Scrutor устарели. Изучили коментарий в Bootstrapper.cs для миграции")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SingletonServiceAttribute : Attribute;
