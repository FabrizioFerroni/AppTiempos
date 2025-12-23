using System.Net;
using static AppTiemposV3.Api.Helpers.Helpers;
using static AppTiemposV3.Api.Helpers.MetadataHelper;
using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Audits;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.SharedClases.Exceptions;
using AppTiemposV3.SharedClases.Utilidades.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NanoidDotNet;

namespace AppTiemposV3.Api.Repositories;

public class ActivityRepository : IActivityContract<ActivityResponseDto>
{
    private readonly AppDbContext _dbCxt;
    private readonly IMapper _iMapper;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IUserContract _userContext;
    private readonly IGenericContract _genericContract;
    private readonly IAuditHelperService _auditHelperService;
    private readonly IEntityIdProvider _entityIdProvider;
    private Guid _userId => _userContext.GetUserId();

    public ActivityRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IUserContract userContext, IGenericContract genericContract, IAuditHelperService auditHelperService, IEntityIdProvider entityIdProvider)
    {
        _dbCxt = dbCxt;
        _iMapper = iMapper;
        _userManager = userManager;
        _userContext = userContext;
        _genericContract = genericContract;
        _auditHelperService = auditHelperService;
        _entityIdProvider = entityIdProvider;
    }

    public async Task<DataAResponse<ActivityResponseDto>> GetAllActivities()
    {
        UserEntity user = await GetUserByIdAsync(_userId);

        List<ActivityResponseDto> activities = await _dbCxt.Activities
            .Where(u => u.UserId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ProjectTo<ActivityResponseDto>(_iMapper.ConfigurationProvider)
            .ToListAsync();


        return new DataAResponse<ActivityResponseDto>(true, activities, HttpStatusCode.OK);
    }

    public async Task<Pageable<List<ActivityResponseDto>>> GetAllActivitiesPag(PaginationDto pagination, string buscarPor)
    {
        return await _genericContract.GetAllPaginatedAsync<ActivitiesEntity, ActivityResponseDto>(pagination,
            buscarPor, _userId);
    }

    public async Task<Pageable<List<ActivityResponseDto>>> GetAllActivitiesPerDayPag(PaginationDto pagination, DateOnly startDate)
    {
        return await _genericContract.GetAllPaginatedPerDayAsync<ActivitiesEntity, ActivityResponseDto>(pagination, startDate, _userId);
    }

    public async Task<DataAResponse<ActivityResponseDto>> GetAllActivitiesPerDay(DateOnly startDate)
    {
        UserEntity user = await GetUserByIdAsync(_userId);

        List<ActivitiesEntity> activitiesEntities = await _dbCxt.Activities
                .Include(a => a.User)
                .Include(a => a.Requeriment)
                .Where(a => a.UserId == user.Id && a.StartDate >= startDate)
                .OrderByDescending(a => a.StartDate)
                .ThenByDescending(a => a.StartTime)
                .ToListAsync();

        List<ActivityResponseDto> activitiesDto = _iMapper.Map<List<ActivityResponseDto>>(activitiesEntities);

        return new DataAResponse<ActivityResponseDto>(true, activitiesDto, HttpStatusCode.OK);
    }

    public async Task<Pageable<List<ActivityResponseDto>>> GetAllActivitiesPerRangePag(PaginationDto pagination, DateOnly startDate, DateOnly endDate)
    {
        return await _genericContract.GetAllPaginatedPerRangeAsync<ActivitiesEntity, ActivityResponseDto>(pagination, startDate, endDate, _userId);
    }

    public async Task<DataAResponse<ActivityResponseDto>> GetLastThreeActivities(int year, int weekNumber)
    {
        UserEntity user = await GetUserByIdAsync(_userId);
        
        (DateOnly start, DateOnly end) = GetDateRangeFromWeek(year, weekNumber);

        List<ActivitiesEntity> activitiesEntities = await _dbCxt.Activities
            .Include(a => a.User)
            .Include(a => a.Requeriment)
            .Where(u => u.StartDate >= start && u.StartDate <= end)
            .Where(a => a.UserId == user.Id)
            .OrderByDescending(a => a.StartDate)
            .ThenByDescending(a => a.StartTime)
            .ToListAsync();

        List<ActivityResponseDto> activitiesDto = _iMapper.Map<List<ActivityResponseDto>>(activitiesEntities);
        // aca

        return new DataAResponse<ActivityResponseDto>(true, activitiesDto, HttpStatusCode.OK);
    }
    
    public async Task<DataResponse<ActivityResponseDto>> GetActivityById(Guid id)
    {
        UserEntity? user = await GetUserByIdAsync(_userId);

        ActivitiesEntity? act = await _dbCxt.Activities
            .Include(r => r.Requeriment)
            .Include(u => u.User)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
        
        if(act == null)
            throw new NotFoundException("No activities found");
        
        ActivityResponseDto actReq = _iMapper.Map<ActivityResponseDto>(act);

        return new DataResponse<ActivityResponseDto> (true, actReq, HttpStatusCode.OK);
    }

    public async Task<DataResponse<ActivityResponseDto>> GetActivityByUrl(string url)
    {
        UserEntity? user = await GetUserByIdAsync(_userId);

        ActivitiesEntity? act = await _dbCxt.Activities
            .Include(r => r.Requeriment)
                .ThenInclude(r => r.Category)
            .Include(u => u.User)
            .FirstOrDefaultAsync(a => a.UrlIndetificator == url && a.UserId == user.Id);
        
        if(act == null)
            throw new NotFoundException("No activities found");
       
        double totalWorked = _dbCxt.Activities
            .Where(a => !a.IsDeleted)
            .Where(a => a.RequerimentId == act.RequerimentId)
            .AsEnumerable() 
            .Sum(a =>
                {
                    TimeSpan start = a.StartTime.ToTimeSpan();
                    TimeSpan end = a.EndTime?.ToTimeSpan() ?? start;
                    return (end - start).TotalSeconds;
                }
            );

        TimeSpan totalTime = TimeSpan.FromSeconds(totalWorked);
        act.WorkedTime = totalTime;
        
        ActivityResponseDto actReq = _iMapper.Map<ActivityResponseDto>(act);

        return new DataResponse<ActivityResponseDto> (true, actReq, HttpStatusCode.OK);
    }

    public async Task<DataResponse<Guid>> GetRequerimentActivity(string reqId)
    {
        if (reqId.StartsWith("ReqID"))
        {
            reqId =  reqId.Replace("ReqID", "");
        }
        
        RequerimentsEntity? req = await _dbCxt.Requeriments
            .FirstOrDefaultAsync(r => r.ReqID == reqId && r.UserId == _userId);
        
        if (req == null)
            throw new NotFoundException("No requeriment found");


        RequerimentResponseDto resCat = _iMapper.Map<RequerimentResponseDto>(req);

        return new DataResponse<Guid> (true, resCat.Id, HttpStatusCode.OK);
    }

    public async Task<GeneralResponse> CreateActivity(CreateActivityDto dto)
    {
        ActivitiesEntity act = _iMapper.Map<ActivitiesEntity>(dto);
        
        UserEntity? user = await GetUserByIdAsync(_userId);

        RequerimentsEntity? req = await GetRequerimentByIdAsync(dto.RequerimentId, _userId); //_dbCxt.Requeriments.FirstOrDefaultAsync(r => r.Id.Equals(dto.RequerimentId));


        act.StartTime = dto.StartTime;
        act.UrlIndetificator = await Nanoid.GenerateAsync(Nanoid.Alphabets.LowercaseLettersAndDigits, 10);
        act.UserId = user.Id;
        act.StatusMessage = "in-progress";
            
        await _dbCxt.Activities.AddAsync(act);

        await EnsureSavedAsync("Hubo un error al crear la actividad");
        
        try
        {
            string reqId = !string.IsNullOrWhiteSpace(req.ReqID) ? $"Se creo una nueva actividad para el ReqID{req.ReqID}" : $"Se creo una nueva actividad"; 

            await _auditHelperService.CreateAuditAsync(
                user!.FullName,
                reqId, 
                AuditAction.Created.ToString(),
                nameof(ActivitiesEntity),
                "activities",
                BuildChanges(act),
                BuildCreateMetadata(user.Id, "CREATE")
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new GeneralResponse(true, "Actividad creada correctamente");
    }

    public async Task<GeneralResponse> UpdateActivity(Guid id, UpdateActivityDto dto)
    {
        UserEntity user = await GetUserByIdAsync(_userId);

        ActivitiesEntity? act = await _dbCxt.Activities
            .Include(r => r.Requeriment)
            .Include(u => u.User)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
        
        ActivitiesEntity? oldAct = await _dbCxt.Activities
            .Include(r => r.Requeriment)
            .Include(u => u.User)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
            
        _iMapper.Map(dto, act);

        if (dto.EndTime < dto.StartTime)
        {
            throw new BadRequestException("La hora de fin no puede ser menor a la hora de inicio.");
        }
        
        if (dto.EndTime < act!.StartTime)
        {
            throw new BadRequestException("La hora de fin no puede ser menor a la hora de inicio.");
        }

        act.Id = id;
        act.UrlIndetificator = act.UrlIndetificator;
        act.UserId = user.Id;
        act.ModifiedAt = DateTime.Now;

        _dbCxt.Entry(act).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo un error al actualizar la actividad. Intente mas tarde");
        
        try
        {
            await _auditHelperService.CreateAuditAsync(
                user!.FullName,
                $"Se edito la actividad del dia {oldAct!.StartDate.ToShortDateString()} del ReqID{oldAct!.Requeriment.ReqID}",
                AuditAction.Updated.ToString(),
                nameof(ActivitiesEntity),
                "activities",
                BuildChanges(act, oldAct, dto),
                BuildCreateMetadata(user.Id, "UPDATE")
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new GeneralResponse(true, "Se actualizo con exito la actividad.");
    }

    public async Task<GeneralResponse> DeleteActivity(Guid id)
    {
        UserEntity? user = await GetUserByIdAsync(_userId);

        ActivitiesEntity? act = await _dbCxt.Activities
            .Include(r => r.Requeriment)
            .Include(u => u.User)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
        
        if(act == null)
            throw new NotFoundException("No activities found");
            
        act.IsDeleted = true;
        act.DeletedAt = DateTime.Now;
        
        _dbCxt.Entry(act).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo un error al eliminar la actividad. Intente mas tarde");
        
        try
        {
            await _auditHelperService.CreateAuditAsync(
                user!.FullName,
                $"Se elimino la actividad del dia {act!.StartDate.ToShortDateString()} del ReqID{act!.Requeriment.ReqID}",
                AuditAction.Deleted.ToString(),
                nameof(ActivitiesEntity),
                "activities",
                metadata: BuildCreateMetadata(user.Id, "DELETE")
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new GeneralResponse(true, "Se eliminó con éxito la actividadcon");
    }

    public async Task<GeneralResponse> RestoreActivity(Guid id)
    {
        UserEntity? user = await GetUserByIdAsync(_userId);

        ActivitiesEntity? act = await _dbCxt.Activities
            .Include(r => r.Requeriment)
            .Include(u => u.User)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
        
        if(act == null)
            throw new NotFoundException("No activities found");
            
        act.IsDeleted = false;
        act.DeletedAt = null;
        act.ModifiedAt = DateTime.Now;

        await EnsureSavedAsync("Hubo un error al restaurar la actividad. Intente mas tarde");
        
        try
        {
            await _auditHelperService.CreateAuditAsync(
                user!.FullName,
                $"Se restauro la actividad del dia {act!.StartDate.ToShortDateString()} del ReqID{act!.Requeriment.ReqID}",
                AuditAction.Restored.ToString(),
                nameof(ActivitiesEntity),
                "activities",
                metadata: BuildCreateMetadata(user.Id, "RESTORE")
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new GeneralResponse(true, "Se restauró con éxito la actividadcon");
    }
    

    private async Task<UserEntity> GetUserByIdAsync(Guid userId)
    {
        UserEntity? user = await _userManager.FindByIdAsync(userId.ToString());
        return user ?? throw new NotFoundException("El usuario no fue encontrado");
    }

    private async Task<RequerimentsEntity> GetRequerimentByIdAsync(Guid id, Guid userId)
    {
        RequerimentsEntity? requeriment = await _dbCxt.Requeriments
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        return requeriment ?? throw new NotFoundException("Requerimiento no encontrado");
    }

    private async Task EnsureSavedAsync(string errorMessage)
    {
        int result = await _dbCxt.SaveChangesAsync();
        if (result <= 0)
            throw new InternalServerErrorException(errorMessage);
    }

    private (DateOnly start, DateOnly end) GetDateRangeFromWeek(int year, int weekNumber)
    {
        DateTime firstDay = new DateTime(year, 1, 1);

        int offset = DayOfWeek.Monday - firstDay.DayOfWeek;
        if (offset > 0) offset -= 7;

        DateTime firstMonday = firstDay.AddDays(offset);
        DateTime startOfWeek = firstMonday.AddDays((weekNumber - 1) * 7);
        DateTime endOfWeek = startOfWeek.AddDays(6);

        return (DateOnly.FromDateTime(startOfWeek), DateOnly.FromDateTime(endOfWeek));
    }

    private static List<AuditChangeDto> BuildChanges(
        ActivitiesEntity newActivity,
        ActivitiesEntity? oldActivity = null,
        UpdateActivityDto? dto = null)
    {
        List<AuditChangeDto> changes = new();

        if (oldActivity is null)
        {
            AddCreate(nameof(newActivity.StartDate), newActivity.StartDate);
            AddCreate(nameof(newActivity.StartTime), newActivity.StartTime);
            AddCreate(nameof(newActivity.RequerimentId), newActivity.RequerimentId);
            AddCreate(nameof(newActivity.Description), newActivity.Description);
            AddCreate(nameof(newActivity.Etapa), newActivity.Etapa);

            return changes;
        }

        if (dto is not null)
        {
            if (dto.StartDate != oldActivity.StartDate)
                AddUpdate(nameof(newActivity.StartDate), oldActivity.StartDate, newActivity.StartDate);

            if (dto.StartTime != oldActivity.StartTime)
                AddUpdate(nameof(newActivity.StartTime), oldActivity.StartTime, newActivity.StartTime);

            if (dto.EndTime != oldActivity.EndTime)
                AddUpdate(nameof(newActivity.EndTime), oldActivity.EndTime, newActivity.EndTime);

            if (dto.RequerimentId.HasValue &&
                dto.RequerimentId.Value != oldActivity.RequerimentId)
                AddUpdate(nameof(newActivity.RequerimentId), oldActivity.RequerimentId, newActivity.RequerimentId);

            if (!string.IsNullOrWhiteSpace(dto.Description) &&
                dto.Description != oldActivity.Description)
                AddUpdate(nameof(newActivity.Description), oldActivity.Description, newActivity.Description);

            if (dto.IsLoaded != oldActivity.IsLoaded)
                AddUpdate(nameof(newActivity.IsLoaded), oldActivity.IsLoaded, newActivity.IsLoaded);

            if (!string.IsNullOrWhiteSpace(dto.StatusMessage) &&
                dto.StatusMessage != oldActivity.StatusMessage)
                AddUpdate(nameof(newActivity.StatusMessage), oldActivity.StatusMessage, newActivity.StatusMessage);

            if (!string.IsNullOrWhiteSpace(dto.Comment) &&
                dto.Comment != oldActivity.Comment)
                AddUpdate(nameof(newActivity.Comment), oldActivity.Comment, newActivity.Comment);

            if (dto.Etapa != oldActivity.Etapa)
                AddUpdate(nameof(newActivity.Etapa), oldActivity.Etapa, newActivity.Etapa);
        }

        return changes;

        void AddCreate(string field, object? value)
        {
            changes.Add(new AuditChangeDto
            {
                Field = field,
                NewValue = value?.ToString()
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