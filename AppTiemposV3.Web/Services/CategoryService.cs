using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Categories;
using AppTiemposV3.SharedClases.GenericModels;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;

namespace AppTiemposV3.Web.Services;

public class CategoryService: ICategoryContract<CategoryResponseDto>
{
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl = "/api";
    private readonly JsonSerializerOptions? options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    //
    public CategoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DataAResponse<CategoryResponseDto>> GetAllCategories()
    {
        DataAResponse<CategoryResponseDto>? categories = await _httpClient.GetFromJsonAsync<DataAResponse<CategoryResponseDto>>($"{BaseUrl}/categories/todos", options);
        
        if (categories is null) 
            return new DataAResponse<CategoryResponseDto>(true, [], HttpStatusCode.OK);
        
        return categories;
    }

    public Task<Pageable<List<CategoryResponseDto>>> GetAllCategoriesPag(PaginationDto pagination, string buscarPor)
    {
        throw new NotImplementedException();
    }

    public Task<DataResponse<CategoryResponseDto>> GetCategoryPorId(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<DataResponse<CategoryResponseDto>> GetCategoryPorSlug(string slug)
    {
        throw new NotImplementedException();
    }

    public async Task<DataResponse<Guid>> GetCategoryIdPorNombre(string nombre)
    {
        string name = nombre.Trim().ToLower().Replace(" ", "-");
        DataResponse<Guid>? categoryId = await _httpClient.GetFromJsonAsync<DataResponse<Guid>>($"{BaseUrl}/categories/n/{name}", options);
        
        if (categoryId is null) 
            return new DataResponse<Guid>(true, Guid.Empty, HttpStatusCode.OK);
        
        return categoryId;
    }

    public async Task<GeneralResponse> CreateCategory(CreateCategoryDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/categories", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public Task<GeneralResponse> UpdateCategory(Guid id, UpdateCategoryDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<GeneralResponse> DeleteCategory(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<GeneralResponse> RestoreCategory(Guid id)
    {
        throw new NotImplementedException();
    }
}