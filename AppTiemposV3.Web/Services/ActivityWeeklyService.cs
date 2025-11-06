using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;

namespace AppTiemposV3.Web.Services;

public class ActivityWeeklyService : IActivityWeeklyContract<ActivitiesByDay>
{
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl = "/api";
    private readonly JsonSerializerOptions? options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public ActivityWeeklyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<DataAResponse<ActivitiesByDay>> GetAllActivitiesPerRangeWeek(int year, int weekNumber)
    {
        string yearString = year.ToString();
        string weekNumberString = weekNumber.ToString();
        string url = $"{BaseUrl}/activities/weekly/{yearString}/{weekNumberString}";

        DataAResponse<ActivitiesByDay>? activities = await _httpClient.GetFromJsonAsync<DataAResponse<ActivitiesByDay>>(url, options);
        
        if (activities is null) 
            return new DataAResponse<ActivitiesByDay>(true, [], HttpStatusCode.OK);
        
        return activities;
    }
}