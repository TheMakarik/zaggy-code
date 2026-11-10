using ZaggyCode.Modules.LanguageRuntime.Python;

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

        if (_runners.TryGetValue(dto.Language, out var existingRunner))
            return existingRunner;

        var runner = CreateRunner(dto);
        _runners[dto.Language] = runner;
        return runner;
    }

    private ILanguageRunner CreateRunner(LanguageRunnerDto dto) => dto.Language switch
    {
        Language.Python => ActivatorUtilities.CreateInstance<PythonLanguageRunner>(serviceProvider, dto),
        _ => throw new NotSupportedException($"Язык {dto.Language} пока не поддерживается.")
    };
}
