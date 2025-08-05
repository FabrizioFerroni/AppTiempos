namespace AppTiemposV3.SharedClases.Contracts;

public interface IUserContract
{
    Guid GetUserId();
    string? GetEmail();
    string? GetRole();
}