namespace AppTiemposV3.SharedClases.DTOs;

public class ValidationResultDto
{
    public bool IsValid { get; set; }
    public string? Error { get; set; }
    public string? Warning { get; set; }
}