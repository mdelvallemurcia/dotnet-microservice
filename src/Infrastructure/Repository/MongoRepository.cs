using System.Linq.Expressions;
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
