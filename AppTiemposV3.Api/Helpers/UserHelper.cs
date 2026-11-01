using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace AppTiemposV3.Api.Helpers;

public static class UserHelper
{
    /// <summary>
    /// Obtiene un usuario específico por su Id.
    /// </summary>
    /// <param name="userId">El Id del usuario a buscar.</param>
    /// <returns>Retorna el <see cref="UserEntity"/> correspondiente.</returns>
    /// <exception cref="NotFoundException">Se lanza si no se encuentra el usuario.</exception>
    public static async Task<UserEntity> GetUserByIdAsync(UserManager<UserEntity> userManager, Guid userId)
    {
        UserEntity? user = await userManager.FindByIdAsync(userId.ToString());
        return user ?? throw new NotFoundException("El usuario no fue encontrado");
    }
}
