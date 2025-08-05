

using System.Net;

namespace AppTiemposV3.SharedClases.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public int StatusCode { get; } = (int)HttpStatusCode.Unauthorized;

        public UnauthorizedException(string? message = "No está autorizado para realizar esta acción.")
            : base(message)
        {
        }
    }
}
