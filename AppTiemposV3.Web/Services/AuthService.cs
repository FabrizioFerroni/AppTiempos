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

    public async Task<GeneralResponse> Register(UserDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/register", GenerateStringContent(SerializeObj(dto)));
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, "Error occured while register. Please try again later.");
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<LoginResponse?> Login(LoginDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/login", GenerateStringContent(SerializeObj(dto)));
        
        if(!response.IsSuccessStatusCode)
            return new LoginResponse(false, null!, "Error occured while logging in. Please try again later.");
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        LoginResponse? result =  DeserializeJsonString<LoginResponse>(apiResponse);

        if (result?.Token != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", result.Token.AccessToken);
        }

        return result;
    }
}