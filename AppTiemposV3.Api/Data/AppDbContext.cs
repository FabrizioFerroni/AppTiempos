using AppTiemposV3.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace AppTiemposV3.Api.Data;

public class AppDbContext : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>
{

    public DbSet<RequerimentsEntity> Requeriments { get; set; }
    
    public DbSet<CategoriesEntity> Categories { get; set; }
    
    public DbSet<ActivitiesEntity> Activities { get; set; }
    
    public DbSet<TrainingEntity> Trainings { get; set; }
    
    public DbSet<InvitationEntity> Invitations { get; set; }

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserEntity>(e => e.ToTable(name: "usuarios"));
        builder.Entity<IdentityRole<Guid>>(e => e.ToTable(name: "roles"));
        builder.Entity<IdentityUserRole<Guid>>(e => e.ToTable(name: "usuario_roles"));
        builder.Entity<IdentityUserClaim<Guid>>(e => e.ToTable(name: "usuario_claims"));
        builder.Entity<IdentityUserLogin<Guid>>(e => e.ToTable(name: "usuario_logins"));
        builder.Entity<IdentityRoleClaim<Guid>>(e => e.ToTable(name: "roles_claim"));
        builder.Entity<IdentityUserToken<Guid>>(e => e.ToTable(name: "usuario_tokens"));
        builder.Entity<RequerimentsEntity>(e => e.ToTable(name: "requeriments"));
        builder.Entity<CategoriesEntity>(e => e.ToTable(name: "categories"));
        builder.Entity<ActivitiesEntity>(e => e.ToTable(name: "activities"));
        builder.Entity<TrainingEntity>(e => e.ToTable(name: "trainings"));
        builder.Entity<InvitationEntity>(e => e.ToTable(name: "invitaciones"));

        Type[]? excludedTypes = new[] { typeof(CategoriesEntity)};
        
        // Filtro global de Soft Delete para entidades que hereden de BaseEntity
        foreach (IMutableEntityType? entityType in builder.Model.GetEntityTypes())
        {
            Type? clrType = entityType.ClrType;

            if (typeof(BaseEntity).IsAssignableFrom(clrType))
            {
                // 1. Filtro global para soft delete
                ParameterExpression? parameter = Expression.Parameter(clrType, "e");
                MemberExpression? property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                BinaryExpression? condition = Expression.Equal(property, Expression.Constant(false));
                LambdaExpression? lambda = Expression.Lambda(condition, parameter);

                builder.Entity(clrType).HasQueryFilter(lambda);

                // 2. Indice automatico para UserId si existe
                
                IMutableProperty? userIdProp = entityType.FindProperty("UserId");
                bool alreadyHasIndex = entityType.GetIndexes().Any(i => i.Properties.Contains(userIdProp));
                if (userIdProp != null && userIdProp.ClrType == typeof(Guid) && !excludedTypes.Contains(clrType) && !alreadyHasIndex)
                {
                    entityType.AddIndex(userIdProp);
                }
            }
        }

