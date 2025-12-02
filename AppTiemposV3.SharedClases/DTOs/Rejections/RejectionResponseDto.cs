using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.DTOs.RejectionDetails;
using AppTiemposV3.SharedClases.Enums;
using static AppTiemposV3.SharedClases.Utilidades.SanitizeText;

namespace AppTiemposV3.SharedClases.DTOs.Rejections;

public class RejectionResponseDto
{
    public Guid Id { get; set; } = Guid.Empty;
    
    [StringLength(10)]
    public string? UrlIndetificator { get; set; }
    
    public int TotalRejections { get; set; } = 0;

    public string Status { get; set; } = "pending";

    public bool IsResolve { get; set; } = false;

    public DateTime? ResolvedDate { get; set; } = null;
    
    public RequerimentDtoRej Requeriment { get; set; } = new RequerimentDtoRej();
    
    public UserDtoARej Usuario { get; set; } = new UserDtoARej();
    
    public ICollection<RejectionDetailResponseDto> RejectionsDetails { get; set; } = new List<RejectionDetailResponseDto>();
}


public class RequerimentDtoRej
{
    public Guid Id { get; set; }
    public string ReqID { get; set; } = string.Empty;
    public string ReqIDDesc => "ReqID" + ReqID;
    public string Titulo { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string StoryPoint { get; set; } = string.Empty;
    public string? Url { get; set; } = string.Empty;
    public string? Descripcion {  get; set; } = string.Empty;
    public Estados? Estado { get; set; } = null;
    public string? ConjuntoCambios { get; set; } = null;
    public CategoryDtoResRej Category { get; set; } = new CategoryDtoResRej();
    private int _folderId;
    [JsonIgnore]
    public int FolderId
    {
        set => _folderId = value;
    }
    public string? FolderPath => _folderId + " - " + ReqID + " - " + SanitizeTitulo(Titulo);
}


public class UserDtoARej
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;
}

public class CategoryDtoResRej
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public string Color { get; set; } = string.Empty;
}