using System.Net;
using System.Text;
using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Rejections;
using AppTiemposV3.SharedClases.Exceptions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;

namespace AppTiemposV3.Api.Repositories;

public class RejectionRepository: IRejectionContract<RejectionResponseDto>
{
    private readonly AppDbContext _dbCxt;
    private readonly IMapper _iMapper;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IGenericContract _genericContract;
    private readonly IUserContract _userContext;
    private Guid _userId => _userContext.GetUserId();

    public RejectionRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IGenericContract genericContract, IUserContract userContext)
    {
        _dbCxt = dbCxt;
        _iMapper = iMapper;
        _userManager = userManager;
        _genericContract = genericContract;
        _userContext = userContext;
    }
    
    public async Task<DataResponse<RejectionKpiResponse>> GetRejectionKpi()
    {
        RejectionKpiResponse resp = new RejectionKpiResponse();
        
        UserEntity user = await GetUserByIdAsync(_userId);
       
        StringBuilder? sb = new StringBuilder();

        sb.AppendLine("SELECT");
        sb.AppendLine("    (");
        sb.AppendLine("        SELECT COUNT(r.Id)");
        sb.AppendLine("        FROM rechazos AS r");
        sb.AppendLine("        WHERE r.UserId = @UserId");
        sb.AppendLine("          AND r.IsDeleted = 0");
        sb.AppendLine("    ) AS TotalRejections,");
        sb.AppendLine();
        sb.AppendLine("    (");
        sb.AppendLine("        SELECT COUNT(r.Id)");
        sb.AppendLine("        FROM rechazos AS r");
        sb.AppendLine("        WHERE r.Status = 'pending'");
        sb.AppendLine("          AND r.UserId = @UserId");
        sb.AppendLine("          AND r.IsDeleted = 0");
        sb.AppendLine("    ) AS PendingRejections,");
        sb.AppendLine();
        sb.AppendLine("    (");
        sb.AppendLine("        SELECT COUNT(r.Id)");
        sb.AppendLine("        FROM rechazos AS r");
        sb.AppendLine("        WHERE r.Status = 'in-progress'");
        sb.AppendLine("          AND r.UserId = @UserId");
        sb.AppendLine("          AND r.IsDeleted = 0");
        sb.AppendLine("    ) AS InProgressRejections,");
        sb.AppendLine();
        sb.AppendLine("    (");
        sb.AppendLine("        SELECT COUNT(r.Id)");
        sb.AppendLine("        FROM rechazos AS r");
        sb.AppendLine("        WHERE r.IsResolve = 1");
        sb.AppendLine("          AND r.UserId = @UserId");
        sb.AppendLine("          AND r.IsDeleted = 0");
        sb.AppendLine("    ) AS SuccessRejections");
 
        string sql = sb.ToString();
        
        MySqlParameter userFiltro = new MySqlParameter("@UserId", user.Id);
        
        List<Dictionary<string, object?>> kpiData = await QueryRawAsync(_dbCxt, sql, userFiltro);

        foreach (Dictionary<string, object?> row in kpiData)
        {
            resp.TotalRejections = Convert.ToInt32(row["TotalRejections"]!.ToString()!);
            resp.PendingRejections = Convert.ToInt32(row["PendingRejections"]!.ToString()!);
            resp.InProgressRejections = Convert.ToInt32(row["InProgressRejections"]!.ToString()!);
            resp.SuccessRejections = Convert.ToInt32(row["SuccessRejections"]!.ToString()!);
        }
        

        DataResponse<RejectionKpiResponse> respback = new DataResponse<RejectionKpiResponse>(true, resp, HttpStatusCode.OK);
        return respback;
    }

    public async Task<Pageable<List<RejectionResponseDto>>> GetAllRejections(PaginationDtoAdvanced pagination)
    {
        Pageable<List<RejectionResponseDto>> response =  await _genericContract.GetAllPaginatedFAAsync<RejectionEntity, RejectionResponseDto>(pagination, _userId);
        
        response.Content = response.Content
            .OrderByDescending(r =>
                r.RejectionsDetails.Any()
                    ? r.RejectionsDetails.Max(d => d.RejectionDate)
                    : DateOnly.MinValue
            )
            .ToList();
        
        return response;
    }

    public async Task<DataResponse<RejectionResponseDto>> GetRejectionPorId(Guid id)
    {
        UserEntity user = await GetUserByIdAsync(_userId);

        RejectionEntity rejection = await GetRejectionByIdAsync(id, user.Id);
        
        RejectionResponseDto res = _iMapper.Map<RejectionResponseDto>(rejection);
        
        return new DataResponse<RejectionResponseDto>(true, res, HttpStatusCode.OK);
    }

    public async Task<DataResponse<RejectionResponseDto>> GetRejectionPorUrl(string url)
    {
        UserEntity user = await GetUserByIdAsync(_userId);
        
        RejectionEntity? rejection = await _dbCxt.Rejections
            .Include(r => r.Requeriment)
            .Include(r => r.RejectionsDetails)
            .Where(r => r.UrlIndetificator == url && r.UserId == user.Id)
            .OrderByDescending(r =>
                r.RejectionsDetails.Any()
                    ? r.RejectionsDetails.Max(d => d.RejectionDate)
                    : DateOnly.MinValue
            )
            .FirstOrDefaultAsync();

        if (rejection is null)
        {
            throw new NotFoundException("Rejection Url Not Found");
        }
        
        RejectionResponseDto responseDto = _iMapper.Map<RejectionResponseDto>(rejection);
        
        DataResponse<RejectionResponseDto> res = new DataResponse<RejectionResponseDto>(true, responseDto, HttpStatusCode.OK);
        
        return res;
    }

    public async Task<DataResponse<CreateRejectionResponseDto>> CreateRejection(CreateRejectionDto dto)
    {
        UserEntity user = await GetUserByIdAsync(_userId);
        
        if (await RejectionExists(dto.RequerimentId, _userId, null))
            throw new BadRequestException("Ese requerimiento ya tiene un rechazo para tu usuario");
        
        RejectionEntity rej = _iMapper.Map<RejectionEntity>(dto);
        rej.UserId = user.Id;
        rej.UrlIndetificator  = await GenerateAsync(LowercaseLettersAndDigits, 10);
            
        await _dbCxt.Rejections.AddAsync(rej);

        await EnsureSavedAsync("Hubo un error al crear el rechazo");

        CreateRejectionResponseDto data = new CreateRejectionResponseDto
        {
            Id = rej.Id,
            ReqId = await GetReqIdByRequerimentId(dto.RequerimentId, _userId),
            Message = "Rechazo creado correctamente"
        };

        DataResponse<CreateRejectionResponseDto> response = new DataResponse<CreateRejectionResponseDto>(true, data, HttpStatusCode.Created);
        
        return response;
    }

    public async Task<GeneralResponse> UpdateRejection(Guid id, UpdateRejectionDto dto)
    {
        UserEntity user = await GetUserByIdAsync(_userId);

        RejectionEntity rej = await GetRejectionByIdAsync(id, user.Id);
        
        if (await RejectionExists(dto.RequerimentId!, user.Id, id))
            throw new BadRequestException("El rechazo con ese Id de requerimiento ya existe para tu usuario"); //TODO: ver esto

        _iMapper.Map(dto, rej);
        
        rej.Id = id;
        rej.UserId = user.Id;
        rej.ModifiedAt = DateTime.Now;
        
        _dbCxt.Entry(rej).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo problemas para actualizar el registro");

        return new GeneralResponse(true, "Rechazo actualizado con éxito");
    }

    public async Task<GeneralResponse> UpdateRejectionCount(Guid id, UpdateRejectionCountDto dto)
    {
        UserEntity user = await GetUserByIdAsync(_userId);
        
        RejectionEntity rej = await GetRejectionByIdAsync(id, user.Id);

        if (rej == null)
            throw new NotFoundException("Rechazo no encontrado");

        rej.TotalRejections = dto.TotalRejections;

        _dbCxt.Entry(rej).State = EntityState.Modified;
        
        await EnsureSavedAsync("Error al actualizar el rechazo");

        return new GeneralResponse(true, "Actualizado");
    }

    public async Task<GeneralResponse> DeleteRejection(Guid id)
    {
        UserEntity user = await GetUserByIdAsync(_userId);

        RejectionEntity rej = await GetRejectionByIdAsync(id, user.Id);

        rej.IsDeleted = true;
        rej.DeletedAt = DateTime.Now;
            
        _dbCxt.Entry(rej).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo problemas para eliminar el registro");

        return new GeneralResponse(true, "Rechazo eliminado con éxito");
    }

    public async Task<GeneralResponse> RestoreRejection(Guid id)
    {
        UserEntity user = await GetUserByIdAsync(_userId);

        RejectionEntity rej = await GetRejectionByIdAsync(id, user.Id, true);

        rej.IsDeleted = false;
        rej.ModifiedAt = DateTime.Now;
        rej.DeletedAt = null;
            
        _dbCxt.Entry(rej).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo problemas para restaurar el registro");

        return new GeneralResponse(true, "Rechazo restaurado con éxito");
    }
    
    /// <summary>
    /// Obtiene un rechazo en específico para un requerimiento de un usuario.
    /// </summary>
    /// <param name="id">El Id del rechazo a buscar.</param>
    /// <param name="userId">El Id del usuario propietario.</param>
    /// <param name="includeDeleted">Si es true, incluye registros eliminados lógicamente.</param>
    /// <returns>Retorna la entidad <see cref="RejectionEntity"/> correspondiente.</returns>
    /// <exception cref="NotFoundException">Se lanza si no se encuentra la capacitación.</exception>
    private async Task<RejectionEntity> GetRejectionByIdAsync(Guid id, Guid userId, bool includeDeleted = false)
    {
        IQueryable<RejectionEntity> query = _dbCxt.Rejections
            .Include(r => r.Requeriment)
            .Include(u => u.User)
            .Include(rd => rd.RejectionsDetails);

        // Si quiero incluir los eliminados, ignoro los filtros globales
        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        // Filtro por ID y usuario
        query = query.Where(t => t.Id == id && t.UserId == userId);

        RejectionEntity? training = await query.FirstOrDefaultAsync();

        return training ?? throw new NotFoundException("Rechazo no encontrado");
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
    
    /// <summary>
    /// Verifica si existe un requerimiento con rechazo con su id para un usuario, 
    /// opcionalmente excluyendo un requerimiento por su Id.
    /// </summary>
    /// <param name="reqId">El ReqID del requerimiento a buscar.</param>
    /// <param name="userId">El Id del usuario propietario del requerimiento.</param>
    /// <param name="excludeId">Id de un requerimiento a excluir de la búsqueda (opcional).</param>
    /// <returns>Retorna <c>true</c> si existe un requerimiento que cumpla las condiciones; de lo contrario, <c>false</c>.</returns>
    private async Task<bool> RejectionExists(Guid reqId, Guid userId, Guid? excludeId = null)
    {
        return await _dbCxt.Rejections.AnyAsync(r =>
            r.RequerimentId == reqId &&
            r.UserId == userId &&
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

    private async Task<string> GetReqIdByRequerimentId(Guid idRequeriment, Guid userId)
    {
        RequerimentsEntity? requeriment = await _dbCxt.Requeriments
            .FirstOrDefaultAsync(r => r.Id == idRequeriment && r.UserId == userId);

        return requeriment!.ReqID ??  throw new BadRequestException("Requerimiento no encontrado");
    }
}