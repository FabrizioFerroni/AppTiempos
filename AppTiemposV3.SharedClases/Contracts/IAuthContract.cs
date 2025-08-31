using AppTiemposV3.SharedClases.DTOs;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
namespace AppTiemposV3.SharedClases.Contracts;

public interface IAuthContract
{
    Task<GeneralResponse> Invite(InviteDto dto);
    
    Task<GeneralResponse> Register(string token, UserDto dto);
    
    Task<GeneralResponse> AcceptInvitation(Guid id, AcceptInviteDto dto);

    Task<LoginResponse?> Login(LoginDto dto, string origin);
    
    Task<GeneralResponse> activate2FA(Activate2FA dto);

    Task<LoginResponse?> Login2FA(Login2FA dto);
    
    Task<GeneralResponse> ForgotPassword(ForgotPasswordDto dto);
    
    Task<GeneralResponse> ResetPassword(string token, ResetPasswordDto dto);
}