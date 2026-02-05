using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Reports;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts
{
    public interface IReportContract
    {
        Task<Pageable<List<ListAllReportsDto>>> GetAllReports(PaginationDto pagination, int type);
        Task<DataResponse<CountTotalReports>> GetTotalReports(string? search);
        Task<DataResponse<ListReportDto>> GetReportByUrl(string urlIdentificator);
        Task<GeneralResponse> CreateNewReport(CreateNewReportDto dto);
        Task<GeneralResponse> AddOrQuitFavorite(Guid id, AddOrRemoveFavoriteDto dto);
        Task<byte[]> GeneratePDF(Guid id);
        Task<GeneralResponse> DeleteReport(Guid id);
        Task<byte[]> GenerateExcel(Guid id);
    }
}
