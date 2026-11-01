using AppTiemposV3.SharedClases.DTOs.Testers;
using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.RejectionDetails;

public class RejectionDetailResponseDto
{
    public Guid Id { get; set; } = Guid.Empty;
    
    [DataType(DataType.Date)]
    public DateOnly RejectionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    public string RejectionReason { get; set; } = string.Empty;
    
    public string RejectionDetails { get; set; } = string.Empty;
    
    [DataType(DataType.Date)]
    public DateOnly? SolutionDate { get; set; }
    
    public string? SolutionDetails { get; set; }
    
    public string Status { get; set; } = "in-progress";
    
    public int RechazoNro { get; set; } = 0;
    
    public Guid RejectionId { get; set; } = Guid.Empty;

    public TesterResponseDto? Tester { get; set; }

    public bool BadRejected { get; set; } = false;

    public string? ReasonBadRequest { get; set; } = string.Empty;
}