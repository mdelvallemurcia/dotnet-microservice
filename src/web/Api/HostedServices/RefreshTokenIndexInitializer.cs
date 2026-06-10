using Models.Entity;
using MongoDB.Driver;

namespace Api.HostedServices;

// Creates the indexes the auth flow depends on for the RefreshToken collection at startup:
//   - Hash      : unique  -> fast rotation lookups + protects against duplicates
//   - UserName  : queries -> whole-family revocation on reuse detection
//   - ExpiresAt : TTL     -> expired tokens self-purge (ExpireAfter = 0 deletes once ExpiresAt passes)
// Collection name mirrors MongoDbRepository's convention: typeof(T).Name.ToLowerInvariant().
internal sealed class RefreshTokenIndexInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenIndexInitializer> _logger;

    public RefreshTokenIndexInitializer(IServiceScopeFactory scopeFactory, ILogger<RefreshTokenIndexInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var collection = database.GetCollection<RefreshToken>(nameof(RefreshToken).ToLowerInvariant());

        var indexes = new[]
        {
            new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(x => x.Hash),
                new CreateIndexOptions { Unique = true, Name = "ux_refreshtoken_hash" }),
            new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(x => x.UserName),
                new CreateIndexOptions { Name = "ix_refreshtoken_username" }),
            new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(x => x.ExpiresAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = "ttl_refreshtoken_expiresAt" }),
        };

        try
        {
            await collection.Indexes.CreateManyAsync(indexes, cancellationToken);
        }
        catch (MongoException ex)
        {
            // Non-fatal: don't block startup if Mongo is briefly unavailable while Aspire spins services up.
            _logger.LogWarning(ex, "Could not create RefreshToken indexes at startup");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
