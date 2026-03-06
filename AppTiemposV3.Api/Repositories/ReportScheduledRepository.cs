using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.Api.Exports.PDF;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Reports;
using AppTiemposV3.SharedClases.Exceptions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using static AppTiemposV3.Api.Helpers.ReportHelper;
using QuestPDF.Fluent;

namespace AppTiemposV3.Api.Repositories
{
    public class ReportScheduledRepository : IReportScheduledContract
    {
        private readonly AppDbContext _dbCxt;
        private readonly IMapper _iMapper;
        private readonly IAuditHelperService _auditHelperService;
        private readonly IConfiguration _config;

        public ReportScheduledRepository(AppDbContext dbContext, IMapper iMapper, IAuditHelperService auditHelper, IConfiguration config)
        {
            _dbCxt = dbContext;
            _iMapper = iMapper;
            _auditHelperService = auditHelper;
            _config = config;
        }

        public async Task<byte[]> GeneratePDFScheduled(string urlIdentificator, Guid userId)
        {
            ListReportDto reportDto = await GetReportByUrl(urlIdentificator, userId);

            ReportDocumentPDF document = new ReportDocumentPDF(reportDto, _config);

            return document.GeneratePdf();
        }

        public async Task<List<ReportScheduledDto>> GetAllScheduledReports()
        {
            List<ReportEntity> reports = await _dbCxt.Reports
                .AsNoTracking()
                .Where(r => r.IsScheduled && !r.IsDeleted)
                .ToListAsync();

            return reports.Select(r => new ReportScheduledDto
            {
                Id = r.Id,
                Name = r.Name,
                IsScheduled = r.IsScheduled,
                Frecuency = r.Schedule?.Frecuency,
                Destinations = r.Schedule?.Destinations,
                FullName = r.Schedule?.Destinations?.Select(GetNameFromEmail).ToList(),
                UserId = r.UserId,
                UrlId = r.UrlIdentificator
            }).ToList();
        }

        private static string GetNameFromEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            int idx = value.IndexOf('@');
            return idx > 0 ? value[..idx] : value;
        }

        private async Task<ListReportDto> GetReportByUrl(string urlIdentificator, Guid userId)
        {
            ReportEntity report = await GetReportByUrlIdentificatorAsync(urlIdentificator, userId);

            ListReportDto dto = new ListReportDto();
            int queryResult = 0;

            dto.Id = report.Id;
            dto.Name = report.Name;
            dto.Description = report.Description;
            dto.UrlIdentificator = report.UrlIdentificator;
            dto.ReportMode = report.ReportMode;
            dto.CreatedAt = report.CreatedAt;

            if (report.ReportMode == "custom")
            {
                queryResult = 0;
                if (!string.IsNullOrEmpty(report.QueryRaw))
                {
                    List<Dictionary<string, object?>> totalData = await BuildAndExecuteQueryRaw(report.QueryRaw, _dbCxt);
                    dto.DataResult = totalData;
                    if (totalData is not null)
                    {
                        queryResult = totalData.Count;
                    }
                }

            }
            else
            {
                queryResult = 0;
                if (report.QueryRequest is not null)
                {
                    List<Dictionary<string, object?>> totalData = await BuildAndExecuteQueryResult(report.QueryRequest, _dbCxt);
                    dto.DataResult = totalData;

                    if (totalData is not null)
                    {
                        queryResult = totalData.Count;
                    }
                }
            }

            dto.QueryResult = queryResult;

            return dto;
        }


        /// <summary>
        /// Obtiene el reporte específico por su url identificator.
        /// </summary>
        /// <param name="ulrIdentificator">La url identificator del reporte a buscar.</param>
        /// <param name="userId">El id del usuario del reporte a buscar, ya que este metodo debe ser publico sin login para los envios programados.</param>
        /// <returns>Retorna el <see cref="ReportEntity"/> correspondiente.</returns>
        /// <exception cref="NotFoundException">Se lanza si no se encuentra el reporte.</exception>
        private async Task<ReportEntity> GetReportByUrlIdentificatorAsync(string ulrIdentificator, Guid userId)
        {
            ReportEntity? report = await _dbCxt.Reports.FirstOrDefaultAsync(r => r.UrlIdentificator == ulrIdentificator && r.UserId == userId);
            return report ?? throw new NotFoundException("El reporte buscado no fue encontrado");
        }
    }
}
