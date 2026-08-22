namespace ZaggyCode.Avalonia.ViewModels;

public abstract partial class ViewModelBase : ReactiveObject
{
    public Interaction<Unit, Unit> NotImplementedOccurred { get; } = new();

    protected ViewModelBase()
    {
#pragma warning disable AsyncVoidMethod
        ThrowNotImplementedCommand.ThrownExceptions
            .Subscribe(async void (_) => await NotImplementedOccurred.Handle(Unit.Default));
#pragma warning restore AsyncVoidMethod
    }
    
    [ReactiveCommand]
    private void ThrowNotImplemented()
    {
        throw new NotImplementedException();
    }
}
