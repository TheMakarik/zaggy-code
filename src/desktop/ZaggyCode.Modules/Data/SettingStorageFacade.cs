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
                new("Сохранение состояния приложения", BackgroundTaskKind.FlushingStateSettings, stateSettingsStorage.FlushAsync),
                new("Сохранение настроек внешнего вида", BackgroundTaskKind.FlushingViewSettings, viewSettingsStorage.FlushAsync),
                new("Сохранение настроек темы", BackgroundTaskKind.FlushingThemeSettings, themeSettingsStorage.FlushAsync),
                new("Сохранение настроек Python", BackgroundTaskKind.FlushingPythonSettings, pythonSettingsStorage.FlushAsync),
                new("Сохранение настроек C#", BackgroundTaskKind.FlushingCSharpSettings, csharpSettingsStorage.FlushAsync)
            ]);

    public Task RevertAllAtBackground()
        => StartBackground(
            BackgroundTaskKind.RevertingAllSettings,
            "Отмена изменений",
            [
                new("Отмена изменений состояния приложения", BackgroundTaskKind.RevertingStateSettings, Revert(stateSettingsStorage)),
                new("Отмена изменений настроек внешнего вида", BackgroundTaskKind.RevertingViewSettings, Revert(viewSettingsStorage)),
                new("Отмена изменений настроек темы", BackgroundTaskKind.RevertingThemeSettings, Revert(themeSettingsStorage)),
                new("Отмена изменений настроек Python", BackgroundTaskKind.RevertingPythonSettings, Revert(pythonSettingsStorage)),
                new("Отмена изменений настроек C#", BackgroundTaskKind.RevertingCSharpSettings, Revert(csharpSettingsStorage))
            ]);

    public Task ToDefaultAllAtBackground()
        => StartBackground(
            BackgroundTaskKind.ToDefaultAllSettings,
            "Сброс настроек к значениям по умолчанию",
            [
                new("Сброс состояния приложения к значениям по умолчанию", BackgroundTaskKind.ToDefaultStateSettings, stateSettingsStorage.ToDefaultAsync),
                new("Сброс настроек внешнего вида к значениям по умолчанию", BackgroundTaskKind.ToDefaultViewSettings, viewSettingsStorage.ToDefaultAsync),
                new("Сброс настроек темы к значениям по умолчанию", BackgroundTaskKind.ToDefaultThemeSettings, themeSettingsStorage.ToDefaultAsync),
                new("Сброс настроек Python к значениям по умолчанию", BackgroundTaskKind.ToDefaultPythonSettings, pythonSettingsStorage.ToDefaultAsync),
                new("Сброс настроек C# к значениям по умолчанию", BackgroundTaskKind.ToDefaultCSharpSettings, csharpSettingsStorage.ToDefaultAsync)
            ]);

    private Task StartBackground(
        BackgroundTaskKind kind,
        string operation,
        IReadOnlyList<SettingsStorageStep> steps)
        => backgroundTaskHelper.StartBackground(new BackgroundTaskStartDto
        {
            Kind = kind,
            HasNoProgress = false,
            DelegateToStart = progress => RunSequentiallyAsync(progress, operation, steps)
        }).Task;

    private async Task RunSequentiallyAsync(
        IProgress<BackgroundProgressMove> progress,
        string operation,
        IReadOnlyList<SettingsStorageStep> steps)
    {
        logger.LogInformation("Фоновая операция «{operation}»: начало, шагов {count}", operation, steps.Count);

        for (var index = 0; index < steps.Count; index++)
        {
            var step = steps[index];
            var completed = index + 1;

            progress.Report(new BackgroundProgressMove(
                GetPercent(index, steps.Count),
                step.OperationKind,
                step.Description));

            logger.LogDebug("{description}", step.Description);

            await RunStepAsync(step, operation);
        }

        progress.Report(new BackgroundProgressMove(
            GetPercent(steps.Count, steps.Count),
            steps[^1].OperationKind,
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
            logger.LogError(e, "Ошибка при выполнении операции «{operation}»: {description}", operation, step.Description);
        }
    }

    private static Func<ValueTask> Revert(ISettingsStorage storage)
        => () =>
        {
            storage.Revert();
            return ValueTask.CompletedTask;
        };

    private static double GetPercent(int completed, int total)
        => total == 0 ? 100 : completed * 100d / total;
}
