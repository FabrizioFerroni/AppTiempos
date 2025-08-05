using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
namespace AppTiemposV3.Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/categories")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ICategoryContract<CategoryResponseDto> _categoryContract;

    public CategoryController(ICategoryContract<CategoryResponseDto> categoryContract)
    {
        _categoryContract = categoryContract;
    }
    
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllCategoriesPaginados([FromQuery] PaginationDto pagination, [FromQuery]  string buscarPor = "Name")
    {
        Pageable<List<CategoryResponseDto>> response = await _categoryContract.GetAllCategoriesPag(pagination, buscarPor);
        return Ok(response);
    }
    
    [HttpGet("todos")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllCategories()
    {
        DataAResponse<CategoryResponseDto> response = await _categoryContract.GetAllCategories();
        return Ok(response);
    }

    [HttpGet("c/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        DataResponse<CategoryResponseDto> response = await _categoryContract.GetCategoryPorId(id);
        return Ok(response);
    }
    
    [HttpGet("{slug}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetByCategorySlug(string slug)
    {
        DataResponse<CategoryResponseDto> response = await _categoryContract.GetCategoryPorSlug(slug);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        GeneralResponse  response = await _categoryContract.CreateCategory(dto);
        return StatusCode(201, response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        GeneralResponse response = await _categoryContract.UpdateCategory(id, dto);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        GeneralResponse response = await _categoryContract.DeleteCategory(id);
        return Ok(response);
    }
    
    [HttpPost("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> RestoreCategory(Guid id)
    {
        GeneralResponse response = await _categoryContract.RestoreCategory(id);
        return Ok(response);
    }
}