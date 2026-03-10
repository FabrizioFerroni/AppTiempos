using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.RequerimentsAttachments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Controllers
{
    [Authorize]
    [Route("api/requeriments-attachments")]
    [ApiController]
    public class RequerimentAttachmentController : ControllerBase
    {
        private IRequerimentAttachmentContract<RequerimentsAttachmentsDto> _requerimentAttachmentContract;

        public RequerimentAttachmentController(IRequerimentAttachmentContract<RequerimentsAttachmentsDto> requerimentAttachmentContract)
        {
            _requerimentAttachmentContract = requerimentAttachmentContract;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllDocuments()
        {
            DataAResponse<RequerimentsAttachmentsDto> response = await _requerimentAttachmentContract.GetAllRAttachments();

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentById(Guid id)
        {
            DataResponse<RequerimentsAttachmentsDto> response = await _requerimentAttachmentContract.GetRAttachmentById(id);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("requeriment/{reqID}")]
        public async Task<IActionResult> GetAllDocumentsByRequerimentID(Guid reqID)
        {
            DataAResponse<RequerimentsAttachmentsDto> response = await _requerimentAttachmentContract.GetAllRAttachmentsByRequerimentId(reqID);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost()]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument([FromForm] CreateOrUpdateRequerimentAttachmentDto dto, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Archivo no seleccionado");

            using MemoryStream? ms = new MemoryStream();
            await file.CopyToAsync(ms);
            byte[] fileData = ms.ToArray();

            GeneralResponse response = await _requerimentAttachmentContract.CreateRAttachment(dto, fileData, file.FileName);

            if (!response.Flag)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateUploadDocument(Guid id, [FromForm] CreateOrUpdateRequerimentAttachmentDto dto, IFormFile? file)
        {
            byte[]? fileBytes = null;
            string? fileName = null;

            if (file != null && file.Length > 0)
            {
                using MemoryStream? ms = new MemoryStream();
                await file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
                fileName = file.FileName;
            }

            GeneralResponse response = await _requerimentAttachmentContract.UpdateRAttachment(id, dto, fileBytes!, fileName!);

            if (!response.Flag)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            GeneralResponse response = await _requerimentAttachmentContract.DeleteRAttachment(id);

            if (!response.Flag)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
