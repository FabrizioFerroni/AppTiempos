using AppTiemposV3.SharedClases.DTOs.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTiemposV3.SharedClases.Contracts
{
    public interface IBackupContract
    {
        Task<List<BackupScheduledJobDto>> GetAllConfigs();
        Task GuardarRegistroBackupenBD(long tamanoEnBytes, string filePath, Guid userId, Guid configId);
    }
}
