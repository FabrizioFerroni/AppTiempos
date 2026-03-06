using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.Api.Entities;

public class CategoriesEntity : BaseEntity
{
    [Required]
    public required string Name { get; set; } = string.Empty;
    
    public string? Descripcion {  get; set; } = string.Empty;
    
    public string? Color { get; set; } = string.Empty;
    
    public string? Slug {  get; set; } = string.Empty;
        
    public ICollection<RequerimentsEntity> Requeriments { get; set; } = new List<RequerimentsEntity>();
}