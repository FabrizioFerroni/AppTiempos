using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Services;

public class DashboardService : IDashboardContract<DashboardKPIDto>
{
    
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl = "/api";
    private readonly JsonSerializerOptions? options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public DashboardService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<DataResponse<DashboardKPIDto>> GetKpiDashboard(int year, int weekNumber)
    {
        string yearString = year.ToString();
        string weekNumberString = weekNumber.ToString();
        string url = $"{BaseUrl}/dashboard/kpi/{yearString}/{weekNumberString}";

        DataResponse<DashboardKPIDto>? kpis = await _httpClient.GetFromJsonAsync<DataResponse<DashboardKPIDto>>(url, options);
        
        DashboardKPIDto nullDto = new DashboardKPIDto()
        {
            TotalHours = 0.0,
            CompletedTasks = 0,
            PendingTasks = 0,
            DashboardKPIChart = [
                new DashboardKPIChart()
                {
                    Day = "Lunes",
                    DayNumber = 1,
                    HoursTotal = 0
                },
                new DashboardKPIChart()
                {
                    Day = "Martes",
                    DayNumber = 2,
                    HoursTotal = 0
                },
                new DashboardKPIChart()
                {
                    Day = "Miercoles",
                    DayNumber = 3,
                    HoursTotal = 0
                },
                new DashboardKPIChart()
                {
                    Day = "Jueves",
                    DayNumber = 4,
                    HoursTotal = 0
                },
                new DashboardKPIChart()
                {
                    Day = "Viernes",
                    DayNumber = 5,
                    HoursTotal = 0
                },
                new DashboardKPIChart()
                {
                    Day = "Sabado",
                    DayNumber = 6,
                    HoursTotal = 0
                }
            ]
        };
        
        if (kpis is null) 
            return new DataResponse<DashboardKPIDto>(true, nullDto!, HttpStatusCode.OK);
        
        return kpis;
    }
}