using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities.ConfigurationTable;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Configurations;
using Microsoft.EntityFrameworkCore;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;

namespace AppTiemposV3.Api.Repositories
{
    public class BackupRepository : IBackupContract
    {
        private readonly AppDbContext _dbCxt;

        public BackupRepository(AppDbContext dbCxt)
        {
            _dbCxt = dbCxt;
        }

        public async Task<List<BackupScheduledJobDto>> GetAllConfigs()
        {
            return await _dbCxt.Configurations
                .Where(bc => bc.ActualConfig == 1 && bc.IsDeleted == false)
                .Select(s => new BackupScheduledJobDto
                {
                    ConfigId = s.Id,
                    UserId = s.UserId,
                    BackupScheduled = new BackupScheduled
                    {
                        AutoBackup = s.AutoBackupEnabled,
                        Frecuencia = s.BackupFrecuencia,
                        Time = s.BackupTime,
                        MaxBackup = s.MaxBackup
                    }
                })
                .ToListAsync();
        }

        public async Task GuardarRegistroBackupenBD(long tamanoEnBytes, string filePath, Guid userId, Guid configId)
        {
            BackupLogsEntity nuevoRegistro = new BackupLogsEntity
            {
                UserId = userId,
                Size = tamanoEnBytes,
                ConfigurationEntityId = configId,
                Type = "Automatico",
                PathToBackup = filePath
            };
            await _dbCxt.BackupLogs.AddAsync(nuevoRegistro);
            await EnsureSavedAsync("Error al guardar el registro de backup en la base de datos.", _dbCxt);
        }
    }
}
