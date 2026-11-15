namespace ZaggyCode.Avalonia.ViewModels.Settings;

public sealed partial class CSharpSettingsViewModel : ViewModelBase
{
    #region Reactive properties

    [Reactive] private bool _useTopLevelStatements;
    [Reactive] private bool _enableImplicitUsings;
    [Reactive] private bool _blockIo;
    [Reactive] private bool _hasChanges;

    #endregion

    private readonly CSharpSettings _defaultCSharpSettings;
    private readonly ISettingsStorage<CSharpSettings> _csharpSettingsStorage;
    private bool _originalUseTopLevelStatements;
    private bool _originalEnableImplicitUsings;
    private bool _originalBlockIo;

    public CSharpSettingsViewModel(
        ISettingsStorage<CSharpSettings> csharpSettingsStorage,
        IOptions<CSharpDefaultSettingsOptions> csharpDefaultSettingsOptions)
    {
        _defaultCSharpSettings = csharpDefaultSettingsOptions.Value.Settings;
        _csharpSettingsStorage = csharpSettingsStorage;

        var current = csharpSettingsStorage.Value;
        _originalUseTopLevelStatements = current.UseTopLevelStatements;
        _originalEnableImplicitUsings = current.EnableImplicitUsings;
        _originalBlockIo = current.BlockIo;

        LoadFromCSharpSettings(current);
        HasChanges = false;

        this.WhenAnyPropertyChanged(
                nameof(UseTopLevelStatements),
                nameof(EnableImplicitUsings),
                nameof(BlockIo))
            .Subscribe(_ => UpdateHasChanges());

        csharpSettingsStorage.Value.PropertyChanged += (_, args) => ApplySettingsChange(args.PropertyName);
    }

    private void ApplySettingsChange(string? propertyName)
    {
        var current = _csharpSettingsStorage.Value;

        switch (propertyName)
        {
            case nameof(CSharpSettings.UseTopLevelStatements):
                UseTopLevelStatements = current.UseTopLevelStatements;
                break;
            case nameof(CSharpSettings.EnableImplicitUsings):
                EnableImplicitUsings = current.EnableImplicitUsings;
                break;
            case nameof(CSharpSettings.BlockIo):
                BlockIo = current.BlockIo;
                break;
        }
    }

    public void ResetToDefaults()
    {
        UseTopLevelStatements = _defaultCSharpSettings.UseTopLevelStatements;
        EnableImplicitUsings = _defaultCSharpSettings.EnableImplicitUsings;
        BlockIo = _defaultCSharpSettings.BlockIo;
    }

    public void AcceptChanges()
    {
        _originalUseTopLevelStatements = UseTopLevelStatements;
        _originalEnableImplicitUsings = EnableImplicitUsings;
        _originalBlockIo = BlockIo;
        HasChanges = false;
    }

    private void LoadFromCSharpSettings(CSharpSettings csharpSettings)
    {
        UseTopLevelStatements = csharpSettings.UseTopLevelStatements;
        EnableImplicitUsings = csharpSettings.EnableImplicitUsings;
        BlockIo = csharpSettings.BlockIo;
    }

    private void UpdateHasChanges()
    {
        HasChanges =
            UseTopLevelStatements != _originalUseTopLevelStatements ||
            EnableImplicitUsings != _originalEnableImplicitUsings ||
            BlockIo != _originalBlockIo;
    }
}
