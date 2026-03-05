using System.Linq.Expressions;
using Models.Dto;
using Models.Entity;
using MongoDB.Driver;

namespace Infrastructure.Repository;

public class MongoRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly IMongoCollection<T> _collection;

    public MongoRepository(IMongoDatabase database)
    {
        var collectionName = typeof(T).Name.ToLowerInvariant();
        _collection = database.GetCollection<T>(collectionName);
    }

    public Task<List<T>> GetAllAsync(CancellationToken ct = default)
    {
        return _collection
            .Find(FilterDefinition<T>.Empty)
            .ToListAsync(ct);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var findResult = await _collection.FindAsync(x => x.Id == id, cancellationToken: ct);
        var record = await findResult.FirstOrDefaultAsync(cancellationToken: ct);

        return record;
    }

    public Task<List<T>> FilterAsync(Expression<Func<T, bool>> filter, CancellationToken ct = default)
    {
        return _collection
            .Find(filter)
            .ToListAsync(ct);
    }

    public async Task<PagedData<T>> GetPagedAsync(
            int page,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true)
    {
        filter ??= _ => true;

        var query = _collection.Find(filter);

        if (orderBy != null)
        {
            query = ascending
                ? query.SortBy(orderBy)
                : query.SortByDescending(orderBy);
        }

        var totalItems = await query.CountDocumentsAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return new PagedData<T>(
            items,
            page,
            pageSize,
            totalItems
        );
    }

    public Task InsertAsync(T entity, CancellationToken ct = default)
    {
        return _collection.InsertOneAsync(entity, cancellationToken: ct);
    }

    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        return _collection.ReplaceOneAsync(
            x => x.Id.Equals(entity.Id),
            entity,
            cancellationToken: ct);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _collection.DeleteOneAsync(x => x.Id.Equals(id), ct);
    }
}
