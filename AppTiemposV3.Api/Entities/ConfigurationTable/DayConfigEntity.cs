using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppTiemposV3.Api.Entities.ConfigurationTable
{
    public class DayConfigEntity : BaseEntity
    {
        public int Day { get; set; }
        public string DayName { get; set; } = string.Empty;
        public double MinHours { get; set; }
        public double MaxHours { get; set; }
        public bool Enabled { get; set; }
        [Required]
        public Guid ConfigurationEntityId { get; set; }
        [ForeignKey("ConfigurationEntityId")]
        public ConfigurationEntity Configuration { get; set; } = null!;
    }
}
