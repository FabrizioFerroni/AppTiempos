using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.Api.Entities;

public class RejectionDetailEntity : BaseEntity
{
    [Required]
    [DataType(DataType.Date)]
    public DateOnly RejectionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [Required]
    [StringLength(255)]
    public string RejectionReason { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string RejectionDetails { get; set; } = string.Empty;
    
    [DataType(DataType.Date)]
    public DateOnly? SolutionDate { get; set; }
    
    [StringLength(255)]
    public string? SolutionDetails { get; set; }
    
    [DataType(DataType.Time)]
    public TimeOnly? EstimatedFixTime { get; set; }
    
    [DataType(DataType.Time)]
    public TimeOnly? ActualFixTime { get; set; } = null;
    
    public string Status { get; set; } = "in-progress";
    
    [Required]
    public required Guid RejectionId { get; set; }

    public RejectionEntity Rejection { get; set; } = null!;
    
    [Required] public required Guid UserId { get; set; }

    public UserEntity User { get; set; } = null!;

    public int RechazoNro { get; set; } = 0;
}