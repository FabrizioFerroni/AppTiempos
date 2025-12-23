using AppTiemposV3.SharedClases.Enums;

namespace AppTiemposV3.SharedClases.DTOs.Requeriments
{
    public class UpdateRequerimentDto
    {
        public Guid Id { get; set; }
        public string? ReqID { get; set; }
        public string? Titulo { get; set; }
        public string? Cliente { get; set; }
        public string? StoryPoint { get; set; }
        public string? Url { get; set; }
        public string? Descripcion { get; set; }
        public Guid CategoryId { get; set; }
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

        /*public string? ConjuntoCambiosText
        {
            get => ConjuntoCambios == null ? null : string.Join(",", ConjuntoCambios);
            set => ConjuntoCambios = string.IsNullOrWhiteSpace(value)
                ? null
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }*/
        
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

        public Estados? Estado  { get; set; }
        public Etapas? EtapaActual { get; set; }
    }
}
