using System.Net;
using System.Text;
using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using AppTiemposV3.SharedClases.Exceptions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;

namespace AppTiemposV3.Api.Repositories;

public class TrainingRepository : ITrainingContract<TrainingResponseDto>
{
    private readonly AppDbContext _dbCxt;
    private readonly IMapper _iMapper;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IGenericContract _genericContract;
    private readonly IUserContract _userContext;
    private Guid _userId => _userContext.GetUserId();

    public TrainingRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IGenericContract genericContract, IUserContract userContext)
    {
        _dbCxt = dbCxt;
        _iMapper = iMapper;
        _userManager = userManager;
        _genericContract = genericContract;
        _userContext = userContext;
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
        Pageable<List<TrainingResponseDto>> response =  await _genericContract.GetAllPaginatedFAAsync<TrainingEntity, TrainingResponseDto>(pagination, _userId);
        
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
        
        await _dbCxt.Trainings.AddAsync(training);

        await EnsureSavedAsync("Hubo un error al crear la capacitación");

        return new GeneralResponse(true, "Capacitación creada correctamente");
    }

    public async Task<GeneralResponse> UpdateTraining(Guid id, UpdateTrainingDto dto)
    {
        UserEntity user = await GetUserByIdAsync(_userId);
        
        TrainingEntity training = await GetTrainingByIdAsync(id, user.Id);
        
        if (await TrainingExists(dto.RequerimentId!, _userId, training.Id))
            throw new BadRequestException("Hay una capacitación pendiente de poner hora de fin para ese requerimiento");
        
        _iMapper.Map(dto, training);
            
        training.Id = id;
        training.UserId = user.Id;
        training.ModifiedAt = DateTime.Now;
            
        _dbCxt.Entry(training).State = EntityState.Modified;
        
        await EnsureSavedAsync("Hubo un problema para eliminar el registro");

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

        await EnsureSavedAsync("Hubo un problema para eliminar el registro");

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
}