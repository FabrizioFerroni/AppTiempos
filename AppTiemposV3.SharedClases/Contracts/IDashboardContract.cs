using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts;

public interface IDashboardContract<T>
{
    Task<DataResponse<T>> GetKpiDashboard(int year, int weekNumber);
}