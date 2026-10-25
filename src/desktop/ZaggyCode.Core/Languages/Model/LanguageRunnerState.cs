namespace ZaggyCode.Core.Languages.Model;

//Состояние раннера языка, может поменяться в любой момент, будь то во время выполнения кода,
//будь то перед запуском,
//раннер обязан принять эти изменения
public sealed record LanguageRunnerState(ExecutionSpeed Speed);