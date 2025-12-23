using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.Api.Files.MailTemplates.Models;
using AppTiemposV3.SharedClases.Constants;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.Exceptions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.Utilidades.TokenHelper;

namespace AppTiemposV3.Api.Repositories;

public class AuthRepository : IAuthContract
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _iMapper;
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthRepository> _logger;
    private readonly IEmailService _emailService;

    public AuthRepository(AppDbContext dbContext, IMapper iMapper, SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager,
        RoleManager<IdentityRole<Guid>> roleManager, IConfiguration config, ILogger<AuthRepository> logger, IEmailService emailService)
    {
        _dbContext = dbContext;
        _iMapper = iMapper;
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<GeneralResponse> Invite(InviteDto dto)
    {
        UserEntity? user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is not null)
            throw new BadRequestException("El usuario ya se encuentra registrado. Prueba con otro");
        
        
        InvitationEntity invitation = _iMapper.Map<InvitationEntity>(dto);
        
        object data = new
            { nombre = invitation.FullName, email = invitation.Email, expired = DateTime.Now.AddMinutes(300) };

        string token = CrearToken(data);
        
        invitation.Token = token;
            
        await _dbContext.Invitations.AddAsync(invitation);

        await EnsureSavedAsync("Hubo un error al crear la invitación");

        return new GeneralResponse(true, "Se registro correctamente");
    }

    public async Task<GeneralResponse> Register(string token, UserDto dto)
    {
        Dictionary<string, string>? datos = LeerToken<Dictionary<string, string>>(token);
        
        string email = datos!["email"].ToString();
        string expired = datos!["expired"].ToString();

        if (email != dto.Email)
        {
            throw new BadRequestException("El email no coinciden");
        }
        
        InvitationEntity? invitation = await _dbContext.Invitations.FirstOrDefaultAsync(x => x.Email == email);

        if (invitation is null)
        {
            throw new BadRequestException("No existe ninguna invitacion con ese email");
        }
        
        if (!invitation.Accepted)
        {
            throw new BadRequestException("No se ha aceptado tu solicitud.");    
        }

        if (invitation.Finished)
        {
            throw new BadRequestException("Ya se ha utilizado esta invitación");
        }

        if (invitation.Finished && invitation.Token == token)
        {
            throw new BadRequestException("Ya se ha utilizado este token");
        }

        if (token != invitation.Token)
        {
            throw new BadRequestException("El token no coinciden");
        }
        
        DateTimeOffset expiredDate = DateTimeOffset.Parse(expired);

        // comparar con la hora actual en UTC
        if (DateTimeOffset.UtcNow > expiredDate.ToUniversalTime())
        {
            _logger.LogWarning("El token ha expirado");
            AcceptInviteDto dtoInvite = new AcceptInviteDto();
            dtoInvite.Accepted = false;
            await AcceptInvitation(invitation.Id, dtoInvite);
            throw new BadRequestException("La invitacion ha expirado, ponte en contacto con el administrador.");
        }
        
        UserEntity newUser = new UserEntity()
        {
            FullName = dto.FullName,
            Area = dto.Area,
            Email = email,
            PasswordHash = dto.Password,
            UserName = email.Split('@')[0],
            EmailConfirmed = true
        };

        UserEntity? user = await _userManager.FindByEmailAsync(newUser.Email);

        if (user is not null)
            throw new BadRequestException("El usuario ya se encuentra registrado. Prueba con otro");

        IdentityResult? createdUser = await _userManager.CreateAsync(newUser!, dto.Password);

        if (!createdUser.Succeeded)
            throw new InternalServerErrorException(createdUser.Errors.First().Description);


        // Chequea si existe el rol admin y se lo asigna al primer usuario creado, si existe registra como user por defecto
        IdentityRole<Guid>? checkAdmin = await _roleManager.FindByNameAsync("Admin");
        if (checkAdmin is null)
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>() { Name = "Admin" });
            await _userManager.AddToRoleAsync(newUser, "Admin");
            return new GeneralResponse( true, "El usuario ha sido registrado");
        }
        else
        {
            IdentityRole<Guid>? checkUser = await _roleManager.FindByNameAsync("User");

            if (checkUser is null)
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>() { Name = "User" });
            }

            await _userManager.AddToRoleAsync(newUser, "User");
            
            //TODO: Tengo que guardar el token en la bd y poner un campo de que fue registrado ya para invalidar que no vuelvan a mandar con el mismo token aunque va a fallar al registra.

            invitation.Finished = true;
            
            _dbContext.Invitations.Update(invitation);
            await EnsureSavedAsync("Hubo un error al actualizar la invitacion. Intente mas tarde");

            return new GeneralResponse(true, "El usuario ha sido registrado");
        }
    }

    public async Task<GeneralResponse> AcceptInvitation(Guid id, AcceptInviteDto dto)
    {
        InvitationEntity? invitation = await _dbContext.Invitations.FirstOrDefaultAsync(x => x.Id == id);

        if (invitation is null)
        {
            throw new BadRequestException("No se ha encontrado una invitación con ese id");
        }

        if (dto.Accepted)
        {
            invitation.Accepted = true;

            object data = new
                { nombre = invitation.FullName, email = invitation.Email, expired = DateTime.Now.AddMinutes(300) };

            string token = CrearToken(data);

            invitation.Token = token;

            bool result = await SendCompleteRegistration(invitation.Email, token, invitation.FullName);

            if (!result)
            {
                throw new BadRequestException("No se ha podido enviar el email");
            }

            invitation.DateSent = DateTime.Now;
        }
        else
        {
            invitation.Accepted = false;
            invitation.DateSent = null;
        }


        invitation.ModifiedAt = DateTime.Now;
        
        _dbContext.Invitations.Update(invitation);
        await EnsureSavedAsync("Hubo un error al actualizar la invitacion. Intente mas tarde");

        return new GeneralResponse(true, "Se actualizo con exito la invitacion");
    }

    public async Task<LoginResponse?> Login(LoginDto dto, string origin)
    {
            if (dto == null) throw new BadRequestException("El dto está vacío");

            UserEntity? user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null) throw new NotFoundException("El usuario no existe");
            
            if (user.EmailConfirmed == false)
            {
                _logger.LogWarning("El usuario " + user.UserName + " no ha verificado la cuenta que acaba de registrar");
                throw new BadRequestException("No has verificado tu usuario, por favor verifica tu cuenta");
            }
            
            if (user.TwoFactorEnabled)
            {
                await _signInManager.SignOutAsync();
                
                SignInResult? passwordSignInAsync =  await _signInManager.PasswordSignInAsync(user, dto.Password, false, true);

                if (!passwordSignInAsync.RequiresTwoFactor)
                {
                    throw new BadRequestException("El correo o la contraseña son inválidos.");
                }
                
                string code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

                await Send2FaCode(user.Email!, code, user.UserName!, user.FullName);

           
                _logger.LogInformation($"Se ha enviado el codigo al email {user.Email} para que pueda iniciar sesión");
                return new LoginResponse(true, true, null!, $"Se te ha enviado el codigo al email {user.Email} para que puedas iniciar sesión");
                
            }

            await _signInManager.SignOutAsync();
            
            SignInResult? result =
                await _signInManager.PasswordSignInAsync(user, dto.Password, isPersistent: false,
                    lockoutOnFailure: true);

            if (result.Succeeded)
            {
                IList<string> userRole = await _userManager.GetRolesAsync(user);

                UserSession? userSession = new UserSession(user.Id, user.FullName, user.Area, user.Email,
                    userRole.First(), user.LastPasswordChange);

                TokenDto token = GenerateToken(userSession);

                return new LoginResponse(true, false, token, "Te has logueado con éxito");
            }
            else if (result.IsNotAllowed)
            {
                throw new BadRequestException("Debes confirmar tu correo electrónico antes de iniciar sesión.");
            }
            else if (result.IsLockedOut)
            {
                await SendBlockOut(user.Email!, origin!, user.UserName!, user.FullName);
                _logger.LogWarning($"El usuario {user.UserName} ha sido bloqueado por demasiados intentos fallidos");
                throw new BadRequestException($"El usuario {user.UserName} ha sido bloqueado por demasiados intentos fallidos, se envio un mail con instrucciones a seguir. O espere 30 minutos para su reactivación." );
            }
            else
            {
                throw new BadRequestException("El correo o la contraseña son inválidos.");
            }
    }

    public async Task<GeneralResponse> activate2FA(Activate2FA dto)
    {
        UserEntity? user = await _userManager.FindByEmailAsync(dto.Email);
        UserEntity? oldUser = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null) throw new NotFoundException("El usuario no existe");
        
        user.TwoFactorEnabled = dto.IsActivated;
        
        IdentityResult upd = await _userManager.UpdateAsync(user);
        
        if (!upd.Succeeded)
        {
            _logger.LogWarning($"No se ha podido actualizar al usuario {user.UserName} con éxito");
            throw new BadRequestException($"No se ha podido actualizar al usuario {user.UserName} con éxito");
        }

        _logger.LogInformation($"El usuario {user.UserName} {(dto.IsActivated ? "activo" : "desactivo")} la verificación en dos pasos con éxito");
        return new GeneralResponse(true,$"El usuario {user.UserName} {(dto.IsActivated ? "activo" : "desactivo")} la verificación en dos pasos con éxito" );
    }

    public async Task<LoginResponse?> Login2FA(Login2FA dto)
    {
        UserEntity? user = await _userManager.FindByEmailAsync(dto.Email);
       
       bool isValid = await _userManager.VerifyTwoFactorTokenAsync(user!, "Email", dto.Code);

       if (!isValid)
           throw new BadRequestException("Código inválido");

       IList<string> userRole = await _userManager.GetRolesAsync(user!);

       UserSession userSession = new UserSession(user!.Id, user!.FullName, user!.Area, user!.Email,
           userRole.First(), user!.LastPasswordChange);

       TokenDto token = GenerateToken(userSession);
        
       return new LoginResponse(true, true, token, "Te has logueado con éxito");
    }

    public async Task<GeneralResponse> ForgotPassword(ForgotPasswordDto dto)
    {
        UserEntity? user = await _userManager.FindByEmailAsync(dto.Email);
        UserEntity? oldUser = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null || !user.EmailConfirmed)
        {
            _logger.LogWarning("No se encontro el usuario que pidio el cambio de clave");
            throw new  NotFoundException("No se encontro el usuario buscado" );
        }
        
        string tokenUser = await _userManager.GeneratePasswordResetTokenAsync(user);
        string tokenUserEncoded = WebUtility.UrlEncode(tokenUser);
        string token = CrearToken(new { email = user.Email, expired = DateTime.Now.AddMinutes(60), tokenUser = tokenUserEncoded });
        
        await SendForgotPassword(dto.Email, token, user.FullName);
        
        _logger.LogInformation("El usuario " + user.UserName + " necesita revisar su correo " + user.Email + " para poder seguir los pasos de cambiar clave");

        return new GeneralResponse(true, "Se ha enviado instrucciones al correo.");
    }

    public async Task<GeneralResponse> ResetPassword(string token, ResetPasswordDto dto)
    {
        Dictionary<string, string>? datos = LeerToken<Dictionary<string, string>>(token);
        
        string email = datos!["email"].ToString();
        string expired = datos!["expired"].ToString();
        string tokenUser = WebUtility.UrlDecode(datos!["tokenUser"].ToString());
        
        
        DateTimeOffset expiredDate = DateTimeOffset.Parse(expired);

        // comparar con la hora actual en UTC
        if (DateTimeOffset.UtcNow > expiredDate.ToUniversalTime())
        {
            _logger.LogWarning("El token ha expirado");
            throw new BadRequestException("El token ha expirado.");
        }
        
        UserEntity? user = await _userManager.FindByEmailAsync(email);
        UserEntity? oldUser = await _userManager.FindByEmailAsync(email);

        if (user == null || !user.EmailConfirmed)
        {
            _logger.LogWarning("No se encontro el usuario que pidio el cambio de clave");
            throw new  NotFoundException("No se encontro el usuario buscado" );
        }

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            _logger.LogWarning("Las contraseñas no coinciden");
            throw new BadRequestException("Las contraseñas no coinciden");
        }
        
        IdentityResult? result = await _userManager.ResetPasswordAsync(user, tokenUser, dto.NewPassword);
        
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                _logger.LogError($"Error en reset password: {error.Code} - {error.Description}");
            }
            throw new BadRequestException("El token es invalido");
        }
        
        await _userManager.UpdateSecurityStampAsync(user);

        user.LockoutEnd = null;
        
        user.LastPasswordChange = DateTime.Now;

        await _userManager.UpdateAsync(user);

        return new GeneralResponse(true, "Se ha cambiado con éxito la contraseña.");
    }

    private TokenDto GenerateToken(UserSession userSession)
    {
        SymmetricSecurityKey? securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        SymmetricSecurityKey? securityKeyR =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretRefresh"]!));
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        SigningCredentials credentialsR = new SigningCredentials(securityKeyR, SecurityAlgorithms.HmacSha256);

        Claim[] userClaims =
        {
            new Claim(CustomClaimTypes.Id, userSession.Id.ToString()!),
            new Claim(CustomClaimTypes.FullName, userSession.FullName!),
            new Claim(CustomClaimTypes.Area, userSession.Area!.ToString()!),
            new Claim(CustomClaimTypes.Email, userSession.Email!),
            new Claim(CustomClaimTypes.Role, userSession.Role!),
            new Claim(CustomClaimTypes.PwdChange, userSession.LastPasswordChange?.ToString("O")!),
        };

        JwtSecurityToken? token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"]!,
            audience: _config["Jwt:Audience"]!,
            claims: userClaims,
            expires: DateTime.Now.AddHours(10),
            signingCredentials: credentials
        );
        
        // expires: DateTime.Now.AddMinutes(10),

        JwtSecurityToken? tokenR = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"]!,
            audience: _config["Jwt:Audience"]!,
            claims: userClaims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentialsR
        );

        return new TokenDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = new JwtSecurityTokenHandler().WriteToken(tokenR),
        };
    }
    
    private async Task<bool> SendBlockOut(string email, string origin, string username, string name = null)
    {
        MailDto dto = new MailDto();
        dto.Link = $"{origin}/auth/olvide-clave";
        dto.Name = name;
        dto.Username = username;

        string body = await _emailService.GetEmailTemplateAsync("blockedout", dto);

        string to = $"{name} <{email}>";

        bool res = await _emailService.Send(
            to: to,
            subject: "Lo sentimos 😢, te mandamos un instructivo para desbloquear tu cuenta",
            html: body,
            ""
        );
        
        _logger.LogInformation($"Se envio con éxito el codigo de 2fa al usuario con email {email}");

        if (!res)
        {
            throw new BadRequestException("No se ha podido enviar el correo");
        }

        return true;
    }
    
    private async Task<bool> Send2FaCode(string email, string code,  string username, string name = null)
    {
        TwoFactorEmailModel dto = new TwoFactorEmailModel();
        dto.AppName = "Time Tracker Pro";
        dto.UrlApp = "https://localhost:7026";
        dto.Token = code;
        dto.Minutes = 10;
        dto.SupportEmail = "soporte@fabriziodev.tech";
        dto.UnsubscribeUrl = "https://localhost:7026/unsubscribe";
        dto.Year = DateTime.UtcNow.Year;
        

        string body = await _emailService.GetEmailTemplateAsync<TwoFactorEmailModel>("token2fa", dto);
        
        string to = $"{name} <{email}>";

        bool res = await _emailService.Send(
            to: to,
            subject: "Código doble factor 🔐",
            html: body,
            ""
        );
        
        if (!res)
        {
            throw new BadRequestException("No se ha podido enviar el correo");
        }

        _logger.LogInformation($"Se envio con exito el codigo de 2fa al usuario con email {email}");
        return true;
    }

    private async Task<bool> SendForgotPassword(string email, string token, string name)
    {
        ForgotPasswordEmailModel dto = new ForgotPasswordEmailModel();
        dto.AppName = "Time Tracker Pro";
        dto.UrlApp = "https://localhost:7026";
        dto.FullName = name;
        dto.UrlChangePassword = $"{dto.UrlApp}/cambiar-clave/{token}";
        dto.Email = email;
        dto.Year = DateTime.UtcNow.Year;
        dto.SupportEmail = "soporte@fabriziodev.tech";
        
        string body = await _emailService.GetEmailTemplateAsync<ForgotPasswordEmailModel>("forgotPassword", dto);
        
        string to = $"{name} <{email}>";

        bool res = await _emailService.Send(
            to: to,
            subject: "Solicitud de restablecimiento de clave \ud83d\udd11",
            html: body,
            ""
        );
        
        if (!res)
        {
            throw new BadRequestException("No se ha podido enviar el correo");
        }
        
        _logger.LogInformation($"Se envio con exito el token de cambio de contraseña al email {email}");
        return true;
    }

    private async Task<bool> SendCompleteRegistration(string email, string token, string name)
    {
        SendInvitationEmailModel dto = new SendInvitationEmailModel();
        dto.AppName = "Time Tracker Pro";
        dto.UrlApp = "https://localhost:7026";
        dto.FullName = name;
        dto.UrlInvitation = $"{dto.UrlApp}/unirme/{token}";
        dto.Email = email;
        dto.Year = DateTime.UtcNow.Year;
        dto.SupportEmail = "soporte@fabriziodev.tech";
        dto.Tiempo = "5 horas";
        
        string body = await _emailService.GetEmailTemplateAsync<SendInvitationEmailModel>("sendInvitation", dto);
        
        string to = $"{name} <{email}>";
        
        bool res = await _emailService.Send(
            to: to,
            subject: $"Completa tu registro en {dto.AppName}",
            html: body,
            ""
        );
        
        if (!res)
        {
            throw new BadRequestException("No se ha podido enviar el correo");
        }
        
        return true;
    }

    private async Task EnsureSavedAsync(string errorMessage)
    {
        int result = await _dbContext.SaveChangesAsync();
        if (result <= 0)
            throw new InternalServerErrorException(errorMessage);
    }

}