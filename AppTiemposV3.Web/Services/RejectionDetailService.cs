using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.RejectionDetails;
using static Microsoft.AspNetCore.WebUtilities.QueryHelpers; 
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;

namespace AppTiemposV3.Web.Services;

public class RejectionDetailService: IRejectionDetailContract<RejectionDetailResponseDto>
{
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl = "/api";
    private readonly JsonSerializerOptions? options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public RejectionDetailService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<DataResponse<RejectionDetailResponseDto>> GetRejectionDetailPorId(Guid id)
    {
        DataResponse<RejectionDetailResponseDto>? rejection = await _httpClient.GetFromJsonAsync<DataResponse<RejectionDetailResponseDto>>($"{BaseUrl}/rejections-details/{id}", options);
        
        if (rejection is null) 
            return new DataResponse<RejectionDetailResponseDto>(true, null!, HttpStatusCode.OK);
        
        return rejection;
    }

    public async Task<GeneralResponse> CreateRejectionDetail(CreateRejectionDetailDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/rejections-details", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<GeneralResponse> UpdateRejectionDetail(Guid id, UpdateRejectionDetailDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PutAsync($"{BaseUrl}/rejections-details/{id}", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<GeneralResponse> DeleteRejectionDetail(Guid id)
    {
        HttpResponseMessage? response = await _httpClient.DeleteAsync($"{BaseUrl}/rejections-details/{id}");

        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse); 
    }

    public async Task<GeneralResponse> RestoreRejectionDetail(Guid id)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/rejections-details/{id}", null);

        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse); 
    }
}