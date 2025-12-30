namespace Application.Abstractions.Data;

public interface ITrackedUnitOfWork : IUnitOfWork
{
    void ClearChanges();
}
