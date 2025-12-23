using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Audits;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.SharedClases.Exceptions;
using AppTiemposV3.SharedClases.Utilidades.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Net;
using System.Text;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;
using static AppTiemposV3.Api.Helpers.Helpers;
using static AppTiemposV3.Api.Helpers.MetadataHelper;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;


namespace AppTiemposV3.Api.Repositories
{
    public class RequerimentRepository : IRequerimentContract<RequerimentResponseDto>
    {
        private readonly AppDbContext _dbCxt;
        private readonly IMapper _iMapper;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IGenericContract _genericContract;
        private readonly IUserContract _userContext;
        private readonly IEntityIdProvider _entityIdProvider;
        private readonly IAuditHelperService _auditHelperService;
        private readonly string[] _allowedCategoryNames =
        {
            "Nuevo",
            "Nuevo (Alta prioridad)",
            "Incidente critico",
            "Incidente no critico",
            "Nueva configuración",
            "Nueva configuración con prueba"
        };
        private Guid _userId => _userContext.GetUserId(); //TODO: Luego cambiar esto a repository

        public RequerimentRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IGenericContract genericContract, IUserContract userContext, IEntityIdProvider entityIdProvider, IAuditHelperService auditHelperService)
        {
            _dbCxt = dbCxt;
            _iMapper = iMapper;
            _userManager = userManager;
            _genericContract = genericContract;
            _userContext = userContext;
            _entityIdProvider = entityIdProvider;
            _auditHelperService = auditHelperService;

        }

