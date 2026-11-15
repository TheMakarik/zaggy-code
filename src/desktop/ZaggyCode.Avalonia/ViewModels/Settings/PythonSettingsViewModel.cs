namespace ZaggyCode.Avalonia.ViewModels.Settings;

public sealed partial class PythonSettingsViewModel : ViewModelBase
{
    private readonly ISettingsStorage<PythonSettings> _pythonSettingsStorage;
    private readonly IPythonFunctionNameValidator _pythonFunctionNameValidator;

    [Reactive] private PythonFunctionNameValidationResult _entryFunctionNameValidationResult;
    [Reactive] private bool _isSettingsValid;

    public PythonSettingsViewModel(
        ISettingsStorage<PythonSettings> pythonSettingsStorage,
        IPythonFunctionNameValidator pythonFunctionNameValidator)
    {
        _pythonSettingsStorage = pythonSettingsStorage;
        _pythonFunctionNameValidator = pythonFunctionNameValidator;

        pythonSettingsStorage.Value.PropertyChanged += (_, _) =>
        {
            this.RaisePropertyChanged(nameof(HasChanges));
            ApplySettingsChange();
        };

        UpdateEntryFunctionNameValidation();
    }

    public bool HasChanges => _pythonSettingsStorage.HasChanges;

    public bool UseEntryFunction
    {
        get => _pythonSettingsStorage.Value.UseEntryFunction;
        set => SetSetting(settings => settings.UseEntryFunction = value);
    }

    public string EntryFunctionName
    {
        get => _pythonSettingsStorage.Value.EntryFunctionName;
        set => SetSetting(settings => settings.EntryFunctionName = value);
    }

    public bool SupressIo
    {
        get => _pythonSettingsStorage.Value.SupressIo;
        set => SetSetting(settings => settings.SupressIo = value);
    }

    private void SetSetting(Action<PythonSettings> update, [CallerMemberName] string? propertyName = null)
    {
        update(_pythonSettingsStorage.Value);
        this.RaisePropertyChanged(propertyName);
        UpdateEntryFunctionNameValidation();
    }

    private void ApplySettingsChange()
    {
        this.RaisePropertyChanged(nameof(UseEntryFunction));
        this.RaisePropertyChanged(nameof(EntryFunctionName));
        this.RaisePropertyChanged(nameof(SupressIo));
        UpdateEntryFunctionNameValidation();
    }

    private void UpdateEntryFunctionNameValidation()
    {
        if (!UseEntryFunction)
        {
            EntryFunctionNameValidationResult = PythonFunctionNameValidationResult.Success;
            IsSettingsValid = true;
            return;
        }

        EntryFunctionNameValidationResult = _pythonFunctionNameValidator.Validate(EntryFunctionName);
        IsSettingsValid = EntryFunctionNameValidationResult == PythonFunctionNameValidationResult.Success;
    }
}
