namespace ZaggyCode.Modules.LanguageRuntime;

// Раннеры создаются один раз на язык: IronPython-движок и модуль robot
// не должны перезагружаться между запусками кода.
public sealed class LanguageRunnerFactory(IServiceProvider serviceProvider) : ILanguageRunnerFactory
{
    private readonly Lock _lock = new();
    private readonly Dictionary<Language, ILanguageRunner> _runners = [];

    public ILanguageRunner Create(LanguageRunnerDto dto)
    {
        using var scope = _lock.EnterScope();

        if (!_runners.TryGetValue(dto.Language, out var runner))
        {
            runner = serviceProvider.GetRequiredKeyedService<ILanguageRunner>(dto.Language);
            _runners[dto.Language] = runner;
        }

        runner.Configure(dto);
        return runner;
    }
}
