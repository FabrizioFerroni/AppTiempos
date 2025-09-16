using AppTiemposV3.SharedClases.Contracts;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using AppTiemposV3.SharedClases.GenericModels;
using Microsoft.AspNetCore.Mvc;

namespace AppTiemposV3.Api.Controllers;

[Route("api/generics")]
[ApiController]
public class GenericController : ControllerBase
{
    private readonly IGenericSContract<ColorModel> _service;

    public GenericController(IGenericSContract<ColorModel> service)
    {
        _service = service;
    }
    
    [HttpGet("colors")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetColors()
    {
        DataAResponse<ColorModel> response = await _service.GetAllColors();
        return Ok(response.Data);
    }
}