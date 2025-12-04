using AppTiemposV3.SharedClases.DTOs.Users;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts;

public interface IUserCContract<T>
{
    Task<DataResponse<T>> GetUserLogged();

    Task<GeneralResponse> UpdateUserProfile(UpdateUserDto dto);
    
    Task<GeneralResponse> UpdateUserPassword(UpdatePasswordUserDto dto);
    
    Task<GeneralResponse> UpdateTwoFactor(EnableTwoFactorUser dto);
}