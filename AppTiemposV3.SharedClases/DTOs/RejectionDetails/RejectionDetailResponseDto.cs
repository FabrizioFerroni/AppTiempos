using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.RejectionDetails;

public class RejectionDetailResponseDto
{
    public Guid Id { get; set; } = Guid.Empty;
    
    [DataType(DataType.Date)]
    public DateOnly RejectionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [StringLength(255)]
    public string RejectionReason { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string RejectionDetails { get; set; } = string.Empty;
    
    [DataType(DataType.Date)]
    public DateOnly? SolutionDate { get; set; }
    
    [StringLength(255)]
    public string? SolutionDetails { get; set; }
    
    public string? EstimatedFixTime { get; set; }
    
    public string? ActualFixTime { get; set; }
    
    public string Status { get; set; } = "in-progress";
    
    public int RechazoNro { get; set; } = 0;
    
    public Guid RejectionId { get; set; } = Guid.Empty;
}