        /// <summary>
        /// Crea un nuevo requerimiento para un usuario específico.
        /// </summary>
        /// <param name="dto">Objeto con los datos del nuevo requerimiento.</param>
        /// <returns>Retorna un <see cref="GeneralResponse"/> indicando el éxito de la creación.</returns>
        /// <exception cref="BadRequestException">Se lanza si ya existe un requerimiento con el mismo ReqID para el usuario.</exception>
        /// <exception cref="InternalServerErrorException">Se lanza si ocurre un error al guardar los cambios en la base de datos.</exception>
        public async Task<GeneralResponse> CreateRequeriment(CreateRequerimentDto dto)
        {
            if (await RequerimentExists(dto.ReqID, _userId, null))
                throw new BadRequestException("El requerimiento con ese ReqId ya existe para tu usuario");

            UserEntity? user = await GetUserByIdAsync(_userId);

            RequerimentsEntity req = _iMapper.Map<RequerimentsEntity>(dto);
            req.UserId = _userId;
            
            List<Guid> allowedCategoryGuids  = await _dbCxt.Categories
                .Where(c => _allowedCategoryNames.Contains(c.Name)) 
                .Select(c => c.Id)
                .ToListAsync();
            
            if (allowedCategoryGuids.Contains(dto.CategoryId))
            {
                req.FolderId = await FolderIdIdentity(user.Id);
            }
            else
            {
                req.FolderId = null;
            }
            
            req.Estado = Estados.Pendiente;
            req.EtapaActual = Etapas.Alta;
            req.ConjuntoCambios = null;
            
            await _dbCxt.Requeriments.AddAsync(req);

            await EnsureSavedAsync("Hubo un error al crear el requerimiento");

            try
            {
                await _auditHelperService.CreateAuditAsync(
                    user!.FullName,
                    $"Se creo un nuevo requerimiento con ReqID{req.ReqID}",
                    AuditAction.Created.ToString(),
                    nameof(RequerimentsEntity),
                    "requeriments",
                    BuildChanges(req),
                    BuildCreateMetadata(user!.Id, "CREATE")
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new GeneralResponse(true, "Requerimiento creado correctamente");
        }

        /// <summary>
        /// Marca un requerimiento como eliminado para un usuario específico.
        /// </summary>
        /// <param name="id">El Id del requerimiento a eliminar.</param>
        /// <returns>Retorna un <see cref="GeneralResponse"/> indicando el éxito de la eliminación.</returns>
        /// <exception cref="NotFoundException">Se lanza si no se encuentra el usuario o el requerimiento.</exception>
        /// <exception cref="InternalServerErrorException">Se lanza si ocurre un error al guardar los cambios en la base de datos.</exception>
        public async Task<GeneralResponse> DeleteRequeriment(Guid id)
        {
            UserEntity user = await GetUserByIdAsync(_userId);

            RequerimentsEntity req = await GetRequerimentByIdAsync(id, user.Id);

            req.IsDeleted = true;
            req.DeletedAt = DateTime.Now;
            
            
            _dbCxt.Entry(req).State = EntityState.Modified;

            await EnsureSavedAsync("Hubo problemas para eliminar el registro");

            try
            {
                await _auditHelperService.CreateAuditAsync(
                    user!.FullName,
                    $"Se elimino el requerimiento para el ReqID{req.ReqID}",
                    AuditAction.Deleted.ToString(),
                    nameof(RequerimentsEntity),
                    "requeriments",
                    metadata: BuildCreateMetadata(user!.Id, "DELETE")
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new GeneralResponse(true, "Requerimiento eliminado con éxito");
        }
        
        /// <summary>
        /// Restaura un requerimiento previamente marcado como eliminado para un usuario específico.
        /// </summary>
        /// <param name="id">El Id del requerimiento a restaurar.</param>
        /// <returns>Retorna un <see cref="GeneralResponse"/> indicando el éxito de la restauración.</returns>
        /// <exception cref="NotFoundException">Se lanza si no se encuentra el usuario o el requerimiento.</exception>
        /// <exception cref="InternalServerErrorException">Se lanza si ocurre un error al guardar los cambios en la base de datos.</exception>
        public async Task<GeneralResponse> RestoreRequeriment(Guid id)
        {
            UserEntity user = await GetUserByIdAsync(_userId);

            RequerimentsEntity req = await GetRequerimentByIdAsync(id, user.Id);

            req.IsDeleted = false;
            req.ModifiedAt = DateTime.Now;
            req.DeletedAt = null;

            await EnsureSavedAsync("Hubo problemas para restaurar el registro");

            try
            {
                await _auditHelperService.CreateAuditAsync(
                    user!.FullName,
                    $"Se restauro el requerimiento para el ReqID{req.ReqID}",
                    AuditAction.Restored.ToString(),
                    nameof(RequerimentsEntity),
                    "requeriments",
                    metadata: BuildCreateMetadata(user!.Id, "RESTORE")
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new GeneralResponse(true, "Requerimiento restaurado con éxito");
        }

        public async Task<DataResponse<Guid>> GetIdByReqId(string reqId)
        {
            if (reqId.StartsWith("ReqID"))
            {
                reqId =  reqId.Replace("ReqID", "");
            }
        
            RequerimentsEntity? req = await _dbCxt.Requeriments
                .FirstOrDefaultAsync(r => r.ReqID == reqId && r.UserId == _userId);
        
            if (req == null)
                throw new NotFoundException("No requeriment found");


            RequerimentResponseDto resCat = _iMapper.Map<RequerimentResponseDto>(req);

            return new DataResponse<Guid> (true, resCat.Id, HttpStatusCode.OK);
        }

        /// <summary>
        /// Obtiene todos los requerimientos de un usuario específico, ordenados por fecha de creación descendente.
        /// </summary>
        /// <returns>Retorna un <see cref="DataAResponse{RequerimentResponseDto}"/> con la lista de requerimientos del usuario.</returns>
        public async Task<DataAResponse<RequerimentResponseDto>> GetAllRequeriments()
        {
            //TODO: Ver esto
            UserEntity user = await GetUserByIdAsync(_userId);

            List<RequerimentResponseDto> requeriments = await _dbCxt.Requeriments
                                        .Where(u => u.UserId == user.Id)
                                        .OrderByDescending(o => o.CreatedAt)
                                        .ProjectTo<RequerimentResponseDto>(_iMapper.ConfigurationProvider)
                                        .ToListAsync();


            return new DataAResponse<RequerimentResponseDto>(true, requeriments, HttpStatusCode.OK);
        }

        /// <summary>
        /// Obtiene una lista paginada de requerimientos de un usuario, con opción de filtrar por categoría u otros campos.
        /// </summary>
        /// <param name="pagination">Objeto que contiene la información de paginación y búsqueda.</param>
        /// <param name="buscarPor">El campo por el cual se desea filtrar la búsqueda (por ejemplo, "categoryid").</param>
        /// <returns>Retorna un <see cref="Pageable{List{RequerimentResponseDto}}"/> con los requerimientos filtrados y paginados.</returns>
        public async Task<Pageable<List<RequerimentResponseDto>>> GetAllRequerimentsPag(PaginationDto pagination, string buscarPor)
        {
            // TODO: Ver esto
            if (buscarPor == "categoryid")
            {
                CategoriesEntity? category = await _dbCxt.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower().Contains(pagination!.Search!.ToLower()));

                if (category is null)
                {
                    pagination.Search = Guid.Empty.ToString();
                }
                else
                {
                   pagination.Search = category!.Id.ToString();
                }
            }
            
            return await _genericContract.GetAllPaginatedAsync<RequerimentsEntity, RequerimentResponseDto>(pagination,
                buscarPor, _userId);
        }

        /// <summary>
        /// Obtiene un requerimiento de un usuario específico por su Id, incluyendo la última etapa de su actividad.
        /// </summary>
        /// <param name="id">El Id del requerimiento a buscar.</param>
        /// <returns>Retorna un <see cref="DataResponse{RequerimentResponseDto}"/> con los datos del requerimiento y su última etapa.</returns>
        /// <exception cref="NotFoundException">Se lanza si no se encuentra el usuario o el requerimiento.</exception>
        public async Task<DataResponse<RequerimentResponseDto>> GetRequerimentporId(Guid id)
        {
            UserEntity user = await GetUserByIdAsync(_userId);

            RequerimentsEntity req = await GetRequerimentByIdAsync(id, user.Id);
            
            double totalWorked = _dbCxt.Activities
                .Where(a => !a.IsDeleted)
                .Where(a => a.RequerimentId == req.Id)
                .AsEnumerable() 
                .Sum(a =>
                    {
                        TimeSpan start = a.StartTime.ToTimeSpan();
                        TimeSpan end = a.EndTime?.ToTimeSpan() ?? start;
                        return (end - start).TotalSeconds;
                    }
                );

            TimeSpan totalTime = TimeSpan.FromSeconds(totalWorked);
            req.WorkedTime = totalTime;


            RequerimentResponseDto resReq = _iMapper.Map<RequerimentResponseDto>(req);

            resReq.Etapa = await GetEtapaActivity(req.Id);
            resReq.TotalRejections = await GetRejections(req.Id, user.Id);

            return new DataResponse<RequerimentResponseDto> (true, resReq, HttpStatusCode.OK);
        }

        /// <summary>
        /// Obtiene un requerimiento de un usuario específico por su ReqID.
        /// </summary>
        /// <param name="reqId">El ReqID del requerimiento a buscar.</param>
        /// <returns>Retorna un <see cref="DataResponse{RequerimentResponseDto}"/> con los datos del requerimiento.</returns>
        /// <exception cref="NotFoundException">Se lanza si no se encuentra el requerimiento.</exception>
        public async Task<DataResponse<RequerimentResponseDto>> GetRequerimentporReqId(string reqId)
        {
            UserEntity user = await GetUserByIdAsync(_userId);

            RequerimentsEntity? req = await _dbCxt.Requeriments.FirstOrDefaultAsync(r => r.ReqID == reqId && r.UserId == user.Id);


            if(req is null)
                throw new NotFoundException("Requerimiento no encontrado");

            RequerimentResponseDto? resReq = _iMapper.Map<RequerimentResponseDto>(req);

            return new DataResponse<RequerimentResponseDto>(true, resReq, HttpStatusCode.OK);
        }

        /// <summary>
        /// Actualiza un requerimiento existente de un usuario con los datos proporcionados.
        /// </summary>
        /// <param name="id">El Id del requerimiento a actualizar.</param>
        /// <param name="dto">Objeto con los datos de actualización del requerimiento.</param>
        /// <returns>Retorna un <see cref="GeneralResponse"/> indicando el éxito de la operación.</returns>
        /// <exception cref="NotFoundException">Se lanza si no se encuentra el usuario o el requerimiento.</exception>
        /// <exception cref="BadRequestException">Se lanza si el ReqID ya existe para el usuario.</exception>
        /// <exception cref="InternalServerErrorException">Se lanza si ocurre un error al guardar los cambios en la base de datos.</exception>
        public async Task<GeneralResponse> UpdateRequeriment(Guid id, UpdateRequerimentDto dto)
        {
            UserEntity user = await GetUserByIdAsync(_userId);

            RequerimentsEntity req = await GetRequerimentByIdAsync(id, user.Id);
            RequerimentsEntity oldReq = await GetRequerimentByIdAsync(id, user.Id);

            if (await RequerimentExists(dto.ReqID!, user.Id, id))
                throw new BadRequestException("El requerimiento con ese ReqId ya existe para tu usuario"); //TODO: ver esto

            if (dto.Estado == Estados.Finalizado && req.EtapaActual != Etapas.Validacion)
            {
                throw new BadRequestException("No se puede finalizar el requerimiento sin antes tener un estado de validación.");
            }
            
            _iMapper.Map(dto, req);
            
            req.Id = id;
            req.UserId = user.Id;
            req.ModifiedAt = DateTime.Now;
            
            _dbCxt.Entry(req).State = EntityState.Modified;

            await EnsureSavedAsync("Hubo un error al actualizar el req. Intente mas tarde");

            try
            {
                await _auditHelperService.CreateAuditAsync(
                    user!.FullName,
                    $"Se edito un requerimiento para el ReqID{req.ReqID}",
                    AuditAction.Updated.ToString(),
                    nameof(RequerimentsEntity),
                    "requeriments",
                    BuildChanges(req, oldReq, dto),
                    BuildCreateMetadata(user!.Id, "UPDATE")
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new GeneralResponse(true, "Se actualizo con exito el req");
        }
        
        /// <summary>
        /// Verifica si existe un requerimiento con un ReqID específico para un usuario, 
        /// opcionalmente excluyendo un requerimiento por su Id.
        /// </summary>
        /// <param name="reqId">El ReqID del requerimiento a buscar.</param>
        /// <param name="userId">El Id del usuario propietario del requerimiento.</param>
        /// <param name="excludeId">Id de un requerimiento a excluir de la búsqueda (opcional).</param>
        /// <returns>Retorna <c>true</c> si existe un requerimiento que cumpla las condiciones; de lo contrario, <c>false</c>.</returns>
        private async Task<bool> RequerimentExists(string reqId, Guid userId, Guid? excludeId = null)
        {
            return await _dbCxt.Requeriments.AnyAsync(r =>
                r.ReqID == reqId &&
                r.UserId == userId &&
                (excludeId == null || r.Id != excludeId));
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
        /// Obtiene un requerimiento específico de un usuario, incluyendo su categoría y actividades.
        /// </summary>
        /// <param name="id">El Id del requerimiento a buscar.</param>
        /// <param name="userId">El Id del usuario propietario del requerimiento.</param>
        /// <returns>Retorna el <see cref="RequerimentsEntity"/> correspondiente.</returns>
        /// <exception cref="NotFoundException">Se lanza si no se encuentra el requerimiento.</exception>
        private async Task<RequerimentsEntity> GetRequerimentByIdAsync(Guid id, Guid userId)
        {
            RequerimentsEntity? requeriment = await _dbCxt.Requeriments
                .Include(c => c.Category)
                .Include(a => a.Activities)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            return requeriment ?? throw new NotFoundException("Requerimiento no encontrado");
        }

        /// <summary>
        /// Intenta guardar los cambios pendientes en el contexto de la base de datos.
        /// </summary>
        /// <param name="errorMessage">Mensaje de error a lanzar si no se guardan cambios.</param>
        /// <exception cref="InternalServerErrorException">
        /// Se lanza si no se guardan cambios en la base de datos.
        /// </exception>
        private async Task EnsureSavedAsync(string errorMessage)
        {
            int result = await _dbCxt.SaveChangesAsync();
            if (result <= 0)
                throw new InternalServerErrorException(errorMessage);
        }

        /// <summary>
        /// Obtiene la última etapa de una actividad asociada a un requerimiento específico.
        /// </summary>
        /// <param name="requerimentId">El Id del requerimiento para el cual se busca la última actividad.</param>
        /// <returns>
        /// Retorna la etapa (<see cref="Etapas"/>) de la última actividad registrada. 
        /// Si no existen actividades, retorna <see cref="Etapas.Alta"/> como valor por defecto.
        /// </returns>
        private async Task<Etapas> GetEtapaActivity(Guid requerimentId)
        {
            ActivitiesEntity? ultimaActividad = await _dbCxt.Activities
                .Where(a => a.RequerimentId == requerimentId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (ultimaActividad is null)
            {
                return Etapas.Alta;
            }

            return ultimaActividad.Etapa;
        }
        
        /// <summary>
        /// Calcula el siguiente FolderId disponible para un usuario específico.
        /// </summary>
        /// <param name="userId">El Id del usuario para el cual se busca el FolderId.</param>
        /// <returns>Retorna un entero que representa el siguiente FolderId disponible.</returns>
        private async Task<int> FolderIdIdentity(Guid userId)
        {
            int maxFolderId = await _dbCxt.Requeriments
                .Where(r => r.UserId == userId && r.FolderId != null)
                .MaxAsync(r => (int?)r.FolderId) ?? 0;
            
            return maxFolderId + 1;
        }

        private async Task<int> GetRejections(Guid requerimentId, Guid userId)
        {
            int totalRejections = 0;

            StringBuilder? sb = new StringBuilder();
            
            sb.AppendLine("SELECT");
            sb.AppendLine("  IFNULL(R.TotalRejections, 0) AS TotalRejections");
            sb.AppendLine("FROM rechazos AS R");
            sb.AppendLine("WHERE R.RequerimentId = @RequerimentId");
            sb.AppendLine("  AND R.UserId = @UserId");
            sb.AppendLine("  AND R.IsDeleted = 0");
            
            string sql = sb.ToString();
        
            MySqlParameter requerimentFiltro = new MySqlParameter("@RequerimentId", requerimentId);
            MySqlParameter userFiltro = new MySqlParameter("@UserId", userId);
        
            List<Dictionary<string, object?>> sqlResponse = await QueryRawAsync(_dbCxt, sql, requerimentFiltro, userFiltro);

            foreach (Dictionary<string, object?> row in sqlResponse)
            {
                totalRejections = Convert.ToInt32(row["TotalRejections"]!.ToString()!);
            }

            return totalRejections;
        }
        
        private static List<AuditChangeDto> BuildChanges(
            RequerimentsEntity newReq,
            RequerimentsEntity? oldReq = null,
            UpdateRequerimentDto? dto = null)
        {
            List<AuditChangeDto> changes = new();

            if (oldReq is null)
            {
                AddCreate(nameof(newReq.ReqID), newReq.ReqID);
                AddCreate(nameof(newReq.Titulo), newReq.Titulo);
                AddCreate(nameof(newReq.Cliente), newReq.Cliente);
                AddCreate(nameof(newReq.Url), newReq.Url);
                string[]? newCambios = newReq.ConjuntoCambios;
                if(newCambios is { Length: > 0 })
                {
                    AddCreate(nameof(newReq.ConjuntoCambios), string.Join(", ", newCambios));
                }
                AddCreate(nameof(newReq.Estado), newReq.Estado);
                AddCreate(nameof(newReq.EtapaActual), newReq.EtapaActual);
                return changes;
            }

            if (dto is not null)
            {
                if (!string.IsNullOrWhiteSpace(dto.ReqID) &&
                    dto.ReqID != oldReq.ReqID)
                    AddUpdate(nameof(newReq.ReqID), oldReq.ReqID, newReq.ReqID);

                if (!string.IsNullOrWhiteSpace(dto.Titulo) &&
                    dto.Titulo != oldReq.Titulo)
                    AddUpdate(nameof(newReq.Titulo), oldReq.Titulo, newReq.Titulo);
                
                if (!string.IsNullOrWhiteSpace(dto.StoryPoint) &&
                    dto.StoryPoint != oldReq.StoryPoint)
                    AddUpdate(nameof(newReq.StoryPoint), oldReq.StoryPoint, newReq.StoryPoint);
                
                if (!string.IsNullOrWhiteSpace(dto.Url) &&
                    dto.Url != oldReq.Url)
                    AddUpdate(nameof(newReq.Url), oldReq.Url, newReq.Url);
                
                if (dto.CategoryId != Guid.Empty &&
                    dto.CategoryId != oldReq.CategoryId)
                    AddUpdate(nameof(newReq.CategoryId), oldReq.CategoryId, newReq.CategoryId);
                
                if (!string.IsNullOrWhiteSpace(dto.Descripcion) &&
                    dto.Descripcion != oldReq.Descripcion)
                    AddUpdate(nameof(newReq.Descripcion), oldReq.Descripcion, newReq.Descripcion);

                if (!string.IsNullOrWhiteSpace(dto.Cliente) &&
                    dto.Cliente != oldReq.Cliente)
                    AddUpdate(nameof(newReq.Cliente), oldReq.Cliente, newReq.Cliente);

                string[]? oldCambios = oldReq.ConjuntoCambios;
                string[]? newCambios = newReq.ConjuntoCambios;

                if (oldReq.ConjuntoCambios != null)
                {
                    if (AreDifferent(oldCambios, newCambios))
                    {
                        AddUpdate(
                            nameof(newReq.ConjuntoCambios),
                            oldCambios is { Length: > 0 } ? string.Join(", ", oldCambios) : null,
                            newCambios is { Length: > 0 } ? string.Join(", ", newCambios) : null
                        );
                    }
                }
                else
                {
                    AddCreate(nameof(newReq.ConjuntoCambios), NormalizeValue(newReq.ConjuntoCambios));  
                }
                
                if (dto.Estado.HasValue &&
                    dto.Estado.Value != oldReq.Estado)
                    AddUpdate(nameof(newReq.Estado), oldReq.Estado, newReq.Estado);

                if (dto.EtapaActual.HasValue &&
                    dto.EtapaActual.Value != oldReq.EtapaActual)
                    AddUpdate(nameof(newReq.EtapaActual), oldReq.EtapaActual, newReq.EtapaActual);
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
        
        private static bool AreDifferent(string[]? oldValue, string[]? newValue)
        {
            if (oldValue == null && newValue == null)
                return false;

            if (oldValue == null || newValue == null)
                return true;

            return !oldValue
                .OrderBy(x => x)
                .SequenceEqual(newValue.OrderBy(x => x));
        }
    }
}
