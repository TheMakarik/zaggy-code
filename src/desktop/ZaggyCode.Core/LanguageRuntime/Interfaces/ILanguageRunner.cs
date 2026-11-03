using ZaggyCode.Core.LanguageRuntime.Model;

namespace ZaggyCode.Core.LanguageRuntime.Interfaces;

public interface ILanguageRunner : IAsyncDisposable
{
    ExecutionSpeed Speed { get; set; }

    IAsyncEnumerable<ComputedLineOfCode> ExecuteAsync(string code, CancellationToken cancellationToken = default);
}
