using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.Api.Files.MailTemplates.Models;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Audits;
using AppTiemposV3.SharedClases.DTOs.Invitations;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.SharedClases.Exceptions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.Utilidades.TokenHelper;
using static AppTiemposV3.Api.Helpers.Helpers;
using static AppTiemposV3.Api.Helpers.MetadataHelper;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;


namespace AppTiemposV3.Api.Repositories;

public class InvitationRepository : IInvitationContract<InvitationResponseDto>
{
    private readonly AppDbContext _dbCxt;
    private readonly IMapper _iMapper;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IUserContract _userContext;
    private Guid _userId => _userContext.GetUserId();
    private readonly IGenericContract _genericContract;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;
    private readonly IAuditHelperService _auditHelperService;

    public InvitationRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IUserContract userContext, IGenericContract genericContract, IEmailService emailService, IConfiguration config, IAuditHelperService auditHelperService)
    {
        _dbCxt = dbCxt;
        _iMapper = iMapper;
        _userManager = userManager;
        _userContext = userContext;
        _genericContract = genericContract;
        _emailService = emailService;
        _config = config;
        _auditHelperService = auditHelperService;

    }

    public async Task<Pageable<List<InvitationResponseDto>>> GetAllInvitations(PaginationDtoAdvanced pagination)
    {
        List<AdvancedFilters>? filtersList = pagination.Filters?.ToList() ?? new List<AdvancedFilters>();

        filtersList.Add(new AdvancedFilters
        {
            Key = "Finished",
            Value = "false"
        });
        
        filtersList.Add(new AdvancedFilters
        {
            Key = "Accepted",
            Value = "false"
        });
        
        filtersList.Add(new AdvancedFilters
        {
            Key = "Declined",
            Value = "false"
        });


        pagination.Filters = filtersList.ToArray();
        
        Pageable<List<InvitationResponseDto>> response =  await _genericContract.GetAllPaginatedFaAsync<InvitationEntity, InvitationResponseDto>(pagination, _userId);

        return response;
    }

    public async Task<DataResponse<InvitationResponseDto>> GetInvitationPorId(Guid id)
    {
        InvitationEntity inv = await GetInvitationById(id);
        
        InvitationResponseDto resp = _iMapper.Map<InvitationResponseDto>(inv);

        return new DataResponse<InvitationResponseDto>(true, resp, HttpStatusCode.OK);
    }

    public async Task<DataResponse<InvitationResponseDto>> GetInvitationPorToken(string token)
    {
        InvitationEntity? invitation = await _dbCxt.Invitations
            .Where(i => i.Token == token)
            .Where(i => i.Finished == false)
            .Where(i => i.Accepted == false)
            .Where(i => i.Declined == false)
            .FirstOrDefaultAsync();
        
        InvitationResponseDto resp = _iMapper.Map<InvitationResponseDto>(invitation);

        return new DataResponse<InvitationResponseDto>(true, resp, HttpStatusCode.OK);
    }

    public async Task<GeneralResponse> CreateInvitation(CreateInvitationDto dto)
    {
        if (await InvitationExists(dto.Email))
        {
            throw new BadRequestException("El email que pusiste ya existe");
        }
        
        InvitationEntity inv = _iMapper.Map<InvitationEntity>(dto);
        
        await _dbCxt.Invitations.AddAsync(inv);

        await EnsureSavedAsync("Hubo problemas al guardar el registro");

        return new GeneralResponse(true, "Invitacion enviada con éxito");
    }

    public async Task<GeneralResponse> AcceptOrDeclineInvitation(Guid id, AcceptOrDeclineInvitationDto dto)
    {
        InvitationEntity inv = await GetInvitationById(id);
        InvitationEntity oldInv = await GetInvitationById(id);

        inv.ModifiedAt = DateTime.Now;

        if (dto.AcceptDecline)
        {
            object data = new
            {
                nombre = inv.FullName, 
                email = inv.Email, 
                expired = DateTime.Now.AddMinutes(300)
            };

            string token = CrearToken(data);
            inv.Token = token;
            inv.DateSent = DateTime.Now;
            inv.Accepted = true;
            inv.Finished = false;
            inv.Declined = false;
            
            bool result = await SendAcceptedInvitation(inv.Email, token, inv.FullName);

            if (!result)
            {
                throw new BadRequestException("No se ha podido enviar el email");
            }

            inv.DateSent = DateTime.Now;
        }
        else
        {
            inv.Accepted = false;
            inv.Finished = true;
            inv.Declined = true;
            inv.Token = null;
            
            bool result = await SendDeclinedInvitation(inv.Email, inv.FullName);

            if (!result)
            {
                throw new BadRequestException("No se ha podido enviar el email");
            }

            inv.DateSent = DateTime.Now;
        }
            
        _dbCxt.Entry(inv).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo problemas para aceptar o denegar la invitacion");

        return new GeneralResponse(true, $"Invitación {(dto.AcceptDecline ? "aceptada" : "declinada")} con éxito");
    }

    public async Task<GeneralResponse> DeleteInvitation(Guid id)
    {
        InvitationEntity inv = await GetInvitationById(id);

        inv.IsDeleted = true;
        inv.ModifiedAt = DateTime.Now;
        inv.DeletedAt = DateTime.Now;
            
        _dbCxt.Entry(inv).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo problemas para restaurar el registro");

        return new GeneralResponse(true, "Invitacion eliminada con éxito");
    }

    public async Task<DataResponse<EstadosInvitaciones>> VerifyInvitation(string token)
    {
        InvitationEntity? invitation = await _dbCxt.Invitations
            .FirstOrDefaultAsync(i => i.Token == token);

        EstadosInvitaciones status = GetInvitationStatus(invitation);

        return new DataResponse<EstadosInvitaciones>(true, status, HttpStatusCode.OK);
    }
    
    private EstadosInvitaciones GetInvitationStatus(InvitationEntity? invitation)
    {
        if (invitation is null)
            return EstadosInvitaciones.SinAceptar;

        if (!invitation.Finished)
            return invitation.Accepted
                ? EstadosInvitaciones.Aceptado
                : invitation.Declined 
                    ? EstadosInvitaciones.Rechazado 
                    : EstadosInvitaciones.SinAceptar;

        if (invitation.Accepted)
            return EstadosInvitaciones.FinalizadayAceptada;

        if (invitation.Declined)
            return EstadosInvitaciones.FinalizadayRechazado;

        return EstadosInvitaciones.Finalizado;
    }

    /// <summary>
    /// Verifica si existe un requerimiento con rechazo con su id para un usuario, 
    /// opcionalmente excluyendo un requerimiento por su Id.
    /// </summary>
    /// <param name="id">El id de la invitacion a buscar.</param>
    /// <param name="excludeId">Id de una invitacion a excluir de la búsqueda (opcional).</param>
    /// <returns>Retorna <c>true</c> si existe una invitacion que cumpla las condiciones; de lo contrario, <c>false</c>.</returns>
    private async Task<bool> InvitationExists(string email, Guid? excludeId = null)
    {
        return await _dbCxt.Invitations.AnyAsync(r =>
            r.Email == email &&
            (excludeId == null || r.Id != excludeId));
    }
    
    /// <summary>
    /// Intenta guardar los cambios pendientes en el contexto de la base de datos.
    /// </summary>
    /// <param name="errorMessage">Mensaje de error a lanzar si no se guardan cambios.</param>
    /// <exception cref="InternalServerErrorException">
    /// Se lanza si no se guardan cambios en la base de datos.
    /// </exception>
    private async Task EnsureSavedAsync(string errorMessage)
    {
        int result = await _dbCxt.SaveChangesAsync();
        if (result <= 0)
            throw new InternalServerErrorException(errorMessage);
    }

    private async Task<InvitationEntity> GetInvitationById(Guid id)
    {
        InvitationEntity? invitation = await _dbCxt.Invitations
            .Where(i => i.Id == id)
            .Where(i => i.Finished == false)
            .Where(i => i.Accepted == false)
            .Where(i => i.Declined == false)
            .FirstOrDefaultAsync();

        return invitation ??  throw new BadRequestException("Invitación no encontrado");
    }
    
    private async Task<bool> SendAcceptedInvitation(string email, string token, string name)
    {
        SendInvitationEmailModel dto = new SendInvitationEmailModel();
        dto.AppName = _config["appName"]!;
        dto.UrlApp = _config["urlFront"]!;
        dto.FullName = name;
        dto.UrlInvitation = $"{_config["urlFront"]!}/unirme/{token}";
        dto.Email = email;
        dto.Year = DateTime.UtcNow.Year;
        dto.SupportEmail = _config["emailSoporte"]!;
        dto.Tiempo = "5 horas";
        
        string body = await _emailService.GetEmailTemplateAsync("sendInvitation", dto);
        
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
    
    private async Task<bool> SendDeclinedInvitation(string email, string name)
    {
        SendInvitationEmailModel dto = new SendInvitationEmailModel();
        dto.AppName = _config["appName"]!;
        dto.UrlApp = _config["urlFront"]!;
        dto.FullName = name;
        dto.Email = email;
        dto.Year = DateTime.UtcNow.Year;
        dto.SupportEmail = _config["emailSoporte"]!;
        
        string body = await _emailService.GetEmailTemplateAsync("declinedInvitation", dto);
        
        string to = $"{name} <{email}>";
        
        bool res = await _emailService.Send(
            to: to,
            subject: $"Lo sentimos, no se ha aceptado tu solicitud en {_config["appName"]}",
            html: body,
            ""
        );
        
        if (!res)
        {
            throw new BadRequestException("No se ha podido enviar el correo");
        }
        
        return true;
    }
}