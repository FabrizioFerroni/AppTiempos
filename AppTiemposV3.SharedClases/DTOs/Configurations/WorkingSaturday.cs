namespace AppTiemposV3.SharedClases.DTOs.Configurations
{
    public class WorkingSaturday
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public double Hours => (EndTime - StartTime).TotalHours;
    }
}
