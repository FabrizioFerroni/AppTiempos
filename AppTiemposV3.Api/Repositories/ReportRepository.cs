using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.Api.Exports.Excel;
using AppTiemposV3.Api.Exports.PDF;
using AppTiemposV3.Api.Scheduled;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Reports;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.SharedClases.Exceptions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using QuestPDF.Fluent;
using System.Net;
using System.Security;
using System.Text;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;
using static AppTiemposV3.Api.Helpers.MetadataHelper;
using static AppTiemposV3.Api.Utilidades.Helpers;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static System.Globalization.CultureInfo;
using static System.Globalization.NumberStyles;

namespace AppTiemposV3.Api.Repositories
{
    public class ReportRepository : IReportContract
    {
        private readonly AppDbContext _dbCxt;
        private readonly IMapper _iMapper;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IGenericContract _genericContract;
        private readonly IUserContract _userContext;
        private readonly IAuditHelperService _auditHelperService;
        private Guid _userId => _userContext.GetUserId();
        private readonly ILogger<ReportRepository> _logger;
        public ReportRepository(
            AppDbContext dbCxt,
            IMapper iMapper,
            UserManager<UserEntity> userManager,
            IGenericContract genericContract,
            IUserContract userContext,
            IAuditHelperService auditHelperService,
            ILogger<ReportRepository> logger
            )
        {
            _dbCxt = dbCxt;
            _iMapper = iMapper;
            _userManager = userManager;
            _genericContract = genericContract;
            _userContext = userContext;
            _auditHelperService = auditHelperService;
            _logger = logger;
        }

        public async Task<DataResponse<CountTotalReports>> GetTotalReports(string? search)
        {
            CountTotalReports resp = new CountTotalReports();

            UserEntity user = await GetUserByIdAsync(_userId);

            StringBuilder? sb = new StringBuilder();

            sb.AppendLine("SELECT");
            sb.AppendLine("   COUNT(r.Id) AS Alls,");
            sb.AppendLine("   SUM(CASE WHEN r.IsFavorite = 1 THEN 1 ELSE 0 END) AS Favorites,");
            sb.AppendLine("   SUM(CASE WHEN r.IsScheduled = 1 THEN 1 ELSE 0 END) AS Scheduled");
            sb.AppendLine("FROM reportes AS r");
            sb.AppendLine("WHERE r.UserId = @UserId");
            sb.AppendLine("AND r.IsDeleted = 0");

            if (!string.IsNullOrWhiteSpace(search))
            {
                sb.AppendLine("  AND r.Name LIKE @Search");
            }

            string sql = sb.ToString();

            List<MySqlParameter>? parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@UserId", user.Id));

            if (!string.IsNullOrWhiteSpace(search))
            {
                parameters.Add(new MySqlParameter("@Search", $"%{search}%"));
            }

            List<Dictionary<string, object?>> totalData = await QueryRawAsync(_dbCxt, sql, parameters.ToArray());

            foreach (Dictionary<string, object?> row in totalData)
            {
                resp.Alls = row["Alls"] != DBNull.Value ? Convert.ToInt32(row["Alls"]) : 0;
                resp.Favorites = row["Favorites"] != DBNull.Value ? Convert.ToInt32(row["Favorites"]) : 0;
                resp.Scheduled = row["Scheduled"] != DBNull.Value ? Convert.ToInt32(row["Scheduled"]) : 0;
            }


            DataResponse<CountTotalReports> respback = new DataResponse<CountTotalReports>(true, resp, HttpStatusCode.OK);
            return respback;
        }

