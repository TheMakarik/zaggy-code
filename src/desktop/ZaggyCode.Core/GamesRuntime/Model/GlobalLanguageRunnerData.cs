namespace ZaggyCode.Core.GamesRuntime.Model;

//Данные которые не поменяются при смене языка
public sealed record GlobalLanguageRunnerData(TextWriter Writer, TextReader Reader, IRobotExecutor RobotExecutor);
