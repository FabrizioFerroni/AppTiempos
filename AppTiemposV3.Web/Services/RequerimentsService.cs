using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;

namespace AppTiemposV3.Web.Services;

public class RequerimentsService : IRequerimentContract<RequerimentResponseDto>
{
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl = "/api";
    private readonly JsonSerializerOptions? options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    //
    public RequerimentsService(HttpClient httpClient)
    {
         _httpClient = httpClient;
    }

    public async Task<DataAResponse<RequerimentResponseDto>> GetAllRequeriments()
    {
        DataAResponse<RequerimentResponseDto>? requeriments = await _httpClient.GetFromJsonAsync<DataAResponse<RequerimentResponseDto>>($"{BaseUrl}/requeriments/todos", options);
        
        if (requeriments is null) 
            return new DataAResponse<RequerimentResponseDto>(true, [], HttpStatusCode.OK);
        
        return requeriments;
    }

    public async Task<Pageable<List<RequerimentResponseDto>>> GetAllRequerimentsPag(PaginationDto pagination, string buscarPor)
    {
        if (buscarPor == "")
        {
            buscarPor = "cliente";
        }

        if (buscarPor == "conjunto de cambios")
        {
            buscarPor = "conjuntocambios";
        }
        
        if (buscarPor == "categoria")
        {
            buscarPor = "categoryid";
        }
        
        if(pagination.Ordenar == "")
        {
            pagination.Ordenar = "ReqID";
        }
        
        if(pagination.Ordenar == "Conjunto de Cambios")
        {
            pagination.Ordenar = "ConjuntoCambios";
        }
        
        if(pagination.Ordenar == "Categoria")
        {
            pagination.Ordenar = "categoryid";
        }
        
        string url = $"{BaseUrl}/requeriments?pagina={pagination.Pagina}" +
                     $"&registrosPorPagina={pagination.RegistrosPorPagina}" +
                     $"&buscarPor={buscarPor}" +
                     $"&ascending={pagination.Ascending}" +
                     $"&ordenar={pagination.Ordenar}";
        
        //if (!string.IsNullOrWhiteSpace(pagination.Search))
        if (!string.IsNullOrWhiteSpace(pagination.Search))
            url += $"&search={pagination.Search}";

        return await _httpClient.GetFromJsonAsync<Pageable<List<RequerimentResponseDto>>>(url, options)
               ?? new Pageable<List<RequerimentResponseDto>> { Content = null };
    }

    public async Task<DataResponse<RequerimentResponseDto>> GetRequerimentporId(Guid id)
    {
        DataResponse<RequerimentResponseDto>? requeriment = await _httpClient.GetFromJsonAsync<DataResponse<RequerimentResponseDto>>($"{BaseUrl}/requeriments/{id}", options);
        
        if (requeriment is null) 
            return new DataResponse<RequerimentResponseDto>(true, null!, HttpStatusCode.OK);
        
        return requeriment;
    }

    public Task<DataResponse<RequerimentResponseDto>> GetRequerimentporReqId(string reqId)
    {
        throw new NotImplementedException();
    }

    public async Task<GeneralResponse> CreateRequeriment(CreateRequerimentDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/requeriments", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<GeneralResponse> UpdateRequeriment(Guid id, UpdateRequerimentDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PutAsync($"{BaseUrl}/requeriments/{id}", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<GeneralResponse> DeleteRequeriment(Guid id)
    {
        HttpResponseMessage? response = await _httpClient.DeleteAsync($"{BaseUrl}/requeriments/{id}");

        if (!response.IsSuccessStatusCode)
        {
            return new GeneralResponse(false, "Error al eliminar el requerimiento.");
        }

        return await response.Content.ReadFromJsonAsync<GeneralResponse>() ??  new GeneralResponse(false, "Hubo un error."); 
    }

    public Task<GeneralResponse> RestoreRequeriment(Guid id)
    {
        throw new NotImplementedException();
    }
}