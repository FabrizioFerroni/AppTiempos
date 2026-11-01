using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.Api.Entities;

public class RejectionDetailEntity : BaseEntity
{
    [Required]
    [DataType(DataType.Date)]
    public DateOnly RejectionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [Required]
    public string RejectionReason { get; set; } = string.Empty;
    
    [Required]
    public string RejectionDetails { get; set; } = string.Empty;
    
    [DataType(DataType.Date)]
    public DateOnly? SolutionDate { get; set; }
    
    public string? SolutionDetails { get; set; }
    
    public string Status { get; set; } = "in-progress";
    
    [Required]
    public required Guid RejectionId { get; set; }

    public RejectionEntity Rejection { get; set; } = null!;
    
    [Required] public required Guid UserId { get; set; }

    public UserEntity User { get; set; } = null!;

    public int RechazoNro { get; set; } = 0;

    public Guid? TesterId { get; set; }
    public TesterEntity? Tester { get; set; }

    public bool BadRejected { get; set; } = false;

    public string? ReasonBadRequest { get; set; } = string.Empty;
}