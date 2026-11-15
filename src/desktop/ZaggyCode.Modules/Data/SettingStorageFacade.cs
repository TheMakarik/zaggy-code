namespace ZaggyCode.Modules.Data;

//#:NO_AI
public sealed class SettingStorageFacade(
    ISettingsStorage<StateSettings> stateSettingsStorage,
    ISettingsStorage<ViewSettings> viewSettingsStorage,
    ISettingsStorage<ThemeSettings> themeSettingsStorage,
    ISettingsStorage<PythonSettings> pythonSettingsStorage,
    ISettingsStorage<CSharpSettings> csharpSettingsStorage,
    IGameCodeStorage gameCodeStorage,
    IBackgroundTaskHelper backgroundTaskHelper,
    ILogger<SettingStorageFacade> logger) : ISettingStorageFacade
{
    public async Task LoadAllAsync()
    {
        // Код игр читает LastGamePath из StateSettings, поэтому загружается после настроек.
        await Task.WhenAll(SettingsTargets.Select(target => target.Storage.LoadAsync()));
        await gameCodeStorage.LoadAsync();
    }

    public void FlashAllAtBackground()
        => StartBackground(
            BackgroundTaskKind.FlushingAllSettings,
            static target => target.FlashKind,
            "сохранение",
            static target => target.Storage.FlushAsync());

    public void RevertAllAtBackground()
        => StartBackground(
            BackgroundTaskKind.RevertingAllSettings,
            static target => target.RevertKind,
            "отмена изменений",
            static target => RevertTarget(target));

    public void ToDefaultAllAtBackground()
        => StartBackground(
            BackgroundTaskKind.ToDefaultAllSettings,
            static target => target.DefaultKind,
            "сброс к значениям по умолчанию",
            static target => target.Storage.ToDefaultAsync());

    private IReadOnlyList<SettingsStorageTarget> SettingsTargets =>
    [
        new(
            nameof(StateSettings),
            BackgroundTaskKind.FlushingStateSettings,
            BackgroundTaskKind.RevertingStateSettings,
            BackgroundTaskKind.ToDefaultStateSettings,
            stateSettingsStorage),
        new(
            nameof(ViewSettings),
            BackgroundTaskKind.FlushingViewSettings,
            BackgroundTaskKind.RevertingViewSettings,
            BackgroundTaskKind.ToDefaultViewSettings,
            viewSettingsStorage),
        new(
            nameof(ThemeSettings),
            BackgroundTaskKind.FlushingThemeSettings,
            BackgroundTaskKind.RevertingThemeSettings,
            BackgroundTaskKind.ToDefaultThemeSettings,
            themeSettingsStorage),
        new(
            nameof(PythonSettings),
            BackgroundTaskKind.FlushingPythonSettings,
            BackgroundTaskKind.RevertingPythonSettings,
            BackgroundTaskKind.ToDefaultPythonSettings,
            pythonSettingsStorage),
        new(
            nameof(CSharpSettings),
            BackgroundTaskKind.FlushingCSharpSettings,
            BackgroundTaskKind.RevertingCSharpSettings,
            BackgroundTaskKind.ToDefaultCSharpSettings,
            csharpSettingsStorage)
    ];

    private void StartBackground(
        BackgroundTaskKind allKind,
        Func<SettingsStorageTarget, BackgroundTaskKind> kindSelector,
        string operation,
        Func<SettingsStorageTarget, ValueTask> action)
    {
        backgroundTaskHelper.StartBackground(new BackgroundTaskStartDto
        {
            Kind = allKind,
            DelegateToStart = progress => RunAtBackgroundAsync(progress, kindSelector, operation, action)
        });
    }

    private async Task RunAtBackgroundAsync(
        IProgress<BackgroundProgressMove> progress,
        Func<SettingsStorageTarget, BackgroundTaskKind> kindSelector,
        string operation,
        Func<SettingsStorageTarget, ValueTask> action)
    {
        var targets = SettingsTargets;
        var completed = 0;

        foreach (var target in targets)
        {
            await RunTargetAsync(target, operation, action);

            completed++;
            progress.Report(new BackgroundProgressMove(
                GetPercent(completed, targets.Count),
                kindSelector(target),
                $"{target.Name}: {operation} ({completed} из {targets.Count})"));
        }
    }

    private async Task RunTargetAsync(
        SettingsStorageTarget target,
        string operation,
        Func<SettingsStorageTarget, ValueTask> action)
    {
        try
        {
            await action(target);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при выполнении операции «{operation}» для {name}", operation, target.Name);
        }
    }

    private static ValueTask RevertTarget(SettingsStorageTarget target)
    {
        target.Storage.Revert();
        return ValueTask.CompletedTask;
    }

    private static double GetPercent(int completed, int total)
        => total == 0 ? 100 : completed * 100d / total;
}
