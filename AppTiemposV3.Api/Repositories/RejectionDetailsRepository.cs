using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.RejectionDetails;
using AppTiemposV3.SharedClases.DTOs.Rejections;
using AppTiemposV3.SharedClases.Exceptions;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;
using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppTiemposV3.Api.Repositories;

public class RejectionDetailsRepository : IRejectionDetailContract<RejectionDetailResponseDto>
{
    private readonly AppDbContext _dbCxt;
    private readonly IMapper _iMapper;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IGenericContract _genericContract;
    private readonly IUserContract _userContext;
    private readonly IRejectionContract<RejectionResponseDto> _rejectionContract;
    private Guid _userId => _userContext.GetUserId();

    public RejectionDetailsRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IGenericContract genericContract, IUserContract userContext, IRejectionContract<RejectionResponseDto> rejectionContract)
    {
        _dbCxt = dbCxt;
        _iMapper = iMapper;
        _userManager = userManager;
        _genericContract = genericContract;
        _userContext = userContext;
        _rejectionContract = rejectionContract;
    }

    public async Task<DataResponse<RejectionDetailResponseDto>> GetRejectionDetailPorId(Guid id)
    {
        RejectionDetailEntity rejDetail = await GetRejectionDetailByIdAsync(id);
        
        RejectionDetailResponseDto res = _iMapper.Map<RejectionDetailResponseDto>(rejDetail);
        
        return new DataResponse<RejectionDetailResponseDto>(true, res, HttpStatusCode.OK);
    }

    public async Task<GeneralResponse> CreateRejectionDetail(CreateRejectionDetailDto dto)
    {
        await RejectionExists(dto.RejectionId);
        
        UserEntity user = await GetUserByIdAsync(_userId);

        RejectionDetailEntity rejDetailE = await GetRejectionDetailByRejectionIdAsync(dto.RejectionId);

        if (rejDetailE is not null && (rejDetailE!.Status == "in-progress" || rejDetailE!.Status == "pending"))
        {
            throw new BadRequestException("Hay un rechazo pendiente que todavía hay que resolver.");
        }
        
        RejectionDetailEntity rejDetails = _iMapper.Map<RejectionDetailEntity>(dto);
        
        rejDetails.UserId = user.Id;
        rejDetails.RechazoNro = await RechazoNroIdentity(user.Id, rejDetails.RejectionId);
            
        await _dbCxt.RejectionDetails.AddAsync(rejDetails);

        await EnsureSavedAsync("Hubo un error al crear el detalle del rechazo", _dbCxt);
        
        await UpdateCountRejections(dto.RejectionId, "Create");

        return new GeneralResponse(true, "Detalle del rechazo creado correctamente");
    }

    public async Task<GeneralResponse> UpdateRejectionDetail(Guid id, UpdateRejectionDetailDto dto)
    {
        await RejectionExists(dto.RejectionId);
        
        RejectionDetailEntity rejDetail = await GetRejectionDetailByIdAsync(id, dto.RejectionId);

        _iMapper.Map(dto, rejDetail);
        
        rejDetail.Id = id;
        rejDetail.ModifiedAt = DateTime.Now;
        
        _dbCxt.Entry(rejDetail).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo problemas para actualizar el registro", _dbCxt);
        
        await UpdateCountRejections(rejDetail.RejectionId, "Update");
        
        return new GeneralResponse(true, "Detalle del rechazo actualizado correctamente");
    }

    public async Task<GeneralResponse> DeleteRejectionDetail(Guid id)
    {
        RejectionDetailEntity rejDetail = await GetRejectionDetailByIdAsync(id);
        
        rejDetail.IsDeleted = true;
        rejDetail.DeletedAt = DateTime.Now;
        
        _dbCxt.Entry(rejDetail).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo problemas para eliminar el registro", _dbCxt);

        await UpdateCountRejections(rejDetail.RejectionId, "Delete");
        
        return new GeneralResponse(true, "Detalle del rechazo eliminado correctamente");
    }

    public async Task<GeneralResponse> RestoreRejectionDetail(Guid id)
    {
        RejectionDetailEntity rejDetail = await GetRejectionDetailByIdAsync(id, includeDeleted: true);
        
        rejDetail.IsDeleted = false;
        rejDetail.ModifiedAt = DateTime.Now;
        rejDetail.DeletedAt = null;
        
        _dbCxt.Entry(rejDetail).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo problemas para restaurar el registro", _dbCxt);
        
        await UpdateCountRejections(rejDetail.RejectionId, "Restore");
        
        return new GeneralResponse(true, "Detalle del rechazo restaurado correctamente");
    }

    /// <summary>
    /// Obtiene un detalle del rechazo en específico para un rechazo.
    /// </summary>
    /// <param name="id">El Id del detalle del rechazo a buscar.</param>
    /// <param name="rejectionId">El id del rechazo padre</param>
    /// <param name="includeDeleted">Si es true, incluye registros eliminados lógicamente.</param>
    /// <returns>Retorna la entidad <see cref="RejectionEntity"/> correspondiente.</returns>
    /// <exception cref="NotFoundException">Se lanza si no se encuentra la capacitación.</exception>
    private async Task<RejectionDetailEntity> GetRejectionDetailByIdAsync(Guid id, Guid? rejectionId = null, bool includeDeleted = false)
    {
        IQueryable<RejectionDetailEntity> query = _dbCxt.RejectionDetails
            .Include(r => r.Rejection);

        // Si quiero incluir los eliminados, ignoro los filtros globales
        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        // Filtro por ID y usuario
        query = query.Where(t => t.Id == id);
        
        if (rejectionId.HasValue)
        {
            query = query.Where(t => t.RejectionId == rejectionId);
        }

        RejectionDetailEntity? rejDet = await query.FirstOrDefaultAsync();

        return rejDet ?? throw new NotFoundException("Detalle del rechazo no encontrado");
    }
    
    /// <summary>
    /// Verifica si existe un requerimiento con rechazo con su id para un usuario, 
    /// opcionalmente excluyendo un requerimiento por su Id.
    /// </summary>
    /// <param name="reqId">El ReqID del requerimiento a buscar.</param>
    /// <param name="excludeId">Id de un requerimiento a excluir de la búsqueda (opcional).</param>
    /// <returns>Retorna <c>true</c> si existe un requerimiento que cumpla las condiciones; de lo contrario, <c>false</c>.</returns>
    private async Task<bool> RejectionDetailsExists(Guid reqId, Guid? excludeId = null)
    {
        return await _dbCxt.RejectionDetails.AnyAsync(r =>
            r.RejectionId == reqId &&
            (excludeId == null || r.Id != excludeId));
    }

    private async Task<bool> RejectionExists(Guid rejectionId)
    {
        bool exists = await _dbCxt.Rejections
            .AnyAsync(r => r.Id == rejectionId);

        if (!exists)
            throw new NotFoundException("El RejectionId no existe");
        
        return exists;
    }
    
    /// <summary>
    /// Obtiene un usuario específico por su Id.
    /// </summary>
    /// <param name="userId">El Id del usuario a buscar.</param>
    /// <returns>Retorna el <see cref="UserEntity"/> correspondiente.</returns>
    /// <exception cref="NotFoundException">Se lanza si no se encuentra el usuario.</exception>
    private async Task<UserEntity> GetUserByIdAsync(Guid userId)
    {
        UserEntity? user = await _userManager.FindByIdAsync(userId.ToString());
        return user ?? throw new NotFoundException("El usuario no fue encontrado");
    }

    private async Task UpdateCountRejections(Guid rejectionId, string method = "Create")
    {
        DataResponse<RejectionResponseDto> rejectionPadre = await _rejectionContract.GetRejectionPorId(rejectionId);

        int rejections;
        int totalRejections = rejectionPadre.Data.TotalRejections;

        switch (method)
        {
            case "Create":
                rejections = totalRejections >= 0 ? totalRejections + 1 : totalRejections;
                break;
            case "Update":
                rejections = totalRejections;
                break;
            case "Delete":
                rejections = totalRejections >= 0 ? totalRejections - 1 : totalRejections;
                break;
            case "Restore":
                rejections = totalRejections >= 0 ? totalRejections + 1 : totalRejections;
                break;
            default:
                rejections = totalRejections;
                break;
        }
        
        UpdateRejectionCountDto updateRej = new UpdateRejectionCountDto
        { 
            TotalRejections = rejections,
        };

        GeneralResponse? responseUpd = await _rejectionContract.UpdateRejectionCount(rejectionId, updateRej);
        
        if (!responseUpd.Flag)
        {
            throw new BadRequestException("No se pudo actualizar el rechazo con la cantidad de rechazos.");
        }
    }
    
    
    private async Task<RejectionDetailEntity> GetRejectionDetailByRejectionIdAsync(Guid rejectionId, bool includeDeleted = false)
    {
        IQueryable<RejectionDetailEntity> query = _dbCxt.RejectionDetails
            .Include(r => r.Rejection);

        // Si quiero incluir los eliminados, ignoro los filtros globales
        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        // Filtro por ID y usuario
        query = query.Where(t => t.RejectionId == rejectionId);
        
        RejectionDetailEntity? rejDet = await query.FirstOrDefaultAsync();

        return rejDet!;
    }

    /// <summary>
    /// Calcula el siguiente RechazoNro disponible para un usuario específico.
    /// </summary>
    /// <param name="userId">El Id del usuario para el cual se busca el RechazoNro.</param>
    /// <param name="rejectionId">El Id del rechazo para poder contar la cantidad de rechazos</param>
    /// <returns>Retorna un entero que representa el siguiente RechazoNro disponible.</returns>
    private async Task<int> RechazoNroIdentity(Guid userId, Guid rejectionId)
    {
        int maxRechazoNro = await _dbCxt.RejectionDetails
            .Where(r => r.UserId == userId)
            .Where(r => r.RejectionId == rejectionId)
            .MaxAsync(r => (int?)r.RechazoNro) ?? 0;
            
        return maxRechazoNro + 1;
    }
}