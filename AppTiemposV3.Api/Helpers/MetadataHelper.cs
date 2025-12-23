namespace AppTiemposV3.Api.Helpers;

public static class MetadataHelper
{
    public static Dictionary<string, string> BuildCreateMetadata(
        Guid userId,
        string operacion,
        string? userIdentity = null
    )
    {
        DateTime now = DateTime.Now;

        string userIdentifier = string.Empty;

        if(userId != Guid.Empty)
        {
            userIdentifier = userId.ToString();
        }
        else
        {
            userIdentifier = !string.IsNullOrWhiteSpace(userIdentity) ? userIdentity : "Usuario desconocido";
        }

        return new Dictionary<string, string>
        {
            { "Fecha", now.ToString("dd-MM-yyyy") },
            { "Hora", now.ToString("HH:mm:ss") },
            { "UserId", userIdentifier },
            { "Operacion", operacion }
        };
    }
}