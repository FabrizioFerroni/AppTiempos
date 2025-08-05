using System.Net;

namespace AppTiemposV3.SharedClases.Exceptions;

public class InternalServerErrorException: Exception
{
    public int StatusCode { get; } = (int)HttpStatusCode.InternalServerError;

    public InternalServerErrorException(string? message = "Ocurrió un error interno en el servidor.")
        : base(message)
    {
    }
}