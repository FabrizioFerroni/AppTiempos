using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.Requeriments
{
    public class CreateRequerimentDto : RequerimentDto
    {
        [Required]
        [StringLength(15)]
        public required string ReqID { get; set; } = string.Empty;

        [Required]
        public required string Titulo { get; set; } = string.Empty;

        [Required]
        public required string Cliente { get; set; } = string.Empty;

        [Url]
        public string? Url { get; set; } = string.Empty;


        public string CambioInput { get; set; } = "";
        public string[]? ConjuntoCambios { get; set; }

        public void AddCambioFromInput()
        {
            if (string.IsNullOrWhiteSpace(CambioInput))
                return;

            ConjuntoCambios ??= [];
            ConjuntoCambios = ConjuntoCambios.Append(CambioInput).ToArray();

            CambioInput = "";
        }

        public void AddCambio(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            ConjuntoCambios = ConjuntoCambios!
                .Append(value.Trim())
                .Distinct()
                .ToArray();
        }

        public void RemoveCambio(string value)
        {
            ConjuntoCambios = ConjuntoCambios?
                .Where(x => x != value)
                .ToArray();
        }

    }
}
