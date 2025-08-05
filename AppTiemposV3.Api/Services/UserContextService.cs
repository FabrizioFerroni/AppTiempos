using AppTiemposV3.SharedClases.Constants;
using AppTiemposV3.SharedClases.Contracts;

namespace AppTiemposV3.Api.Services;

public class UserContextService : IUserContract
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public Guid GetUserId()
    {
        string? value = _httpContextAccessor.HttpContext?.User.FindFirst(CustomClaimTypes.Id)?.Value;
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
    }

    public string? GetEmail() =>
        _httpContextAccessor.HttpContext?.User.FindFirst(CustomClaimTypes.Email)?.Value;

    public string? GetRole() =>
        _httpContextAccessor.HttpContext?.User.FindFirst(CustomClaimTypes.Role)?.Value;
}