using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;

namespace AppTiemposV3.Web.Services;

public class ActivityService: IActivityContract<ActivityResponseDto>
{
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl = "/api";
    private readonly JsonSerializerOptions? options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public ActivityService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<DataAResponse<ActivityResponseDto>> GetAllActivities()
    {
        throw new NotImplementedException();
    }

    public Task<Pageable<List<ActivityResponseDto>>> GetAllActivitiesPag(PaginationDto pagination, string buscarPor)
    {
        throw new NotImplementedException();
    }

    public async Task<Pageable<List<ActivityResponseDto>>> GetAllActivitiesPerDayPag(PaginationDto pagination, DateOnly startDate)
    {
        if(pagination.Ordenar == "")
        {
            pagination.Ordenar = "StartDateTimeCombo";
        }
        
        if(pagination.Ordenar == "Descripcion")
        {
            pagination.Ordenar = "Description";
        }
        
        if(pagination.Ordenar == "ReqID")
        {
            pagination.Ordenar = "RequerimentId";
        }
        
        if(pagination.Ordenar == "Fecha y Hora de inicio")
        {
            pagination.Ordenar = "StartDateTimeCombo";
        }

        string dateString = startDate.ToString("yyyy-MM-dd");

        string url = $"{BaseUrl}/activities/date/{dateString}?pagina={pagination.Pagina}" +
                     $"&registrosPorPagina={pagination.RegistrosPorPagina}" +
                     $"&ascending={pagination.Ascending}" +
                     $"&ordenar={pagination.Ordenar}";
        
        if (!string.IsNullOrWhiteSpace(pagination.Search))
            url += $"&search={pagination.Search}";

        return await _httpClient.GetFromJsonAsync<Pageable<List<ActivityResponseDto>>>(url, options)
               ?? new Pageable<List<ActivityResponseDto>> { Content = null };
    }

    public async Task<DataAResponse<ActivityResponseDto>> GetAllActivitiesPerDay(DateOnly startDate)
    {
        string dateString = startDate.ToString("yyyy-MM-dd");
        string url = $"{BaseUrl}/activities/t/date/{dateString}";
        
        DataAResponse<ActivityResponseDto>? activities = await _httpClient.GetFromJsonAsync<DataAResponse<ActivityResponseDto>>(url, options);
        
        if (activities is null) 
            return new DataAResponse<ActivityResponseDto>(true, [], HttpStatusCode.OK);
        
        return activities;
    }


    public Task<Pageable<List<ActivityResponseDto>>> GetAllActivitiesPerRangePag(PaginationDto pagination, DateOnly startDate, DateOnly endDate)
    {
        throw new NotImplementedException();
    }

    public async Task<DataAResponse<ActivityResponseDto>> GetLastThreeActivities(int year, int weekNumber)
    {
        string yearString = year.ToString();
        string weekNumberString = weekNumber.ToString();
        string url = $"{BaseUrl}/activities/u/ultimos-3/{yearString}/{weekNumberString}";

        DataAResponse<ActivityResponseDto>? activities = await _httpClient.GetFromJsonAsync<DataAResponse<ActivityResponseDto>>(url, options);
        
        if (activities is null) 
            return new DataAResponse<ActivityResponseDto>(true, [], HttpStatusCode.OK);
        
        return activities;
    }

    public async Task<DataResponse<ActivityResponseDto>> GetActivityById(Guid id)
    {
        DataResponse<ActivityResponseDto>? activity = await _httpClient.GetFromJsonAsync<DataResponse<ActivityResponseDto>>($"{BaseUrl}/activities/a/{id}", options);
        
        if (activity is null) 
            return new DataResponse<ActivityResponseDto>(true, null!, HttpStatusCode.OK);
        
        return activity;
    }

    public async Task<DataResponse<ActivityResponseDto>> GetActivityByUrl(string url)
    {
        DataResponse<ActivityResponseDto>? activity = await _httpClient.GetFromJsonAsync<DataResponse<ActivityResponseDto>>($"{BaseUrl}/activities/{url}", options);
        
        if (activity is null) 
            return new DataResponse<ActivityResponseDto>(true, null!, HttpStatusCode.OK);
        
        return activity;
    }

    public async Task<DataResponse<Guid>> GetRequerimentActivity(string reqId)
    {
        if (reqId.StartsWith("ReqID"))
        {
            reqId =  reqId.Replace("ReqID", "");
        }
        
        DataResponse<Guid>? requerimentId = await _httpClient.GetFromJsonAsync<DataResponse<Guid>>($"{BaseUrl}/activities/r/{reqId}", options);
        
        if (requerimentId is null) 
            return new DataResponse<Guid>(true, Guid.Empty, HttpStatusCode.OK);
        
        return requerimentId;
    }

    public async Task<GeneralResponse> CreateActivity(CreateActivityDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/activities", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<GeneralResponse> UpdateActivity(Guid id, UpdateActivityDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PutAsync($"{BaseUrl}/activities/{id}", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<GeneralResponse> DeleteActivity(Guid id)
    {
        HttpResponseMessage? response = await _httpClient.DeleteAsync($"{BaseUrl}/activities/{id}");
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public Task<GeneralResponse> RestoreActivity(Guid id)
    {
        throw new NotImplementedException();
    }
}