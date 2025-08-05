using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Constants;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using AppTiemposV3.SharedClases.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AppTiemposV3.Api.Repositories;

public class AuthRepository : IAuthContract
{
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _config;

    public AuthRepository(SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager,
        RoleManager<IdentityRole<Guid>> roleManager, IConfiguration config)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
    }

    public async Task<GeneralResponse> Register(UserDto dto)
    {
        string username = dto.Email.Split('@')[0];
        UserEntity newUser = new UserEntity()
        {
            FullName = dto.FullName,
            Area = dto.Area,
            Email = dto.Email,
            PasswordHash = dto.Password,
            UserName = username,
        };

        UserEntity? user = await _userManager.FindByEmailAsync(newUser.Email);

        if (user is not null)
            return new GeneralResponse(false, "El usuario ya se encuentra registrado. Prueba con otro");

        IdentityResult? createdUser = await _userManager.CreateAsync(newUser!, dto.Password);

        if (!createdUser.Succeeded)
            throw new InternalServerErrorException(createdUser.Errors.First().Description);
        //return new GeneralResponse(false, "Ha ocurrido un error al intentar registro. Por favor, intente novamente.");


        // Chequea si existe el rol admin y se lo asigna al primer usuario loguado, si existe registra como user por defecto
        IdentityRole<Guid>? checkAdmin = await _roleManager.FindByNameAsync("Admin");
        if (checkAdmin is null)
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>() { Name = "Admin" });
            await _userManager.AddToRoleAsync(newUser, "Admin");
            return new GeneralResponse(true, "El usuario ha sido registrado");
        }
        else
        {
            IdentityRole<Guid>? checkUser = await _roleManager.FindByNameAsync("User");

            if (checkUser is null)
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>() { Name = "User" });
            }


            await _userManager.AddToRoleAsync(newUser, "User");

            return new GeneralResponse(true, "El usuario ha sido registrado");
        }
    }

    public async Task<LoginResponse?> Login(LoginDto dto)
    {
        if (dto == null) return new LoginResponse(false, null!, "El dto está vacío");

        UserEntity? user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null) return new LoginResponse(false, null!, "El usuario no existe");

        SignInResult? result =
            await _signInManager.PasswordSignInAsync(user, dto.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            IList<string> userRole = await _userManager.GetRolesAsync(user);

            UserSession? userSession = new UserSession(user.Id, user.FullName, user.Area, user.Email, userRole.First(), user.LastPasswordChange);

            TokenDto token = GenerateToken(userSession);

            return new LoginResponse(true, token, "Te has logueado con éxito");
        }
        else if (result.IsNotAllowed)
        {
            return new LoginResponse(false, null!, "Debes confirmar tu correo electrónico antes de iniciar sesión.");
        }
        else if (result.IsLockedOut)
        {
            return new LoginResponse(false, null!,
                "Tu cuenta está bloqueada temporalmente\n por múltiples intentos fallidos.");
        }
        else
        {
            return new LoginResponse(false, null!, "El correo o la contraseña son inválidos.");
        }
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
            new Claim(CustomClaimTypes.Area, userSession.Area!),
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
}