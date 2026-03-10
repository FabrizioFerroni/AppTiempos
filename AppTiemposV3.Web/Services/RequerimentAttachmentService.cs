using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Invitations;
using AppTiemposV3.SharedClases.DTOs.RequerimentsAttachments;
using AppTiemposV3.SharedClases.Enums;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;

namespace AppTiemposV3.Web.Services
{
    public class RequerimentAttachmentService : IRequerimentAttachmentContract<RequerimentsAttachmentsDto>
    {

        private readonly HttpClient _httpClient;
        private readonly string BaseUrl = "/api/requeriments-attachments";
        private readonly JsonSerializerOptions _options;

        public RequerimentAttachmentService(HttpClient httpClient, JsonSerializerOptions options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<GeneralResponse> CreateRAttachment(CreateOrUpdateRequerimentAttachmentDto dto, byte[] fileBytes, string fileName)
        {
            using MultipartFormDataContent? content = new MultipartFormDataContent();
            ByteArrayContent? byteContent = new ByteArrayContent(fileBytes);

            content.Add(byteContent, "file", fileName!);
            content.Add(new StringContent(dto.Etapa.ToString()), nameof(dto.Etapa));
            content.Add(new StringContent(dto.AttachmentBy ?? ""), nameof(dto.AttachmentBy));
            content.Add(new StringContent(dto.Descripcion ?? ""), nameof(dto.Descripcion));
            content.Add(new StringContent(dto.AttachmentAt.ToString("o")), nameof(dto.AttachmentAt)); // Formato ISO 8601
            content.Add(new StringContent(dto.RequerimentId.ToString() ?? ""), nameof(dto.RequerimentId));

            HttpResponseMessage? response = await _httpClient.PostAsync($"{BaseUrl}", content);

            string apiResponse = await response.Content.ReadAsStringAsync();

            ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);

            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false, resultError?.Message!);

            return DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<GeneralResponse> DeleteRAttachment(Guid id)
        {
            HttpResponseMessage? response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return new GeneralResponse(false, "Error al eliminar el documento.");
            }

            return await response.Content.ReadFromJsonAsync<GeneralResponse>() ?? new GeneralResponse(false, "Hubo un error.");
        }

        public async Task<DataAResponse<RequerimentsAttachmentsDto>> GetAllRAttachments()
        {
            DataAResponse<RequerimentsAttachmentsDto>? documents = await _httpClient.GetFromJsonAsync<DataAResponse<RequerimentsAttachmentsDto>>($"{BaseUrl}", _options);

            if (documents is null)
                return new DataAResponse<RequerimentsAttachmentsDto>(true, null!, HttpStatusCode.OK);

            return documents;
        }

        public async Task<DataAResponse<RequerimentsAttachmentsDto>> GetAllRAttachmentsByRequerimentId(Guid requerimentId)
        {
            DataAResponse<RequerimentsAttachmentsDto>? documents = await _httpClient.GetFromJsonAsync<DataAResponse<RequerimentsAttachmentsDto>>($"{BaseUrl}/requeriment/{requerimentId}", _options);

            if (documents is null)
                return new DataAResponse<RequerimentsAttachmentsDto>(true, null!, HttpStatusCode.OK);

            return documents;
        }

        public async Task<DataResponse<RequerimentsAttachmentsDto>> GetRAttachmentById(Guid id)
        {
            DataResponse<RequerimentsAttachmentsDto>? documents = await _httpClient.GetFromJsonAsync<DataResponse<RequerimentsAttachmentsDto>>($"{BaseUrl}/{id}", _options);

            if (documents is null)
                return new DataResponse<RequerimentsAttachmentsDto>(true, null!, HttpStatusCode.OK);

            return documents;
        }

        public async Task<GeneralResponse> UpdateRAttachment(Guid id, CreateOrUpdateRequerimentAttachmentDto dto, byte[] fileBytes, string fileName)
        {
            using MultipartFormDataContent? content = new MultipartFormDataContent();

            if (fileBytes != null && fileBytes.Length > 0)
            {
                ByteArrayContent byteContent = new ByteArrayContent(fileBytes);
                content.Add(byteContent, "file", fileName ?? "");
            }

            content.Add(new StringContent(dto.Etapa.ToString()), nameof(dto.Etapa));
            content.Add(new StringContent(dto.AttachmentBy ?? ""), nameof(dto.AttachmentBy));
            content.Add(new StringContent(dto.Descripcion ?? ""), nameof(dto.Descripcion));
            content.Add(new StringContent(dto.AttachmentAt.ToString("o")), nameof(dto.AttachmentAt)); // Formato ISO 8601
            content.Add(new StringContent(dto.RequerimentId.ToString() ?? ""), nameof(dto.RequerimentId));

            HttpResponseMessage? response = await _httpClient.PutAsync($"{BaseUrl}/{id}", content);

            string apiResponse = await response.Content.ReadAsStringAsync();

            ErrorResponse? resultError = DeserializeJsonString<ErrorResponse>(apiResponse);

            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false, resultError?.Message!);

            return DeserializeJsonString<GeneralResponse>(apiResponse);
        }
    }
}
