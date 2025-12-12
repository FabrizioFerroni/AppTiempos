using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Invitations;
using AppTiemposV3.SharedClases.Enums;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static Microsoft.AspNetCore.WebUtilities.QueryHelpers; 
using static AppTiemposV3.SharedClases.GenericModels.Generics;

namespace AppTiemposV3.Web.Services;

public class InvitationService : IInvitationContract<InvitationResponseDto>
{
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl = "/api";
    private readonly JsonSerializerOptions? options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public InvitationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Pageable<List<InvitationResponseDto>>> GetAllInvitations(PaginationDtoAdvanced pagination)
    {
        string url = $"{BaseUrl}/invitations";
        
        Dictionary<string, string?> queryParams = new Dictionary<string, string?>
        {
            ["pagina"] = pagination.Pagina.ToString(),
            ["registrosPorPagina"] = pagination.RegistrosPorPagina.ToString(),
            ["ascending"] = pagination.Ascending.ToString()
        };

        if (!string.IsNullOrEmpty(pagination.Ordenar))
        {
            queryParams["ordenar"] = pagination.Ordenar!;
            
            switch (queryParams["ordenar"])
            {
                case "Nombre Completo":
                    queryParams["ordenar"] = "FullName";
                    break;
                case "Fecha Recibido":
                    queryParams["ordenar"] = "DateReceived";
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
        return await _httpClient.GetFromJsonAsync<Pageable<List<InvitationResponseDto>>>(finalUrl, options)
               ?? new Pageable<List<InvitationResponseDto>> { Content = null! };
    }

    public async Task<DataResponse<InvitationResponseDto>> GetInvitationPorId(Guid id)
    {
        DataResponse<InvitationResponseDto>? invitation = await _httpClient.GetFromJsonAsync<DataResponse<InvitationResponseDto>>($"{BaseUrl}/invitations/{id}", options);
        
        if (invitation is null) 
            return new DataResponse<InvitationResponseDto>(true, null!, HttpStatusCode.OK);
        
        return invitation;
    }

    public async Task<DataResponse<InvitationResponseDto>> GetInvitationPorToken(string token)
    {
        DataResponse<InvitationResponseDto>? invitation = await _httpClient.GetFromJsonAsync<DataResponse<InvitationResponseDto>>($"{BaseUrl}/invitations/token/{token}", options);
        
        if (invitation is null) 
            return new DataResponse<InvitationResponseDto>(true, null!, HttpStatusCode.OK);
        
        return invitation;
    }

    public async Task<GeneralResponse> CreateInvitation(CreateInvitationDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/invitations", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<GeneralResponse> AcceptOrDeclineInvitation(Guid id, AcceptOrDeclineInvitationDto dto)
    {
        HttpResponseMessage? response = await _httpClient.PutAsync($"{BaseUrl}/invitations/{id}", GenerateStringContent(SerializeObj(dto)));
        
        string apiResponse = await response.Content.ReadAsStringAsync();
        
        ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
        
        if(!response.IsSuccessStatusCode)
            return new GeneralResponse(false, resultError?.Message!);
        
        return DeserializeJsonString<GeneralResponse>(apiResponse);
    }

    public async Task<GeneralResponse> DeleteInvitation(Guid id)
    {
        HttpResponseMessage? response = await _httpClient.DeleteAsync($"{BaseUrl}/invitations/{id}");

        if (!response.IsSuccessStatusCode)
        {
            return new GeneralResponse(false, "Error al eliminar la invitación.");
        }

        return await response.Content.ReadFromJsonAsync<GeneralResponse>() ??  new GeneralResponse(false, "Hubo un error."); 
    }

    /*public async Task<DataResponse<EstadosInvitaciones>> VerifyInvitation(string token)
    {
        DataResponse<EstadosInvitaciones>? invitation = await _httpClient.GetFromJsonAsync<DataResponse<EstadosInvitaciones>>($"{BaseUrl}/invitations/verify/{token}", options);
        
        if (invitation is null) 
            return new DataResponse<EstadosInvitaciones>(true, EstadosInvitaciones.SinAceptar, HttpStatusCode.OK);
        
        return invitation;
    }*/
    public async Task<DataResponse<EstadosInvitaciones>> VerifyInvitation(string token)
    {
        var responseString = await _httpClient
            .GetFromJsonAsync<DataResponse<string>>($"{BaseUrl}/invitations/verify/{token}", options);

        if (responseString is null)
            return new DataResponse<EstadosInvitaciones>(true, EstadosInvitaciones.SinAceptar, HttpStatusCode.OK);

        // Convertimos el string a enum
        EstadosInvitaciones estado = Enum.Parse<EstadosInvitaciones>(responseString.Data);

        return new DataResponse<EstadosInvitaciones>(responseString.Success, estado, responseString.Status);
    }

}