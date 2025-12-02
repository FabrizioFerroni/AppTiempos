using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.RejectionDetails;

public class UpdateRejectionDetailDto
{
    public Guid Id { get; set; } = Guid.Empty;
    
    [DataType(DataType.Date)]
    public DateOnly RejectionDate { get; set; }  = DateOnly.FromDateTime(DateTime.Today);
    
    [StringLength(255)]
    public string? RejectionReason { get; set; }
    
    [StringLength(255)]
    public string? RejectionDetails { get; set; }
    
    [DataType(DataType.Date)]
    public DateOnly? SolutionDate { get; set; }
    
    [StringLength(255)]
    public string? SolutionDetails { get; set; }
    
    [DataType(DataType.Time)]
    public TimeOnly? EstimatedFixTime { get; set; }
    
    [DataType(DataType.Time)]
    public TimeOnly? ActualFixTime { get; set; }
    
    public string? Status { get; set; }
    
    public Guid RejectionId { get; set; }
}