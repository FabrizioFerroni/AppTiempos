using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.Api.Repositories;
using AppTiemposV3.Api.Utilidades;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using AppTiemposV3.Api.Events;
using AppTiemposV3.Api.Middlewares;
using AppTiemposV3.Api.Services;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Categories;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.AddServiceDefaults();

IServiceCollection? services = builder.Services;

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

services.AddControllers();
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
services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseMySql(connectBdMySql, ServerVersion.AutoDetect(connectBdMySql), b =>
    {
        b.EnableRetryOnFailure(3);
        b.CommandTimeout(60);
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
// Services and repositories
services.AddScoped<IAuthContract, AuthRepository>();
services.AddScoped<IActivityContract<ActivityResponseDto>, ActivityRepository>();
services.AddScoped<IRequerimentContract<RequerimentResponseDto>, RequerimentRepository>();
services.AddScoped<ICategoryContract<CategoryResponseDto>, CategoryRepository>();
services.AddScoped<IUserContract, UserContextService>();
services.AddScoped<IGenericContract, GenericRepository>();
services.AddScoped<IEmailService, EmailService>();
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
            .AllowAnyHeader()
            .AllowCredentials()
            .WithHeaders(HeaderNames.ContentType);
    });
});




WebApplication? app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultCorsPolicy");
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();