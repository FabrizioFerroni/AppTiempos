using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Users;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;

namespace AppTiemposV3.Web.Services;

public class UserService : IUserCContract<UserResponseDto>
{
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl = "/api";
    private readonly JsonSerializerOptions? options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DataResponse<UserResponseDto>> GetUserLogged()
    {
        string url = $"{BaseUrl}/users/profile";
        
        DataResponse<UserResponseDto>? user = await _httpClient.GetFromJsonAsync<DataResponse<UserResponseDto>>(url, options);
        
        if (user is null) 
            return new DataResponse<UserResponseDto>(true, null!, HttpStatusCode.OK);
        
        return user;
    }

    public async Task<GeneralResponse> UpdateUserProfile(UpdateUserDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PutAsync($"{BaseUrl}/users/profile", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<GeneralResponse> UpdateUserPassword(UpdatePasswordUserDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PutAsync($"{BaseUrl}/users/password", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<GeneralResponse> UpdateTwoFactor(EnableTwoFactorUser dto)
    {
        HttpResponseMessage? response = await _httpClient.PutAsync($"{BaseUrl}/users/2fa", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }
}