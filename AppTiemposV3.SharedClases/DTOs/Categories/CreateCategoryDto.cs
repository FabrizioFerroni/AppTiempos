using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.Categories;

public class CreateCategoryDto 
{
    [Required]
    public required string Name { get; set; } = string.Empty;
    
    public string? Descripcion {  get; set; } = string.Empty;
    
    public string? Color { get; set; } = string.Empty;
}