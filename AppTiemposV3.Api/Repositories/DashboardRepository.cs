using System.Net;
using System.Text.Json;
using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.Exceptions;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;

namespace AppTiemposV3.Api.Repositories;

public class DashboardRepository : IDashboardContract<DashboardKPIDto>
{
    
    private readonly AppDbContext _dbCxt;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IUserContract _userContext;
    private Guid _userId => _userContext.GetUserId();
    
    public DashboardRepository(AppDbContext dbCxt, UserManager<UserEntity> userManager, IUserContract userContext)
    {
        _dbCxt = dbCxt;
        _userManager = userManager;
        _userContext = userContext;
    }
    
    public async Task<DataResponse<DashboardKPIDto>> GetKpiDashboard(int year, int weekNumber)
    {
        DashboardKPIDto? resp = new DashboardKPIDto();
        
        _ = await GetUserByIdAsync(_userId);

        string yearweekCon = $"{year}{weekNumber:D2}";
        int yearweek = Convert.ToInt32(yearweekCon);

        string sql = @"SELECT 
                            DAYNAME(a.StartDate) AS Day,
                            WEEKDAY(a.StartDate) + 1 AS DayNumber,
                            COALESCE(ROUND(SUM(
                                TIMESTAMPDIFF(
                                    SECOND,
                                    CONCAT(a.StartDate, ' ', a.StartTime),
                                    CONCAT(a.StartDate, ' ', a.EndTime)
                                )
                            ) / 3600, 2), 0) AS HoursTotal
                        FROM activities AS a
                        WHERE a.EndTime IS NOT NULL
                            AND a.UserId = @userId  
                            AND YEARWEEK(a.StartDate, 1) = @yearweek
                        GROUP BY a.StartDate
                        ORDER BY a.StartDate;
                        ";
        MySqlParameter filtro = new MySqlParameter("@yearweek", yearweek);
        MySqlParameter userFiltro = new MySqlParameter("@userId", _userId);
        
        List<Dictionary<string, object?>> DashboardKPIChartData = await QueryRawAsync(_dbCxt, sql, filtro, userFiltro);
        
        List<DashboardKPIChart> chartDays = new();

        foreach (Dictionary<string, object?> row in DashboardKPIChartData)
        {
            DashboardKPIChart chart = new DashboardKPIChart
            {
                Day = TranslateDayToSpanish(row["Day"]!.ToString()!),
                DayNumber = Convert.ToInt16(row["DayNumber"]!.ToString()!),
                HoursTotal = Convert.ToDouble(row["HoursTotal"]!.ToString()!)
            };

            chartDays.Add(chart);
        }
        
        chartDays = CompleteMissingDays(chartDays);
        resp.DashboardKPIChart = chartDays;
        
        string sqlHs = @"
            SELECT
                COALESCE(ROUND((
                    SELECT 
                        SUM(
                            TIMESTAMPDIFF(
                                SECOND,
                                CONCAT(a.StartDate, ' ', a.StartTime),
                                CONCAT(a.StartDate, ' ', a.EndTime)
                            )
                        ) / 3600
                    FROM activities AS a
                    WHERE a.EndTime IS NOT NULL
                      AND YEARWEEK(a.StartDate, 1) = @yearweekHs
                      AND a.UserId = @userId  
                ), 2),0 ) AS TotalHours,
                
                (
                    SELECT 
                        COUNT(a.Id)
                    FROM activities AS a
                    WHERE a.StatusMessage = 'completed'
                      AND a.EndTime IS NOT NULL
                      AND YEARWEEK(a.StartDate, 1) = @yearweekHs
                      AND a.UserId = @userId  
                ) AS CompletedTasks,                
                (
                    SELECT COUNT(a.Id)
                    FROM activities AS a
                    WHERE a.StatusMessage = 'in-progress'
                      AND YEARWEEK(a.StartDate, 1) = @yearweekHs
                      AND a.UserId = @userId  
                ) AS PendingTasks;
        ";
        MySqlParameter filtroHs = new MySqlParameter("@yearweekHs", yearweek);
        
        MySqlParameter userFiltroHs = new MySqlParameter("@userId", _userId);
        
        List<Dictionary<string, object?>> HorasKPIData = await QueryRawAsync(_dbCxt, sqlHs, filtroHs, userFiltroHs);

        foreach (Dictionary<string, object?> row in HorasKPIData)
        {
            resp.TotalHours = Convert.ToDouble(row["TotalHours"]!.ToString()!);
            resp.CompletedTasks = Convert.ToInt32(row["CompletedTasks"]!.ToString()!);
            resp.PendingTasks = Convert.ToInt32(row["PendingTasks"]!.ToString()!);
        }
        

        DataResponse<DashboardKPIDto> respback = new DataResponse<DashboardKPIDto>(true, resp, HttpStatusCode.OK);
        return respback;
    }
    
    private async Task<UserEntity> GetUserByIdAsync(Guid userId)
    {
        UserEntity? user = await _userManager.FindByIdAsync(userId.ToString());
        return user ?? throw new NotFoundException("El usuario no fue encontrado");
    }
    
    private string TranslateDayToSpanish(string day)
    {
        return day switch
        {
            "Monday" => "Lunes",
            "Tuesday" => "Martes",
            "Wednesday" => "Miercoles",
            "Thursday" => "Jueves",
            "Friday" => "Viernes",
            "Saturday" => "Sábado",
            "Sunday" => "Domingo",
            _ => day 
        };
    }
    
    private static List<DashboardKPIChart> CompleteMissingDays(List<DashboardKPIChart> chartDays)
    {
        List<string> allDays = new List<string>
        {
            "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado", "Domingo"
        };

        foreach ((string day, int index) in allDays.Select((string d, int i) => (d, i)))
        {
            if (!chartDays.Any(c => c.Day.Equals(day, StringComparison.OrdinalIgnoreCase)))
            {
                chartDays.Add(new DashboardKPIChart
                {
                    Day = day,
                    DayNumber = index + 1,
                    HoursTotal = 0
                });
            }
        }

        return chartDays.OrderBy(c => c.DayNumber).ToList();
    }
}