        public async Task<Pageable<List<ListAllReportsDto>>> GetAllReports(PaginationDto pagination, int type)
        {
            Pageable<List<ReportEntity>> responseData = await _genericContract.GetAllPaginatedReportedAsync<ReportEntity>(pagination, _userId);
            List<ReportEntity> reportData = responseData.Content;

            IEnumerable<ReportEntity> filteredData = type switch
            {
                1 => reportData.Where(r => r.IsFavorite),      
                2 => reportData.Where(r => r.Schedule.Scheduled),      
                _ => reportData                                 
            };

            List<ListAllReportsDto>? mappedData = filteredData.Select(r => new ListAllReportsDto() { 
                Id = r.Id,
                Name = r.Name,
                UrlIdentificator = r.UrlIdentificator,
                Description = r.Description,
                ReportMode = r.ReportMode,
                JoinsCount = r.QueryRequest?.Joins.Count ?? 0,
                Schedule = r.Schedule,
                RunCount = r.RunCount,
                IsFavorite = r.IsFavorite,
                CreatedAt = r.CreatedAt,
                LastRun = r.LastRun,
                QueryRequest = r.QueryRequest,
                QueryRaw = r.QueryRaw
            }).ToList();


            return new Pageable<List<ListAllReportsDto>>
            {
                Content = mappedData,
                TotalPages = responseData.TotalPages, 
                PaginationDetails = responseData.PaginationDetails,
                TotalElements = responseData.TotalElements,
                Last = responseData.Last,
                First = responseData.First
            };
        }

        public async Task<DataResponse<ListReportDto>> GetReportByUrl(string urlIdentificator)
        {
            ReportEntity report = await GetReportByUrlIdentificatorAsync(urlIdentificator);

            await UpdateCountEjecution(report!);

            ListReportDto dto = new ListReportDto();
            int queryResult = 0;

            dto.Id = report.Id;
            dto.Name = report.Name;
            dto.Description = report.Description;
            dto.UrlIdentificator = report.UrlIdentificator;
            dto.ReportMode = report.ReportMode;
            dto.CreatedAt = report.CreatedAt;

            if(report.ReportMode == "custom")
            {
                queryResult = 0;
                if (!string.IsNullOrEmpty(report.QueryRaw))
                {
                    List<Dictionary<string, object?>> totalData = await BuildAndExecuteQueryRaw(report.QueryRaw);
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
                    List<Dictionary<string, object?>> totalData = await BuildAndExecuteQueryResult(report.QueryRequest);
                    dto.DataResult = totalData;
                    
                    if(totalData is not null)
                    {
                        queryResult = totalData.Count;
                    }
                }
            }

            dto.QueryResult = queryResult;

            return new DataResponse<ListReportDto>(true, dto, HttpStatusCode.OK);
        }

