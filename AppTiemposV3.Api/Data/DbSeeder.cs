using AppTiemposV3.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static AppTiemposV3.SharedClases.Enums.Areas;

namespace AppTiemposV3.Api.Data
{
    public class DbSeederDto
    {
        public bool Status { get; set; } = false;
        public string Response { get; set; } = string.Empty;
        public int Result { get; set; } = 0;
    }
    public static class DbSeeder
    {
        public static async Task<DbSeederDto> SeedData(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            DbSeederDto dto = new DbSeederDto();
            using IServiceScope? scope = serviceProvider.CreateScope();
            RoleManager<IdentityRole<Guid>>? roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            UserManager<UserEntity>? userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();
            ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            string[] roleNames = { "Admin", "User" };
            foreach (string? roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }

            string? adminEmail = configuration["Security:AdminEmail"];
            string? adminPass = configuration["Security:AdminPassword"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPass))
            {
                logger.LogWarning("Seeder: No se encontró Security__AdminEmail o Security__AdminPassword en la configuración.");
                return new DbSeederDto 
                { 
                    Status = false,
                    Response = "Seeder: No se encontró Security__AdminEmail o Security__AdminPassword en la configuración.",
                    Result = 3
                };
            }

            UserEntity? adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                UserEntity? user = new UserEntity
                {
                    UserName = adminEmail.Split('@')[0],
                    Email = adminEmail,
                    FullName = "Administrador Sistema",
                    EmailConfirmed = true,
                    IsAccountConfigurated = true,
                    Area = Desarrollo 
                };

                IdentityResult? result = await userManager.CreateAsync(user, adminPass);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }

                dto.Status = true;
                dto.Response = "Seeder: Se crearon el user principal de la app";
                dto.Result = 1;
            }
            else
            {
                dto.Status = true;
                dto.Response = "Seeder: El usuario ya fue creado, se salto seeder";
                dto.Result = 2;
            }

            return dto;
        }
        public static async Task<DbSeederDto> SeedTestersAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            DbSeederDto dto = new DbSeederDto();
            using IServiceScope? scope = serviceProvider.CreateScope();

            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            string? testersConfig = configuration["InitialTesters"];

            if (string.IsNullOrEmpty(testersConfig))
            {
                logger.LogInformation("Seeder Testers: No se encontró la configuración 'InitialTesters'. Se omite el sembrado de testers.");
                return new DbSeederDto
                {
                    Status = false,
                    Response = "Seeder Testers: No hay testers configurados para inicializar.",
                    Result = 3
                };
            }

            string[] testerNames = testersConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            int addedCount = 0;

            foreach (string name in testerNames)
            {
                bool exists = await context.Testers.AnyAsync(t => t.Nombre == name);

                if (!exists)
                {
                    TesterEntity newTester = new TesterEntity
                    {
                        Nombre = name,
                        Activo = true
                    };

                    context.Testers.Add(newTester);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                await context.SaveChangesAsync();

                dto.Status = true;
                dto.Response = $"Seeder Testers: Se crearon {addedCount} testers nuevos.";
                dto.Result = 1;
            }
            else
            {
                dto.Status = true;
                dto.Response = "Seeder Testers: Todos los testers configurados ya existían en la base de datos. Se saltó el seeder.";
                dto.Result = 2;
            }

            return dto;
        }
    }
}

