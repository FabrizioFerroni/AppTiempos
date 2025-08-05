using System.Net;

namespace AppTiemposV3.SharedClases.DTOs;

public class ServiceResponse
{
    public record class GeneralResponse(bool Flag, string Message);

    public record class DataResponse<T>(bool Success, T Data, HttpStatusCode Status);

    public record class DataAResponse<T>(bool Success, List<T> Data, HttpStatusCode Status);

    public record class LoginResponse(bool Flag, TokenDto Token, string? Message);
}