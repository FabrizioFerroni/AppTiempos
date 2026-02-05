using AppTiemposV3.SharedClases.DTOs.Reports;

namespace AppTiemposV3.SharedClases.Contracts
{
    public interface IReportScheduledContract
    {
        Task<List<ReportScheduledDto>> GetAllScheduledReports();
        Task<byte[]> GeneratePDFScheduled(string urlIdentificator, Guid userId);
    }
}
