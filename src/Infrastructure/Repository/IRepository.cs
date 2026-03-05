using System.Linq.Expressions;
using Models.Dto;
using Models.Entity;

namespace Infrastructure.Repository;

public interface IRepository<T> where T : BaseEntity
{
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> FilterAsync(Expression<Func<T, bool>> filter, CancellationToken ct = default);
    Task<PagedData<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true);

    Task InsertAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
