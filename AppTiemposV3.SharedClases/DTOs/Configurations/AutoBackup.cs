using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTiemposV3.SharedClases.DTOs.Configurations
{
    public class AutoBackup
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public long Size { get; set; }
        public string? PathToBackup { get; set; }
    }
}
