namespace ZaggyCode.Core.Languages.Interfaces;

public interface ILanguageCache
{
    public TextReader Input { get; set; }
    public TextWriter Output { get; set; }
    public ExecutionSpeed Speed { get; set; }
    public IRobotExecutor Executor { get; set; }
    
    public ILanguageRunner Get(Language language);
}