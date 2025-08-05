using AppTiemposV3.SharedClases.DTOs;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
namespace AppTiemposV3.SharedClases.Contracts;

public interface IAuthContract
{
    Task<GeneralResponse> Register(UserDto dto);

    Task<LoginResponse?> Login(LoginDto dto);
}