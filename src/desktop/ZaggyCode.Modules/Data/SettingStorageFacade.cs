namespace ZaggyCode.Modules.Data;

//#:NO_AI
public sealed class SettingStorageFacade(
    ISettingsStorage<StateSettings> stateSettingsStorage,
    ISettingsStorage<ViewSettings> viewSettingsStorage,
    ISettingsStorage<ThemeSettings> themeSettingsStorage,
    ISettingsStorage<PythonSettings> pythonSettingsStorage,
    ISettingsStorage<CSharpSettings> csharpSettingsStorage,
    IBackgroundTaskHelper backgroundTaskHelper,
    ILogger<SettingStorageFacade> logger) : ISettingStorageFacade
{
    public Task FlashAllAtBackground()
        => StartBackground(
            BackgroundTaskKind.FlushingAllSettings,
            "Сохранение настроек",
            [
                new SettingsStorageStep("Сохранение состояния приложения", stateSettingsStorage.FlushAsync),
                new SettingsStorageStep("Сохранение настроек внешнего вида", viewSettingsStorage.FlushAsync),
                new SettingsStorageStep("Сохранение настроек темы", themeSettingsStorage.FlushAsync),
                new SettingsStorageStep("Сохранение настроек Python", pythonSettingsStorage.FlushAsync),
                new SettingsStorageStep("Сохранение настроек C#", csharpSettingsStorage.FlushAsync)
            ]);

    public Task RevertAllAtBackground()
        => StartBackground(
            BackgroundTaskKind.RevertingAllSettings,
            "Отмена изменений",
            [
                new SettingsStorageStep("Отмена изменений состояния приложения", Revert(stateSettingsStorage)),
                new SettingsStorageStep("Отмена изменений настроек внешнего вида", Revert(viewSettingsStorage)),
                new SettingsStorageStep("Отмена изменений настроек темы", Revert(themeSettingsStorage)),
                new SettingsStorageStep("Отмена изменений настроек Python", Revert(pythonSettingsStorage)),
                new SettingsStorageStep("Отмена изменений настроек C#", Revert(csharpSettingsStorage))
            ]);

    public Task ToDefaultAllAtBackground()
        => StartBackground(
            BackgroundTaskKind.ToDefaultAllSettings,
            "Сброс настроек к значениям по умолчанию",
            [
                new SettingsStorageStep("Сброс состояния приложения к значениям по умолчанию", stateSettingsStorage.ToDefaultAsync),
                new SettingsStorageStep("Сброс настроек внешнего вида к значениям по умолчанию", viewSettingsStorage.ToDefaultAsync),
                new SettingsStorageStep("Сброс настроек темы к значениям по умолчанию", themeSettingsStorage.ToDefaultAsync),
                new SettingsStorageStep("Сброс настроек Python к значениям по умолчанию", pythonSettingsStorage.ToDefaultAsync),
                new SettingsStorageStep("Сброс настроек C# к значениям по умолчанию", csharpSettingsStorage.ToDefaultAsync)
            ]);

    private Task StartBackground(
        BackgroundTaskKind kind,
        string operation,
        IReadOnlyList<SettingsStorageStep> steps)
        => backgroundTaskHelper.StartBackground(new BackgroundTaskStartDto
        {
            Kind = kind,
            HasNoProgress = false,
            DelegateToStart = progress => RunSequentiallyAsync(progress, kind, operation, steps)
        }).Task;

    private async Task RunSequentiallyAsync(
        IProgress<BackgroundProgressMove> progress,
        BackgroundTaskKind kind,
        string operation,
        IReadOnlyList<SettingsStorageStep> steps)
    {
        logger.LogInformation("Фоновая операция «{operation}»: начало, шагов {count}", operation, steps.Count);

        for (var index = 0; index < steps.Count; index++)
        {
            var step = steps[index];

            progress.Report(new BackgroundProgressMove(
                GetPercent(index, steps.Count),
                kind,
                step.Description));

            logger.LogDebug("{description}", step.Description);

            await RunStepAsync(step, operation);
        }

        progress.Report(new BackgroundProgressMove(
            GetPercent(steps.Count, steps.Count),
            kind,
            "Готово"));

        logger.LogInformation("Фоновая операция «{operation}»: завершена", operation);
    }

    private async Task RunStepAsync(SettingsStorageStep step, string operation)
    {
        try
        {
            await step.Action();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при выполнении операции «{operation}»: {description}",
                operation,
                step.Description);
        }
    }

    private static Func<ValueTask> Revert<T>(ISettingsStorage<T> storage)
        where T : class, INotifyPropertyChanged
        => () =>
        {
            storage.Revert();
            return ValueTask.CompletedTask;
        };

    private static double GetPercent(int completed, int total)
        => total == 0 ? 100 : completed * 100d / total;
}
