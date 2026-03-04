using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppTiemposV3.Api.Entities.ConfigurationTable
{
    public class WorkingSaturdayEntity : BaseEntity
    {
        public DateOnly Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        [NotMapped]
        public double Hours => (EndTime - StartTime).TotalHours;
        [Required]
        public Guid ConfigurationEntityId { get; set; }
        [ForeignKey("ConfigurationEntityId")]
        public ConfigurationEntity Configuration { get; set; } = null!;
    }
}
