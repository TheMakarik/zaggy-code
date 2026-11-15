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
            "Сохранение",
            [
                new(nameof(StateSettings), BackgroundTaskKind.FlushingStateSettings, stateSettingsStorage.FlushAsync),
                new(nameof(ViewSettings), BackgroundTaskKind.FlushingViewSettings, viewSettingsStorage.FlushAsync),
                new(nameof(ThemeSettings), BackgroundTaskKind.FlushingThemeSettings, themeSettingsStorage.FlushAsync),
                new(nameof(PythonSettings), BackgroundTaskKind.FlushingPythonSettings, pythonSettingsStorage.FlushAsync),
                new(nameof(CSharpSettings), BackgroundTaskKind.FlushingCSharpSettings, csharpSettingsStorage.FlushAsync)
            ]);

    public Task RevertAllAtBackground()
        => StartBackground(
            BackgroundTaskKind.RevertingAllSettings,
            "Отмена изменений",
            [
                new(nameof(StateSettings), BackgroundTaskKind.RevertingStateSettings, Revert(stateSettingsStorage)),
                new(nameof(ViewSettings), BackgroundTaskKind.RevertingViewSettings, Revert(viewSettingsStorage)),
                new(nameof(ThemeSettings), BackgroundTaskKind.RevertingThemeSettings, Revert(themeSettingsStorage)),
                new(nameof(PythonSettings), BackgroundTaskKind.RevertingPythonSettings, Revert(pythonSettingsStorage)),
                new(nameof(CSharpSettings), BackgroundTaskKind.RevertingCSharpSettings, Revert(csharpSettingsStorage))
            ]);

    public Task ToDefaultAllAtBackground()
        => StartBackground(
            BackgroundTaskKind.ToDefaultAllSettings,
            "Сброс к значениям по умолчанию",
            [
                new(nameof(StateSettings), BackgroundTaskKind.ToDefaultStateSettings, stateSettingsStorage.ToDefaultAsync),
                new(nameof(ViewSettings), BackgroundTaskKind.ToDefaultViewSettings, viewSettingsStorage.ToDefaultAsync),
                new(nameof(ThemeSettings), BackgroundTaskKind.ToDefaultThemeSettings, themeSettingsStorage.ToDefaultAsync),
                new(nameof(PythonSettings), BackgroundTaskKind.ToDefaultPythonSettings, pythonSettingsStorage.ToDefaultAsync),
                new(nameof(CSharpSettings), BackgroundTaskKind.ToDefaultCSharpSettings, csharpSettingsStorage.ToDefaultAsync)
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
                $"{operation}: {step.Name}, шаг {completed} из {steps.Count}"));

            logger.LogDebug("Фоновая операция «{operation}»: начало шага {completed}/{count} — {name}",
                operation,
                completed,
                steps.Count,
                step.Name);

            await RunStepAsync(step, operation);

            logger.LogDebug("Фоновая операция «{operation}»: шаг {completed}/{count} завершён — {name}",
                operation,
                completed,
                steps.Count,
                step.Name);
        }

        progress.Report(new BackgroundProgressMove(
            GetPercent(steps.Count, steps.Count),
            steps[^1].OperationKind,
            $"{operation}: готово ({steps.Count} из {steps.Count})"));

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
            logger.LogError(e, "Ошибка при выполнении операции «{operation}» для {name}", operation, step.Name);
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
