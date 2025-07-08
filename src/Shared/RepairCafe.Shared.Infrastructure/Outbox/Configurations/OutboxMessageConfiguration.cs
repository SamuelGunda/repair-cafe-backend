using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepairCafe.Shared.Infrastructure.Outbox.Models;

namespace RepairCafe.Shared.Infrastructure.Outbox.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .IsRequired();
            
        builder.Property(x => x.Type)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Property(x => x.Content)
            .IsRequired();
            
        builder.Property(x => x.OccurredOnUtc)
            .IsRequired();
            
        builder.Property(x => x.ProcessedOnUtc)
            .IsRequired(false);
            
        builder.Property(x => x.Error)
            .IsRequired(false);
            
        builder.Property(x => x.RetryCount)
            .IsRequired();
            
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
            
        builder.HasIndex(x => new { x.Status, x.OccurredOnUtc });
    }
}
