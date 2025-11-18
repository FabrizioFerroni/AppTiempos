using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppTiemposV3.Api.Entities;

public class TrainingEntity : BaseEntity
{
    [Required]
    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [Required]
    [DataType(DataType.Time)]
    public TimeOnly StartTime { get; set; }
    
    [DataType(DataType.Time)]
    public TimeOnly? EndTime { get; set; } = null;
    
    [Required]
    public string Capacitator { get; set; } = string.Empty;
    
    public string? Description { get; set; } = null;
    
    public bool IsLoaded { get; set; } = false;

    public string Status { get; set; } = "in-progress";
    
    public string? Notes { get; set; } = null;
    
    [Required]
    public required Guid RequerimentId { get; set; } = Guid.Empty;
    
    public RequerimentsEntity Requeriment { get; set; } = null!;
    
    [NotMapped]
    public TimeSpan CapacitationTime { get; set; }
    
    [Required]
    public required Guid UserId { get; set; }

    public UserEntity User { get; set; } = null!;
}