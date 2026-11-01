using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.Api.Entities;

public class RejectionEntity : BaseEntity
{
    public int TotalRejections { get; set; } = 0;
    
    [StringLength(10)]
    public string? UrlIndetificator { get; set; }

    public string Status { get; set; } = "pending";

    public bool IsResolve { get; set; } = false;

    public DateTime? ResolvedDate { get; set; } = null;

    public Guid? TesterId { get; set; }
    public TesterEntity? Tester { get; set; }

    public bool IsResolveTesting { get; set; } = false;

    public DateTime? ResolvedDateTesting { get; set; } = null;

    public string? StatusTesting { get; set; } = "pending";

    [Required] public required Guid RequerimentId { get; set; } 

    public RequerimentsEntity Requeriment { get; set; } = null!;

    [Required] public required Guid UserId { get; set; }

    public UserEntity User { get; set; } = null!;

    public ICollection<RejectionDetailEntity> RejectionsDetails { get; set; } = new List<RejectionDetailEntity>();

}