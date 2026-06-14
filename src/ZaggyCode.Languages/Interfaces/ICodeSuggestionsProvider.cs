namespace ZaggyCode.Languages.Interfaces;

public interface ICodeSuggestionsProvider
{
    public IEnumerable<string> GetSuggestions(string line, string code);
}