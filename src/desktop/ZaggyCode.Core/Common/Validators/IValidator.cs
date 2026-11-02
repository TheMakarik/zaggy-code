namespace ZaggyCode.Core.Common.Validators;

public interface IValidator<T>
{
    public bool IsValid(T entity);
}