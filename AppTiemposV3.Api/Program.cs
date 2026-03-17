using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.Api.Events;
using AppTiemposV3.Api.Middlewares;
using AppTiemposV3.Api.Repositories;
using AppTiemposV3.Api.Scheduled;
using AppTiemposV3.Api.Services;
using AppTiemposV3.Api.Services.Interfaces;
using AppTiemposV3.Api.Utilidades;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Audits;
using AppTiemposV3.SharedClases.DTOs.Categories;
using AppTiemposV3.SharedClases.DTOs.Invitations;
using AppTiemposV3.SharedClases.DTOs.RejectionDetails;
using AppTiemposV3.SharedClases.DTOs.Rejections;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.DTOs.RequerimentsAttachments;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using AppTiemposV3.SharedClases.DTOs.Users;
using AppTiemposV3.SharedClases.GenericModels;
using AppTiemposV3.SharedClases.Utilidades;
using AppTiemposV3.SharedClases.Utilidades.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Quartz;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Swashbuckle.AspNetCore.Filters;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using static AppTiemposV3.Api.Data.DbSeeder;
using static QuestPDF.Infrastructure.LicenseType;
using static QuestPDF.Settings;
using static Serilog.Events.LogEventLevel;
using static System.Console;
using static System.Text.Encoding;
using static System.TimeZoneInfo;
using TimeOnlyJsonConverter = AppTiemposV3.Api.Utilidades.TimeOnlyJsonConverter;
using static Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders;
using Serilog.Events;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

OutputEncoding = UTF8;

//Licencia QuestPDF
License = Community;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

LogEventLevel logEventBd;
if (builder.Environment.IsDevelopment())
{
    logEventBd = Information;
}
else
{
    logEventBd = LogEventLevel.Error;
}

builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", logEventBd)
    .MinimumLevel.Override("AppTiemposV3.Api", Information) 
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] : {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Sixteen,
        applyThemeToRedirectedOutput: true
    )
    .WriteTo.File("Logs/reportes-.txt",
        rollingInterval: RollingInterval.Day, 
        retainedFileCountLimit: 14) 
);

builder.AddServiceDefaults();

IServiceCollection? services = builder.Services;

string keysPath;

if (builder.Environment.IsDevelopment())
{
    keysPath = Path.Combine(builder.Environment.ContentRootPath, "keys");
}
else
{
    keysPath = "/app/keys";
}

if (!Directory.Exists(keysPath))
{
    Directory.CreateDirectory(keysPath);
}

services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("AppTiemposV3")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(14)); 

services.AddSingleton(_ =>
    new MapperConfiguration(conf =>
    {
        conf.AddProfile(new AutoMapperProfiles());
    }).CreateMapper()
);

// Add services to the container.
services.AddControllers().AddNewtonsoftJson(options =>
{
    // Evita errores por referencias cíclicas (EF Core)
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

    // Serializa enums como strings en lugar de números
    options.SerializerSettings.Converters.Add(new StringEnumConverter());

    // Usa camelCase en nombres de propiedades JSON
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

    // Ignora valores null al serializar
    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

    // Indenta el JSON para facilitar el debugging
    options.SerializerSettings.Formatting = Formatting.Indented;
    
    options.SerializerSettings.Error = (sender, args) =>
    {
        // Evita que explote la API por un error de serialización
        args.ErrorContext.Handled = true;
    };
    
    // Respeta los nombres definidos con [JsonProperty("nombreJson")]
    options.SerializerSettings.ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new CamelCaseNamingStrategy
        {
            ProcessDictionaryKeys = true,
            OverrideSpecifiedNames = false // <== esto es importante
        }
    };
    
    options.SerializerSettings.Converters.Add(new DateOnlyJsonConverter());
    
    options.SerializerSettings.Converters.Add(new TimeOnlyJsonConverter());
    
    options.SerializerSettings.Converters.Add(new TimeOnlyNullableJsonConverter());
    options.SerializerSettings.Converters.Add(new DateOnlyNullableJsonConverter());
});

services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
// Add authentication to Swagger UI
services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Ingresar Bearer [space] tu Token \r\n\r\n " +
                      "Ejemplo: Bearer 123456abcder",
        Type = SecuritySchemeType.ApiKey 
    });

    opt.OperationFilter<SecurityRequirementsOperationFilter>();
});



// Starting
string connectBdMySql = builder.Configuration.GetConnectionString("MySQL") ?? 
                        throw new InvalidOperationException("Falta especificar la cadena de conexion a la base de datos");
MySqlServerVersion? serverVersion = new MySqlServerVersion(new Version(8, 0, 45));

services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseMySql(connectBdMySql, serverVersion, b =>
    {
        b.EnableRetryOnFailure(3);
        b.CommandTimeout(3600); //60
        b.MigrationsHistoryTable("ef_migrations");
        b.UseRelationalNulls();
    });
    
    if (builder.Environment.IsDevelopment())
    {
        opt.EnableSensitiveDataLogging(); 
        opt.EnableDetailedErrors(); 
    }
    opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    
    
});

// Add Identity & JWT Authentication
// Identity 
services.AddIdentity<UserEntity, IdentityRole<Guid>>(config =>
    {
        config.Tokens.AuthenticatorIssuer = "JWT";
        config.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider; //Esto se puede cambiar a Authenticator
        config.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
        config.Tokens.ChangePhoneNumberTokenProvider = TokenOptions.DefaultEmailProvider;
        config.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
        config.User.RequireUniqueEmail = true;
        config.SignIn.RequireConfirmedEmail = true;
        config.SignIn.RequireConfirmedAccount = true;

        config.Password.RequiredLength = 8;
        config.Password.RequiredUniqueChars = 3;
        config.Password.RequireNonAlphanumeric = true;
        config.Password.RequireUppercase = true;
        config.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._#$";

        config.Lockout.AllowedForNewUsers = true;
        config.Lockout.MaxFailedAccessAttempts = 3;
        config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        config.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole<Guid>>();

//JWT Authentication
services.AddScoped<CustomJwtEvents>();
services.AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).
    AddJwtBearer(conf =>
    {
        conf.RequireHttpsMetadata = false;
        conf.SaveToken = true;
        conf.TokenValidationParameters= new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))    ,
            ClockSkew = TimeSpan.Zero,
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
        };
        
        conf.EventsType = typeof(CustomJwtEvents);
    });

