using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using static Microsoft.AspNetCore.WebUtilities.QueryHelpers; 
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;


namespace AppTiemposV3.Web.Services;

public class TrainingService : ITrainingContract<TrainingResponseDto>
{
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl = "/api";
    private readonly JsonSerializerOptions? options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public TrainingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DataResponse<TrainingKpiResponse>> GetTrainingKpi()
    {
        string url = $"{BaseUrl}/trainings/kpi";
        
        DataResponse<TrainingKpiResponse>? kpis = await _httpClient.GetFromJsonAsync<DataResponse<TrainingKpiResponse>>(url, options);
        
        if (kpis is null) 
            return new DataResponse<TrainingKpiResponse>(true, null!, HttpStatusCode.OK);
        
        return kpis;
    }

    public async Task<Pageable<List<TrainingResponseDto>>> GetAllTrainings(PaginationDtoAdvanced pagination)
    {
        string url = $"{BaseUrl}/trainings";
        
        
        Dictionary<string, string?> queryParams = new Dictionary<string, string?>
        {
            ["pagina"] = pagination.Pagina.ToString(),
            ["registrosPorPagina"] = pagination.RegistrosPorPagina.ToString(),
            ["ascending"] = pagination.Ascending.ToString()
        };

        if (!string.IsNullOrEmpty(pagination.Ordenar))
        {
            queryParams["ordenar"] = pagination.Ordenar!.ToString();
            
            switch (queryParams["ordenar"])
            {
                case "Fecha":
                    queryParams["ordenar"] = "StartDate";
                    break;
                case "ReqID":
                    queryParams["ordenar"] = "RequerimentId";
                    break;
                case "Capacitador":
                    queryParams["ordenar"] = "Capacitator";
                    break;
                case "Tiempo cargado":
                    queryParams["ordenar"] = "IsLoaded";
                    break;
                case "Estado":
                    queryParams["ordenar"] = "Status";
                    break;
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
        return await _httpClient.GetFromJsonAsync<Pageable<List<TrainingResponseDto>>>(finalUrl, options)
               ?? new Pageable<List<TrainingResponseDto>> { Content = null! };
    }

    public async Task<DataResponse<TrainingResponseDto>> GetTrainingPorId(Guid id)
    {
        DataResponse<TrainingResponseDto>? requeriment = await _httpClient.GetFromJsonAsync<DataResponse<TrainingResponseDto>>($"{BaseUrl}/trainings/{id}", options);
        
        if (requeriment is null) 
            return new DataResponse<TrainingResponseDto>(true, null!, HttpStatusCode.OK);
        
        return requeriment;
    }

    public async Task<GeneralResponse> CreateTraining(CreateTrainingDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/trainings", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<GeneralResponse> UpdateTraining(Guid id, UpdateTrainingDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PutAsync($"{BaseUrl}/trainings/{id}", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<GeneralResponse> DeleteTraining(Guid id)
    {
        HttpResponseMessage? response = await _httpClient.DeleteAsync($"{BaseUrl}/trainings/{id}");

        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse); 
    }

    public async Task<GeneralResponse> RestoreTraining(Guid id)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/trainings/{id}", null);

        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse); 
    }
}