        public async Task<GeneralResponse> CreateNewReport(CreateNewReportDto dto)
        {
            UserEntity user = await GetUserByIdAsync(_userId);

            if(dto.QueryRequest is not null)
            {
                if (!IsValidIdentifier(dto.QueryRequest.BaseTable))
                    throw new SecurityException("Nombres de tabla inválido");

                string[]? allowedTables = new[] { "activities", "trainings", "rechazos", "rechazos_detalles", "requirements" };

                if (!allowedTables.Contains(dto.QueryRequest.BaseTable))
                    return new GeneralResponse(false, "Tabla no permitida");

                // Validar que no haya caracteres extraños en nombres de campos
                if (dto.QueryRequest.Fields.Any(f => f.Field.Contains(";--") || f.Field.Contains("/*")))
                    return new GeneralResponse(false, "Estructura de campos inválida");

                foreach (JoinDTO? join in dto.QueryRequest.Joins)
                {
                    if (!IsValidIdentifier(join.Table) || !IsValidIdentifier(join.ParentTable))
                        throw new SecurityException("Nombres de tabla en Join inválidos");

                    // Solo permitir INNER, LEFT, RIGHT JOIN
                    if (!join.Type.ToUpper().EndsWith("JOIN")) throw new SecurityException("Tipo de Join inválido");
                }

                foreach (FilterDTO? filter in dto.QueryRequest.Filters)
                {
                    if (!IsValidIdentifier(filter.Field) || !IsValidIdentifier(filter.Table))
                        throw new Exception("Intento de inyección en identificadores");

                    // Lista blanca de operadores permitidos
                    string[]? allowedOperators = new[] { "=", "!=", "<>", ">", "<", ">=", "<=", "LIKE", "IN", "BETWEEN", "NOT IN" };
                    if (!allowedOperators.Contains(filter.Operator.ToUpper()))
                        throw new Exception("Operador no permitido");

                    if (filter.Value.Contains(";")) throw new SecurityException("Caracteres prohibidos en filtros");
                }
                 string[] _allowedAggregations = { "SUM", "AVG", "COUNT", "MIN", "MAX", "COUNT_DISTINCT" };

                foreach (MetricDTO? metric in dto.QueryRequest.Metrics)
                {
                    if (!_allowedAggregations.Contains(metric.Aggregation.ToUpper()))
                    {
                        throw new SecurityException($"Función de agregación no permitida: {metric.Aggregation}");
                    }

                    string safeLabel = $"`{metric.Label!.Replace("`", "")}`";
                }
            }

            if (!string.IsNullOrEmpty(dto.QueryRaw)){
                ValidateRawSql(dto.QueryRaw);
            }


            //TODO: Hay que validar las consultas por inyección sql
            ReportEntity newReport = _iMapper.Map<ReportEntity>(dto);
            newReport.UserId = user.Id;

            await _dbCxt.Reports.AddAsync(newReport);

            await EnsureSavedAsync("Hubo un error al crear el reporte", _dbCxt);

            try
            {
                await _auditHelperService.CreateAuditAsync(
                    user!.FullName,
                    $"Se creo un nuevo reporte",
                    AuditAction.Created.ToString(),
                    nameof(ReportEntity),
                    "reports",
                    metadata: BuildCreateMetadata(user!.Id, "CREATE")
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new GeneralResponse(true, "Reporte creado correctamente");
        }

        public async Task<GeneralResponse> AddOrQuitFavorite(Guid id, AddOrRemoveFavoriteDto dto)
        {
            ReportEntity report = await GetReportByIdAsync(id);

            report.IsFavorite = dto.Result;

            report.ModifiedAt = DateTime.Now;

            _dbCxt.Entry(report).State = EntityState.Modified;

            await EnsureSavedAsync(
                "Hubo un error al actualizar el conteo del reporte",
                _dbCxt
            );

            return new GeneralResponse(true, $"{(dto.Result ? "Reporte agregado correctamente a favoritos" : "Reporte quitado correctamente de favoritos")}");
        }

        public async Task<byte[]> GeneratePDF(Guid id)
        {
            ReportEntity report = await GetReportByIdAsync(id);

            DataResponse<ListReportDto> reportDto = await GetReportByUrl(report.UrlIdentificator);

            ReportDocumentPDF document = new ReportDocumentPDF(reportDto.Data);

            //document.ShowInCompanion();

            return document.GeneratePdf();
        }

        public async Task<GeneralResponse> DeleteReport(Guid id)
        {
            UserEntity user = await GetUserByIdAsync(_userId);

            ReportEntity report = await GetReportByIdAsync(id);

            report.IsDeleted = true;
            report.DeletedAt = DateTime.Now;

            _dbCxt.Entry(report).State = EntityState.Modified;

            await EnsureSavedAsync(
               "Hubo un error al eliminar el reporte",
               _dbCxt
           );

            try
            {
                await _auditHelperService.CreateAuditAsync(
                    user!.FullName,
                    $"Se elimino el reporte",
                    AuditAction.Deleted.ToString(),
                    nameof(ReportEntity),
                    "reports",
                    metadata: BuildCreateMetadata(user!.Id, "DELETE")
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new GeneralResponse(true, "Reporte eliminado correctamente");
        }

        public async Task<byte[]> GenerateExcel(Guid id)
        {
            ReportEntity report = await GetReportByIdAsync(id);
            DataResponse<ListReportDto> reportDto = await GetReportByUrl(report.UrlIdentificator);

            ReportDocumentXLSX document = new ReportDocumentXLSX(reportDto.Data);

            return document.GenerateExcel();
        }

        /// <summary>
        /// Obtiene un usuario específico por su Id.
        /// </summary>
        /// <param name="userId">El Id del usuario a buscar.</param>
        /// <returns>Retorna el <see cref="UserEntity"/> correspondiente.</returns>
        /// <exception cref="NotFoundException">Se lanza si no se encuentra el usuario.</exception>
        private async Task<UserEntity> GetUserByIdAsync(Guid userId)
        {
            UserEntity? user = await _userManager.FindByIdAsync(userId.ToString());
            return user ?? throw new NotFoundException("El usuario no fue encontrado");
        }

        /// <summary>
        /// Obtiene el reporte específico por su url identificator.
        /// </summary>
        /// <param name="ulrIdentificator">La url identificator del reporte a buscar.</param>
        /// <returns>Retorna el <see cref="ReportEntity"/> correspondiente.</returns>
        /// <exception cref="NotFoundException">Se lanza si no se encuentra el reporte.</exception>
        private async Task<ReportEntity> GetReportByUrlIdentificatorAsync(string ulrIdentificator)
        {
            ReportEntity? report = await _dbCxt.Reports.FirstOrDefaultAsync(r => r.UrlIdentificator == ulrIdentificator && r.UserId == _userId);
            return report ?? throw new NotFoundException("El reporte buscado no fue encontrado");
        }

        /// <summary>
        /// Obtiene el reporte específico por su id.
        /// </summary>
        /// <param name="id">El id del reporte a buscar.</param>
        /// <returns>Retorna el <see cref="ReportEntity"/> correspondiente.</returns>
        /// <exception cref="NotFoundException">Se lanza si no se encuentra el reporte.</exception>
        private async Task<ReportEntity> GetReportByIdAsync(Guid id)
        {
            ReportEntity? report = await _dbCxt.Reports.FirstOrDefaultAsync(r => r.Id == id && r.UserId == _userId);
            return report ?? throw new NotFoundException("El reporte buscado no fue encontrado");
        }

        private async Task UpdateCountEjecution(ReportEntity report)
        {
            report.RunCount++;
            report.LastRun = DateTime.Now;
            report.ModifiedAt = DateTime.Now;

            _dbCxt.Entry(report).State = EntityState.Modified;

            await EnsureSavedAsync(
                "Hubo un error al actualizar el conteo del reporte",
                _dbCxt
            );
        }

        private bool IsValidIdentifier(string name)
        {
            // Solo permite letras, números y guiones bajos. Evita espacios, comillas, guiones medios o puntos.
            return !string.IsNullOrWhiteSpace(name) && name.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        private string RenameTableName(string table)
        {
            return table switch
            {
                "requirements" => "requeriments",
                _ => table
            };
        }

        private async Task<List<Dictionary<string, object?>>> BuildAndExecuteQueryRaw(string queryRaw)
        {
            return await EjecutarQueryDinamica(queryRaw, _dbCxt);           
        }

        private async Task<List<Dictionary<string, object?>>> BuildAndExecuteQueryResult(QueryRequestDTO queryRequest)
        {
            QueryRequestDTO qDto = queryRequest;
            StringBuilder sb = new StringBuilder();
            List<MySqlParameter>? parameters = new List<MySqlParameter>();

            sb.AppendLine("SELECT");

            List<string>? columns = new List<string>();

            if (qDto.Fields?.Any() == true)
            {
                foreach (SelectedFieldDTO? field in qDto.Fields)
                {
                    string tableAlias = BuildAliases(field.Table);
                    string aliasPart = string.IsNullOrWhiteSpace(field.Alias)
                        ? ""
                        : $" AS {(field.Alias.Contains(" ") ? $"`{field.Alias}`" : field.Alias)}";

                    columns.Add($"  {tableAlias}.{field.Field}{aliasPart}");
                }
            }

            if (qDto.Metrics?.Any() == true)
            {
                foreach (MetricDTO? metric in qDto.Metrics)
                {
                    string tableAlias = BuildAliases(RenameTableName(metric.Table));
                    string aggFunc = metric.Aggregation.ToUpper() == "COUNT_DISTINCT"
                        ? "COUNT(DISTINCT "
                        : $"{metric.Aggregation.ToUpper()}(";

                    string labelPart = string.IsNullOrWhiteSpace(metric.Label)
                        ? ""
                        : $" AS {(metric.Label.Contains(" ") ? $"`{metric.Label}`" : metric.Label)}";

                    columns.Add($"  {aggFunc}{tableAlias}.{metric.Field}){labelPart}");
                }
            }

            if (columns.Count > 0)
            {
                sb.AppendLine(string.Join("," + Environment.NewLine, columns));
            }
            else
            {
                sb.AppendLine("  *");
            }

            sb.AppendLine($"FROM {RenameTableName(qDto.BaseTable)} AS {BuildAliases(qDto.BaseTable)}");

            if (qDto.Joins is not null)
            {
                List<JoinDTO> joins = qDto.Joins;

                foreach (JoinDTO join in joins)
                {
                    string tableAlias = BuildAliases(RenameTableName(join.Table));
                    string parentTableAlias = BuildAliases(RenameTableName(join.ParentTable));

                    sb.AppendLine($"{join.Type} {RenameTableName(join.Table)} AS {tableAlias} ON {tableAlias}.{join.TargetField} = {parentTableAlias}.{join.Field}");
                }
            }

            string finalQueryPart = string.Empty;

            if (qDto.Filters is not null && qDto.Filters.Any())
            {
                StringBuilder whereClause = new StringBuilder("WHERE ");
                List<FilterDTO> filters = qDto.Filters;

                for (int i = 0; i < filters.Count; i++)
                {
                    FilterDTO? filter = filters[i];
                    string tableAlias = BuildAliases(filter.Table);

                    if (i > 0)
                    {
                        whereClause.Append($"{Environment.NewLine} AND ");
                    }

                    string formattedValue = FormatSqlValue(filter.Value, filter.Operator);

                    whereClause.Append($"{tableAlias}.{filter.Field} {filter.Operator} {formattedValue}");
                }

                finalQueryPart = whereClause.ToString();
            }

            sb.AppendLine(finalQueryPart);

            if (qDto.GroupBy is not null && qDto.GroupBy.Any() && qDto.Metrics!.Any())
            {
                sb.AppendLine("GROUP BY ");

                List<GroupFieldDTO> groupFields = qDto.GroupBy;

                foreach(GroupFieldDTO? groupField in groupFields)
                {
                    string tableAlias = BuildAliases(RenameTableName(groupField.Table));
                    string columnGroup = $"  {tableAlias}.{groupField.Field}";

                    if (groupField != groupFields.Last())
                    {
                        columnGroup += ",";
                    }

                    sb.AppendLine(columnGroup);
                }
            }

            string sql = sb.ToString();
            return await QueryRawAsync(_dbCxt, sql, parameters.ToArray());
        }

        private string FormatSqlValue(string value, string op)
        {
            if (string.IsNullOrEmpty(value)) return "NULL";

            op = op.ToUpper().Trim();

            if (op == "IN")
            {
                string cleanValue = value.Replace("(", "").Replace(")", "");

                IEnumerable<string>? elements = cleanValue.Split(',')
                    .Select(e => e.Trim())
                    .Select(e => {
                        if (e.StartsWith("'") && e.EndsWith("'")) return e;

                        if (double.TryParse(e, Any, InvariantCulture, out _))
                            return e;

                        return $"'{e}'";
                    });

                return $"({string.Join(", ", elements)})";
            }

            if (op == "BETWEEN") return value;

            if (value.StartsWith("'") && value.EndsWith("'")) return value;

            if (double.TryParse(value, Any, InvariantCulture, out _))
            {
                return value;
            }

            return $"'{value}'";
        }
    }
}
