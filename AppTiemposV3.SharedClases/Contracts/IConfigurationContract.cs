using AppTiemposV3.SharedClases.DTOs.Configurations;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts
{
    public interface IConfigurationContract
    {
        Task<DataResponse<ListActualConfig>> GetConfiguration();
        Task<GeneralResponse> CreateConfig(CreateConfigurationDto dto);
        Task<GeneralResponse> UpdateConfig(UpdateConfigDto dto);
        Task<GeneralResponse> ResetConfig();
        Task<Stream?> DownloadBackup();
        Task<DataResponse<AutoBackup>> GetLastManualBackup();
        Task<DataResponse<List<AutoBackup>>> GetAutoBackupsHistory();
        Task<DataResponse<int>> GetTotalAutomaticBackups();
        Task<Stream?> DownloadFileBackup(Guid id);
        Task<GeneralResponse> RestoreBackupFromFileServer(Guid id);
        Task<GeneralResponse> RestoreFromUpload(byte[] fileBytes, string fileName);
        Task<DataResponse<bool>> HasConfiguration();
        Task<GeneralResponse> ImportDataFromExcel(byte[] fileBytes, string? fileName);
        Task<DataResponse<SaturdayBannerConfigDto>> ThisWeekHaveSaturdayWork();
        Task<DataResponse<ProgressHoursConfigDto>> ProgressHours();
    }
}
