namespace RepairCafe.Modules.Auth.Domain.Entities;

public record AuthTokens(string AccessToken, string RefreshToken, DateTime ExpiresAt)
{
    public AuthTokens() : this(string.Empty, string.Empty, DateTime.MinValue) { }

    public bool IsValid => !string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(RefreshToken) && ExpiresAt > DateTime.UtcNow;
}