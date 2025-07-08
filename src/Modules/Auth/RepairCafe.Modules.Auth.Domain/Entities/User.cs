using Microsoft.AspNetCore.Identity;
using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Modules.Auth.Domain.Entities;

public class User : IdentityUser<Guid>, IAggregateRoot, IEntity<Guid>
{
    public Guid MemberId { get; private set; }
    public new string UserName { get; private set; }
    public new string Email { get; private set; }
    
    // TODO: Move Refresh tokens to a separate table, RefreshToken entity with a one-to-many relationship to User for multiple devices or sessions.
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private User() 
    {
        MemberId = Guid.Empty;
    }

    private User(Guid id, string firstName, string lastName, string email)
    {
        Id = id;
        UserName = $"{firstName}.{lastName}".ToLowerInvariant();
        Email = email;
        
        MemberId = id;

        base.UserName = UserName;
        base.Email = email;
        NormalizedUserName = UserName.ToUpperInvariant();
        NormalizedEmail = email.ToUpperInvariant();
        SecurityStamp = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
    }

    public static User Create(string firstName, string lastName, string email)
    {
        var user = new User(Guid.NewGuid(), firstName, lastName, email);
        //TODO: Integrate with Member Module to create a Member entity
        return user;
    }

    public void ChangeEmail(string newEmail)
    {
        if (Email == newEmail) return;

        Email = newEmail;
        base.Email = newEmail;
        NormalizedEmail = newEmail.ToUpperInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRefreshToken(string refreshToken, DateTime expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        UpdatedAt = DateTime.UtcNow;
    }

    protected void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();

    object[] IEntity.GetKeys()
    {
        return new object[] { Id };
    }
}