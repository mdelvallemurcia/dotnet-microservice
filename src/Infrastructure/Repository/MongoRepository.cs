using Models.Entity;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Infrastructure.Repository;

public class MongoRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly IMongoCollection<T> _collection;

    public MongoRepository(IMongoDatabase database)
    {
        var collectionName = typeof(T).Name.ToLowerInvariant();
        _collection = database.GetCollection<T>(collectionName);
    }

    public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _collection
            .Find(FilterDefinition<T>.Empty)
            .ToListAsync(ct);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<T>> FilterAsync(
        Expression<Func<T, bool>> filter,
        CancellationToken ct = default)
    {
        return await _collection
            .Find(filter)
            .ToListAsync(ct);
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
