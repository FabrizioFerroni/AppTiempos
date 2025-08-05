using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.Trainings;

public class UpdateTrainingDto
{
    [DataType(DataType.Time)]
    public DateOnly? StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [DataType(DataType.Time)]
    public TimeOnly? StartTime { get; set; }

    [DataType(DataType.Time)]
    public TimeOnly? EndTime { get; set; }
    
    public string? Capacitor { get; set; }
    
    public string? Description { get; set; }
    
    public string? Notes { get; set; }
    
    public bool? IsLoaded { get; set; }
    
    public string? Status { get; set; }
}