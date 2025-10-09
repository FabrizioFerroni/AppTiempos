using System.Net;
using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.Api.Utilidades;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Categories;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.Exceptions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Api.Utilidades.GenerateSlug;

namespace AppTiemposV3.Api.Repositories;

public class CategoryRepository : ICategoryContract<CategoryResponseDto>
{
    private readonly AppDbContext _dbCxt;
    private readonly IMapper _iMapper;
    private readonly IGenericContract _genericContract;
    private readonly ILogger<CategoryRepository> _logger;

    public CategoryRepository(AppDbContext dbCxt, IMapper iMapper, IGenericContract genericContract,  ILogger<CategoryRepository> logger)
    {
        _dbCxt = dbCxt;
        _iMapper = iMapper;
        _genericContract = genericContract;
        _logger = logger;
    }
    
    public async Task<DataAResponse<CategoryResponseDto>> GetAllCategories()
    {
        List<CategoryResponseDto> categories = await _dbCxt.Categories
            .OrderBy(o => o.Name)
            .ProjectTo<CategoryResponseDto>(_iMapper.ConfigurationProvider)
            .ToListAsync();

        return new DataAResponse<CategoryResponseDto>(true, categories, HttpStatusCode.OK);
    }

    public async Task<Pageable<List<CategoryResponseDto>>> GetAllCategoriesPag(PaginationDto pagination, string buscarPor)
    {
        return await _genericContract.GetAllPaginatedAsync<CategoriesEntity, CategoryResponseDto>(pagination,
            buscarPor, Guid.Empty);
    }

    public async Task<DataResponse<CategoryResponseDto>> GetCategoryPorId(Guid id)
    {
        CategoriesEntity cat = await GetCategoryByIdAsync(id);

        CategoryResponseDto resCat = _iMapper.Map<CategoryResponseDto>(cat);

        return new DataResponse<CategoryResponseDto> (true, resCat, HttpStatusCode.OK);
    }

    public async Task<DataResponse<CategoryResponseDto>> GetCategoryPorSlug(string slug)
    {
        CategoriesEntity cat = await GetCategoryBySlugAsync(slug);

        CategoryResponseDto resCat = _iMapper.Map<CategoryResponseDto>(cat);

        return new DataResponse<CategoryResponseDto> (true, resCat, HttpStatusCode.OK);
    }

    public async Task<DataResponse<Guid>> GetCategoryIdPorNombre(string nombre)
    {
        string name = nombre.Replace("-", " ");
        CategoriesEntity cat = await GetCategoryByNombreAsync(name);

        CategoryResponseDto resCat = _iMapper.Map<CategoryResponseDto>(cat);

        return new DataResponse<Guid> (true, resCat.Id, HttpStatusCode.OK);
    }

