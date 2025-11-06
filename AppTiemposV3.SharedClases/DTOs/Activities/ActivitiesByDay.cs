namespace AppTiemposV3.SharedClases.DTOs.Activities;

public class ActivitiesByDay
{
    public DateOnly Day { get; set; }
    public string DayName { get; set; } = "";
    public string DayNameAndDay { get; set; } = "";
    public TimeSpan Worked { get; set; }
    public List<ActivityResponseDto> Activities { get; set; } = new();
}