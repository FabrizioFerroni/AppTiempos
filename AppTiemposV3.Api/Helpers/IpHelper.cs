using System.Net;

namespace AppTiemposV3.Api.Helpers;

public static class IpHelper
{
    public static string GetClientIp(IHttpContextAccessor httpContextAccessor)
    {
        HttpContext? context = httpContextAccessor.HttpContext;
        
        if (context == null)
            return "unknown";

        // Proxy / load balancer
        string? forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
            return forwarded.Split(',')[0].Trim();

        return NormalizeIp(context.Connection.RemoteIpAddress);
    }
    
    
    private static string NormalizeIp(IPAddress? ip)
    {
        if (ip == null)
            return "unknown";

        if (IPAddress.IsLoopback(ip))
            return "127.0.0.1";

        if (ip.IsIPv4MappedToIPv6)
            return ip.MapToIPv4().ToString();

        return ip.ToString();
    }
}