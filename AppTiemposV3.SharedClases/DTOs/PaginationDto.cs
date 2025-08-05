namespace AppTiemposV3.SharedClases.DTOs;

public class PaginationDto
{
    public int Pagina { get; set; } = 1;
    
    private int RegistroPorPagina { get; set; } = 10;
    
    private readonly int CantidadMaximaRegistrosPorPagina = 50;

    public int RegistrosPorPagina
    {
        get
        {
            return RegistroPorPagina;
        }
        set
        {
            RegistroPorPagina = (value > CantidadMaximaRegistrosPorPagina) ? CantidadMaximaRegistrosPorPagina : value; 
            
        }
    }
    
    public string? Search { get; set; } = null;
    
    public bool Ascending { get; set; } = true;
    
    public string? Ordenar { get; set; } = string.Empty;
}