namespace AppTiemposV3.SharedClases.DTOs;

public class Activate2FA
{
    public bool IsActivated { get; set; } = false;
    public string Email { get; set; } = string.Empty;

    public Activate2FA()
    {
        IsActivated = false;
    }
}