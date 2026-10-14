using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.Exceptions;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;
using static AppTiemposV3.SharedClases.Helpers.TimeFormatHelper;
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
        DashboardKPIDto resp = new DashboardKPIDto();

        _ = await GetUserByIdAsync(_userId);

        int yearweek = Convert.ToInt32($"{year}{weekNumber:D2}");

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("SELECT");
        sb.AppendLine("    DAYNAME(A.StartDate) AS Day,");
        sb.AppendLine("    WEEKDAY(A.StartDate) + 1 AS DayNumber,");
        sb.AppendLine("    COALESCE(SUM(TIMESTAMPDIFF(MINUTE, A.StartTime, A.EndTime)), 0) AS MinutesTotal");
        sb.AppendLine("FROM activities AS A");
        sb.AppendLine("WHERE A.EndTime IS NOT NULL");
        sb.AppendLine("    AND A.IsDeleted = 0");
        sb.AppendLine("    AND A.UserId = @userId");
        sb.AppendLine("    AND YEARWEEK(A.StartDate, 1) = @yearweek");
        sb.AppendLine("GROUP BY A.StartDate");
        sb.AppendLine("ORDER BY A.StartDate;");

        List<Dictionary<string, object?>> chartData = await QueryRawAsync(
            _dbCxt,
            sb.ToString(),
            new MySqlParameter("@yearweek", yearweek),
            new MySqlParameter("@userId", _userId));

        List<DashboardKPIChart> chartDays = new();

        foreach (Dictionary<string, object?> row in chartData)
        {
            int minutes = Convert.ToInt32(row["MinutesTotal"]);

            chartDays.Add(new DashboardKPIChart
            {
                Day = TranslateDayToSpanish(row["Day"]!.ToString()!),
                DayNumber = Convert.ToInt16(row["DayNumber"]!.ToString()!),
                HoursTotal = FormatoDbl(minutes)
            });
        }

        resp.DashboardKPIChart = CompleteMissingDays(chartDays);

        StringBuilder sbHs = new StringBuilder();

        sbHs.AppendLine("SELECT");
        sbHs.AppendLine("    COALESCE(SUM(CASE WHEN A.EndTime IS NOT NULL");
        sbHs.AppendLine("        THEN TIMESTAMPDIFF(MINUTE, A.StartTime, A.EndTime) END), 0) AS TotalMinutes,");
        sbHs.AppendLine("    COALESCE(SUM(A.StatusMessage = 'completed' AND A.EndTime IS NOT NULL), 0) AS CompletedTasks,");
        sbHs.AppendLine("    COALESCE(SUM(A.StatusMessage = 'in-progress'), 0) AS PendingTasks");
        sbHs.AppendLine("FROM activities AS A");
        sbHs.AppendLine("WHERE A.UserId = @userId");
        sbHs.AppendLine("    AND A.IsDeleted = 0");
        sbHs.AppendLine("    AND YEARWEEK(A.StartDate, 1) = @yearweek;");

        List<Dictionary<string, object?>> kpiData = await QueryRawAsync(
            _dbCxt,
            sbHs.ToString(),
            new MySqlParameter("@yearweek", yearweek),
            new MySqlParameter("@userId", _userId));

        Dictionary<string, object?>? kpiRow = kpiData.FirstOrDefault();

        if (kpiRow != null)
        {
            resp.TotalHours = FormatoDbl(Convert.ToInt32(kpiRow["TotalMinutes"]));
            resp.CompletedTasks = Convert.ToInt32(kpiRow["CompletedTasks"]);
            resp.PendingTasks = Convert.ToInt32(kpiRow["PendingTasks"]);
        }

        return new DataResponse<DashboardKPIDto>(true, resp, HttpStatusCode.OK);
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