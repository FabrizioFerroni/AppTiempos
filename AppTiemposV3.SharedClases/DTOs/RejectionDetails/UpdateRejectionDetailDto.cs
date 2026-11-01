using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.RejectionDetails;

public class UpdateRejectionDetailDto
{
    public Guid Id { get; set; } = Guid.Empty;
    
    [DataType(DataType.Date)]
    public DateOnly RejectionDate { get; set; }  = DateOnly.FromDateTime(DateTime.Today);
    
    public string? RejectionReason { get; set; }
    
    public string? RejectionDetails { get; set; }
    
    [DataType(DataType.Date)]
    public DateOnly? SolutionDate { get; set; }
    
    [StringLength(255)]
    public string? SolutionDetails { get; set; }
    
    public string? Status { get; set; }
    
    public Guid RejectionId { get; set; }

    public Guid TesterId { get; set; } = Guid.Empty;

    public bool BadRejected { get; set; } = false;

    public string? ReasonBadRequest { get; set; } = string.Empty;
}