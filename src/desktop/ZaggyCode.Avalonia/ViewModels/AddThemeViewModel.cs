using System.Collections;
using System.ComponentModel;

namespace ZaggyCode.Avalonia.ViewModels;

public sealed partial class AddThemeViewModel : ViewModelBase, INotifyDataErrorInfo
{
    private readonly IReadOnlyCollection<string> _existingNames;

    [Reactive] private string _themeName = string.Empty;
    [Reactive] private bool _canConfirm;

    public Interaction<string?, Unit> CloseInteraction { get; } = new();

    public string ZaggyQuestionPath { get; }

    public AddThemeViewModel(IReadOnlyCollection<string> existingNames, string zaggyQuestionPath)
    {
        _existingNames = existingNames;
        ZaggyQuestionPath = zaggyQuestionPath;

        this.WhenAnyValue(viewModel => viewModel.ThemeName)
            .Subscribe(_ =>
            {
                CanConfirm = Validate(ThemeName) is null;
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(ThemeName)));
            });
    }

    private string? Validate(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Введите название темы";

        return _existingNames.Contains(name.Trim(), StringComparer.OrdinalIgnoreCase)
            ? "Тема с таким названием уже существует"
            : null;
    }

    public bool HasErrors => Validate(ThemeName) is not null;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        string[] errors = propertyName == nameof(ThemeName) && Validate(ThemeName) is { } error
            ? [error]
            : [];

        return errors;
    }

    [ReactiveCommand(CanExecute = nameof(CanConfirm))]
    private async Task Confirm()
        => await CloseInteraction.Handle(ThemeName.Trim());

    [ReactiveCommand]
    private async Task Cancel()
        => await CloseInteraction.Handle(null);
}
