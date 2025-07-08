using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RepairCafe.Modules.Auth.Domain.Entities;
using RepairCafe.Shared.Infrastructure.Outbox.Configurations;
using RepairCafe.Shared.Infrastructure.Outbox.Models;
using RepairCafe.Shared.Infrastructure.Persistence;
using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Modules.Auth.Infrastructure.Persistence;

public class AuthDbContext : IdentityDbContext<User, Role, Guid>, IUnitOfWork, IOutboxDbContext
{
    private readonly IServiceProvider _serviceProvider;

    public AuthDbContext(DbContextOptions<AuthDbContext> options,
        IServiceProvider serviceProvider)
        : base(options)
    {
        _serviceProvider = serviceProvider;
    }
    
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public new DbSet<User> Users { get; set; }
    public new DbSet<Role> Roles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        
        ConfigureIdentityTables(modelBuilder);
    }
    
    public IEnumerable<IAggregateRoot> GetAggregatesWithDomainEvents()
    {
        return ChangeTracker.Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();
    }
    
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is null)
        {
            await Database.BeginTransactionAsync(cancellationToken);
        }
    }
    
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            await Database.CurrentTransaction.CommitAsync(cancellationToken);
        }
    }
    
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            await Database.CurrentTransaction.RollbackAsync(cancellationToken);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEventDispatcher = _serviceProvider.GetRequiredService<IDomainEventDispatcher>();
        await domainEventDispatcher.DispatchEventsAsync(cancellationToken);
        
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private static void ConfigureIdentityTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        
        modelBuilder.Ignore<IdentityUserClaim<Guid>>();
        modelBuilder.Ignore<IdentityUserLogin<Guid>>();
        modelBuilder.Ignore<IdentityUserToken<Guid>>();
        modelBuilder.Ignore<IdentityRoleClaim<Guid>>();
    }
}