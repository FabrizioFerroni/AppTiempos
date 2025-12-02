namespace AppTiemposV3.SharedClases.DTOs;

public class SavedEventArgs
{
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
    public DateOnly? StartDate { get; set; }
    public Guid? IdResponse { get; set; }
    public string? Obs { get; set; }
}