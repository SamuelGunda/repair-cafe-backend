using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepairCafe.Modules.Auth.Domain.Entities;

namespace RepairCafe.Modules.Auth.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.MemberId)
            .IsRequired();
        
        builder.Property(u => u.RefreshToken)
            .HasMaxLength(256)
            .IsRequired(false);
            
        builder.Property(u => u.RefreshTokenExpiryTime)
            .IsRequired(false);
            
        builder.Property(u => u.CreatedAt)
            .IsRequired();
            
        builder.Property(u => u.UpdatedAt)
            .IsRequired(false);
            
        builder.Ignore(u => u.DomainEvents);
    }
}
