using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Audits;
using AppTiemposV3.SharedClases.Exceptions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;
using static AppTiemposV3.Api.Helpers.IpHelper;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Repositories;

public class AuditRepository : IAuditContract<AuditsResponseDto>
{
    private readonly AppDbContext _dbCxt;
    private readonly IMapper _iMapper;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IGenericContract _genericContract;
    private readonly IUserContract _userContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid _userId => _userContext.GetUserId();

    public AuditRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IGenericContract genericContract, IUserContract userContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbCxt = dbCxt;
        _iMapper = iMapper;
        _userManager = userManager;
        _genericContract = genericContract;
        _userContext = userContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Pageable<List<AuditsResponseDto>>> GetAllAudits(PaginationDtoAdvanced pagination)
    {
        Pageable<List<AuditsResponseDto>> response = await _genericContract.GetAllPaginatedAuditAsync<AuditEntity, AuditsResponseDto>(pagination);
        
        return response;
    }

    public async Task<DataAResponse<AuditUserResponseDto>> GetLastFourAuditsUser()
    {
        UserEntity user = await GetUserByIdAsync(_userId);
        
        List<AuditUserResponseDto> responseEntity = await _dbCxt.Audits
            .Include(au => au.User )
            .Where(u => u.UserId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .Take(4)
            .ProjectTo<AuditUserResponseDto>(_iMapper.ConfigurationProvider)
            .ToListAsync();
        
        return new DataAResponse<AuditUserResponseDto>(true, responseEntity, HttpStatusCode.OK);
    }

    public async Task<DataResponse<AuditKpiResponse>> GetKpis()
    {
        AuditKpiResponse resp = new AuditKpiResponse();
       
        StringBuilder? sb = new StringBuilder();
        
        sb.AppendLine("SELECT");
        sb.AppendLine("    (");
        sb.AppendLine("        SELECT COUNT(A.Id)");
        sb.AppendLine("        FROM auditorias AS A");
        sb.AppendLine("        WHERE A.IsDeleted = 0");
        sb.AppendLine("    ) AS TotalAudits,");
        sb.AppendLine();
        sb.AppendLine("    (");
        sb.AppendLine("        SELECT COUNT(AC.Id)");
        sb.AppendLine("        FROM  activities AS AC");
        sb.AppendLine("        WHERE AC.IsDeleted = 0");
        sb.AppendLine("    ) AS ActivityData,");
        sb.AppendLine();
        sb.AppendLine("    (");
        sb.AppendLine("        SELECT COUNT(T.Id)");
        sb.AppendLine("        FROM trainings AS T");
        sb.AppendLine("        WHERE T.IsDeleted = 0");
        sb.AppendLine("    ) AS TrainingData,");
        sb.AppendLine();
        sb.AppendLine("    (");
        sb.AppendLine("        SELECT COUNT(R.Id)");
        sb.AppendLine("        FROM requeriments AS R");
        sb.AppendLine("        WHERE R.IsDeleted = 0");
        sb.AppendLine("    ) AS RequerimentData,");
        sb.AppendLine();
        sb.AppendLine("    (");
        sb.AppendLine("        SELECT COUNT(RE.Id)");
        sb.AppendLine("        FROM rechazos AS RE");
        sb.AppendLine("        WHERE RE.IsDeleted = 0");
        sb.AppendLine("    ) AS RejectionData");
        
        string sql = sb.ToString();
        
        List<Dictionary<string, object?>> kpiData = await QueryRawAsync(_dbCxt, sql);
        
        foreach (Dictionary<string, object?> row in kpiData)
        {
            resp.TotalAudits = Convert.ToInt32(row["TotalAudits"]!.ToString()!);
            resp.ActivityData = Convert.ToInt32(row["ActivityData"]!.ToString()!);
            resp.TrainingData = Convert.ToInt32(row["TrainingData"]!.ToString()!);
            resp.RequerimentData = Convert.ToInt32(row["RequerimentData"]!.ToString()!);
            resp.RejectionData = Convert.ToInt32(row["RejectionData"]!.ToString()!);
        }
        
        return new DataResponse<AuditKpiResponse>(true, resp, HttpStatusCode.OK);
    }

    public async Task<GeneralResponse> CreateAudit(CreateAuditDto dto)
    {

        UserEntity? user =  await GetUserByIdAsync(_userId);
        
        if (!dto.Changes.Any())
            dto.Changes = new List<AuditChangeDto>();

        AuditEntity auditEntity = new AuditEntity
        {
            UserId = user.Id,
            UserName = dto.UserName,
            Action = dto.Action,
            ActionActivity = dto.ActionActivity,
            Entity = dto.Entity,
            EntityId = dto.EntityId,
            EntityName = dto.EntityName,
            IpAddress = GetClientIp(_httpContextAccessor),
            Changes = dto.Changes
                .Select(c => new AuditChange
                {
                    Field = c.Field,
                    OldValue = c.OldValue,
                    NewValue = c.NewValue
                })
                .ToList(),
            Metadata = dto.Metadata ?? new Dictionary<string, string>()
        };

        await _dbCxt.Audits.AddAsync(auditEntity);

        await EnsureSavedAsync("Hubo un error al crear la auditoria", _dbCxt);

        return new GeneralResponse(true, "Auditoria creada correctamente");
    }
    
    private async Task<UserEntity> GetUserByIdAsync(Guid userId)
    {
        UserEntity? user = await _userManager.FindByIdAsync(userId.ToString());
        return user ?? throw new NotFoundException("El usuario no fue encontrado");
    }
}