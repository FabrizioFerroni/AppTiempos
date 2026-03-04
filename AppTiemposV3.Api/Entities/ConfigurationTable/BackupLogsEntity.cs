using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppTiemposV3.Api.Entities.ConfigurationTable
{
    public class BackupLogsEntity : BaseEntity
    {
        public long Size { get; set; } = 0;
        public string Type { get; set; } = "Manual";
        public string? PathToBackup { get; set; } = string.Empty;
        [Required]
        public Guid ConfigurationEntityId { get; set; }
        [ForeignKey("ConfigurationEntityId")]
        public ConfigurationEntity Configuration { get; set; } = null!;
        [Required] public required Guid UserId { get; set; }
        public UserEntity User { get; set; } = null!;
    }
}
