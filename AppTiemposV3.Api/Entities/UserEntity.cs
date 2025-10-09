using System.ComponentModel.DataAnnotations;
using AppTiemposV3.SharedClases.Enums;
using Microsoft.AspNetCore.Identity;

namespace AppTiemposV3.Api.Entities;

public class UserEntity : IdentityUser<Guid>
{
    [Required(ErrorMessage = "El campo Nombre Completo es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El campo Nombre Completo debe tener entre {2} y {1} caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Areas Area { get; set; }

    public DateTime LastPasswordChange { get; set; } = DateTime.Now;


    // Relations

    public ICollection<RequerimentsEntity> Requeriments { get; set; } = new List<RequerimentsEntity>();
    
    public ICollection<ActivitiesEntity> Activities { get; set; } = new List<ActivitiesEntity>();
    
    public ICollection<TrainingEntity> Trainings { get; set; } = new List<TrainingEntity>();
}