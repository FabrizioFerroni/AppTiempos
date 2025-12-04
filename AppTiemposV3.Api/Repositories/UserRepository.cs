using System.Net;
using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Users;
using AppTiemposV3.SharedClases.Exceptions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserDto = AppTiemposV3.SharedClases.DTOs.UserDto;
using MySqlConnector;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Api.Helpers.Helpers;

namespace AppTiemposV3.Api.Repositories;

public class UserRepository : IUserCContract<UserResponseDto>
{
    private readonly AppDbContext _dbCxt;
    private readonly IMapper _iMapper;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IUserContract _userContext;
    private readonly ILogger<UserRepository> _logger;
    private Guid _userId => _userContext.GetUserId();

    public UserRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IUserContract userContext, ILogger<UserRepository> logger)
    {
        _dbCxt = dbCxt;
        _iMapper = iMapper;
        _userManager = userManager;
        _userContext = userContext;
        _logger = logger;
    }
    
    public async Task<DataResponse<UserResponseDto>> GetUserLogged()
    {
        UserEntity user = await GetUserByIdAsync(_userId);
        
        UserResponseDto resUser = _iMapper.Map<UserResponseDto>(user);
        
        IList<string> userRole = await _userManager.GetRolesAsync(user);
        
        resUser.Rol = userRole.First();
        resUser.TwoFactorEnable = user.TwoFactorEnabled;
        
        DataResponse<UserResponseDto> respback = new DataResponse<UserResponseDto>(true, resUser, HttpStatusCode.OK);
        return respback;
    }

    public async Task<GeneralResponse> UpdateUserProfile(UpdateUserDto dto)
    {
        UserEntity user = await GetUserByIdAsync(_userId);
        
        _iMapper.Map(dto, user);
        
        await _userManager.UpdateAsync(user);
        
        // _dbCxt.Entry(user).State = EntityState.Modified;
        //
        // await EnsureSavedAsync("Hubo un problema para actualizar el registro");
        
        return new GeneralResponse(true, "Usuario actualizado correctamente");
    }

    public async Task<GeneralResponse> UpdateUserPassword(UpdatePasswordUserDto dto)
    {
        UserEntity user = await GetUserByIdAsync(_userId);
        
        bool isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, dto.ActualPassword);

        if (!isCurrentPasswordValid)
        {
            _logger.LogWarning("La contraseña actual es incorrecta");
            throw new BadRequestException("La contraseña actual es incorrecta");
        }
        
        if (dto.NewPassword != dto.ConfirmPassword)
        {
            _logger.LogWarning("Las contraseñas no coinciden");
            throw new BadRequestException("Las contraseñas no coinciden");
        }
        
        await ResetUserPassword(user, dto.NewPassword);

        return new GeneralResponse(true, "Contraseña actualizada correctamente");
    }

    public async Task<GeneralResponse> UpdateTwoFactor(EnableTwoFactorUser dto)
    {
        UserEntity user = await GetUserByIdAsync(_userId);

        user.TwoFactorEnabled = dto.EnableTwoFactor;
        
        await _userManager.UpdateAsync(user);
        
        return new GeneralResponse(true, "Se ha actualizado correctamente");
    }

    /// <summary>
    /// Obtiene un usuario específico por su Id.
    /// </summary>
    /// <param name="userId">El Id del usuario a buscar.</param>
    /// <returns>Retorna el <see cref="UserEntity"/> correspondiente.</returns>
    /// <exception cref="NotFoundException">Se lanza si no se encuentra el usuario.</exception>
    private async Task<UserEntity> GetUserByIdAsync(Guid userId)
    {
        UserEntity? user = await _userManager.FindByIdAsync(userId.ToString());
        return user ?? throw new NotFoundException("El usuario no fue encontrado");
    }
    
    private async Task ResetUserPassword(UserEntity user, string newPassword)
    {
        string token = await _userManager.GeneratePasswordResetTokenAsync(user);
        IdentityResult? result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            string errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException(errors);
        }
        
        await _userManager.UpdateSecurityStampAsync(user);

        user.LockoutEnd = null;
        
        user.LastPasswordChange = DateTime.Now;

        await _userManager.UpdateAsync(user);
    }
    
    /// <summary>
    /// Intenta guardar los cambios pendientes en el contexto de la base de datos.
    /// </summary>
    /// <param name="errorMessage">Mensaje de error a lanzar si no se guardan cambios.</param>
    /// <exception cref="InternalServerErrorException">
    /// Se lanza si no se guardan cambios en la base de datos.
    /// </exception>
    private async Task EnsureSavedAsync(string errorMessage)
    {
        int result = await _dbCxt.SaveChangesAsync();
        if (result <= 0)
            throw new InternalServerErrorException(errorMessage);
    }
}