services.AddHttpContextAccessor();
services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();
// Services and repositories
services.AddScoped<IAuthContract, AuthRepository>();
services.AddScoped<IAuditContract<AuditsResponseDto>, AuditRepository>();
services.AddScoped<IActivityContract<ActivityResponseDto>, ActivityRepository>();
services.AddScoped<IDashboardContract<DashboardKPIDto>, DashboardRepository>();
services.AddScoped<IActivityWeeklyContract<ActivitiesByDay>, ActivityWeeklyRepository>();
services.AddScoped<IRequerimentContract<RequerimentResponseDto>, RequerimentRepository>();
services.AddScoped<ICategoryContract<CategoryResponseDto>, CategoryRepository>();
services.AddScoped<ITrainingContract<TrainingResponseDto>, TrainingRepository>();
services.AddScoped<IRejectionContract<RejectionResponseDto>, RejectionRepository>();
services.AddScoped<IRejectionDetailContract<RejectionDetailResponseDto>, RejectionDetailsRepository>();
services.AddScoped<IInvitationContract<InvitationResponseDto>, InvitationRepository>();
services.AddScoped<IRequerimentAttachmentContract<RequerimentsAttachmentsDto>, RequerimentAttachmentRepository>();
services.AddScoped<IReportContract, ReportRepository>();
services.AddScoped<IConfigurationContract, ConfigurationRepository>();
services.AddScoped<IBackupContract, BackupRepository>();
services.AddScoped<IReportScheduledContract, ReportScheduledRepository>();
services.AddScoped<IUserCContract<UserResponseDto>, UserRepository>();
services.AddScoped<IUserContract, UserContextService>();
services.AddScoped<IGenericContract, GenericRepository>();
services.AddScoped<IAuditHelperService, AuditHelperService>();
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<IEntityIdProvider, EntityIdProvider>();
services.AddScoped<IGenericSContract<ColorModel>, GenericService>();
// Ending Services and repositories

string[] origins = builder.Configuration.GetSection("origins").Get<string[]>()!;
if (origins == null || origins.Length == 0)
{
    throw new InvalidOperationException("No se configuraron origenes CORS validos.");
}

services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(origins) 
            .AllowAnyMethod()
            .AllowCredentials()
            .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization);
    });
});


services.AddQuartz(q =>
{
    string tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                  ? "Argentina Standard Time"
                  : "America/Argentina/Cordoba";

    JobKey? jobKey = new JobKey("ReportScheduledJob");

    q.AddJob<ReportScheduled>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("TriggerDiario8AM")
        .WithCronSchedule("0 0 8 * * ?", x => x
            .InTimeZone(FindSystemTimeZoneById(tzId)))
    );

    JobKey? logsJobKey = new JobKey("SendLogsJob");

    q.AddJob<SendLogsJob>(opts => opts.WithIdentity(logsJobKey));

    q.AddTrigger(opts => opts
        .ForJob(logsJobKey)
        .WithIdentity("MidnightLogsTrigger")
        .WithCronSchedule("0 5 0 ? * MON", x => x
            .InTimeZone(FindSystemTimeZoneById(tzId)))
    );


    JobKey? backupJobKey = new JobKey("BackupsBDJob");
    q.AddJob<BackupsBDJob>(opts => opts.WithIdentity(backupJobKey));

    q.AddTrigger(opts => opts
        .ForJob(backupJobKey)
        .WithIdentity("BackupsBDJob-trigger")
        .WithCronSchedule("0 */5 * * * ?", x => x 
            .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(tzId)))
    );

});

services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);



// En Program.cs
try
{
    WebApplication? app = builder.Build();

    if (string.IsNullOrEmpty(app.Environment.WebRootPath))
    {
        app.Environment.WebRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
    }

    using (IServiceScope? scope = app.Services.CreateScope())
    {
        IServiceProvider? servicesProv = scope.ServiceProvider;
        IConfiguration? config = servicesProv.GetRequiredService<IConfiguration>();
        ILogger<Program>? logger = servicesProv.GetRequiredService<ILogger<Program>>();

        try
        {
            AppDbContext? context = servicesProv.GetRequiredService<AppDbContext>();
            context.Database.Migrate(); 

            DbSeederDto? response = await SeedData(servicesProv, config);

            if (response.Status)
            {
                if(response.Result == 1)
                {
                    logger.LogInformation($"✅ {response.Response}");
                } else if(response.Result == 2)
                {
                    logger.LogInformation($"ℹ️ {response.Response}");
                }
            }
            else
            {
                if (response.Result == 3)
                {
                    logger.LogInformation($"❌ {response.Response}");
                }
            }
        }
        catch (Exception ex)
        {            
            logger.LogError(ex, "Ocurrió un error al migrar o sembrar la base de datos.");
        }
    }

    if (!Directory.Exists(app.Environment.WebRootPath))
    {
        Directory.CreateDirectory(app.Environment.WebRootPath);
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseStaticFiles();

    app.UseCors("DefaultCorsPolicy");
    app.UseMiddleware<ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = XForwardedFor | XForwardedProto
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapDefaultEndpoints();

    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    throw;
}