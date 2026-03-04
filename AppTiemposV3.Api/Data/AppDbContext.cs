using AppTiemposV3.Api.Entities;
using AppTiemposV3.Api.Entities.ConfigurationTable;
using AppTiemposV3.SharedClases.DTOs.Reports;
using AppTiemposV3.SharedClases.Enums;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Text.Json;
using static Grpc.Core.Metadata;

namespace AppTiemposV3.Api.Data;

public class AppDbContext : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>
{
    public DbSet<RequerimentsEntity> Requeriments { get; set; } = null!;    
    public DbSet<CategoriesEntity> Categories { get; set; } = null!;    
    public DbSet<ActivitiesEntity> Activities { get; set; } = null!;
    public DbSet<TrainingEntity> Trainings { get; set; } = null!;    
    public DbSet<InvitationEntity> Invitations { get; set; } = null!;    
    public DbSet<RejectionEntity> Rejections { get; set; } = null!;    
    public DbSet<RejectionDetailEntity> RejectionDetails { get; set; } = null!;    
    public DbSet<AuditEntity> Audits { get; set; } = null!;
    public DbSet<ReportEntity> Reports { get; set; } = null!;
    public DbSet<ConfigurationEntity> Configurations { get; set; } = null!;
    public DbSet<DayConfigEntity> DayConfigs { get; set; } = null!;
    public DbSet<WeeklyHourConfig> WeeklyHourConfigs { get; set; } = null!;
    public DbSet<NotificationConfigEntity> NotificationConfigEntity { get; set; } = null!;
    public DbSet<WorkingSaturdayEntity> WorkingSaturdays { get; set; }
    public DbSet<BackupLogsEntity> BackupLogs { get; set; }



    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new AuditEntityConfiguration());
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
        builder.Entity<RejectionEntity>(e => e.ToTable(name: "rechazos"));
        builder.Entity<RejectionDetailEntity>(e => e.ToTable(name: "rechazos_detalles"));
        builder.Entity<ReportEntity>(e => e.ToTable(name: "reportes"));
        builder.Entity<ConfigurationEntity>(e => e.ToTable(name: "configuraciones"));
        builder.Entity<DayConfigEntity>(e => e.ToTable(name: "configuraciones_dias"));
        builder.Entity<WeeklyHourConfig>(e => e.ToTable(name: "configuraciones_horarios"));
        builder.Entity<WorkingSaturdayEntity>(e => e.ToTable(name: "configuraciones_sabados"));
        builder.Entity<NotificationConfigEntity>(e => e.ToTable(name: "configuraciones_notificaciones"));
        builder.Entity<BackupLogsEntity>(e => e.ToTable(name: "configuraciones_backups_logs"));

        Type[]? excludedTypes = new[] { typeof(CategoriesEntity) };

        // Filtro global de Soft Delete para entidades que hereden de BaseEntity
        foreach (IMutableEntityType entityType in builder.Model.GetEntityTypes())
        {
            Type clrType = entityType.ClrType;

            if (typeof(BaseEntity).IsAssignableFrom(clrType))
            {
                ParameterExpression? parameter = Expression.Parameter(clrType, "e");
                MemberExpression? property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                BinaryExpression? condition = Expression.Equal(property, Expression.Constant(false));
                LambdaExpression? lambda = Expression.Lambda(condition, parameter);

                builder.Entity(clrType).HasQueryFilter(lambda);

                IMutableProperty? userIdProp = entityType.FindProperty("UserId");

                if (userIdProp != null && !excludedTypes.Contains(clrType))
                {
                    // Solo creamos el índice si no existe uno ya
                    if (!entityType.GetIndexes().Any(i => i.Properties.Contains(userIdProp)))
                    {
                        entityType.AddIndex(userIdProp);
                    }
                }
            }
        }

        builder.Entity<UserEntity>(u =>
        {
            u.Property(e => e.Area)
                .HasConversion<int>()
                .HasColumnType("int");

            u.Property(ia => ia.IsAccountConfigurated)
                .HasColumnType("tinyint")
                .HasDefaultValueSql("0");
        });

        builder.Entity<RequerimentsEntity>(e =>
        {
            e.HasOne(r => r.User)
                  .WithMany(u => u.Requeriments)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(rc => rc.Category)
                .WithMany(c => c.Requeriments)
                .HasForeignKey(rf => rf.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(t => t.Trainings)
                .WithOne(r => r.Requeriment)
                .HasForeignKey(f => f.RequerimentId);

            e.HasIndex(r => r.UserId)
             .HasDatabaseName("IX_Requeriments_UserId");

            e.HasIndex(r => new { r.UserId, r.ReqID })
             .HasDatabaseName("IX_Requeriments_UserId_ReqID")
             .IsUnique();

            e.HasIndex(r => new { r.UserId, r.FolderId })
                .HasDatabaseName("IX_Requeriments_UserId_FolderId")
                .IsUnique();

            e.Property(c => c.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.Property(id => id.IsDeleted)
            .HasDefaultValue(false);

            e.Property(r => r.ConjuntoCambios)
                .HasColumnType("json");

            e.Property(id => id.Descripcion)
                .HasDefaultValueSql(null);

            e.Property(id => id.Url)
                .HasDefaultValueSql(null);

            e.Property(r => r.Estado)
                .HasConversion<int>()
                .HasColumnType("int")
                .HasDefaultValueSql("1");

            e.Property(et => et.EtapaActual)
                .HasConversion<int>()
                .HasDefaultValueSql("1");
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

            a.Property(e => e.Etapa)
                .HasConversion<int>()
                .HasDefaultValueSql("1");

            a.HasIndex(e => e.Etapa)
                .HasDatabaseName("IX_Activities_Etapa");
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

        builder.Entity<RejectionEntity>(r =>
        {
            r.HasOne(re => re.User)
                .WithMany(u => u.Rejections)
                .HasForeignKey(re => re.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            r.HasOne(ar => ar.Requeriment)
                .WithMany(re => re.Rejections)
                .HasForeignKey(af => af.RequerimentId)
                .OnDelete(DeleteBehavior.Restrict);

            r.HasMany(r => r.RejectionsDetails)
                .WithOne(d => d.Rejection)
                .HasForeignKey(d => d.RejectionId);


            r.Property(c => c.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            r.Property(c => c.IsDeleted)
                .HasDefaultValue(false);
        });

        builder.Entity<RejectionDetailEntity>(rd =>
        {
            rd.HasOne(ar => ar.Rejection)
                .WithMany(re => re.RejectionsDetails)
                .HasForeignKey(af => af.RejectionId)
                .OnDelete(DeleteBehavior.Restrict);


            rd.HasOne(re => re.User)
                .WithMany(u => u.RejectionsDetails)
                .HasForeignKey(re => re.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            rd.Property(c => c.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            rd.Property(ab => ab.RejectionDate)
                .HasConversion(
                    c => c.ToDateTime(TimeOnly.MinValue),
                    c => DateOnly.FromDateTime(c))
                .HasColumnType("date");

            rd.Property(e => e.EstimatedFixTime)
                .HasColumnType("time");

            rd.Property(e => e.ActualFixTime)
                .HasColumnType("time");

            rd.Property(ab => ab.EstimatedFixTime)
                .HasConversion(
                    c => c.HasValue ? TimeSpan.FromTicks(c.Value.Ticks) : (TimeSpan?)null,
                    c => c.HasValue ? TimeOnly.FromTimeSpan(c.Value) : (TimeOnly?)null
                )
                .HasColumnType("time");

            rd.Property(ab => ab.ActualFixTime)
                .HasConversion(
                    c => c.HasValue ? TimeSpan.FromTicks(c.Value.Ticks) : (TimeSpan?)null,
                    c => c.HasValue ? TimeOnly.FromTimeSpan(c.Value) : (TimeOnly?)null
                )
                .HasColumnType("time");

            rd.Property(rn => rn.RechazoNro)
                .HasDefaultValueSql("0");
        });

        builder.Entity<ReportEntity>(re =>
        {
            re.HasOne(re => re.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(re => re.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            re.Property(rc => rc.RunCount)
                .HasDefaultValueSql("0");


            re.Property(rc => rc.IsScheduled)
                .HasDefaultValueSql("0");

            re.Property(e => e.QueryRequest)
              .HasColumnType("json")
              .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<QueryRequestDTO>(v, JsonOptions) ?? new QueryRequestDTO()
              );

            re.Property(e => e.Schedule)
              .HasColumnType("json")
              .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<ScheduleReportDto>(v, JsonOptions) ?? new ScheduleReportDto()
            );

            re.Property(c => c.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            re.Property(c => c.LastRun)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            re.Property(c => c.IsDeleted)
                .HasDefaultValue(false);

            re.Property(c => c.IsFavorite)
                .HasDefaultValue(false);
        });

        builder.Entity<ConfigurationEntity>(c =>
        {
            c.HasOne(e => e.WeeklyPar)
             .WithMany()
             .HasForeignKey(e => e.WeeklyParId)
             .OnDelete(DeleteBehavior.Cascade);

            c.HasOne(e => e.WeeklyImpar)
             .WithMany()
             .HasForeignKey(e => e.WeeklyImparId)
             .OnDelete(DeleteBehavior.Cascade);

            c.HasOne(e => e.NotificationConfig)
             .WithMany()
             .HasForeignKey(e => e.NotificationConfigId)
             .OnDelete(DeleteBehavior.Cascade);

            c.Property(ab => ab.AutoBackupEnabled)
                .HasDefaultValueSql("0");

            c.Property(ac => ac.ActualConfig)
                .HasDefaultValueSql("1");

            c.Property(c => c.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            c.HasOne(re => re.User)
                .WithMany(u => u.Configurations)
                .HasForeignKey(ce => ce.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DayConfigEntity>(dc =>
        {
            dc.HasOne(d => d.Configuration)
              .WithMany(c => c.DayConfigs)
              .HasForeignKey(d => d.ConfigurationEntityId)
              .OnDelete(DeleteBehavior.Cascade);

            dc.Property(c => c.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        builder.Entity<WorkingSaturdayEntity>(ws =>
        {
            ws.HasOne(s => s.Configuration)
              .WithMany(c => c.WorkingSaturdays)
              .HasForeignKey(s => s.ConfigurationEntityId)
              .OnDelete(DeleteBehavior.Cascade);

            ws.Property(s => s.Date).HasColumnType("date");
            ws.Property(s => s.StartTime).HasColumnType("time");
            ws.Property(s => s.EndTime).HasColumnType("time");

            ws.Property(c => c.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        builder.Entity<NotificationConfigEntity>(nc =>
        {
            nc.Property(c => c.CreatedAt)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            nc.Property(n => n.EnableNotificationDiario)
                .HasDefaultValueSql("0");

            nc.Property(n => n.EnableNotificationSemanal)
                .HasDefaultValueSql("0");

            nc.Property(n => n.EnableNotificationMetaAlcanzada)
                .HasDefaultValueSql("0");

            nc.Property(n => n.NotificationsEmail)
                .HasDefaultValueSql("0");
        });

        builder.Entity<BackupLogsEntity>(bl =>
        {
            bl.Property(ab => ab.Size).HasDefaultValueSql("0");
            bl.Property(t => t.Type).HasDefaultValueSql("Manual");
            bl.HasOne(s => s.Configuration)
              .WithMany(c => c.BackupsLogs)
              .HasForeignKey(s => s.ConfigurationEntityId)
              .OnDelete(DeleteBehavior.Restrict);

            bl.HasOne(s => s.User)
              .WithMany(c => c.BackupsLogs)
              .HasForeignKey(s => s.UserId)
              .OnDelete(DeleteBehavior.Restrict);

            bl.Property(c => c.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}   