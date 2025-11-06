using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts;


public interface IActivityWeeklyContract<T>
{
    Task<DataAResponse<T>> GetAllActivitiesPerRangeWeek(int year, int weekNumber);
}