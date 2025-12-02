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

    [Required] public required Guid RequerimentId { get; set; } 

    public RequerimentsEntity Requeriment { get; set; } = null!;

    [Required] public required Guid UserId { get; set; }

    public UserEntity User { get; set; } = null!;

    public ICollection<RejectionDetailEntity> RejectionsDetails { get; set; } = new List<RejectionDetailEntity>();
}