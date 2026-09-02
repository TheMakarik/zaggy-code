namespace ZaggyCode.Avalonia.ViewModels;

public sealed partial class CSharpSettingsViewModel : ViewModelBase
{
    #region Reactive properties

    [Reactive] private bool _useTopLevelStatements;
    [Reactive] private bool _enableImplicitUsings;
    [Reactive] private bool _blockIo;
    [Reactive] private bool _hasChanges;

    #endregion

    private readonly CSharpSettings _defaultCSharpSettings;
    private bool _originalUseTopLevelStatements;
    private bool _originalEnableImplicitUsings;
    private bool _originalBlockIo;

    public CSharpSettingsViewModel(
        IObservableStorage<CSharpSettings> csharpSettingsStorage,
        IOptions<CSharpDefaultSettingsOptions> csharpDefaultSettingsOptions)
    {
        _defaultCSharpSettings = csharpDefaultSettingsOptions.Value.Settings;

        var current = csharpSettingsStorage.Current;
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
