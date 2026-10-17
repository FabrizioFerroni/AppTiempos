using AppTiemposV3.SharedClases.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTiemposV3.SharedClases.DTOs.Activities
{
    public class RequerimentDataActivity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public Etapas EtapaActual { get; set; } = Etapas.Alta;
    }
}
