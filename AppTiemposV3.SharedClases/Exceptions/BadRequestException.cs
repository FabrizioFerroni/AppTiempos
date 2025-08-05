using System.Net;

namespace AppTiemposV3.SharedClases.Exceptions
{
    public class BadRequestException : Exception
    {
        public int StatusCode { get; } = (int)HttpStatusCode.BadRequest;

        public BadRequestException(string? message = "La solicitud es inválida.")
            : base(message)
        {
        }
    }
}
