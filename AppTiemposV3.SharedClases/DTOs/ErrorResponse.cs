namespace AppTiemposV3.SharedClases.DTOs;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string Path { get; set; }
    public DateTime Timestamp { get; set; }
}