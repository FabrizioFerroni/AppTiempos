using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.Exceptions;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;
using System.Net;
using System.Text;

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

        StringBuilder? sb = new StringBuilder();

        sb.AppendLine("SELECT");
        sb.AppendLine("    DAYNAME(a.StartDate) AS Day,");
        sb.AppendLine("    WEEKDAY(a.StartDate) + 1 AS DayNumber,");
        sb.AppendLine("    COALESCE(ROUND(SUM(");
        sb.AppendLine("        TIMESTAMPDIFF(");
        sb.AppendLine("            SECOND,");
        sb.AppendLine("            CONCAT(a.StartDate, ' ', a.StartTime),");
        sb.AppendLine("            CONCAT(a.StartDate, ' ', a.EndTime)");
        sb.AppendLine("        )");
        sb.AppendLine("    ) / 3600, 2), 0) AS HoursTotal");
        sb.AppendLine("FROM activities AS a");
        sb.AppendLine("WHERE a.EndTime IS NOT NULL");
        sb.AppendLine("    AND a.UserId = @userId");
        sb.AppendLine("    AND YEARWEEK(a.StartDate, 1) = @yearweek");
        sb.AppendLine("GROUP BY a.StartDate");
        sb.AppendLine("ORDER BY a.StartDate;");

        MySqlParameter filtro = new MySqlParameter("@yearweek", yearweek);
        MySqlParameter userFiltro = new MySqlParameter("@userId", _userId);
        
        List<Dictionary<string, object?>> DashboardKPIChartData = await QueryRawAsync(_dbCxt, sb.ToString(), filtro, userFiltro);
        
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

        StringBuilder? sbHs = new StringBuilder();


        sbHs.AppendLine("SELECT");
        sbHs.AppendLine("    COALESCE(ROUND((");
        sbHs.AppendLine("        SELECT");
        sbHs.AppendLine("            SUM(");
        sbHs.AppendLine("                TIMESTAMPDIFF(");
        sbHs.AppendLine("                    SECOND,");
        sbHs.AppendLine("                    CONCAT(a.StartDate, ' ', a.StartTime),");
        sbHs.AppendLine("                    CONCAT(a.StartDate, ' ', a.EndTime)");
        sbHs.AppendLine("                )");
        sbHs.AppendLine("            ) / 3600");
        sbHs.AppendLine("        FROM activities AS a");
        sbHs.AppendLine("        WHERE a.EndTime IS NOT NULL");
        sbHs.AppendLine("            AND YEARWEEK(a.StartDate, 1) = @yearweekHs");
        sbHs.AppendLine("            AND a.UserId = @userId");
        sbHs.AppendLine("    ), 2), 0) AS TotalHours,");
        sbHs.AppendLine("    (");
        sbHs.AppendLine("        SELECT");
        sbHs.AppendLine("            COUNT(a.Id)");
        sbHs.AppendLine("        FROM activities AS a");
        sbHs.AppendLine("        WHERE a.StatusMessage = 'completed'");
        sbHs.AppendLine("            AND a.EndTime IS NOT NULL");
        sbHs.AppendLine("            AND YEARWEEK(a.StartDate, 1) = @yearweekHs");
        sbHs.AppendLine("            AND a.UserId = @userId");
        sbHs.AppendLine("    ) AS CompletedTasks,");
        sbHs.AppendLine("    (");
        sbHs.AppendLine("        SELECT");
        sbHs.AppendLine("            COUNT(a.Id)");
        sbHs.AppendLine("        FROM activities AS a");
        sbHs.AppendLine("        WHERE a.StatusMessage = 'in-progress'");
        sbHs.AppendLine("            AND YEARWEEK(a.StartDate, 1) = @yearweekHs");
        sbHs.AppendLine("            AND a.UserId = @userId");
        sbHs.AppendLine("    ) AS PendingTasks;");

        MySqlParameter filtroHs = new MySqlParameter("@yearweekHs", yearweek);
        
        MySqlParameter userFiltroHs = new MySqlParameter("@userId", _userId);
        
        List<Dictionary<string, object?>> HorasKPIData = await QueryRawAsync(_dbCxt, sbHs.ToString(), filtroHs, userFiltroHs);

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