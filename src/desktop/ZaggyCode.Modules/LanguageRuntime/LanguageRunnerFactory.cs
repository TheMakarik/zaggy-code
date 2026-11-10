using ZaggyCode.Modules.LanguageRuntime.Python;

namespace ZaggyCode.Modules.LanguageRuntime;

public sealed class LanguageRunnerFactory(IServiceProvider serviceProvider) : ILanguageRunnerFactory
{
    public ILanguageRunner Create(LanguageRunnerDto dto) => dto.Language switch
    {
        Language.Python => ActivatorUtilities.CreateInstance<PythonLanguageRunner>(serviceProvider, dto),
        _ => throw new NotSupportedException($"Язык {dto.Language} пока не поддерживается.")
    };
}
