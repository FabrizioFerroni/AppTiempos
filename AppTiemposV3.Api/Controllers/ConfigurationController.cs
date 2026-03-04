using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;


namespace AppTiemposV3.Api.Controllers
{
    [Authorize]
    [Route("api/configurations")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationContract _configurationContract;
        public ConfigurationController(IConfigurationContract configurationContract)
        {
            _configurationContract = configurationContract;
        }

        [HttpGet("actual")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetActualConfiguration()
        {
            DataResponse<ListActualConfig> response = await _configurationContract.GetConfiguration();
            return Ok(response);
        }

        [HttpPost("setup")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SetupConfiguration([FromBody] CreateConfigurationDto setupConfigDto)
        {
            GeneralResponse response = await _configurationContract.CreateConfig(setupConfigDto);

            if (!response.Flag)
            {
                return BadRequest(response);
            }

            return StatusCode(201, response);
        }

        [HttpPut]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateConfigy([FromBody] UpdateConfigDto dto)
        {
            GeneralResponse response = await _configurationContract.UpdateConfig(dto);
            return Ok(response);
        }

        [HttpDelete]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RestoreConfig()
        {
            GeneralResponse response = await _configurationContract.ResetConfig();
            return Ok(response);
        }


        [HttpGet("download/backup")]
        public async Task<IActionResult> DownloadBackupSQL()
        {
            Stream? response = await _configurationContract.DownloadBackup();

            if (response == null) return NotFound();

            string contentType = "application/sql";
            string fileName = $"backup_{DateTime.Now:yyyyMMddHHmm}.sql";

            return File(response, contentType, fileName);
        }

        [HttpGet("backup/manual/last")]
        public async Task<IActionResult> GetLastManualBackup()
        {
            DataResponse<AutoBackup> response = await _configurationContract.GetLastManualBackup();
            return Ok(response);
        }

        [HttpGet("backup/auto/history")]
        public async Task<IActionResult> GetAutoBackupsHistory()
        {
            DataResponse<List<AutoBackup>> response = await _configurationContract.GetAutoBackupsHistory();
            return Ok(response);
        }

        [HttpGet("backup/auto/total")]
        public async Task<IActionResult> GetTotalAutomaticBackups()
        {
            DataResponse<int> response = await _configurationContract.GetTotalAutomaticBackups();
            return Ok(response);
        }

        [HttpGet("backup/download/{id}")]
        public async Task<IActionResult> DownloadFileBackup(Guid id)
        {
            Stream? response = await _configurationContract.DownloadFileBackup(id);

            return File(response!, "application/octet-stream");
        }

        [HttpPost("backup/restore/automatic/{id}")]
        public async Task<IActionResult> Restore(Guid id)
        {
            GeneralResponse response = await _configurationContract.RestoreBackupFromFileServer(id);

            if (!response.Flag)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("backup/restore/upload")]
        public async Task<IActionResult> RestoreFromUpload(IFormFile backupFile)
        {
            if (backupFile == null || backupFile.Length == 0)
                return BadRequest(new GeneralResponse(false, "Archivo no válido"));

            // Convertimos IFormFile a byte[] para cumplir con el contrato
            using MemoryStream? ms = new MemoryStream();
            await backupFile.CopyToAsync(ms);
            byte[] fileData = ms.ToArray();

            GeneralResponse response = await _configurationContract.RestoreFromUpload(fileData, backupFile.FileName);

            if (!response.Flag)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("has-setup")]
        public async Task<IActionResult> HasConfiguration()
        {
            DataResponse<bool> response = await _configurationContract.HasConfiguration();
            return Ok(response);
        }

        [HttpPost("importar/datos")]
        public async Task<IActionResult> ImportarDatosExcel(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Archivo no seleccionado");

            using MemoryStream? ms = new MemoryStream();
            await file.CopyToAsync(ms);
            byte[] fileData = ms.ToArray();

            GeneralResponse response = await _configurationContract.ImportDataFromExcel(fileData, file.FileName);

            if (!response.Flag)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("has-work-saturday")]
        public async Task<IActionResult> ThisWeekHaveSaturdayWork()
        {
           DataResponse<SaturdayBannerConfigDto> response = await _configurationContract.ThisWeekHaveSaturdayWork();

            if (!response.Success) return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("progress")]
        public async Task<IActionResult> ProgressHours()
        {
            DataResponse<ProgressHoursConfigDto> response = await _configurationContract.ProgressHours();

            if (!response.Success) return BadRequest(response);

            return Ok(response);
        }
    }
}
