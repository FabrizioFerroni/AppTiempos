using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts;

public interface ITrainingContract<T>
{
    Task<DataResponse<TrainingKpiResponse>> GetTrainingKpi();
    
    Task<Pageable<List<T>>> GetAllTrainings(PaginationDtoAdvanced pagination);
    
    Task<DataResponse<T>> GetTrainingPorId(Guid id);
    
    Task<GeneralResponse> CreateTraining(CreateTrainingDto dto);

    Task<GeneralResponse> UpdateTraining(Guid id, UpdateTrainingDto dto);

    Task<GeneralResponse> DeleteTraining(Guid id);
        
    Task<GeneralResponse> RestoreTraining(Guid id);
    
}