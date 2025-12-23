using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Audits;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static Microsoft.AspNetCore.WebUtilities.QueryHelpers; 
using static AppTiemposV3.Web.Utils.AuditActionHelper;

namespace AppTiemposV3.Web.Services;

public class AuditService : IAuditContract<AuditsResponseDto>
{
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl = "/api";
    private readonly JsonSerializerOptions? options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public AuditService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Pageable<List<AuditsResponseDto>>> GetAllAudits(PaginationDtoAdvanced pagination)
    {
        string url = $"{BaseUrl}/audits";
        
        Dictionary<string, string?> queryParams = new Dictionary<string, string?>
        {
            ["pagina"] = pagination.Pagina.ToString(),
            ["registrosPorPagina"] = pagination.RegistrosPorPagina.ToString(),
            ["ascending"] = pagination.Ascending.ToString(),
            ["ordenar"] = "CreatedAt"
        };

        if (!string.IsNullOrEmpty(pagination.Ordenar))
        {
            queryParams["ordenar"] = pagination.Ordenar!.ToString();
            
            switch (queryParams["ordenar"])
            {
                default:
                    queryParams["ordenar"] = "CreatedAt";
                    break;
            }
        }
        
        if (pagination.Filters is { Length: > 0 })
        {
            List<string> filterKeys = queryParams.Keys
                .Where(k => k.StartsWith("filters["))
                .ToList();

            foreach (string key in filterKeys)
                queryParams.Remove(key);

            int index = 0;

            foreach (AdvancedFilters filter in pagination.Filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Key) || string.IsNullOrWhiteSpace(filter.Value))
                    continue;

                if (filter.Key == "Action")
                {
                    if(filter.Value == "Todas las acciones")
                        continue;
                    
                    filter.Value = Normalize(filter.Value);
                }

                if (filter.Key == "EntityName")
                {
                    string? valor = GetValueEntidad(filter.Value);
                    
                    if (string.IsNullOrWhiteSpace(valor))
                        continue;

                    filter.Value = valor;
                }

                string key = char.ToUpper(filter.Key[0]) + filter.Key[1..];

                queryParams[$"filters[{index}].key"] = key;
                queryParams[$"filters[{index}].value"] = filter.Value;
                index++;
            }
            
            /*for (int i = 0; i < pagination.Filters.Length; i++)
            {
                AdvancedFilters filter = pagination.Filters[i];
                
                if (filter.Key == "Action")
                {
                    if(filter.Value == "Todas las acciones")
                    {
                        queryParams.Remove($"filters[{i}].key");
                        queryParams.Remove($"filters[{i}].value");
                        continue;
                    }
                    
                    filter.Value = Normalize(filter.Value ?? "");
                }

                if (filter.Key == "EntityName")
                {
                    string? valor = GetValueEntidad(filter.Value ?? "Todas las entidades");

                    if (string.IsNullOrWhiteSpace(valor))
                    {
                        queryParams.Remove($"filters[{i}].key");
                        queryParams.Remove($"filters[{i}].value");
                        continue;
                    }
                    
                    filter.Value = valor;
                }
                
                if (!string.IsNullOrWhiteSpace(filter.Key))
                {
                    string key = filter.Key;
                    key = char.ToUpper(key[0]) + key.Substring(1);
                    queryParams[$"filters[{i}].key"] = key;
                }
                if (!string.IsNullOrWhiteSpace(filter.Value))
                {
                    queryParams[$"filters[{i}].value"] = filter.Value;
                }
            } */
        }

        string finalUrl = AddQueryString(url, queryParams);
        return await _httpClient.GetFromJsonAsync<Pageable<List<AuditsResponseDto>>>(finalUrl, options)
               ?? new Pageable<List<AuditsResponseDto>> { Content = null! };
    }

    public async Task<DataAResponse<AuditUserResponseDto>> GetLastFourAuditsUser()
    {
        string url = $"{BaseUrl}/audits/ultimate-audits";

        DataAResponse<AuditUserResponseDto>? audits = await _httpClient.GetFromJsonAsync<DataAResponse<AuditUserResponseDto>>(url, options);
        
        if (audits is null) 
            return new DataAResponse<AuditUserResponseDto>(true, [], HttpStatusCode.OK);
        
        return audits;
    }

    public async Task<DataResponse<AuditKpiResponse>> GetKpis()
    {
        string url = $"{BaseUrl}/audits/kpi";
        
        DataResponse<AuditKpiResponse>? kpis = await _httpClient.GetFromJsonAsync<DataResponse<AuditKpiResponse>>(url, options);
        
        if (kpis is null) 
            return new DataResponse<AuditKpiResponse>(true, null!, HttpStatusCode.OK);
        
        return kpis;
    }

    public async Task<GeneralResponse> CreateAudit(CreateAuditDto dto)
    {
        throw new NotImplementedException();
    }

    private string GetValueEntidad(string entity)
    {
        switch (entity)
        {
            case "Todas las entidades":
                return "";
            case "Actividades":
                return "activities";
            case "Capacitaciones":
                return "trainings";
            case "Requerimientos":
                return "requeriments";
            case "Rechazos":
                return "rechazos";
            case "Usuarios":
                return "usuarios";
            default:
                return entity;
        }
    }
}