using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTiemposV3.SharedClases.DTOs.Configurations
{
    public class BackupScheduledJobDto
    {
        public Guid ConfigId { get; set; }
        public Guid UserId { get; set; }
        public BackupScheduled BackupScheduled { get; set; } = new BackupScheduled();
    }
}
