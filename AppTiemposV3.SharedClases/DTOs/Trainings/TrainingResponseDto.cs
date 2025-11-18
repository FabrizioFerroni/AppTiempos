using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static AppTiemposV3.SharedClases.Utilidades.SanitizeText;

namespace AppTiemposV3.SharedClases.DTOs.Trainings;

public class TrainingResponseDto
{
    public Guid Id { get; set; } = Guid.Empty;
    
    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; }
    
    public string StartTime { get; set; }
    
    public string? EndTime { get; set; } = null;

    
    public string Capacitator { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public bool IsLoaded { get; set; } = false;
    
    public string Status{ get; set; } = "En Progreso";

    public string? Notes { get; set; } = null;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TimeElapsed => EndTime is not null
        ? (TimeOnly.Parse(EndTime) - TimeOnly.Parse(StartTime)).ToString("hh\\:mm")
        : null;
    
    public RequerimentDtoT Requeriment { get; set; } = new RequerimentDtoT();
    
    public TimeSpan CapacitationTime { get; set; }
    
    public UserDtoT Usuario { get; set; } = new UserDtoT();
}

public class RequerimentDtoT
{
    public Guid Id { get; set; }
    
    public string ReqID { get; set; } = string.Empty;

    public string ReqIDDesc => "ReqID" + ReqID;

    public string Titulo { get; set; } = string.Empty;

    public string Cliente { get; set; } = string.Empty;

    public string StoryPoint { get; set; } = string.Empty;

    public string? Url { get; set; } = string.Empty;

    public string? Descripcion {  get; set; } = string.Empty;
        
    public string? ConjuntoCambios { get; set; } = null;
    
    private int _folderId;
        
    [JsonIgnore]
    public int FolderId
    {
        set => _folderId = value;
    }
        
    public string? FolderPath => _folderId + " - " + ReqID + " - " + SanitizeTitulo(Titulo);
}

public class UserDtoT
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;
}