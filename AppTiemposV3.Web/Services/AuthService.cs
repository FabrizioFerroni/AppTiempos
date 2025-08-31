using System.Net.Http.Headers;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;

namespace AppTiemposV3.Web.Services;

public class AuthService: IAuthContract
{
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl = "/auth";

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<GeneralResponse> Invite(InviteDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<GeneralResponse> Register(string token, UserDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/register/{token}", GenerateStringContent(SerializeObj(dto)));
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse( false, "Error occured while register. Please try again later.");
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public Task<GeneralResponse> AcceptInvitation(Guid id, AcceptInviteDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<LoginResponse?> Login(LoginDto dto, string origin)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/login", GenerateStringContent(SerializeObj(dto)));
        
        if(!response.IsSuccessStatusCode)
            return new LoginResponse( false, null!, "Error occured while logging in. Please try again later.");
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        LoginResponse? result =  DeserializeJsonString<LoginResponse>(apiResponse);

        if (result?.Token != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", result.Token.AccessToken);
        }

        return result;
    }

    public Task<GeneralResponse> activate2FA(Activate2FA dto)
    {
        throw new NotImplementedException();
    }

    public Task<LoginResponse?> Login2FA(Login2FA dto)
    {
        throw new NotImplementedException();
    }

    public Task<GeneralResponse> ForgotPassword(ForgotPasswordDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<GeneralResponse> ResetPassword(string token,ResetPasswordDto dto)
    {
        throw new NotImplementedException();
    }
}