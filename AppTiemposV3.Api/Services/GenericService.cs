using System.Net;
using System.Text.Json;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.Exceptions;
using AppTiemposV3.SharedClases.GenericModels;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static System.IO.File;

namespace AppTiemposV3.Api.Services;

public class GenericService : IGenericSContract<ColorModel>
{
    public async Task<DataAResponse<ColorModel>> GetAllColors()
    {
        string? json = await ReadAllTextAsync("Files/colors.json");
        
        List<ColorModel>? colors = JsonSerializer.Deserialize<List<ColorModel>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (colors == null)
        {
            throw new NotFoundException("Colors not found");
        }
            
        return new DataAResponse<ColorModel>(true, colors, HttpStatusCode.Found);
    }
}