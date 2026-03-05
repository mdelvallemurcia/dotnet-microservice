namespace Models.Entity;

public record RefreshToken(
    string UserName,
    string Fingerprint,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? RevokedAt,
    string Hash,
    string? ReplacedByHash,
    string? RevocationReason,
    string? IpAddress,
    string? UserAgent
) : BaseEntity
{
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
};
