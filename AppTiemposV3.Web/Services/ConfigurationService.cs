using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Configurations;
using AppTiemposV3.SharedClases.Utilidades;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;
using static System.Net.HttpStatusCode;

namespace AppTiemposV3.Web.Services
{
    public class ConfigurationService : IConfigurationContract
    {
        private readonly HttpClient _httpClient;
        private readonly string BaseUrl = "/api/configurations";
        private readonly JsonSerializerOptions _options;

        public ConfigurationService(HttpClient httpClient, JsonSerializerOptions options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<DataResponse<ListActualConfig>> GetConfiguration()
        {
             HttpResponseMessage? response = await _httpClient.GetAsync($"{BaseUrl}/actual");

             response.EnsureSuccessStatusCode();

             DataResponse<ListActualConfig>? result = await response.Content
                 .ReadFromJsonAsync<DataResponse<ListActualConfig>>(_options);

             return result!;
        }

        public async Task<GeneralResponse> CreateConfig(CreateConfigurationDto dto)
        {
            HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/setup", GenerateStringContent(SerializeObj(dto)));

            string apiResponse = await response.Content.ReadAsStringAsync();

            ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);

            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false, resultError?.Message!);

            return DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<GeneralResponse> UpdateConfig(UpdateConfigDto dto)
        {
            HttpResponseMessage response = await _httpClient.PutAsync($"{BaseUrl}", GenerateStringContent(SerializeObj(dto)));

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

        public async Task<GeneralResponse> ResetConfig()
        {
            HttpResponseMessage? response = await _httpClient.DeleteAsync($"{BaseUrl}");

            if (!response.IsSuccessStatusCode)
            {
                return new GeneralResponse(false, "Error al resetear la configuracion.");
            }

            return await response.Content.ReadFromJsonAsync<GeneralResponse>() ?? new GeneralResponse(false, "Hubo un error.");
        }

        public async Task<Stream?> DownloadBackup()
        {

            HttpResponseMessage? response = await _httpClient.GetAsync($"{BaseUrl}/download/backup",
                                             HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<DataResponse<AutoBackup>> GetLastManualBackup()
        {
            HttpResponseMessage? response = await _httpClient.GetAsync($"{BaseUrl}/backup/manual/last");

            response.EnsureSuccessStatusCode();

            DataResponse<AutoBackup>? result = await response.Content
                .ReadFromJsonAsync<DataResponse<AutoBackup>>(_options);

            return result!;
        }

        public async Task<DataResponse<List<AutoBackup>>> GetAutoBackupsHistory()
        {
            HttpResponseMessage? response = await _httpClient.GetAsync($"{BaseUrl}/backup/auto/history");

            response.EnsureSuccessStatusCode();

            DataResponse<List<AutoBackup>>? result = await response.Content
                .ReadFromJsonAsync<DataResponse<List<AutoBackup>>>(_options);

            return result!;
        }


        public async Task<DataResponse<int>> GetTotalAutomaticBackups()
        {
            HttpResponseMessage? response = await _httpClient.GetAsync($"{BaseUrl}/backup/auto/total");

            response.EnsureSuccessStatusCode();

            DataResponse<int>? result = await response.Content.ReadFromJsonAsync<DataResponse<int>>(_options);

            return result!;
        }

        public async Task<Stream?> DownloadFileBackup(Guid id)
        {

            HttpResponseMessage? response = await _httpClient.GetAsync($"{BaseUrl}/backup/download/{id}",
                                             HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<GeneralResponse> RestoreBackupFromFileServer(Guid id)
        {
            HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/backup/restore/automatic/{id}", GenerateStringContent(SerializeObj(new { })));

            string apiResponse = await response.Content.ReadAsStringAsync();

            ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);

            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false, resultError?.Message!);

            return DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<GeneralResponse> RestoreFromUpload(byte[] fileBytes, string fileName)
        {
            using MultipartFormDataContent? content = new MultipartFormDataContent();
            ByteArrayContent? byteContent = new ByteArrayContent(fileBytes);

            content.Add(byteContent, "backupFile", fileName);

            HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/backup/restore/upload", content);

            string apiResponse = await response.Content.ReadAsStringAsync();

            ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);

            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false, resultError?.Message!);

            return DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<DataResponse<bool>> HasConfiguration()
        {
            HttpResponseMessage? response = await _httpClient.GetAsync($"{BaseUrl}/has-setup");

            response.EnsureSuccessStatusCode();

            DataResponse<bool>? result = await response.Content.ReadFromJsonAsync<DataResponse<bool>>(_options);

            return result!;
        }

        public async Task<GeneralResponse> ImportDataFromExcel(byte[] fileBytes, string? fileName)
        {
            using MultipartFormDataContent? content = new MultipartFormDataContent();
            ByteArrayContent? byteContent = new ByteArrayContent(fileBytes);

            content.Add(byteContent, "file", fileName!);

            HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}/importar/datos", content);

            string apiResponse = await response.Content.ReadAsStringAsync();

            ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);

            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false, resultError?.Message!);

            return DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<DataResponse<SaturdayBannerConfigDto>> ThisWeekHaveSaturdayWork()
        {
            HttpResponseMessage? response = await _httpClient.GetAsync($"{BaseUrl}/has-work-saturday");

            response.EnsureSuccessStatusCode();

            DataResponse<SaturdayBannerConfigDto>? result = await response.Content.ReadFromJsonAsync<DataResponse<SaturdayBannerConfigDto>>(_options);

            return result!;
        }

        public async Task<DataResponse<ProgressHoursConfigDto>> ProgressHours()
        {
            HttpResponseMessage? response = await _httpClient.GetAsync($"{BaseUrl}/progress");

            response.EnsureSuccessStatusCode();

            DataResponse<ProgressHoursConfigDto>? result = await response.Content.ReadFromJsonAsync<DataResponse<ProgressHoursConfigDto>>(_options);

            return result!;
        }
    }

}