    public async Task<GeneralResponse> CreateCategory(CreateCategoryDto dto)
    {
        try
        {
            if (await CategoryExists(dto.Name))
                throw new BadRequestException("La categoria con ese nombre ya existe en nuestra base de datos");


            CategoriesEntity cat = _iMapper.Map<CategoriesEntity>(dto);

            cat.Slug = URLFriendly(dto.Name);

            await _dbCxt.Categories.AddAsync(cat);

            await EnsureSavedAsync("Hubo un error al crear la categoria");
            
            return new GeneralResponse(true, "Categoria creada correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError("Hubo un error: {Message}", ex.Message);
            throw new InternalServerErrorException("Hubo un error para crear la categoría");
        }
    }

    public async Task<GeneralResponse> UpdateCategory(Guid id, UpdateCategoryDto dto)
    {
       try {
           CategoriesEntity cat = await GetCategoryByIdAsync(id);

           if (!string.IsNullOrWhiteSpace(dto.Name))
           {
               CategoriesEntity? existing = await FindAnyCategoryByName(dto.Name);

               if (existing != null && existing.Id != id)
               {
                   if (existing.IsDeleted)
                   {
                       throw new BadRequestException(
                           $"El nombre '{dto.Name}' ya fue utilizado en una categoría eliminada. " +
                           $"Podés restaurarla usando su ID: {existing.Id}");
                   }
                   else
                   {
                       throw new BadRequestException("La categoría con ese nombre ya existe.");
                   }
               }

               cat.Name = dto.Name;
               cat.Slug = URLFriendly(dto.Name);
           }

           _iMapper.Map(dto, cat);
           cat.ModifiedAt = DateTime.Now;

           await EnsureSavedAsync("Hubo un error al actualizar la categoría.");
           
           return new GeneralResponse(true, "Se actualizó la categoría con éxito.");
       }
       catch (Exception ex)
       {
            _logger.LogError("Hubo un error: {Message}", ex.Message);
            throw new InternalServerErrorException("Hubo un error para crear la categoría");
       }
       
    }

    public async Task<GeneralResponse> DeleteCategory(Guid id)
    {
        CategoriesEntity cat = await GetCategoryByIdAsync(id);

        cat.IsDeleted = true;
        cat.DeletedAt = DateTime.Now;

        await EnsureSavedAsync("Hubo un error al eliminar la categoria. Intente mas tarde");

        return new GeneralResponse(true, "Se elimino con exito la categoria");
    }
    
    public async Task<GeneralResponse> RestoreCategory(Guid id)
    {
        CategoriesEntity cat = await GetCategoryByIdAsync(id, true);

        cat.IsDeleted = false;
        cat.ModifiedAt = DateTime.Now;
        cat.DeletedAt = null;

        await EnsureSavedAsync("Hubo un error al restaurar la categoria. Intente mas tarde");

        return new GeneralResponse(true, "Se restauro con exito la categoria");
    }
    
    private async Task<bool> CategoryExists(string name)
    {
        return await _dbCxt.Categories.AnyAsync(r => r.Name == name && r.IsDeleted == false); //TODO: Debiera de devolver los existentes si is deleted es falso ( creo que ya esta resuelto )
    }
    
    private async Task<CategoriesEntity?> FindAnyCategoryByName(string name)
    {
        return await _dbCxt.Categories
            .IgnoreQueryFilters() // Por si tenés filtro global de soft delete
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    /*private async Task<CategoriesEntity> GetCategoryByIdAsync(Guid id, bool includeDeleted = false)
    {
        CategoriesEntity? category;
        if (includeDeleted)
        {
            category = await _dbCxt.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted == true);
        }
        else
        {
            category = await _dbCxt.Categories
                .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted == false);
        }

        return category ?? throw new NotFoundException("Categoria no encontrada");
    }*/
    
    private async Task<CategoriesEntity> GetCategoryByIdAsync(Guid id, bool includeDeleted = false)
    {
        IQueryable<CategoriesEntity> query = _dbCxt.Categories;

        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        CategoriesEntity? category = await query.FirstOrDefaultAsync(r => r.Id == id);

        return category ?? throw new NotFoundException("Categoría no encontrada");
    }
    
    private async Task<CategoriesEntity> GetCategoryBySlugAsync(string slug)
    {
        CategoriesEntity? category = await _dbCxt.Categories
            .FirstOrDefaultAsync(r => r.Slug == slug);

        return category ?? throw new NotFoundException("Categoria no encontrada");
    }
    
    private async Task<CategoriesEntity> GetCategoryByNombreAsync(string nombre)
    {
        CategoriesEntity? category = await _dbCxt.Categories
            .FirstOrDefaultAsync(r => r.Name == nombre);

        return category ?? throw new NotFoundException("Categoria no encontrada");
    }

    private async Task EnsureSavedAsync(string errorMessage)
    {
        int result = await _dbCxt.SaveChangesAsync();
        if (result <= 0)
            throw new InternalServerErrorException(errorMessage);
    }
}