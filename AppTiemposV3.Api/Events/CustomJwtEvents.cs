using System.Text.Json;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Constants;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using static System.Text.Json.JsonSerializer;

namespace AppTiemposV3.Api.Events;

public class CustomJwtEvents : JwtBearerEvents
{
    private readonly ILogger<CustomJwtEvents> _logger;

    public CustomJwtEvents(ILogger<CustomJwtEvents> logger)
    {
        _logger = logger;
    }
    
    public override Task Challenge(JwtBearerChallengeContext context)
    {
        context.HandleResponse();
        
        string message = "Token inválido o ausente.";

        if (context.HttpContext.Items.ContainsKey("AuthFailReason"))
        {
            message = context.HttpContext.Items["AuthFailReason"]?.ToString() ?? message;
        }
        
        _logger.LogWarning("Token ausente o invalido. Path: {Path}", context.HttpContext.Request.Path);
        _logger.LogWarning("{Message}. Path: {Path}", message, context.HttpContext.Request.Path);
        
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";

        string result = Serialize(new
        {
            statusCode = 401,
            message,
            path = context.HttpContext.Request.Path.Value,
            timestamp = DateTime.Now.ToString("O"),
        });

        return context.Response.WriteAsync(result);
    }

    public override Task Forbidden(ForbiddenContext context)
    {
        _logger.LogWarning("Acceso denegado para {User}. Path: {Path}", context.HttpContext.User?.Identity?.Name ?? "anonimo", context.HttpContext.Request.Path);
        
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";

        string result = Serialize(new
        {
            statusCode = 403,
            message = "No tenés permiso para acceder a este recurso.",
            path = context.HttpContext.Request.Path.Value,
            timestamp = DateTime.Now.ToString("O"),
        });

        return context.Response.WriteAsync(result);
    }
    
    public override async  Task TokenValidated(TokenValidatedContext context)
    {
        string? userId = context.Principal?.FindFirst(CustomClaimTypes.Id)?.Value;
        
        UserManager<UserEntity> userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<UserEntity>>();
        UserEntity? user = await userManager.FindByIdAsync(userId!);
        
        if (user == null)
        {
            context.HttpContext.Items["AuthFailReason"] = "Usuario no encontrado.";
            _logger.LogWarning("Usuario no encontrado.");
            context.Fail("Usuario no encontrado.");
            return;
        }

        if (!user.EmailConfirmed)
        {
            context.HttpContext.Items["AuthFailReason"] = "El correo electronico no esta confirmado.";
            _logger.LogWarning("El correo electronico no esta confirmado.");
            context.Fail("Correo electrónico no confirmado.");
            return;
        }
        
        string? pwdChangedClaim = context.Principal?.FindFirst(CustomClaimTypes.PwdChange)?.Value;
        
        if (DateTime.TryParse(pwdChangedClaim, out var tokenPwdChanged))
        {
          if (user.LastPasswordChange > tokenPwdChanged)
          {
            context.HttpContext.Items["AuthFailReason"] = "La contraseña fue modificada despues de emitirse el token.";
            _logger.LogWarning("La contraseña fue modificada despues de emitirse el token.");
            context.Fail("La contraseña fue modificada despues de emitirse el token.");
            return; 
          }
        }
        
        _logger.LogInformation("Token validado para el usuario con ID: {UserId}", userId);
    }
}