
using System.Net;

namespace AppTiemposV3.SharedClases.Exceptions
{
    public class NotFoundException : Exception
    {
        public int StatusCode { get; } = (int)HttpStatusCode.NotFound;

        public NotFoundException(string? message = "El recurso solicitado no fue encontrado.")
            : base(message)
        {
        }
    }
}
