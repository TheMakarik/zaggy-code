namespace ZaggyCode.Core.LanguageRuntime.Model;

//Данные которые не поменяются при смене языка
public sealed record GlobalLanguageRunnerData(TextWriter Writer, TextReader Reader);
