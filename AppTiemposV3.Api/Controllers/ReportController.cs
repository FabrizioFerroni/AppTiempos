using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Reports;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportController : ControllerBase
    {

        private readonly IReportContract _reportCnt;

        public ReportController(IReportContract reportCnt)
        {
            _reportCnt = reportCnt;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllReportsPaginados([FromQuery] PaginationDto pagination, [FromQuery] int type = 0)
        {
            Pageable<List<ListAllReportsDto>> response = await _reportCnt.GetAllReports(pagination, type);
            return Ok(response);
        }

        [HttpGet("totals")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTotals([FromQuery] string? search = "")
        {
            DataResponse<CountTotalReports> response = await _reportCnt.GetTotalReports(search);
            return Ok(response);
        }

        [HttpGet("{urlIdentificator}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetReportByUrl(string urlIdentificator)
        {
            DataResponse<ListReportDto> response = await _reportCnt.GetReportByUrl(urlIdentificator);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateRequeriment([FromBody] CreateNewReportDto dto)
        {
            GeneralResponse response = await _reportCnt.CreateNewReport(dto);

            if(!response.Flag)
            {
                return BadRequest(response);
            }

            return StatusCode(201, response);
        }

        [HttpPut("favorite/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AddOrRemoveFavorite(Guid id, [FromBody] AddOrRemoveFavoriteDto dto)
        {
            GeneralResponse response = await _reportCnt.AddOrQuitFavorite(id, dto);

            if (!response.Flag)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }


        [HttpGet("descargar/pdf/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DownloadPdfReport(Guid id)
        {
            byte[]? response = await _reportCnt.GeneratePDF(id);
            return File(response, "application/pdf");
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteReport(Guid id)
        {
            GeneralResponse response = await _reportCnt.DeleteReport(id);

            if (!response.Flag)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("descargar/excel/{id}")]
        public async Task<IActionResult> DownloadExcelReport(Guid id)
        {
            byte[] response = await _reportCnt.GenerateExcel(id);

            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";           

            return File(response, contentType);
        }
    }
}
