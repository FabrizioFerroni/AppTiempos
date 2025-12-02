namespace AppTiemposV3.SharedClases.DTOs.Rejections;

public class CreateRejectionResponseDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ReqId { get; set; } = string.Empty;
}