namespace ZaggyCode.Avalonia.ViewModels.Settings;

public sealed partial class CSharpSettingsViewModel : ViewModelBase
{
    private readonly ISettingsStorage<CSharpSettings> _csharpSettingsStorage;

    public CSharpSettingsViewModel(ISettingsStorage<CSharpSettings> csharpSettingsStorage)
    {
        _csharpSettingsStorage = csharpSettingsStorage;
        csharpSettingsStorage.Value.PropertyChanged += (_, _) =>
        {
            this.RaisePropertyChanged(nameof(HasChanges));
            this.RaisePropertyChanged(nameof(UseTopLevelStatements));
            this.RaisePropertyChanged(nameof(EnableImplicitUsings));
            this.RaisePropertyChanged(nameof(BlockIo));
        };
    }

    public bool HasChanges => _csharpSettingsStorage.HasChanges;

    public bool UseTopLevelStatements
    {
        get => _csharpSettingsStorage.Value.UseTopLevelStatements;
        set => SetSetting(settings => settings.UseTopLevelStatements = value);
    }

    public bool EnableImplicitUsings
    {
        get => _csharpSettingsStorage.Value.EnableImplicitUsings;
        set => SetSetting(settings => settings.EnableImplicitUsings = value);
    }

    public bool BlockIo
    {
        get => _csharpSettingsStorage.Value.BlockIo;
        set => SetSetting(settings => settings.BlockIo = value);
    }

    private void SetSetting(Action<CSharpSettings> update, [CallerMemberName] string? propertyName = null)
    {
        update(_csharpSettingsStorage.Value);
        this.RaisePropertyChanged(propertyName);
    }
}
