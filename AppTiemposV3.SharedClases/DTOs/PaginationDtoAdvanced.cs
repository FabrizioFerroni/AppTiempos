namespace AppTiemposV3.SharedClases.DTOs;

public class PaginationDtoAdvanced
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
    
    public bool Ascending { get; set; } = true;
    
    public string? Ordenar { get; set; } = string.Empty;
    
    public AdvancedFilters[]? Filters { get; set; }
}


public class AdvancedFilters
{
    public string? Key { get; set; } = null;
    public string? Value { get; set; } = null;
}