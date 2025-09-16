using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.GenericModels;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts;

public interface IGenericSContract<T>
{
    Task<DataAResponse<ColorModel>> GetAllColors();
}