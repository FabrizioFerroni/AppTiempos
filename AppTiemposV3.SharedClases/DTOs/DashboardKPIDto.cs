namespace AppTiemposV3.SharedClases.DTOs;

public class DashboardKPIDto
{
    public double TotalHours { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public List<DashboardKPIChart> DashboardKPIChart { get; set; }
    public string? ChartType { get; set; }
}

public class DashboardKPIChart
{
    public string Day { get; set; }
    public int DayNumber { get; set; }
    public double HoursTotal { get; set; }
}