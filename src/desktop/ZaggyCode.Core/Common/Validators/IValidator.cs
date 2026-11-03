namespace ZaggyCode.Core.Common.Validators;

public interface IValidator<in T>
{
    public bool IsValid(T entity);
}