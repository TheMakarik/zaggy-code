namespace ZaggyCode.Core.Languages.Model;

//Данные которые не поменяются при смене языка или перезапуске 
public sealed record GlobalLanguageRunnerData(TextWriter Writer, TextReader Reader);
