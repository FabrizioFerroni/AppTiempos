using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Rejections;
using static Microsoft.AspNetCore.WebUtilities.QueryHelpers; 
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;

namespace AppTiemposV3.Web.Services;

public class RejectionService: IRejectionContract<RejectionResponseDto>
{
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl = "/api";
    private readonly JsonSerializerOptions? options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public RejectionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<DataResponse<RejectionKpiResponse>> GetRejectionKpi()
    {
        string url = $"{BaseUrl}/rejections/kpi";
        
        DataResponse<RejectionKpiResponse>? kpis = await _httpClient.GetFromJsonAsync<DataResponse<RejectionKpiResponse>>(url, options);
        
        if (kpis is null) 
            return new DataResponse<RejectionKpiResponse>(true, null!, HttpStatusCode.OK);
        
        return kpis;
    }

    public async Task<Pageable<List<RejectionResponseDto>>> GetAllRejections(PaginationDtoAdvanced pagination)
    {
       string url = $"{BaseUrl}/rejections";
        
        
        Dictionary<string, string?> queryParams = new Dictionary<string, string?>
        {
            ["pagina"] = pagination.Pagina.ToString(),
            ["registrosPorPagina"] = pagination.RegistrosPorPagina.ToString(),
            ["ascending"] = pagination.Ascending.ToString()
        };

        if (!string.IsNullOrEmpty(pagination.Ordenar))
        {
            queryParams["ordenar"] = pagination.Ordenar!;
            
            switch (queryParams["ordenar"])
            {
                default:
                    queryParams["ordenar"] = "CreatedAt";
                    break;
            }
        }
        
        if (pagination.Filters is { Length: > 0 })
        {
            for (int i = 0; i < pagination.Filters.Length; i++)
            {
                AdvancedFilters filter = pagination.Filters[i];
                if (!string.IsNullOrWhiteSpace(filter.Key))
                {
                    string key = filter.Key;
                    if (!string.IsNullOrEmpty(key))
                    {
                        key = char.ToUpper(key[0]) + key.Substring(1); 
                    }
                    queryParams[$"filters[{i}].key"] = key;
                }
                if (!string.IsNullOrWhiteSpace(filter.Value))
                {
                    queryParams[$"filters[{i}].value"] = filter.Value;
                }
            }
        }

        string finalUrl = AddQueryString(url, queryParams);
        return await _httpClient.GetFromJsonAsync<Pageable<List<RejectionResponseDto>>>(finalUrl, options)
               ?? new Pageable<List<RejectionResponseDto>> { Content = null! };
    }

    public async Task<DataResponse<RejectionResponseDto>> GetRejectionPorId(Guid id)
    {
        DataResponse<RejectionResponseDto>? rejection = await _httpClient.GetFromJsonAsync<DataResponse<RejectionResponseDto>>($"{BaseUrl}/rejections/{id}", options);
        
        if (rejection is null) 
            return new DataResponse<RejectionResponseDto>(true, null!, HttpStatusCode.OK);
        
        return rejection;
    }

    public async Task<DataResponse<RejectionResponseDto>> GetRejectionPorUrl(string url)
    {
        DataResponse<RejectionResponseDto>? rejection = await _httpClient.GetFromJsonAsync<DataResponse<RejectionResponseDto>>($"{BaseUrl}/rejections/url/{url}", options);
        
        if (rejection is null) 
            return new DataResponse<RejectionResponseDto>(true, null!, HttpStatusCode.OK);
        
        return rejection;
    }

    public async Task<DataResponse<CreateRejectionResponseDto>> CreateRejection(CreateRejectionDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/rejections", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
        {
            CreateRejectionResponseDto respFailed = new CreateRejectionResponseDto
            {
                Id = Guid.Empty,
                Message = resultError?.Message!,
            };
            
            return new DataResponse<CreateRejectionResponseDto>(false, respFailed, HttpStatusCode.OK);
        }
        
        return DeserializeJsonString<DataResponse<CreateRejectionResponseDto>>(apiResponse);
    }

    public async Task<GeneralResponse> UpdateRejection(Guid id, UpdateRejectionDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PutAsync($"{BaseUrl}/rejections/{id}", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public Task<GeneralResponse> UpdateRejectionCount(Guid id, UpdateRejectionCountDto dto)
    {
        return Task.FromResult(new GeneralResponse(true, "Metodo no implementado"));
    }

    public async Task<GeneralResponse> DeleteRejection(Guid id)
    {
        HttpResponseMessage? response = await _httpClient.DeleteAsync($"{BaseUrl}/rejections/{id}");

        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse); 
    }

    public async Task<GeneralResponse> RestoreRejection(Guid id)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/rejections/{id}", null);

        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse); 
    }
}