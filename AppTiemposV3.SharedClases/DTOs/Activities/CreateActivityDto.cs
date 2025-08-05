using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.Activities;

public class CreateActivityDto
{
    [Required]
    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [Required]
    [DataType(DataType.Time)]
    public TimeOnly StartTime { get; set; }
    
    [Required]
    public required Guid RequerimentId { get; set; } = Guid.Empty;
    
    [StringLength(255)]
    public string? Description { get; set; } = string.Empty;
    
    [Required]
    public required Guid CategoryId { get; set; } = Guid.Empty;
}