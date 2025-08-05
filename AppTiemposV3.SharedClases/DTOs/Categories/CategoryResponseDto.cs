namespace AppTiemposV3.SharedClases.DTOs.Categories;

public class CategoryResponseDto
{
    public Guid Id { get; set; } = Guid.Empty;
    
    public string? Name { get; set; } = string.Empty;
    
    public string? Descripcion {  get; set; } = string.Empty;
    
    public string? Color { get; set; } = string.Empty;
    
    public string? Slug {  get; set; } = string.Empty;
}