        builder.Entity<RequerimentsEntity>(e =>
        {
            e.HasOne(r => r.User)
                  .WithMany(u => u.Requeriments)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(r => r.UserId)
             .HasDatabaseName("IX_Requeriments_UserId");

            e.HasIndex(r => new { r.UserId, r.ReqID })
             .HasDatabaseName("IX_Requeriments_UserId_ReqID")
             .IsUnique();

            e.Property(c => c.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.Property(id => id.IsDeleted)
            .HasDefaultValue(false);
            
            e.Property(id => id.Descripcion)
                .HasDefaultValueSql(null);
            
            e.Property(id => id.Url)
                .HasDefaultValueSql(null);
        });
        
        builder.Entity<CategoriesEntity>(e =>
        {
            e.HasIndex(c => new { c.Name })
                .HasDatabaseName("IX_Requeriments_Name")
                .IsUnique();

            e.Property(c => c.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.Property(id => id.IsDeleted)
                .HasDefaultValue(false);
        });

        builder.Entity<ActivitiesEntity>(a =>
        {
            a.Property(ab => ab.StartDate)
                .HasConversion(
                    c => c.ToDateTime(TimeOnly.MinValue), 
                    c => DateOnly.FromDateTime(c))
                .HasColumnType("date");
            
            a.Property(e => e.StartDate)
                .HasColumnType("date");
            
            a.Property(e => e.StartTime)
                .HasColumnType("time");
            
            a.Property(e => e.EndTime)
                .HasColumnType("time");
            
            a.Property(ab => ab.StartTime)
                .HasConversion(
                    c => TimeSpan.FromTicks(c.Ticks),
                    c => TimeOnly.FromTimeSpan(c)
                    )
                .HasColumnType("time");
            
            a.Property(ab => ab.EndTime)
                .HasConversion(
                    c => c.HasValue ? TimeSpan.FromTicks(c.Value.Ticks) : (TimeSpan?)null,
                    c => c.HasValue ? TimeOnly.FromTimeSpan(c.Value) : (TimeOnly?)null
                )
                .HasColumnType("time");
            
            a.HasOne(r => r.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            a.HasOne(ar => ar.Requeriment)
                .WithMany(r => r.Activities)
                .HasForeignKey(af => af.RequerimentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            a.HasOne(ac => ac.Category)
                .WithMany(c => c.Activities)
                .HasForeignKey(af => af.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            a.HasIndex(r => r.UserId)
                .HasDatabaseName("IX_Activities_UserId");

            a.HasIndex(r => new { r.UserId, r.RequerimentId })
                .HasDatabaseName("IX_Activities_UserId_ReqID");

            a.Property(c => c.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            a.Property(id => id.IsDeleted)
                .HasDefaultValue(false);
            
            a.Property(id => id.IsLoaded)
                .HasDefaultValue(false);
            
            a.Property(id => id.StatusMessage)
                .HasDefaultValue("En Progreso");
            
            a.Property(id => id.Comment)
                .HasDefaultValueSql(null);
        });

        builder.Entity<TrainingEntity>(t =>
        {
            t.HasOne(r => r.User)
                .WithMany(u => u.Trainings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            t.HasOne(ar => ar.Requeriment)
                .WithMany(r => r.Trainings)
                .HasForeignKey(af => af.RequerimentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            t.HasOne(ac => ac.Category)
                .WithMany(c => c.Trainings)
                .HasForeignKey(af => af.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            t.Property(ab => ab.StartDate)
                .HasConversion(
                    c => c.ToDateTime(TimeOnly.MinValue), 
                    c => DateOnly.FromDateTime(c))
                .HasColumnType("date");
            
            t.Property(e => e.StartDate)
                .HasColumnType("date");
            
            t.Property(e => e.StartTime)
                .HasColumnType("time");
            
            t.Property(e => e.EndTime)
                .HasColumnType("time");
            
            t.Property(ab => ab.StartTime)
                .HasConversion(
                    c => TimeSpan.FromTicks(c.Ticks),
                    c => TimeOnly.FromTimeSpan(c)
                )
                .HasColumnType("time");
            
            t.Property(ab => ab.EndTime)
                .HasConversion(
                    c => c.HasValue ? TimeSpan.FromTicks(c.Value.Ticks) : (TimeSpan?)null,
                    c => c.HasValue ? TimeOnly.FromTimeSpan(c.Value) : (TimeOnly?)null
                )
                .HasColumnType("time");
            
            t.HasIndex(c => new { c.Capacitator, c.UserId })
                .HasDatabaseName("IX_Training_Capacitator_User");

            t.Property(c => c.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            t.Property(id => id.IsDeleted)
                .HasDefaultValue(false);
            
            t.Property(c => c.Status)
                .HasColumnType("longtext")
                .HasDefaultValueSql("En Progreso");
            
            
        });

        builder.Entity<InvitationEntity>(i =>
        {
            i.HasIndex(c => new { c.Email })
                .HasDatabaseName("IX_Invitations_Email")
                .IsUnique();

            i.Property(c => c.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            i.Property(r => r.DateReceived)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            i.Property(id => id.IsDeleted)
                .HasDefaultValue(false);
            
            i.Property(id => id.Accepted)
                .HasDefaultValue(false);
            
            i.Property(id => id.Finished)
                .HasDefaultValue(false);
        });
    }

}