namespace AppTiemposV3.Api.Entities.ConfigurationTable
{
    public class WeeklyHourConfig : BaseEntity
    {
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
