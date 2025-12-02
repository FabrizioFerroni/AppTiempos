using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.RejectionDetails;

public class CreateRejectionDetailDto
{
    [DataType(DataType.Date)]
    public DateOnly RejectionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [StringLength(255)]
    public string RejectionReason { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string RejectionDetails { get; set; } = string.Empty;
    
    public Guid RejectionId { get; set; }
}