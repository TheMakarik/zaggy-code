namespace ZaggyCode.Avalonia.ViewModels.Common.Records;

//#:NO_AI
public sealed record ToastAction(string Text, ToastActionKind Kind = ToastActionKind.Default, System.Action? Click = null);
