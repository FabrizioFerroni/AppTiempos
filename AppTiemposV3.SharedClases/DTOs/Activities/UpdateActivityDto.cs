using System.ComponentModel.DataAnnotations;
using AppTiemposV3.SharedClases.Enums;

namespace AppTiemposV3.SharedClases.DTOs.Activities;

public class UpdateActivityDto
{
    public Guid? Id { get; set; }
    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [DataType(DataType.Time)]
    public TimeOnly StartTime { get; set; }
    
    [DataType(DataType.Time)]
    public TimeOnly EndTime { get; set; }
    
    public Guid? RequerimentId { get; set; }
    
    public string? Description { get; set; }

    public bool IsLoaded { get; set; } = false;
    
    public string? StatusMessage { get; set; }
    
    public string? Comment { get; set; }
    
    public Etapas Etapa { get; set; }
}