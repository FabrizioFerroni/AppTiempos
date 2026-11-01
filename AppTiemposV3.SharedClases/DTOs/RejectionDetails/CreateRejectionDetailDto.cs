using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.RejectionDetails;

public class CreateRejectionDetailDto
{
    [DataType(DataType.Date)]
    public DateOnly RejectionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    public string RejectionReason { get; set; } = string.Empty;
    
    public string RejectionDetails { get; set; } = string.Empty;
    
    public Guid RejectionId { get; set; }

    public Guid TesterId { get; set; } = Guid.Empty;
}