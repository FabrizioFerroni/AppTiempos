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
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Repositories
{
    public class RequerimentRepository : IRequerimentContract<RequerimentResponseDto>
    {
        private readonly AppDbContext _dbCxt;
        private readonly IMapper _iMapper;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IGenericContract _genericContract;

        public RequerimentRepository(AppDbContext dbCxt, IMapper iMapper, UserManager<UserEntity> userManager, IGenericContract genericContract)
        {
            _dbCxt = dbCxt;
            _iMapper = iMapper;
            _userManager = userManager;
            _genericContract = genericContract;
        }

        public async Task<GeneralResponse> CreateRequeriment(CreateRequerimentDto dto, Guid userId)
        {
            if (await RequerimentExists(dto.ReqID, userId))
                throw new BadRequestException("El requerimiento con ese ReqId ya existe para tu usuario");


            RequerimentsEntity req = _iMapper.Map<RequerimentsEntity>(dto);
            req.UserId = userId;
            
            await _dbCxt.Requeriments.AddAsync(req);

            await EnsureSavedAsync("Hubo un error al crear el requerimiento");

            return new GeneralResponse(true, "Requerimiento creado correctamente");
        }

        public async Task<GeneralResponse> DeleteRequeriment(Guid id, Guid userId)
        {
            UserEntity user = await GetUserByIdAsync(userId);

            RequerimentsEntity req = await GetRequerimentByIdAsync(id, user.Id);

            req.IsDeleted = true;
            req.DeletedAt = DateTime.Now;

            await EnsureSavedAsync("Hubo problemas para eliminar el registro");

            return new GeneralResponse(true, "Requerimiento eliminado con éxito");
        }
        
        public async Task<GeneralResponse> RestoreRequeriment(Guid id,  Guid userId)
        {
            UserEntity user = await GetUserByIdAsync(userId);

            RequerimentsEntity req = await GetRequerimentByIdAsync(id, user.Id);

            req.IsDeleted = false;
            req.ModifiedAt = DateTime.Now;
            req.DeletedAt = null;

            await EnsureSavedAsync("Hubo problemas para restaurar el registro");

            return new GeneralResponse(true, "Requerimiento restaurado con éxito");
        }

        public async Task<DataAResponse<RequerimentResponseDto>> GetAllRequeriments(Guid userId)
        {
            UserEntity user = await GetUserByIdAsync(userId);

            List<RequerimentResponseDto> requeriments = await _dbCxt.Requeriments
                                        .Where(u => u.UserId == user.Id)
                                        .OrderByDescending(o => o.CreatedAt)
                                        .ProjectTo<RequerimentResponseDto>(_iMapper.ConfigurationProvider)
                                        .ToListAsync();


            return new DataAResponse<RequerimentResponseDto>(true, requeriments, HttpStatusCode.OK);
        }

        public async Task<Pageable<List<RequerimentResponseDto>>> GetAllRequerimentsPag(PaginationDto pagination, string buscarPor, Guid userId)
        {

            return await _genericContract.GetAllPaginatedAsync<RequerimentsEntity, RequerimentResponseDto>(pagination,
                buscarPor, userId);
        }

        public async Task<DataResponse<RequerimentResponseDto>> GetRequerimentporId(Guid id, Guid userId)
        {
            UserEntity user = await GetUserByIdAsync(userId);

            RequerimentsEntity req = await GetRequerimentByIdAsync(id, user.Id);

            RequerimentResponseDto resReq = _iMapper.Map<RequerimentResponseDto>(req);

            return new DataResponse<RequerimentResponseDto> (true, resReq, HttpStatusCode.OK);
        }

        public async Task<DataResponse<RequerimentResponseDto>> GetRequerimentporReqId(string reqId, Guid userId)
        {
            UserEntity user = await GetUserByIdAsync(userId);

            RequerimentsEntity? req = await _dbCxt.Requeriments.FirstOrDefaultAsync(r => r.ReqID == reqId && r.UserId == user.Id);


            if(req is null)
                throw new NotFoundException("Requerimiento no encontrado");

            RequerimentResponseDto? resReq = _iMapper.Map<RequerimentResponseDto>(req);

            return new DataResponse<RequerimentResponseDto>(true, resReq, HttpStatusCode.OK);
        }

        public async Task<GeneralResponse> UpdateRequeriment(Guid id, UpdateRequerimentDto dto, Guid userId)
        {
            UserEntity user = await GetUserByIdAsync(userId);

            RequerimentsEntity req = await GetRequerimentByIdAsync(id, user.Id);

            if (await RequerimentExists(dto.ReqID!, user.Id))
                throw new BadRequestException("El requerimiento con ese ReqId ya existe para tu usuario"); //TODO: ver esto
            
            _iMapper.Map(dto, req);
            
            req.Id = id;
            req.UserId = user.Id;
            req.ModifiedAt = DateTime.Now;

            await EnsureSavedAsync("Hubo un error al actualizar el req. Intente mas tarde");


            return new GeneralResponse(true, "Se actualizo con exito el req");
        }

        private async Task<bool> RequerimentExists(string reqId, Guid userId)
        {
            return await _dbCxt.Requeriments.AnyAsync(r => r.ReqID == reqId && r.UserId == userId);
        }

        private async Task<UserEntity> GetUserByIdAsync(Guid userId)
        {
            UserEntity? user = await _userManager.FindByIdAsync(userId.ToString());
            return user ?? throw new NotFoundException("El usuario no fue encontrado");
        }

        private async Task<RequerimentsEntity> GetRequerimentByIdAsync(Guid id, Guid userId)
        {
            RequerimentsEntity? requeriment = await _dbCxt.Requeriments
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            return requeriment ?? throw new NotFoundException("Requerimiento no encontrado");
        }

        private async Task EnsureSavedAsync(string errorMessage)
        {
            int result = await _dbCxt.SaveChangesAsync();
            if (result <= 0)
                throw new InternalServerErrorException(errorMessage);
        }
    }
}
