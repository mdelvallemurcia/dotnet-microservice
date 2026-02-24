using Models.Entity;
using System.Linq.Expressions;

namespace Infrastructure.Repository;

public interface IRepository<T> where T : BaseEntity
{
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> FilterAsync(Expression<Func<T, bool>> filter, CancellationToken ct = default);

    Task InsertAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
