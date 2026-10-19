using AppTiemposV3.SharedClases.Enums;

namespace AppTiemposV3.SharedClases.Policies
{
    public static class EtapaPolicy
    {
        private static readonly Dictionary<Areas, Etapas> DefaultPorArea = new()
        {
            [Areas.Implementacion] = Etapas.Alta,
            [Areas.Administracion] = Etapas.Alta,
            [Areas.Externos] = Etapas.Alta,
            [Areas.Gerencia] = Etapas.Alta,
            [Areas.RecursosHumanos] = Etapas.Alta,
            [Areas.Desarrollo] = Etapas.Ejecucion,
            [Areas.SoporteTecnico] = Etapas.Evaluacion,
            [Areas.Testing] = Etapas.Prueba,
        };

        public static Etapas ObtenerDefault(Areas area, bool esIncidente)
        {
            if (area == Areas.Desarrollo)
                return esIncidente ? Etapas.Evaluacion : Etapas.Ejecucion;

            return DefaultPorArea.GetValueOrDefault(area, Etapas.Alta);
        }
    }
}
