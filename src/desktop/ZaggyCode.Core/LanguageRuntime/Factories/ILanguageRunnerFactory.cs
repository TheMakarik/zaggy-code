namespace ZaggyCode.Core.LanguageRuntime.Factories;

public interface ILanguageRunnerFactory
{
    ILanguageRunner Create(LanguageRunnerDto dto);
}
