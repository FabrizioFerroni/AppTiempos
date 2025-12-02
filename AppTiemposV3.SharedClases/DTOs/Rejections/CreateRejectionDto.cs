using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.Rejections;

public class CreateRejectionDto
{
    [Required]
    public required Guid RequerimentId { get; set; } = Guid.Empty;
}