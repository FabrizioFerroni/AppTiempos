using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.DTOs.Testers;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;
using static Microsoft.AspNetCore.WebUtilities.QueryHelpers;

namespace AppTiemposV3.Web.Services;

public class TesterService : ITesterContract<TesterResponseDto>
{
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl = "/api/testers";
    private readonly JsonSerializerOptions? options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public TesterService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GeneralResponse> CreateTester(CreateTesterDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}", GenerateStringContent(SerializeObj(dto)));

        string apiResponse = await response.Content.ReadAsStringAsync();

        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);

        if (!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);

        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<GeneralResponse> DeleteTester(Guid id)
    {
        HttpResponseMessage? response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

        string apiResponse = await response.Content.ReadAsStringAsync();

        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);

        if (!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);

        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<DataAResponse<TesterResponseDto>> GetAllTesters()
    {
        DataAResponse<TesterResponseDto>? response = await _httpClient.GetFromJsonAsync<DataAResponse<TesterResponseDto>>($"{BaseUrl}", options);

        if (response is null)
            return new DataAResponse<TesterResponseDto>(true, [], HttpStatusCode.OK);

        return response;

    }

    public async Task<Pageable<List<TesterResponseDto>>> GetAllTestersPaginated(PaginationDtoAdvanced pagination)
    {
        string url = $"{BaseUrl}/paginated";


        Dictionary<string, string?> queryParams = new Dictionary<string, string?>
        {
            ["pagina"] = pagination.Pagina.ToString(),
            ["registrosPorPagina"] = pagination.RegistrosPorPagina.ToString(),
            ["ascending"] = pagination.Ascending.ToString()
        };

        if (!string.IsNullOrEmpty(pagination.Ordenar))
        {
            queryParams["ordenar"] = pagination.Ordenar!.ToString();

            switch (queryParams["ordenar"])
            {
                case "Nombre":
                    queryParams["ordenar"] = "Nombre";
                    break;
                case "Activo":
                    queryParams["ordenar"] = "Activo";
                    break;
                default:
                    queryParams["ordenar"] = "CreatedAt";
                    break;
            }
        }

        if (pagination.Filters is { Length: > 0 })
        {
            for (int i = 0; i < pagination.Filters.Length; i++)
            {
                AdvancedFilters filter = pagination.Filters[i];
                if (!string.IsNullOrWhiteSpace(filter.Key))
                {
                    string key = filter.Key;
                    if (!string.IsNullOrEmpty(key))
                    {
                        key = char.ToUpper(key[0]) + key.Substring(1);
                    }
                    queryParams[$"filters[{i}].key"] = key;
                }
                if (!string.IsNullOrWhiteSpace(filter.Value))
                {
                    queryParams[$"filters[{i}].value"] = filter.Value;
                }
            }
        }

        string finalUrl = AddQueryString(url, queryParams);
        return await _httpClient.GetFromJsonAsync<Pageable<List<TesterResponseDto>>>(finalUrl, options)
               ?? new Pageable<List<TesterResponseDto>> { Content = null! };
    }

    public async Task<DataResponse<TesterResponseDto>> GetTesterPorId(Guid id)
    {
        DataResponse<TesterResponseDto>? response = await _httpClient.GetFromJsonAsync<DataResponse<TesterResponseDto>>($"{BaseUrl}/{id}", options);

        if (response is null)
            return new DataResponse<TesterResponseDto>(true, null!, HttpStatusCode.OK);

        return response;
    }

    public async Task<DataResponse<TesterResponseDto>> GetTesterPorNombre(string nombre)
    {
        DataResponse<TesterResponseDto>? response = await _httpClient.GetFromJsonAsync<DataResponse<TesterResponseDto>>($"{BaseUrl}/n/{nombre}", options);

        if (response is null)
            return new DataResponse<TesterResponseDto>(true, null!, HttpStatusCode.OK);

        return response;
    }

    public async Task<GeneralResponse> RestoreTester(Guid id)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/{id}", null);

        string apiResponse = await response.Content.ReadAsStringAsync();

        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);

        if (!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);

        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }



    public async Task<GeneralResponse> UpdateTester(Guid id, UpdateTesterDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PutAsync($"{BaseUrl}/{id}", GenerateStringContent(SerializeObj(dto)));

        string apiResponse = await response.Content.ReadAsStringAsync();

        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);

        if (!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);

        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }
}
