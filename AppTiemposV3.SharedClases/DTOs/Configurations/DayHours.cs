namespace AppTiemposV3.SharedClases.DTOs.Configurations
{
    public class DayHours
    {
        public Guid? Id { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
