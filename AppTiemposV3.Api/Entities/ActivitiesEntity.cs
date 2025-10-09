using System.ComponentModel.DataAnnotations;
using AppTiemposV3.SharedClases.Enums;

namespace AppTiemposV3.Api.Entities;

public class ActivitiesEntity : BaseEntity
{
    [Required]
    [StringLength(10)]
    public string UrlIndetificator { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [Required]
    [DataType(DataType.Time)]
    public TimeOnly StartTime { get; set; }
    
    [DataType(DataType.Time)]
    public TimeOnly? EndTime { get; set; } = null;
    
    [StringLength(255)]
    public string? Description { get; set; } = null;
    
    public bool IsLoaded { get; set; } = false;
    
    [StringLength(15)]
    public string StatusMessage { get; set; } = "En Progreso";
    
    [StringLength(255)]
    public string? Comment { get; set; } = null;
    
    public Etapas Etapa { get; set; } = Etapas.Alta;
    
    [Required]
    public required Guid RequerimentId { get; set; } = Guid.Empty;
    
    public RequerimentsEntity Requeriment { get; set; } = null!;
    
   /* [Required]
    public required Guid CategoryId { get; set; } = Guid.Empty;
    
    public CategoriesEntity Category { get; set; } = null!;*/
    
    [Required]
    public required Guid UserId { get; set; } = Guid.Empty;

    public UserEntity User { get; set; } = null!;
}