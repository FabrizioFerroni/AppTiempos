using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
using static AppTiemposV3.SharedClases.Utilidades.SanitizeText;

namespace AppTiemposV3.SharedClases.DTOs.Activities;

public enum TipoA
{
    Informes,
    MigracionPlugins,
    MigracionWeb
}

public class ActivityResponseDto
{
    public Guid Id { get; set; } = Guid.Empty;
    
    public string UrlIndetificator { get; set; } = string.Empty;
    
    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; }
    
    [DataType(DataType.Time)]
    public TimeOnly StartTime { get; set; }
    
    [DataType(DataType.Time)]
    public TimeOnly? EndTime { get; set; } = null;
    
    public string Description { get; set; } = string.Empty;
    
    public bool IsLoaded { get; set; } = false;
    
    public string StatusMessage { get; set; } = "En Progreso";

    public string? Comment { get; set; } = null;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TimeSpan? TimeElapsed => EndTime.HasValue ? EndTime.Value - StartTime : null;
    
    public RequerimentDtoA Requeriment { get; set; } = new RequerimentDtoA();
    
    public CategoryDtoA Category { get; set; } = new CategoryDtoA();
    
    public UserDtoA Usuario { get; set; } = new UserDtoA();
}

public class RequerimentDtoA
{
    public Guid Id { get; set; }
    
    public string ReqID { get; set; } = string.Empty;

    public string ReqIDDesc => "ReqID" + ReqID;

    public string Titulo { get; set; } = string.Empty;

    public string Cliente { get; set; } = string.Empty;

    public string StoryPoint { get; set; } = string.Empty;

    public string? Url { get; set; } = string.Empty;

    public string? Descripcion {  get; set; } = string.Empty;
    
    public TipoA? Tipo { get; set; } = null;
        
    public string? ConjuntoCambios { get; set; } = null;
    
    private int _folderId;
        
    [JsonIgnore]
    public int FolderId
    {
        set => _folderId = value;
    }
        
    public string? FolderPath => _folderId + " - " + ReqID + " - " + SanitizeTitulo(Titulo);
}


public class CategoryDtoA
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public string Color { get; set; } = string.Empty;
}

public class UserDtoA
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;
}