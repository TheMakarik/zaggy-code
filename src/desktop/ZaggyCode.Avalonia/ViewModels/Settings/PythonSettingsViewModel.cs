namespace ZaggyCode.Avalonia.ViewModels.Settings;

public sealed partial class PythonSettingsViewModel : ViewModelBase
{
    #region Reactive properties

    [Reactive] private bool _useEntryFunction;
    [Reactive] private string _entryFunctionName = string.Empty;
    [Reactive] private bool _supressIo;
    [Reactive] private PythonFunctionNameValidationResult _entryFunctionNameValidationResult;
    [Reactive] private bool _isSettingsValid;
    [Reactive] private bool _hasChanges;

    #endregion

    private readonly PythonSettings _defaultPythonSettings;
    private readonly ISettingsStorage<PythonSettings> _pythonSettingsStorage;
    private readonly IPythonFunctionNameValidator _pythonFunctionNameValidator;
    private bool _originalUseEntryFunction;
    private string _originalEntryFunctionName = string.Empty;
    private bool _originalSupressIo;

    public PythonSettingsViewModel(
        ISettingsStorage<PythonSettings> pythonSettingsStorage,
        IOptions<PythonDefaultSettingsOptions> pythonDefaultSettingsOptions,
        IPythonFunctionNameValidator pythonFunctionNameValidator)
    {
        _defaultPythonSettings = pythonDefaultSettingsOptions.Value.Settings;
        _pythonSettingsStorage = pythonSettingsStorage;
        _pythonFunctionNameValidator = pythonFunctionNameValidator;

        var current = pythonSettingsStorage.Value;
        _originalUseEntryFunction = current.UseEntryFunction;
        _originalEntryFunctionName = current.EntryFunctionName;
        _originalSupressIo = current.SupressIo;

        LoadFromPythonSettings(current);
        UpdateEntryFunctionNameValidation();
        HasChanges = false;

        this.WhenAnyPropertyChanged(
                nameof(UseEntryFunction),
                nameof(EntryFunctionName),
                nameof(SupressIo))
            .Subscribe(_ => UpdateHasChanges());

        this.WhenAnyValue(viewModel => viewModel.UseEntryFunction)
            .Subscribe(_ => UpdateEntryFunctionNameValidation());

        this.WhenAnyValue(viewModel => viewModel.EntryFunctionName)
            .Subscribe(_ => UpdateEntryFunctionNameValidation());

        pythonSettingsStorage.Value.PropertyChanged += (_, args) => ApplySettingsChange(args.PropertyName);
    }

    private void ApplySettingsChange(string? propertyName)
    {
        var current = _pythonSettingsStorage.Value;

        switch (propertyName)
        {
            case nameof(PythonSettings.UseEntryFunction):
                UseEntryFunction = current.UseEntryFunction;
                break;
            case nameof(PythonSettings.EntryFunctionName):
                EntryFunctionName = current.EntryFunctionName;
                break;
            case nameof(PythonSettings.SupressIo):
                SupressIo = current.SupressIo;
                break;
        }
    }

    public void ResetToDefaults()
    {
        UseEntryFunction = _defaultPythonSettings.UseEntryFunction;
        EntryFunctionName = _defaultPythonSettings.EntryFunctionName;
        SupressIo = _defaultPythonSettings.SupressIo;
    }

    public void AcceptChanges()
    {
        _originalUseEntryFunction = UseEntryFunction;
        _originalEntryFunctionName = EntryFunctionName;
        _originalSupressIo = SupressIo;
        HasChanges = false;
    }

    private void LoadFromPythonSettings(PythonSettings pythonSettings)
    {
        UseEntryFunction = pythonSettings.UseEntryFunction;
        EntryFunctionName = pythonSettings.EntryFunctionName;
        SupressIo = pythonSettings.SupressIo;
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

    private void UpdateHasChanges()
    {
        HasChanges =
            UseEntryFunction != _originalUseEntryFunction ||
            EntryFunctionName != _originalEntryFunctionName ||
            SupressIo != _originalSupressIo;
    }
}
