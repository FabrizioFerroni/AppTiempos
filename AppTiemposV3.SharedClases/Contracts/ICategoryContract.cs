using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Categories;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts;

public interface ICategoryContract<T>
{
    Task<DataAResponse<T>> GetAllCategories();

    Task<Pageable<List<T>>> GetAllCategoriesPag(PaginationDto pagination, string buscarPor);

    Task<DataResponse<T>> GetCategoryPorId(Guid id);
        
    Task<DataResponse<T>> GetCategoryPorSlug(string slug);
    
    Task<DataResponse<Guid>> GetCategoryIdPorNombre(string nombre);

    Task<GeneralResponse> CreateCategory(CreateCategoryDto dto);

    Task<GeneralResponse> UpdateCategory(Guid id, UpdateCategoryDto dto);

    Task<GeneralResponse> DeleteCategory(Guid id);
    
    Task<GeneralResponse> RestoreCategory(Guid id);
}