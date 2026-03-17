using AppTiemposV3.Api.Entities;
using Microsoft.AspNetCore.Identity;
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
            using var scope = serviceProvider.CreateScope();
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
                    Response = "No se encontró Security__AdminEmail o Security__AdminPassword en la configuración.",
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
                    IsAccountConfigurated = false,
                    Area = Desarrollo 
                };

                IdentityResult? result = await userManager.CreateAsync(user, adminPass);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                    dto.Status = true;
                    dto.Response = "Se creó el usuario administrador y se asignó el rol.";
                    dto.Result = 1;
                }
                else
                {
                    string errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError($"Seeder ERROR: No se pudo crear el usuario admin. Errores: {errorMessages}");

                    dto.Status = false;
                    dto.Response = $"Fallo al crear admin: {errorMessages}";
                    dto.Result = 3;
                }
            }
            else
            {
                dto.Status = true;
                dto.Response = "El usuario ya fue creado, se salto seeder";
                dto.Result = 2;
            }

            return dto;
        }
    }
}
