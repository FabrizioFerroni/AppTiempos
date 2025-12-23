using System.Text.Json;
using AppTiemposV3.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTiemposV3.Api.Data;

public class AuditEntityConfiguration : IEntityTypeConfiguration<AuditEntity>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public void Configure(EntityTypeBuilder<AuditEntity> builder)
    {
        builder.ToTable("auditorias");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
              .IsRequired();

        builder.Property(x => x.UserName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Action)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Entity)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EntityName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(x => x.Changes)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<AuditChange>>(v, JsonOptions) ?? new List<AuditChange>()
            )
            .HasColumnType("json")
            .IsRequired();

        builder.Property(x => x.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonOptions)
                     ?? new Dictionary<string, string>()
            )
            .HasColumnType("json");
        
        builder.HasOne(r => r.User)
            .WithMany(u => u.Audits)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}