using ZaggyCode.Core.LanguageRuntime.Dto;
using ZaggyCode.Core.LanguageRuntime.Model;

namespace ZaggyCode.Core.LanguageRuntime.Interfaces;

public interface ILanguageRunner : IAsyncDisposable
{
    ExecutionSpeed Speed { get; set; }
    void Configure(LanguageRunnerDto dto);
    IAsyncEnumerable<ComputedLineOfCode> ExecuteAsync(string code, CancellationToken cancellationToken = default);
}
