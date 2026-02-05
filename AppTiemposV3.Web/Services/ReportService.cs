using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Reports;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using AppTiemposV3.Web.Components.Icons;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;
using static Microsoft.AspNetCore.WebUtilities.QueryHelpers;
using static System.Text.Json.JsonNamingPolicy;

namespace AppTiemposV3.Web.Services
{
    public class ReportService : IReportContract
    {

        private readonly HttpClient _httpClient;
        private readonly string BaseUrl = "/api";
        private readonly JsonSerializerOptions? options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(CamelCase) }
        };

        public ReportService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Pageable<List<ListAllReportsDto>>> GetAllReports(PaginationDto pagination, int type)
        {

            string url = $"{BaseUrl}/reports?type={type}";


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
                    default:
                        queryParams["ordenar"] = "CreatedAt";
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                queryParams["search"] = pagination.Search;
            }

            return await _httpClient.GetFromJsonAsync<Pageable<List<ListAllReportsDto>>>(AddQueryString(url, queryParams), options)
                   ?? new Pageable<List<ListAllReportsDto>> { Content = null };
        }

        public async Task<DataResponse<CountTotalReports>> GetTotalReports(string? search)
        {
            string url = $"{BaseUrl}/reports/totals";

            if(!string.IsNullOrWhiteSpace(search))
            {
                url = AddQueryString(url, new Dictionary<string, string?>
                {
                    ["search"] = search
                });
            }

            DataResponse<CountTotalReports>? totals = await _httpClient.GetFromJsonAsync<DataResponse<CountTotalReports>>(url, options);

            if (totals is null)
                return new DataResponse<CountTotalReports>(true, null!, HttpStatusCode.OK);

            return totals;
        }

        public async Task<DataResponse<ListReportDto>> GetReportByUrl(string urlIdentificator)
        {
            string url = $"{BaseUrl}/reports/{urlIdentificator}";

            DataResponse<ListReportDto>? report = await _httpClient.GetFromJsonAsync<DataResponse<ListReportDto>>(url, options);

            if (report is null)
                return new DataResponse<ListReportDto>(true, null!, HttpStatusCode.OK);

            return report;

        }

        public async Task<GeneralResponse> CreateNewReport(CreateNewReportDto dto)
        {
            HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/reports", GenerateStringContent(SerializeObj(dto)));

            string apiResponse = await response.Content.ReadAsStringAsync();

            ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);

            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false, resultError?.Message!);

            return DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<GeneralResponse> AddOrQuitFavorite(Guid id, AddOrRemoveFavoriteDto dto)
        {
            HttpResponseMessage response = await _httpClient.PutAsync($"{BaseUrl}/reports/favorite/{id}", GenerateStringContent(SerializeObj(dto)));

            string apiResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);
                    return new GeneralResponse(false, resultError?.Message ?? "Error desconocido en el servidor");
                }
                catch
                {
                    return new GeneralResponse(false, $"Error del servidor ({response.StatusCode}): {apiResponse}");
                }
            }

            return DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<byte[]> GeneratePDF(Guid id)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{BaseUrl}/reports/descargar/pdf/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null!;
            }

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<GeneralResponse> DeleteReport(Guid id)
        {
            HttpResponseMessage? response = await _httpClient.DeleteAsync($"{BaseUrl}/reports/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return new GeneralResponse(false, "Error al eliminar el reporte.");
            }

            return await response.Content.ReadFromJsonAsync<GeneralResponse>() ?? new GeneralResponse(false, "Hubo un error.");
        }

        public async Task<byte[]> GenerateExcel(Guid id)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{BaseUrl}/reports/descargar/excel/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null!;
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
