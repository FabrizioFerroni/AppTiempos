using System.Linq.Expressions;
using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.Exceptions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Reflection;
using AppTiemposV3.Api.Utilidades;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.Enums;
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
        private Guid _userId => _userContext.GetUserId(); //TODO: Luego cambiar esto a repository

        public RequerimentRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IGenericContract genericContract, IUserContract userContext)
        {
            _dbCxt = dbCxt;
            _iMapper = iMapper;
            _userManager = userManager;
            _genericContract = genericContract;
            _userContext = userContext;
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


            RequerimentsEntity req = _iMapper.Map<RequerimentsEntity>(dto);
            req.UserId = _userId;
            req.FolderId = await FolderIdIdentity(req.UserId);
            req.Estado = Estados.Pendiente;
            req.EtapaActual = Etapas.Alta;
            
            await _dbCxt.Requeriments.AddAsync(req);

            await EnsureSavedAsync("Hubo un error al crear el requerimiento");

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

            return new GeneralResponse(true, "Requerimiento restaurado con éxito");
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
                .Where(r => r.UserId == userId)
                .MaxAsync(r => (int?)r.FolderId) ?? 0;
            
            return maxFolderId + 1;
        }
    }
}
