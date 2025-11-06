using System.Globalization;
using System.Net;
using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.Exceptions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Repositories;

public class ActivityWeeklyRepository: IActivityWeeklyContract<ActivitiesByDay>
{
    private readonly AppDbContext _dbCxt;
    private readonly IMapper _iMapper;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IUserContract _userContext;
    private Guid UserId => _userContext.GetUserId();

    public ActivityWeeklyRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IUserContract userContext)
    {
        _dbCxt = dbCxt;
        _iMapper = iMapper;
        _userManager = userManager;
        _userContext = userContext;
    }
    
    public async Task<DataAResponse<ActivitiesByDay>> GetAllActivitiesPerRangeWeek(int year, int weekNumber)
    {
        UserEntity user = await GetUserByIdAsync(UserId);
        var (start, end) = GetDateRangeFromWeek(year, weekNumber);
        
        List<ActivityResponseDto> activities = await _dbCxt.Activities
            .Include(a => a.Requeriment)
            .Include(a => a.User)
            .Where(u => u.UserId == user.Id)
            .Where(a => a.StartDate >= start && a.StartDate <= end)
            .OrderByDescending(o => o.StartDate)
            .ThenByDescending(o => o.StartTime)
            .ProjectTo<ActivityResponseDto>(_iMapper.ConfigurationProvider)
            .ToListAsync();
        
        List<ActivitiesByDay> grouped = activities
            .GroupBy(a => a.StartDate)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                double totalSeconds = g.Sum(a =>
                {
                    if (string.IsNullOrEmpty(a.StartTime))
                        return 0;

                    if (!TimeOnly.TryParse(a.StartTime, out TimeOnly startTime))
                        return 0;

                    TimeOnly endTime = startTime;
                    if (!string.IsNullOrEmpty(a.EndTime) && TimeOnly.TryParse(a.EndTime, out TimeOnly parsedEnd))
                        endTime = parsedEnd;

                    return (endTime.ToTimeSpan() - startTime.ToTimeSpan()).TotalSeconds;
                });
                
                TimeSpan totalWorked = TimeSpan.FromSeconds(totalSeconds);

                string dayName = g.Key.ToDateTime(TimeOnly.MinValue)
                    .ToString("dddd", new CultureInfo("es-ES"));
                dayName = char.ToUpper(dayName[0]) + dayName.Substring(1); 

                string dayNameAndDay = $"{dayName} {g.Key:dd/MM}";

                return new ActivitiesByDay
                {
                    Day = g.Key,
                    DayName = dayName.ToUpperInvariant(),
                    DayNameAndDay = dayNameAndDay,        
                    Worked = totalWorked,                
                    Activities = g.ToList()
                };
            })
            .ToList();

        return new DataAResponse<ActivitiesByDay>(true, grouped, HttpStatusCode.OK);
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
    
    private async Task<UserEntity> GetUserByIdAsync(Guid userId)
    {
        UserEntity? user = await _userManager.FindByIdAsync(userId.ToString());
        return user ?? throw new NotFoundException("El usuario no fue encontrado");
    }
}