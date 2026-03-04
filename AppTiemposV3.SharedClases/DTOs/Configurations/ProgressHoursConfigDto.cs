using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTiemposV3.SharedClases.DTOs.Configurations
{
    public class ProgressHoursConfigDto
    {
        public string HorasRealizadas { get; set; } = string.Empty;
        public double HorasRealizadasDbl { get; set; } = 0;
        public double MetaDelDia { get; set; }
        public int Porcentaje { get; set; }
        public string HorasFaltantes { get; set; } = string.Empty;
    }
}
