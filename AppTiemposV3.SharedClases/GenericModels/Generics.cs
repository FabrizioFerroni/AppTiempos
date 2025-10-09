using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Constants;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.Enums;

namespace AppTiemposV3.SharedClases.GenericModels;

public static class Generics
{
     public static UserSession GetClaimsFromToken(string tokenLocal)
    {
        JwtSecurityTokenHandler? handler = new JwtSecurityTokenHandler();
        JwtSecurityToken? token = handler.ReadJwtToken(tokenLocal);
        IEnumerable<Claim>? claims = token.Claims;

        string idToken = claims.First(c => c.Type == CustomClaimTypes.Id).Value!;
        string fullName = claims.First(c => c.Type == CustomClaimTypes.FullName).Value!;
        Areas area = Enum.Parse<Areas>(claims.First(c => c.Type == CustomClaimTypes.Area).Value!, ignoreCase: true);
        string email = claims.First(c => c.Type == CustomClaimTypes.Email).Value!;
        string role = claims.First(c => c.Type == CustomClaimTypes.Role).Value!;
        string pwdChange = claims.First(c => c.Type == CustomClaimTypes.PwdChange).Value!;
        DateTime.TryParse(pwdChange, out DateTime pwdChangeDT);
        Guid.TryParse(idToken, out Guid id);
        
        return new UserSession(id, fullName, area, email, role, pwdChangeDT);
    }
    
    public static ClaimsPrincipal SetClaimPrincipal(UserSession userSession)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            new List<Claim>
            {
                new(CustomClaimTypes.Id, userSession.Id.ToString()!),
                new(CustomClaimTypes.FullName, userSession.FullName!),
                new(CustomClaimTypes.Area, userSession.Area!.ToString()!),
                new(CustomClaimTypes.Email, userSession.Email!),
                new(CustomClaimTypes.Role, userSession.Role!),
                new(CustomClaimTypes.PwdChange, userSession.LastPasswordChange?.ToString("O")!),
                new(ClaimTypes.Role, userSession.Role!),
                new(ClaimTypes.Name, userSession.FullName!),
                new(ClaimTypes.Email, userSession.Email!),
            }, "JwtAuth"));
    }

    public static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
        };
    }

    public static string SerializeObj<T>(T model) => JsonSerializer.Serialize(model, JsonOptions());
    
    public static T DeserializeJsonString<T>(string json) => JsonSerializer.Deserialize<T>(json, JsonOptions())!;
    
    public static IList<T> DeserializeJsonStringList<T>(string json) => JsonSerializer.Deserialize<IList<T>>(json, JsonOptions())!;
    
    public static StringContent GenerateStringContent(string serializeObj) => new(serializeObj, Encoding.UTF8, "application/json");
}