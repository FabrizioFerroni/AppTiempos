using System.Net;

namespace AppTiemposV3.SharedClases.Exceptions
{

    public class ForbiddenException : Exception
    {
        public int StatusCode { get; } = (int)HttpStatusCode.Forbidden;

        public ForbiddenException(string? message = "Acceso prohibido.")
            : base(message)
        {
        }
    }
}
