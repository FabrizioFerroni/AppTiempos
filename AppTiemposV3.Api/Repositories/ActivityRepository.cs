using System.Net;
using System.Text.Json;
using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.Exceptions;
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
    private readonly IGenericContract _genericContract;
    private readonly IUserContract _userContext;
    private Guid _userId => _userContext.GetUserId();
    
    public ActivityRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IGenericContract genericContract, IUserContract userContext)
    {
        _dbCxt = dbCxt;
        _iMapper = iMapper;
        _userManager = userManager;
        _genericContract = genericContract;
        _userContext = userContext;
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

        List<ActivityResponseDto> activities = await _dbCxt.Activities
            .Include(a => a.User)
            .Include(a => a.Requeriment)
            .Where(u => u.UserId == user.Id && u.StartDate >= startDate)
            .OrderByDescending(o => o.StartDate)
            .ThenByDescending(o => o.StartTime)
            .ProjectTo<ActivityResponseDto>(_iMapper.ConfigurationProvider)
            .ToListAsync();

        return new DataAResponse<ActivityResponseDto>(true, activities, HttpStatusCode.OK);
    }

    public async Task<Pageable<List<ActivityResponseDto>>> GetAllActivitiesPerRangePag(PaginationDto pagination, DateOnly startDate, DateOnly endDate)
    {
        return await _genericContract.GetAllPaginatedPerRangeAsync<ActivitiesEntity, ActivityResponseDto>(pagination, startDate, endDate, _userId);
    }

    public async Task<DataAResponse<ActivityResponseDto>> GetLastThreeActivities()
    {
        UserEntity user = await GetUserByIdAsync(_userId);

        List<ActivityResponseDto> activities = await _dbCxt.Activities
            .Where(u => u.UserId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .Take(3)
            .ProjectTo<ActivityResponseDto>(_iMapper.ConfigurationProvider)
            .ToListAsync();


        return new DataAResponse<ActivityResponseDto>(true, activities, HttpStatusCode.OK);
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
        
        act.StartTime = dto.StartTime;
        act.UrlIndetificator = await Nanoid.GenerateAsync(Nanoid.Alphabets.LowercaseLettersAndDigits, 10);
        act.UserId = user.Id;
        act.StatusMessage = "in-progress";
            
        await _dbCxt.Activities.AddAsync(act);

        await EnsureSavedAsync("Hubo un error al crear la actividad");

        return new GeneralResponse(true, "Actividad creada correctamente");
    }

    public async Task<GeneralResponse> UpdateActivity(Guid id, UpdateActivityDto dto)
    {
        UserEntity user = await GetUserByIdAsync(_userId);

        ActivitiesEntity? act = await _dbCxt.Activities
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

        return new GeneralResponse(true, "Se actualizo con exito la actividad.");
    }

    public async Task<GeneralResponse> DeleteActivity(Guid id)
    {
        UserEntity? user = await GetUserByIdAsync(_userId);

        ActivitiesEntity? act = await _dbCxt.Activities.FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
        
        if(act == null)
            throw new NotFoundException("No activities found");
            
        act.IsDeleted = true;
        act.DeletedAt = DateTime.Now;
        
        _dbCxt.Entry(act).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo un error al eliminar la actividad. Intente mas tarde");

        return new GeneralResponse(true, "Se eliminó con éxito la actividadcon");
    }

    public async Task<GeneralResponse> RestoreActivity(Guid id)
    {
        UserEntity? user = await GetUserByIdAsync(_userId);

        ActivitiesEntity? act = await _dbCxt.Activities.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
        
        if(act == null)
            throw new NotFoundException("No activities found");
            
        act.IsDeleted = false;
        act.DeletedAt = null;
        act.ModifiedAt = DateTime.Now;

        await EnsureSavedAsync("Hubo un error al restaurar la actividad. Intente mas tarde");

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
}