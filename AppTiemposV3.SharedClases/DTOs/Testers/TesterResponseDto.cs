namespace AppTiemposV3.SharedClases.DTOs.Testers;

public class TesterResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
