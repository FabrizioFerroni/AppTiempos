using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Audits;
using AppTiemposV3.SharedClases.DTOs.Testers;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.SharedClases.Exceptions;
using AppTiemposV3.SharedClases.Utilidades.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;
using static AppTiemposV3.Api.Helpers.Helpers;
using static AppTiemposV3.Api.Helpers.MetadataHelper;
using static AppTiemposV3.Api.Helpers.UserHelper;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Repositories;

public class TesterRepository : ITesterContract<TesterResponseDto>
{
    private readonly AppDbContext _dbCxt;
    private readonly IMapper _iMapper;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IGenericContract _genericContract;
    private readonly IUserContract _userContext;
    private readonly IEntityIdProvider _entityIdProvider;
    private readonly IAuditHelperService _auditHelperService;
    private Guid _userId => _userContext.GetUserId();

    public TesterRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IGenericContract genericContract, IUserContract userContext, IEntityIdProvider entityIdProvider, IAuditHelperService auditHelperService)
    {
        _dbCxt = dbCxt;
        _iMapper = iMapper;
        _userManager = userManager;
        _genericContract = genericContract;
        _userContext = userContext;
        _entityIdProvider = entityIdProvider;
        _auditHelperService = auditHelperService;
    }

    public async Task<GeneralResponse> CreateTester(CreateTesterDto dto)
    {
        if(await TesterExists(dto.Nombre))
            throw new BadRequestException("Hay una tester con ese nombre ya registrado en nuestra base de datos");

        TesterEntity tester = _iMapper.Map<TesterEntity>(dto);

        _dbCxt.Testers.Add(tester);
        
        await EnsureSavedAsync("Hubo un problema para crear el registro", _dbCxt);

        try
        {
            UserEntity user = await GetUserByIdAsync(_userManager, _userId);

            await _auditHelperService.CreateAuditAsync(
                user!.FullName,
                $"Se creó el tester {tester.Nombre}",
                AuditAction.Created.ToString(),
                nameof(TesterEntity),
                "testers",
                BuildChanges(tester),
                BuildCreateMetadata(user.Id, "CREATE")
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new GeneralResponse(true, "Tester creado con éxito");
    }

    public async Task<GeneralResponse> DeleteTester(Guid id)
    {
        TesterEntity tester = await GetTesterByIdAsync(id);

        tester.IsDeleted = true;
        tester.DeletedAt = DateTime.Now;

        _dbCxt.Entry(tester).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo un problema para eliminar el registro", _dbCxt);

        try
        {
            UserEntity user = await GetUserByIdAsync(_userManager, _userId);
            await _auditHelperService.CreateAuditAsync(
                user!.FullName,
                $"Se elimino el tester {tester.Nombre}",
                AuditAction.Deleted.ToString(),
                nameof(TesterEntity),
                "testers",
                BuildChanges(tester),
                BuildCreateMetadata(user.Id, "DELETE")
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new GeneralResponse(true, "Tester eliminado con éxito");
    }

    public async Task<DataAResponse<TesterResponseDto>> GetAllTesters()
    {
        List<TesterResponseDto> testers = await _dbCxt.Testers
                                            .Where(t => t.Activo == true)
                                            .OrderBy(o => o.Nombre)
                                            .ProjectTo<TesterResponseDto>(_iMapper.ConfigurationProvider)
                                            .ToListAsync();

        return new DataAResponse<TesterResponseDto>(true, testers, HttpStatusCode.OK);
    }

    public async Task<Pageable<List<TesterResponseDto>>> GetAllTestersPaginated(PaginationDtoAdvanced pagination)
    {
        Pageable<List<TesterResponseDto>> response = await _genericContract.GetAllPaginatedFaAsync<TesterEntity, TesterResponseDto>(pagination, null);

        return response;
    }

    public async Task<DataResponse<TesterResponseDto>> GetTesterPorId(Guid id)
    {
        TesterEntity tester = await GetTesterByIdAsync(id);

        TesterResponseDto resTest = _iMapper.Map<TesterResponseDto>(tester);

        return new DataResponse<TesterResponseDto>(true, resTest, HttpStatusCode.OK);

    }

    public async Task<DataResponse<TesterResponseDto>> GetTesterPorNombre(string nombre)
    {
        IQueryable<TesterEntity> query = _dbCxt.Testers;

        query = query.Where(t => t.Nombre == nombre && t.Activo == true);

        TesterEntity? tester = await query.FirstOrDefaultAsync();

        TesterResponseDto resTest = _iMapper.Map<TesterResponseDto>(tester);

        return new DataResponse<TesterResponseDto>(true, resTest, HttpStatusCode.OK);

    }

    public async Task<GeneralResponse> RestoreTester(Guid id)
    {
        TesterEntity tester = await GetTesterByIdAsync(id, true);

        tester.IsDeleted = false;
        tester.ModifiedAt = DateTime.Now;
        tester.DeletedAt = null;

        _dbCxt.Entry(tester).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo un problema para restaurar el registro", _dbCxt);

        try
        {
            UserEntity user = await GetUserByIdAsync(_userManager, _userId);

            await _auditHelperService.CreateAuditAsync(
                user!.FullName,
                $"Se restauro el tester {tester.Nombre}",
                AuditAction.Restored.ToString(),
                nameof(TesterEntity),
                "testers",
                BuildChanges(tester),
                BuildCreateMetadata(user.Id, "RESTORE")
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new GeneralResponse(true, "Tester restaurado con éxito");
    }

    public async Task<GeneralResponse> UpdateTester(Guid id, UpdateTesterDto dto)
    {
        TesterEntity tester = await GetTesterByIdAsync(id);
        TesterEntity oldTester = await GetTesterByIdAsync(id);

        if (await TesterExists(dto.Nombre, id))
            throw new BadRequestException("Hay una tester con ese nombre ya registrado en nuestra base de datos");

        _iMapper.Map(dto, tester);

        tester.Id = id;
        tester.ModifiedAt = DateTime.Now;

        _dbCxt.Entry(tester).State = EntityState.Modified;

        await EnsureSavedAsync("Hubo un problema para actualizar el registro", _dbCxt);

        try
        {
            UserEntity user = await GetUserByIdAsync(_userManager, _userId);

            await _auditHelperService.CreateAuditAsync(
                user!.FullName,
                $"Se actualizó el tester {oldTester.Nombre}",
                AuditAction.Updated.ToString(),
                nameof(TesterEntity),
                "testers",
                BuildChanges(tester),
                BuildCreateMetadata(user.Id, "UPDATE")
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new GeneralResponse(true, "Tester actualizado con éxito");
    }

    /// <summary>
    /// Verifica si existe un tester en la base de datos,
    /// opcionalmente excluyendo un tester por su Id.
    /// </summary>
    /// <param name="nombre">Nombre de un tester para saber si existe.</param>
    /// <param name="excludeId">Id de un tester a excluir de la búsqueda (opcional).</param>
    /// <returns>Retorna <c>true</c> si existe un tester que cumpla las condiciones; de lo contrario, <c>false</c>.</returns>
    private async Task<bool> TesterExists(string nombre, Guid? excludeId = null)
    {
        return await _dbCxt.Testers.AnyAsync(r => r.Nombre == nombre && (excludeId == null || r.Id != excludeId));
    }

    /// <summary>
    /// Obtiene un tester específico
    /// </summary>
    /// <param name="id">El Id del tester a buscar.</param>
    /// <param name="includeDeleted">Si es true, incluye registros eliminados lógicamente.</param>
    /// <returns>Retorna la entidad <see cref="TesterEntity"/> correspondiente.</returns>
    /// <exception cref="NotFoundException">Se lanza si no se encuentra el tester.</exception>
    private async Task<TesterEntity> GetTesterByIdAsync(Guid id, bool includeDeleted = false)
    {
        IQueryable<TesterEntity> query = _dbCxt.Testers;

        // Si quiero incluir los eliminados, ignoro los filtros globales
        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        // Filtro por ID y activo
        query = query.Where(t => t.Id == id && t.Activo == true);

        TesterEntity? tester = await query.FirstOrDefaultAsync();

        return tester ?? throw new NotFoundException("Tester no encontrado");
    }

    private static List<AuditChangeDto> BuildChanges(
            TesterEntity newTester,
            TesterEntity? oldTester = null,
            UpdateTesterDto? dto = null)
    {
        List<AuditChangeDto> changes = new();

        if (oldTester is null)
        {
            AddCreate(nameof(newTester.Nombre), newTester.Nombre);
            AddCreate(nameof(newTester.Activo), newTester.Activo);
            return changes;
        }

        if (dto is not null)
        {

            if (dto.Activo &&
                dto.Activo != oldTester.Activo)
                AddUpdate(nameof(newTester.Activo), oldTester.Activo, newTester.Activo);

            if (!string.IsNullOrWhiteSpace(dto.Nombre) &&
                dto.Nombre != oldTester.Nombre)
                AddUpdate(nameof(newTester.Nombre), oldTester.Nombre, newTester.Nombre);
        }

        return changes;

        void AddCreate(string field, object? value)
        {
            changes.Add(new AuditChangeDto
            {
                Field = field,
                NewValue = NormalizeValue(value?.ToString())
            });
        }

        void AddUpdate(string field, object? oldValue, object? newValue)
        {
            string? oldVal = oldValue?.ToString();
            string? newVal = newValue?.ToString();

            if (oldVal != newVal)
            {
                changes.Add(new AuditChangeDto
                {
                    Field = field,
                    OldValue = NormalizeValue(oldValue),
                    NewValue = NormalizeValue(newValue)
                });
            }
        }
    }
}
