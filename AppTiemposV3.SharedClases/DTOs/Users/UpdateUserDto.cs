using AppTiemposV3.SharedClases.Enums;

namespace AppTiemposV3.SharedClases.DTOs.Users;

public class UpdateUserDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public Areas Area { get; set; }
}