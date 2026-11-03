using ZaggyCode.Core.LanguageRuntime.Dto;
using ZaggyCode.Core.LanguageRuntime.Interfaces;

namespace ZaggyCode.Core.LanguageRuntime.Factories;

public interface ILanguageRunnerFactory
{
    ILanguageRunner Create(LanguageRunnerDto dto);
}
