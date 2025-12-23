using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Audits;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.SharedClases.Exceptions;
using AppTiemposV3.SharedClases.Utilidades.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Org.BouncyCastle.Ocsp;
using System.Net;
using System.Text;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;
using static AppTiemposV3.Api.Helpers.Helpers;
using static AppTiemposV3.Api.Helpers.MetadataHelper;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Repositories;

public class TrainingRepository : ITrainingContract<TrainingResponseDto>
{
    private readonly AppDbContext _dbCxt;
    private readonly IMapper _iMapper;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IGenericContract _genericContract;
    private readonly IUserContract _userContext;
    private readonly IEntityIdProvider _entityIdProvider;
    private readonly IAuditHelperService _auditHelperService;
    private Guid _userId => _userContext.GetUserId();

    public TrainingRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IGenericContract genericContract, IUserContract userContext, IEntityIdProvider entityIdProvider, IAuditHelperService auditHelperService)
    {
        _dbCxt = dbCxt;
        _iMapper = iMapper;
        _userManager = userManager;
        _genericContract = genericContract;
        _userContext = userContext;
        _entityIdProvider = entityIdProvider;
        _auditHelperService = auditHelperService;

    }

    public async Task<DataResponse<TrainingKpiResponse>> GetTrainingKpi()
    {
        TrainingKpiResponse resp = new TrainingKpiResponse();
        
        UserEntity user = await GetUserByIdAsync(_userId);
       
        StringBuilder? sb = new StringBuilder();

        sb.AppendLine("SELECT");
        sb.AppendLine("    (");
        sb.AppendLine("        SELECT COUNT(t.Id)");
        sb.AppendLine("        FROM trainings AS t");
        sb.AppendLine("        WHERE t.UserId = @UserId");
        sb.AppendLine("          AND t.IsDeleted = 0");
        sb.AppendLine("    ) AS TotalTrainings,");
        sb.AppendLine();
        sb.AppendLine("    (");
        sb.AppendLine("        SELECT COUNT(t.Id)");
        sb.AppendLine("        FROM trainings AS t");
        sb.AppendLine("        WHERE t.Status = 'completed'");
        sb.AppendLine("          AND t.UserId = @UserId");
        sb.AppendLine("          AND t.IsDeleted = 0");
        sb.AppendLine("    ) AS TrainingCompleted,");
        sb.AppendLine();
        sb.AppendLine("    (");
        sb.AppendLine("        SELECT COUNT(t.Id)");
        sb.AppendLine("        FROM trainings AS t");
        sb.AppendLine("        WHERE t.Status = 'in-progress'");
        sb.AppendLine("          AND t.UserId = @UserId");
        sb.AppendLine("          AND t.IsDeleted = 0");
        sb.AppendLine("    ) AS TrainingInProgress,");
        sb.AppendLine();
        sb.AppendLine("    (");
        sb.AppendLine("        SELECT COUNT(t.Id)");
        sb.AppendLine("        FROM trainings AS t");
        sb.AppendLine("        WHERE t.IsLoaded = 1");
        sb.AppendLine("          AND t.UserId = @UserId");
        sb.AppendLine("          AND t.IsDeleted = 0");
        sb.AppendLine("    ) AS TrainingIsLoaded");
 
        string sql = sb.ToString();
        
        MySqlParameter userFiltro = new MySqlParameter("@UserId", user.Id);
        
        List<Dictionary<string, object?>> kpiData = await QueryRawAsync(_dbCxt, sql, userFiltro);

        foreach (Dictionary<string, object?> row in kpiData)
        {
            resp.TotalTrainings = Convert.ToInt32(row["TotalTrainings"]!.ToString()!);
            resp.TrainingCompleted = Convert.ToInt32(row["TrainingCompleted"]!.ToString()!);
            resp.TrainingInProgress = Convert.ToInt32(row["TrainingInProgress"]!.ToString()!);
            resp.TrainingIsLoaded = Convert.ToInt32(row["TrainingIsLoaded"]!.ToString()!);
        }
        

        DataResponse<TrainingKpiResponse> respback = new DataResponse<TrainingKpiResponse>(true, resp, HttpStatusCode.OK);
        return respback;
    }

    public async Task<Pageable<List<TrainingResponseDto>>> GetAllTrainings(PaginationDtoAdvanced pagination)
    {
        Pageable<List<TrainingResponseDto>> response =  await _genericContract.GetAllPaginatedFaAsync<TrainingEntity, TrainingResponseDto>(pagination, _userId);
        
        List<Guid> trainingIds = response.Content.Select(a => a.Id).ToList();

        // Obtener todos los trainings relevantes en una sola consulta
        List<TrainingEntity> trainings = await _dbCxt.Trainings
            .Where(a => trainingIds.Contains(a.Id) && !a.IsDeleted)
            .ToListAsync();

        foreach (TrainingResponseDto item in response.Content)
        {
            double totalWorked = trainings
                .Where(a => a.Id == item.Id)
                .Sum(a =>
                {
                    TimeSpan start = a.StartTime.ToTimeSpan();
                    TimeSpan end = a.EndTime?.ToTimeSpan() ?? start;
                    return (end - start).TotalSeconds;
                });

            item.CapacitationTime = TimeSpan.FromSeconds(totalWorked);
        }

        return response;
    }

    public async Task<DataResponse<TrainingResponseDto>> GetTrainingPorId(Guid id)
    {
        UserEntity user = await GetUserByIdAsync(_userId);

        TrainingEntity training = await GetTrainingByIdAsync(id, user.Id);
        
        double totalWorked = _dbCxt.Trainings
            .Where(a => !a.IsDeleted)
            .Where(a => a.Id == training.Id)
            .AsEnumerable() 
            .Sum(a =>
                {
                    TimeSpan start = a.StartTime.ToTimeSpan();
                    TimeSpan end = a.EndTime?.ToTimeSpan() ?? start;
                    return (end - start).TotalSeconds;
                }
            );

        TimeSpan totalTime = TimeSpan.FromSeconds(totalWorked);
        training.CapacitationTime = totalTime;


        TrainingResponseDto resTra = _iMapper.Map<TrainingResponseDto>(training);
        
        return new DataResponse<TrainingResponseDto> (true, resTra, HttpStatusCode.OK);
    }

    public async Task<GeneralResponse> CreateTraining(CreateTrainingDto dto)
    {
        if (await TrainingExists(dto.RequerimentId, _userId, null))
            throw new BadRequestException("Hay una capacitación pendiente de poner hora de fin para ese requerimiento");
        
        TrainingEntity training = _iMapper.Map<TrainingEntity>(dto);
        
        training.UserId = _userId;

        UserEntity? user = await GetUserByIdAsync(_userId);
        RequerimentsEntity? req = await GetRequerimentByIdAsync(dto.RequerimentId, _userId);

        await _dbCxt.Trainings.AddAsync(training);

        await EnsureSavedAsync("Hubo un error al crear la capacitación");

        try
        {
            string reqId = !string.IsNullOrWhiteSpace(req.ReqID) ? $"Se creo una nueva capacitación para el ReqID{req.ReqID}" : $"Se creo una nueva capacitación";

            await _auditHelperService.CreateAuditAsync(
                user!.FullName,
                reqId,
                AuditAction.Created.ToString(),
                nameof(TrainingEntity),
                "trainings",
                BuildChanges(training),
                BuildCreateMetadata(user.Id, "CREATE")
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return new GeneralResponse(true, "Capacitación creada correctamente");
    }

    public async Task<GeneralResponse> UpdateTraining(Guid id, UpdateTrainingDto dto)
    {
        UserEntity user = await GetUserByIdAsync(_userId);
        
        TrainingEntity training = await GetTrainingByIdAsync(id, user.Id);
        TrainingEntity oldtTraining = await GetTrainingByIdAsync(id, user.Id);
        
        if (await TrainingExists(dto.RequerimentId!, _userId, training.Id))
            throw new BadRequestException("Hay una capacitación pendiente de poner hora de fin para ese requerimiento");
        
        _iMapper.Map(dto, training);
            
        training.Id = id;
        training.UserId = user.Id;
        training.ModifiedAt = DateTime.Now;
            
        _dbCxt.Entry(training).State = EntityState.Modified;
        
        await EnsureSavedAsync("Hubo un problema para eliminar el registro");
        
        try
        {
            await _auditHelperService.CreateAuditAsync(
                user!.FullName,
                $"Se edito la capacitación {oldtTraining.Description}",
                AuditAction.Updated.ToString(),
                nameof(TrainingEntity),
                "trainings",
                BuildChanges(training),
                BuildCreateMetadata(user.Id, "UPDATE")
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new GeneralResponse(true, "Capacitación actualizada con éxito");
    }

    public async Task<GeneralResponse> DeleteTraining(Guid id)
    {
        UserEntity user = await GetUserByIdAsync(_userId);
        
        TrainingEntity training = await GetTrainingByIdAsync(id, user.Id);
        
        training.IsDeleted = true;
        training.DeletedAt = DateTime.Now;
        
        _dbCxt.Entry(training).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo un problema para eliminar el registro");
        

        try
        {
            await _auditHelperService.CreateAuditAsync(
                user!.FullName,
                $"Se elimino la capacitación {training.Description}",
                AuditAction.Deleted.ToString(),
                nameof(TrainingEntity),
                "trainings",
                BuildChanges(training),
                BuildCreateMetadata(user.Id, "DELETE")
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new GeneralResponse(true, "Capacitación eliminada con éxito");
    }

    public async Task<GeneralResponse> RestoreTraining(Guid id)
    {
        UserEntity user = await GetUserByIdAsync(_userId);
        
        TrainingEntity training = await GetTrainingByIdAsync(id, user.Id, true);
        
        training.IsDeleted = false;
        training.ModifiedAt = DateTime.Now;
        training.DeletedAt = null;
        
        _dbCxt.Entry(training).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo un problema para restaurar el registro");
        
        try
        {
            await _auditHelperService.CreateAuditAsync(
                user!.FullName,
                $"Se restauro la capacitación {training.Description}",
                AuditAction.Restored.ToString(),
                nameof(TrainingEntity),
                "trainings",
                BuildChanges(training),
                BuildCreateMetadata(user.Id, "RESTORE")
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new GeneralResponse(true, "Capacitación restaurada con éxito");
    }

    /// <summary>
    /// Verifica si existe una capacitacion pendiente con el ReqID específico para un usuario, 
    /// opcionalmente excluyendo un capacitacion por su Id.
    /// </summary>
    /// <param name="reqId">RquerimentId para buscar si ya hay una capacitacion</param>
    /// <param name="userId">El Id del usuario propietario del capacitacion.</param>
    /// <param name="excludeId">Id de un capacitacion a excluir de la búsqueda (opcional).</param>
    /// <returns>Retorna <c>true</c> si existe una capacitacion que cumpla las condiciones; de lo contrario, <c>false</c>.</returns>
    private async Task<bool> TrainingExists(Guid? reqId, Guid userId, Guid? excludeId = null)
    {
        return await _dbCxt.Trainings.AnyAsync(r =>
            r.RequerimentId == reqId &&
            r.UserId == userId &&
            (excludeId == null || r.Id != excludeId) &&
            (!r.IsLoaded || r.EndTime == null)
            );
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
    /// Obtiene una capacitación específica de un usuario, incluyendo su requerimiento y usuario.
    /// </summary>
    /// <param name="id">El Id de la capacitación a buscar.</param>
    /// <param name="userId">El Id del usuario propietario.</param>
    /// <param name="includeDeleted">Si es true, incluye registros eliminados lógicamente.</param>
    /// <returns>Retorna la entidad <see cref="TrainingEntity"/> correspondiente.</returns>
    /// <exception cref="NotFoundException">Se lanza si no se encuentra la capacitación.</exception>
    private async Task<TrainingEntity> GetTrainingByIdAsync(Guid id, Guid userId, bool includeDeleted = false)
    {
        IQueryable<TrainingEntity> query = _dbCxt.Trainings
            .Include(r => r.Requeriment)
            .Include(u => u.User);

        // Si quiero incluir los eliminados, ignoro los filtros globales
        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        // Filtro por ID y usuario
        query = query.Where(t => t.Id == id && t.UserId == userId);

        TrainingEntity? training = await query.FirstOrDefaultAsync();

        return training ?? throw new NotFoundException("Capacitación no encontrada");
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

    private async Task<RequerimentsEntity> GetRequerimentByIdAsync(Guid id, Guid userId)
    {
        RequerimentsEntity? requeriment = await _dbCxt.Requeriments
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        return requeriment ?? throw new NotFoundException("Requerimiento no encontrado");
    }

    private static List<AuditChangeDto> BuildChanges(
            TrainingEntity newTraining,
            TrainingEntity? oldTraining = null,
            UpdateTrainingDto? dto = null)
        {
            List<AuditChangeDto> changes = new();

            if (oldTraining is null)
            {
                AddCreate(nameof(newTraining.RequerimentId), newTraining.RequerimentId);
                AddCreate(nameof(newTraining.IsLoaded), newTraining.IsLoaded);
                AddCreate(nameof(newTraining.Notes), newTraining.Notes);
                AddCreate(nameof(newTraining.StartDate), newTraining.StartDate);
                AddCreate(nameof(newTraining.StartTime), newTraining.StartTime);
                AddCreate(nameof(newTraining.Status), newTraining.Status);
                AddCreate(nameof(newTraining.UserId), newTraining.UserId);
                AddCreate(nameof(newTraining.EndTime), newTraining.EndTime);
                AddCreate(nameof(newTraining.CapacitationTime), newTraining.CapacitationTime);
                AddCreate(nameof(newTraining.Capacitator), newTraining.Capacitator);
                AddCreate(nameof(newTraining.Description), newTraining.Description);
                return changes;
            }

            if (dto is not null)
            {
                if (dto.RequerimentId != Guid.Empty &&
                    dto.RequerimentId != oldTraining.RequerimentId)
                    AddUpdate(nameof(newTraining.RequerimentId), oldTraining.RequerimentId, newTraining.RequerimentId);

                if (dto.IsLoaded &&
                    dto.IsLoaded != oldTraining.IsLoaded)
                    AddUpdate(nameof(newTraining.IsLoaded), oldTraining.IsLoaded, newTraining.IsLoaded);

                if (!string.IsNullOrWhiteSpace(dto.Notes) &&
                    dto.Notes != oldTraining.Notes)
                    AddUpdate(nameof(newTraining.Notes), oldTraining.Notes, newTraining.Notes);

                if (dto.StartDate != oldTraining.StartDate)
                    AddUpdate(nameof(newTraining.StartDate), oldTraining.StartDate, newTraining.StartDate);

                if (dto.StartTime != oldTraining.StartTime)
                    AddUpdate(nameof(newTraining.StartTime), oldTraining.StartTime, newTraining.StartTime);

                if (dto.EndTime != oldTraining.EndTime)
                    AddUpdate(nameof(newTraining.EndTime), oldTraining.EndTime, newTraining.EndTime);

                if (!string.IsNullOrWhiteSpace(dto.Status) && dto.Status != oldTraining.Status)
                    AddUpdate(nameof(newTraining.Status), oldTraining.Status, newTraining.Status);

                if (!string.IsNullOrWhiteSpace(dto.Capacitator) &&
                    dto.Capacitator != oldTraining.Capacitator)
                    AddUpdate(nameof(newTraining.Capacitator), oldTraining.Capacitator, newTraining.Capacitator);

                if (!string.IsNullOrWhiteSpace(dto.Description) &&
                    dto.Description != oldTraining.Description)
                    AddUpdate(nameof(newTraining.Description), oldTraining.Description, newTraining.Description);
            }

            return changes;

            void AddCreate(string field, object? value)
            {
                changes.Add(new AuditChangeDto
                {
                    Field = field,
                    NewValue = NormalizeValue(value?.ToString())
                });
            }
            
            void AddUpdate(string field, object? oldValue, object? newValue)
            {
                string? oldVal = oldValue?.ToString();
                string? newVal = newValue?.ToString();
                
                if (oldVal != newVal)
                {
                    changes.Add(new AuditChangeDto
                    {
                        Field = field,
                        OldValue = NormalizeValue(oldValue),
                        NewValue = NormalizeValue(newValue)
                    });
                }
            }
        }
}