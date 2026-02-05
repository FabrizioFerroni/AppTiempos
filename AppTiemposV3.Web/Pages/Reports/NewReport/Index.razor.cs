using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Reports;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.Web.Components.Icons;
using AppTiemposV3.Web.Components.UI;
using AppTiemposV3.Web.Services;
using AppTiemposV3.Web.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;
using static AppTiemposV3.Web.Utils.ReportMetadata;

namespace AppTiemposV3.Web.Pages.Reports.NewReport
{
    //cambiar de lado
    public class ReportFilter
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string? Tabla { get; set; }
        public string? Campo { get; set; }
        public string? Operador { get; set; } = "Igual a";
        public string? Valor { get; set; }
        public List<string> OptionsCampos { get; set; } = new();
    }

    public class ReportMetrics
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string? Etiqueta { get; set; }
        public string? Tabla { get; set; }
        public string? Campo { get; set; }
        public string? Agregacion { get; set; } = "Contar";
        public string? Formato { get; set; } = "Número";
        public List<string> OptionsCampos { get; set; } = new();
    }

    public record SelectedField(
        ReportTable Table,
        string Field
    );

    public readonly record struct SelectedFieldKey(
        ReportTable Table,
        string Field
    );

    public partial class Index : ComponentBase, IDisposable
    {
        #region  Variables
        #region InyeccionDependencias
        [Inject] LayoutState State { get; set; } = null!;
        [Inject] private IJSRuntime? JS { get; set; }
        [Inject] private NavigationManager? Router { get; set; }
        [Inject] private NotificationService Toltip { get; set; } = default!;
        [Inject] private ColorService ColorService { get; set; } = null!;
        [Inject] private IReportContract ReportsService { get; set; } = default!;
        #endregion

        private CreateNewReportDto createNewReport = new();
        private bool IsSendingReport = false;
        private bool IsSelectClosed = true;
        private bool IsSelectClosed2 = true;
        private bool SchecheduledProgramed = false;
        private bool SchecheduledProgramedCustom = false;
        private string tab = "joins";
        private string tabVisual = "sql";
        private string _reportMode = "visual";
        private StringBuilder queryVisual = new();
        private StringBuilder queryCustom = new();

        private List<ReportFilter> Filters = new();
        private List<ReportMetrics> Metrics = new();

        private List<string> OptionsBasesDatos =
                TablesUi
                    .Select(t => t.Title)
                    .Where(title =>
                            !string.Equals(title, "Usuarios", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(title, "Categorías", StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .ToList();

        private List<string> OptionsFrecuencia = new() { "Diario", "Semanal (Lunes)", "Mensual (Dia 1)"};
        private List<string> OptionsFrecuenciaCustom = new() { "Diario", "Semanal (Lunes)", "Mensual (Dia 1)"};
        private List<string> OptionsOperador = new() { "Igual a", "Diferente de", "Contiene", "Mayor que", "Mayor igual que", "Menor que", "Menor igual que", "Entre", "En lista", "No en la lista" };
        private List<string> OptionsAgregacion = new() { "Contar", "Suma", "Promedio", "Minimo", "Maximo", "Unicos" };
        private List<string> OptionsFormato = new() { "Numero", "Moneda", "Porcentaje", "Horas", "Textos", "Fechas" };
        private List<string> Recipients = new() { "" };
        private List<string> RecipientsCustom = new() { "" };
        private string? BasesDatosSeleccionado = "";
        // Guardamos la combinación de Tabla y Campo (el Value, no el Label)
        private List<(string Tabla, string Campo)> _groupedFields = new();

        //private List<string> OptionsBasesDatosFilter = new();
        private List<string> OptionsBasesDatosFilter
        {
            get
            {
                List<string>? lista = new List<string>();
                // 1. Agregar la tabla base
                if (!string.IsNullOrEmpty(BasesDatosSeleccionado))
                    lista.Add(BasesDatosSeleccionado);

                // 2. Agregar todas las tablas de los Joins activos
                IEnumerable<string>? joins = ActiveJoins.Select(j => NameToSpanish(j.Table.ToString()));
                lista.AddRange(joins);

                return lista.Distinct().ToList();
            }
        }
        private string? FrecuenciaSeleccionada = "";
        private string? FrecuenciaSeleccionadaCustom = "";
        private readonly HashSet<ReportTable> ExpandedTables = new();
        private readonly HashSet<string> ExpandedPrincipalTables = new();
        private readonly HashSet<ReportTable> ExpandedRelacionadasTables = new();
        private readonly HashSet<ReportTable> ExpandedRelacionadasRelTables = new();
        private List<JoinDefinition> ActiveJoins = new();
        private List<JoinDefinition> ActiveJoinsRel = new();
        private readonly Dictionary<SelectedField, string?> _selectedFields = new();
        private readonly List<JoinDefinition> _activeJoins = new();
        private static readonly Dictionary<ReportTable, string> _aliases = BuildAliases();
        private Dictionary<string, (string Mensaje, bool EsExitoso)> mensajes = new();
        private Dictionary<string, int> resultCopy = new();
        private IEnumerable<JoinDefinition> GetAvailableJoins()
        => GetJoins(GetTableRelations(BasesDatosSeleccionado!))
           .Where(j => !ActiveJoins.Any(a => a.Table == j.Table));

        #endregion

        #region Inicializacion
        protected override async Task OnInitializedAsync()
        {
            ColorService.OnColorChanged += HandleColorChanged;
            State.OnSidebarChanged += StateHasChanged;
            await State.InitializeAsync();
        }
        #endregion

        #region Funciones
        private async Task CopyToClipboard(string copy, string id)
        {
            bool resultado = await JS!.InvokeAsync<bool>("copyToClipboard", copy);

            if (resultado)
            {
                mensajes[id] = ($"Query copiada al portapapeles", true);
                resultCopy[id] = 1;
            }
            else
            {
                mensajes[id] = ($"Error al copiar la query al portapapeles", false);
                resultCopy[id] = 2;
            }

            StateHasChanged();

            await Task.Delay(10000);

            mensajes.Remove(id);
            resultCopy.Remove(id);
            StateHasChanged();
        }

        public static readonly Dictionary<string, ReportTable> _tableMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Actividades"] = ReportTable.Activities,
                ["Capacitaciones"] = ReportTable.Trainings,
                ["Rechazos"] = ReportTable.Rejections,
                ["Rejections"] = ReportTable.Rejections,
                ["Rechazos Detalles"] = ReportTable.RechazosDetails,
                ["Requerimientos"] = ReportTable.Requirements,
                ["Requirements"] = ReportTable.Requirements,
                ["Usuarios"] = ReportTable.Users,
                ["Users"] = ReportTable.Users,
                ["Categorías"] = ReportTable.Categories,
                ["Categories"] = ReportTable.Categories
            };


        private List<JoinDefinition> GetRootJoins()
        {
            return ActiveJoins
                .Where(j => j.ParentTable == GetTableRelations(BasesDatosSeleccionado!)).ToList();
        }

        private IEnumerable<JoinDefinition> GetChildJoins(ReportTable parent)
        {
            return ActiveJoins
                .Where(j => j.ParentTable == parent);
        }

        private IEnumerable<JoinDefinition> GetAvailableJoinsRelaciones(ReportTable table)
        {
            List<ReportTable>? usedTables = GetUsedTables();

            return GetJoins(table)
                .Where(j => !usedTables.Contains(j.Table));
        }

        private List<ReportTable> GetUsedTables()
        {
            List<ReportTable>? used = new List<ReportTable>();
            if (BasesDatosSeleccionado != null)
            {
                used.Add(_tableMap[BasesDatosSeleccionado]);
            }

            used.AddRange(ActiveJoins.Select(j => j.Table));

            return used;
        }


        private List<JoinDefinition> GetOpcionesFiltradas(ReportTable tablaPadre)
        {
            if (!AvailableJoins.TryGetValue(tablaPadre, out var opcionesPosibles))
                return new List<JoinDefinition>();

            List<int>? idsOcupados = ActiveJoins
                .Select(j => (int)j.Table)
                .ToList();

            if (BasesDatosSeleccionado != null)
            {
                idsOcupados.Add((int)_tableMap[BasesDatosSeleccionado]);
            }

            return opcionesPosibles
                .Where(opt => !idsOcupados.Contains((int)opt.Table))
                .ToList();
        }

        private string BaseFromQuery(string baseFromQuery)
        {
            string baseFrom;

            if (baseFromQuery == "Actividades")
            {
                baseFrom = "activities";
            }
            else if (baseFromQuery == "Capacitaciones")
            {
                baseFrom = "trainings";
            }
            else if (baseFromQuery == "Rechazos Detalles")
            {
                baseFrom = "rechazos_detalles";
            }
            else if (baseFromQuery == "Requerimientos")
            {
                baseFrom = "requeriments";
            }
            else if (baseFromQuery == "Categorías")
            {
                baseFrom = "categories";
            }
            else
            {
                baseFrom = BasesDatosSeleccionado!;
            }

            return baseFrom;
        }
        private Task OnBasesDatosSelectedChanged(string value)
        {
            BasesDatosSeleccionado = value;
            createNewReport.TableBase = NormalizeTableName(value) ;

            if (!string.IsNullOrWhiteSpace(value))
            {
                queryVisual.Clear();

                ReportTable table = _tableMap[value];
                string? alias = _aliases[table];

                string baseFrom = BaseFromQuery(value);

                queryVisual.AppendLine("SELECT");
                queryVisual.AppendLine($"    {alias}.*");
                queryVisual.AppendLine($"FROM {baseFrom.ToLowerInvariant()} AS {alias}");
                ExpandedRelacionadasTables.Clear();
                ActiveJoins.Clear();
            }


            return Task.CompletedTask;
        }

        public Task OnBasesDatosFilterSelectedChanged(ReportFilter filter, string value)
        {
            filter.Tabla = value;
            filter.Campo = null;

            LoadOptionFields(filter);

            RebuildQuery();

            return Task.CompletedTask;
        }

        private void OnFilterValueChanged(ReportFilter filter, string newValue)
        {
            filter.Valor = newValue;
            RebuildQuery();
            StateHasChanged();
        }

        private void OnMetricValueChanged(ReportMetrics metric, string newValue)
        {
            metric.Etiqueta = newValue;
            RebuildQuery();
            StateHasChanged();
        }

        public Task OnBasesDatosMetricSelectedChanged(ReportMetrics metric, string value)
        {
            metric.Tabla = value;
            metric.Campo = null;

            LoadOptionFields(metric);

            RebuildQuery(); 
            return Task.CompletedTask;

        }

        private void LoadOptionFields(ReportFilter filter)
        {
            ReportTable tableSelected = GetTableRelationsAll(filter.Tabla!);
            IReadOnlyList<TableField> tableFields = GetFields(tableSelected);

            filter.OptionsCampos = tableFields
                .Select(t => t.Label)
                .ToList();
            StateHasChanged();
        }

        private void LoadOptionFields(ReportMetrics metric)
        {
            ReportTable tableSelected = GetTableRelationsAll(metric.Tabla!);
            IReadOnlyList<TableField> tableFields = GetFields(tableSelected);

            metric.OptionsCampos = tableFields
                .Select(t => t.Label)
                .ToList();
            StateHasChanged();
        }

        private Task OnFrecuencySelectedChanged(string value)
        {
            FrecuenciaSeleccionada = value;
            return Task.CompletedTask;
        }

        private Task OnFrecuencySelectedChangedCustom(string value)
        {
            FrecuenciaSeleccionadaCustom = value;
            return Task.CompletedTask;
        }

        private void HandleLoadedCheck(bool check)
        {
            SchecheduledProgramed = check;
            StateHasChanged();
        }

        private void HandleLoadedCheckCustom(bool check)
        {
            SchecheduledProgramedCustom = check;
            StateHasChanged();
        }

        private void HandleModeConstruction(string value)
        {
            _reportMode = value;
            StateHasChanged();
        }

        private string GetHexColorGradient(string gradient)
        {
            if (string.IsNullOrWhiteSpace(gradient))
                return "#2196F3";

            string? fromClass = gradient
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault(x => x.StartsWith("from-"));

            if (fromClass is null)
                return "#2196F3";

            string colorKey = fromClass.Replace("from-", "");

            return TailwindColorMap.TryGetValue(colorKey, out string? hex)
                ? hex
                : "#2196F3";
        }



        void OnRecipientChanged(int index, string? value)
        {
            Recipients[index] = value ?? string.Empty;
            StateHasChanged(); 
        }

        void OnRecipientChangedCustom(int idx, string? value)
        {
            RecipientsCustom[idx] = value ?? string.Empty;
            StateHasChanged();
        }

        void AddRecipient()
        {
            Recipients.Add("");
        }

        void AddRecipientCustom()
        {
            RecipientsCustom.Add(string.Empty);
        }

        void RemoveRecipient(int index)
        {
            Recipients.RemoveAt(index);
        }

        void RemoveRecipientCustom(int idx)
        {
            RecipientsCustom.RemoveAt(idx);
        }

        private static readonly Dictionary<string, string> TailwindColorMap = new()
        {
            // RED
            { "red-500", "#ef4444" },
            { "red-600", "#dc2626" },

            // PINK
            { "pink-600", "#db2777" },

            // BLUE
            { "blue-500", "#3b82f6" },
            { "blue-600", "#2563eb" },

            // PURPLE
            { "purple-600", "#9333ea" },
            { "purple-500", "#a855f7" },

            // GREEN
            { "green-500", "#22c55e" },
            { "green-600", "#16a34a" },

            // EMERALD
            { "emerald-600", "#059669" },

            // ORANGE
            { "orange-500", "#f97316" },
            { "orange-600", "#ea580c" },

            // INDIGO
            { "indigo-600", "#4f46e5" }
        };

        private void HandleDropdownState(bool closed)
        {
            IsSelectClosed = closed;
        }

        private void HandleSidebarToggle()
        {
            _ = State.ToggleSidebar();
        }

        private async void HandleColorChanged()
        {
            await InvokeAsync(StateHasChanged);
        }

        private void ToggleDetails(ReportTable table)
        {
            if (!ExpandedTables.Add(table))
                ExpandedTables.Remove(table);
        }

        private void TogglePrincipalDetails(string table)
        {
            if (!ExpandedPrincipalTables.Add(table))
                ExpandedPrincipalTables.Remove(table);
        }

        private void ToggleRelacionadaDetails(ReportTable table)
        {
            if (!ExpandedRelacionadasTables.Add(table))
                ExpandedRelacionadasTables.Remove(table);
        }

        private void ToggleRelacionadaRelDetails(ReportTable table)
        {
            if (!ExpandedRelacionadasRelTables.Add(table))
                ExpandedRelacionadasRelTables.Remove(table);
        }

        private ReportTable GetTableRelations(string tableName)
        {
            List<ReportTableUi>? tablas = TablesUi.ToList();
            ReportTableUi? tablaSeleccionada = tablas.Find(t => t.Title == tableName);
            IEnumerable<ReportTable>? reportTable = AvailableJoins.Select(aj => aj.Key);
            ReportTable tableAv = reportTable.FirstOrDefault(aj => aj.Equals(tablaSeleccionada!.Table));
            return tableAv;
        }

        private ReportTable GetTableRelationsAll(string tableName)
        {
            List<ReportTableUi>? tablas = TablesUi.ToList();
            ReportTableUi? tablaSeleccionada = tablas.Find(t => t.Title == tableName);
            return tablaSeleccionada!.Table;
        }

        private void AddJoin(JoinDefinition plantilla)
        {
            if (plantilla == null || string.IsNullOrEmpty(BasesDatosSeleccionado)) return;

            JoinDefinition nuevoJoin = new JoinDefinition
            {
                Table = plantilla.Table,
                Nombre = plantilla.Nombre,
                Field = plantilla.Field,
                TargetField = plantilla.TargetField,
                JoinType = plantilla.JoinType,
                ParentTable = GetTableRelations(BasesDatosSeleccionado!) 
            };

            ActiveJoins.Add(nuevoJoin);

            StateHasChanged();
            RebuildQuery();
        }

        private HashSet<ReportTable> CurrentUsedTables =>
                    ActiveJoins.Select(j => j.Table)
                               .Concat(new[] { _tableMap[BasesDatosSeleccionado!] })
                               .ToHashSet();

        private List<JoinDefinition> GetAvailableOptionsFor(ReportTable tablaPadre, bool esNodoHijo)
        {
            if (!AvailableJoins.TryGetValue(tablaPadre, out List<JoinDefinition>? opciones)) return new();

            HashSet<ReportTable>? ocupadas = ActiveJoins.Select(j => j.Table).ToHashSet();

            if (!string.IsNullOrEmpty(BasesDatosSeleccionado))
                ocupadas.Add(_tableMap[BasesDatosSeleccionado!]);

            if (esNodoHijo && !string.IsNullOrEmpty(BasesDatosSeleccionado))
            {
                ReportTable tablaBaseEnum = _tableMap[BasesDatosSeleccionado!];

                HashSet<ReportTable>? tablasQueYaOfreceLaRaiz = AvailableJoins.ContainsKey(tablaBaseEnum)
                    ? AvailableJoins[tablaBaseEnum].Select(o => o.Table).ToHashSet()
                    : new HashSet<ReportTable>();

                return opciones
                    .Where(opt => !ocupadas.Contains(opt.Table) && !tablasQueYaOfreceLaRaiz.Contains(opt.Table))
                    .ToList();
            }

            return opciones.Where(opt => !ocupadas.Contains(opt.Table)).ToList();
        }


        private void AddJoinRel(JoinDefinition join, ReportTable parent)
        {
            if (!ActiveJoins.Any(j => j.Table == join.Table))
            {
                ActiveJoins.Add(new JoinDefinition
                {
                    Table = join.Table,
                    Nombre = join.Nombre,
                    Field = join.Field,
                    TargetField = join.TargetField,
                    JoinType = join.JoinType,
                    ParentTable = parent
                });
            }

            StateHasChanged();
            RebuildQuery();

        }

        private void RemoveJoin(ReportTable table)
        {

            List<ReportTable>? toRemove = GetAllChildren(table);
            toRemove.Add(table);

            ActiveJoins.RemoveAll(j => toRemove.Contains(j.Table));
            StateHasChanged();

            OptionsBasesDatosFilter.RemoveAll(
                j => NameToSpanish(j) == NameToSpanish(table.ToString())
            );


            RebuildQuery();
        }

        private List<ReportTable> GetAllChildren(ReportTable parent)
        {
            List<ReportTable>? children = ActiveJoins.Where(j => j.ParentTable == parent).Select(j => j.Table).ToList();
            List<ReportTable>? allChildren = new List<ReportTable>(children);
            foreach (ReportTable child in children)
            {
                allChildren.AddRange(GetAllChildren(child));
            }
            return allChildren;
        }

        private void RemoveJoinRel(ReportTable table)
        {
            ActiveJoins.RemoveAll(j => j.Table == table);

            OptionsBasesDatosFilter.RemoveAll(j => NameToSpanish(j) == NameToSpanish(table.ToString()));

            RebuildQuery();
        }

        private string InnerOrLeftCardinal(JoinType joinType, string? nombre, ReportTable tableRel, ReportTable? parentTab, string? basesDatosSeleccionado, string field, string targetField)
        {
            string jtStr = joinType == JoinType.Inner ? "INNER" : "LEFT OUTER";
            string tableRelAlias = _aliases[tableRel];

            string tablebasealias;
            if (parentTab is ReportTable parent)
            {
                tablebasealias = _aliases[parent];
            }
            else if (!string.IsNullOrWhiteSpace(basesDatosSeleccionado) && _tableMap.TryGetValue(basesDatosSeleccionado, out ReportTable baseTable))
            {
                tablebasealias = _aliases[baseTable];
            }
            else
            {
                throw new ArgumentException("No se pudo determinar la tabla base para construir el JOIN.");
            }

            return $"{jtStr} JOIN {nombre} AS {tableRelAlias} ON {tableRelAlias}.{targetField} = {tablebasealias}.{field}";
        }

        private void AddFilter()
        {
            Filters.Add(new ReportFilter());
        }

        private void RemoveFilter(ReportFilter filter)
        {
            Filters.Remove(filter);
        }

        private void AddMetric()
        {
            Metrics.Add(new ReportMetrics());
        }

        private void RemoveMetric(ReportMetrics metric)
        {
            Metrics.Remove(metric);
        }

        void OnAliasChanged(SelectedField field, string alias)
        {
            if (_selectedFields.ContainsKey(field))
                _selectedFields[field] = alias;

            RebuildQuery();
        }

        private void OnFieldToggle(SelectedField field, bool value)
        {
            if (value)
                _selectedFields.TryAdd(field, null);
            else
                _selectedFields.Remove(field);

            RebuildQuery();
        }

        private static string EscapeIfNeeded(string value)
        {
            return value.Contains(' ')
                ? $"`{value}`"
                : value;
        }

        private static string ToSql(JoinType type) => type == JoinType.Inner ? "INNER JOIN" : "LEFT JOIN";

        private void OnGroupingChanged(string tabla, string campo, bool isChecked)
        {
            if (isChecked)
            {
                if (!_groupedFields.Any(f => f.Tabla == tabla && f.Campo == campo))
                    _groupedFields.Add((tabla, campo));
            }
            else
            {
                _groupedFields.RemoveAll(f => f.Tabla == tabla && f.Campo == campo);
            }
            RebuildQuery();
        }

        private void RebuildQuery()
        {
            queryVisual.Clear();
            queryVisual.AppendLine("SELECT");

            List<string> selectLines = new();
            List<string> groupByCols = new();

            // 1. Identificar métricas activas
            List<ReportMetrics>? activeMetrics = Metrics.Where(m => !string.IsNullOrWhiteSpace(m.Tabla) && !string.IsNullOrWhiteSpace(m.Campo)).ToList();

            // --- SINCRONIZACIÓN AUTOMÁTICA ---
            // Si hay métricas, forzamos que los campos de la pestaña 1 estén en la lista de Agrupación
            if (activeMetrics.Any())
            {
                foreach (KeyValuePair<SelectedField, string?> entry in _selectedFields)
                {
                    // Buscamos el nombre de la tabla (string) asociado al objeto ReportTable
                    string? tablaNombre = _tableMap.FirstOrDefault(x => x.Value == entry.Key.Table).Key;

                    if (tablaNombre != null)
                    {
                        // Si no está ya en la lista de agrupados, lo agregamos
                        if (!_groupedFields.Any(f => f.Tabla == tablaNombre && f.Campo == entry.Key.Field))
                        {
                            _groupedFields.Add((tablaNombre, entry.Key.Field));
                        }
                    }
                }
            }

            // --- 2. PROCESAR DIMENSIONES (CAMPOS) ---
            if (activeMetrics.Any())
            {
                // Si hay métricas, el SELECT y GROUP BY se rigen por _groupedFields
                foreach ((string Tabla, string Campo) item in _groupedFields)
                {
                    if (!_tableMap.ContainsKey(item.Tabla)) continue;

                    string tableAlias = _aliases[_tableMap[item.Tabla]];
                    IReadOnlyList<TableField> fieldValues = GetFields(GetTableRelationsAll(item.Tabla));
                    TableField? datosTabla = fieldValues.FirstOrDefault(v => v.Value == item.Campo);
                    string fullColumn = datosTabla!.Nullable ? $"IFNULL({tableAlias}.{item.Campo}, '')" : $"{tableAlias}.{item.Campo}";

                    // Intentamos recuperar el alias (nombre de columna) que el usuario puso en la pestaña 1
                    string? aliasEntry = _selectedFields.FirstOrDefault(x => x.Key.Field == item.Campo).Value;

                    string line = string.IsNullOrWhiteSpace(aliasEntry)
                        ? $"    {fullColumn}"
                        : $"    {fullColumn} AS {EscapeIfNeeded(aliasEntry)}";

                    selectLines.Add(line);
                    groupByCols.Add(fullColumn);
                }
            }
            else
            {
                // Si NO hay métricas, comportamiento estándar de selección simple
                foreach (KeyValuePair<SelectedField, string?> entry in _selectedFields)
                {
                    string tableAlias = _aliases[entry.Key.Table];


                    IReadOnlyList<TableField> fieldValues = GetFields(entry.Key.Table);
                    TableField? datosTabla = fieldValues.FirstOrDefault(v => v.Value == entry.Key.Field);
                    string column = datosTabla!.Nullable ? $"IFNULL({tableAlias}.{entry.Key.Field}, '')" : $"{tableAlias}.{entry.Key.Field}";
                    

                    string line = string.IsNullOrWhiteSpace(entry.Value)
                        ? $"    {column}"
                        : $"    {column} AS {EscapeIfNeeded(entry.Value)}";

                    selectLines.Add(line);
                }
            }

            // --- 3. PROCESAR MÉTRICAS ---
            foreach (ReportMetrics metric in activeMetrics)
            {
                if (!_tableMap.ContainsKey(metric.Tabla!)) continue;

                string tableAlias = _aliases[_tableMap[metric.Tabla!]];
                IReadOnlyList<TableField> fieldValues = GetFields(GetTableRelationsAll(metric.Tabla!));
                TableField? datosTabla = fieldValues.FirstOrDefault(v => v.Label == metric.Campo);

                if (datosTabla == null) continue;

                string fullFieldName = $"{tableAlias}.{datosTabla.Value}";
                string aggregatedCol = GetSqlAggregation(metric.Agregacion!, fullFieldName);
                string alias = !string.IsNullOrWhiteSpace(metric.Etiqueta)
                    ? EscapeIfNeeded(metric.Etiqueta)
                    : $"{metric.Agregacion}_{datosTabla.Value}";

                selectLines.Add($"    {aggregatedCol} AS {alias}");
            }

            // Construir SELECT final
            if (selectLines.Count == 0) queryVisual.AppendLine("    *");
            else queryVisual.AppendLine(string.Join("," + Environment.NewLine, selectLines));

            // --- 4. FROM Y JOINS ---
            if (!string.IsNullOrWhiteSpace(BasesDatosSeleccionado) && _tableMap.ContainsKey(BasesDatosSeleccionado))
            {
                ReportTable baseTable = _tableMap[BasesDatosSeleccionado];
                string baseAlias = _aliases[baseTable];
                queryVisual.AppendLine($"FROM {BaseFromQuery(BasesDatosSeleccionado).ToLowerInvariant()} AS {baseAlias}");

                foreach (JoinDefinition join in ActiveJoins)
                {
                    string joinAlias = _aliases[join.Table];
                    string parentAlias = join.ParentTable is ReportTable parent ? _aliases[parent] : baseAlias;
                    queryVisual.AppendLine($"{ToSql(join.JoinType)} {join.Nombre.ToLowerInvariant()} AS {joinAlias} ON {joinAlias}.{join.TargetField} = {parentAlias}.{join.Field}");
                }
            }

            // --- 5. WHERE (FILTROS) ---
            List<ReportFilter>? activeFilters = Filters.Where(f => !string.IsNullOrWhiteSpace(f.Tabla) && !string.IsNullOrWhiteSpace(f.Campo) && !string.IsNullOrWhiteSpace(f.Valor)).ToList();
            if (activeFilters.Any())
            {
                queryVisual.AppendLine("WHERE");

                for (int i = 0; i < activeFilters.Count; i++)
                {
                    ReportFilter? filter = activeFilters[i];

                    // Obtenemos el alias real de la tabla (ej: "RD" para "Rechazos Detalles")
                    ReportTable tableObj = _tableMap[filter.Tabla!];
                    string tableAlias = _aliases[tableObj];

                    string sqlOp = GetSqlOperator(filter.Operador!);

                    // Usamos GetFields con el nombre de tabla para obtener todos las columnas de su tabla.
                    IReadOnlyList<TableField> fieldValues = GetFields(GetTableRelationsAll(filter.Tabla!));

                    TableField? datosTabla = fieldValues.FirstOrDefault(v => v.Label == filter.Campo);

                    if (datosTabla == null) continue;

                    string line = "";

                    if (filter.Operador == "Entre")
                    {
                        // Esperamos que el usuario escriba "valor1, valor2"
                        string[]? partes = filter.Valor!.Split(',');
                        if (partes.Length == 2)
                        {
                            string v1 = FormatFilterValue("Igual a", NormalizeDate(partes[0].Trim()), datosTabla.Type);
                            string v2 = FormatFilterValue("Igual a", NormalizeDate(partes[1].Trim()), datosTabla.Type);
                            line = $"    {tableAlias}.{datosTabla.Value} BETWEEN {v1} AND {v2}";
                        }
                        else
                        {
                            line = $"    {tableAlias}.{datosTabla.Value} BETWEEN {NormalizeDate(filter.Valor)} AND ???";
                        }
                    }
                    else if (filter.Operador == "En lista" || filter.Operador == "No en la lista")
                    {
                        IEnumerable<string>? items = filter.Valor!.Split(',')
                            .Select(x => FormatFilterValue("Igual a", NormalizeDate(x.Trim()), datosTabla.Type));

                        line = $"    {tableAlias}.{datosTabla.Value} {sqlOp} ({string.Join(", ", items)})";
                    }
                    else
                    {
                        // Caso normal (Igual, mayor, menor, contiene)
                        string val = FormatFilterValue(filter.Operador!, NormalizeDate(filter.Valor!), datosTabla.Type);
                        line = $"    {tableAlias}.{datosTabla.Value} {sqlOp} {val}";
                    }

                    if (i < activeFilters.Count - 1) line += " AND";
                    queryVisual.AppendLine(line);
                }
            }

            // --- 6. GROUP BY ---
            if (activeMetrics.Any() && groupByCols.Any())
            {
                queryVisual.AppendLine("GROUP BY");
                queryVisual.AppendLine("    " + string.Join("," + Environment.NewLine + "    ", groupByCols.Distinct()));
            }

            // Forzar a la UI a refrescar los checkboxes de la pestaña Agrupación
            StateHasChanged();
            BuildQueryObject();
        }

        private string GetNameTableBackend(string tableName)
        {
            List<ReportTableUi>? tablas = TablesUi.ToList();
            ReportTableUi? tablaSeleccionada = tablas.Find(t => t.Title == tableName);
            return tablaSeleccionada!.Key;
        }

        private string GetNameTableBackendRT(string tableName)
        {
            List<ReportTableUi>? tablas = TablesUi.ToList();
            ReportTableUi? tablaSeleccionada = tablas.Find(t => t.Title == tableName);
            return tablaSeleccionada!.Key;
        }

        private string GetNameFieldBackend(string campo, string tabla)
        {
            IReadOnlyList<TableField> fieldValues = GetFields(GetTableRelationsAll(tabla));
            TableField? datosTabla = fieldValues.FirstOrDefault(v => v.Label == campo);
            if (datosTabla == null)
                throw new InvalidOperationException($"Campo '{campo}' no encontrado en tabla '{tabla}'");
            return datosTabla.Value;
        }

        private string GetNameFieldBackendL(string campo, string tabla)
        {
            IReadOnlyList<TableField> fieldValues = GetFields(GetTableRelationsAll(tabla));
            TableField? datosTabla = fieldValues.FirstOrDefault(v => v.Label == campo);

            if (datosTabla == null)
                throw new InvalidOperationException($"Campo '{campo}' no encontrado en tabla '{tabla}'");

            return datosTabla.Value;
        }

        private string GetFilterValues(string value, string operador, string tabla, string campo)
        {
            string sqlOp = GetSqlOperator(operador);

            // Usamos GetFields con el nombre de tabla para obtener todos las columnas de su tabla.
            IReadOnlyList<TableField> fieldValues = GetFields(GetTableRelationsAll(tabla));
            TableField? datosTabla = fieldValues.FirstOrDefault(v => v.Label == campo);

            if (datosTabla == null) return "";

            string line = "";

            if (operador == "Entre")
            {
                // Esperamos que el usuario escriba "valor1, valor2"
                string[]? partes = value.Split(',');
                if (partes.Length == 2)
                {
                    string v1 = FormatFilterValue("Igual a", NormalizeDate(partes[0].Trim()), datosTabla.Type);
                    string v2 = FormatFilterValue("Igual a", NormalizeDate(partes[1].Trim()), datosTabla.Type);
                    line = $"{v1} AND {v2}";
                }
                else
                {
                    line = $"{NormalizeDate(value)} AND ???";
                }
            }
            else if (operador == "En lista" || operador == "No en la lista")
            {
                IEnumerable<string>? items = value.Split(',')
                    .Select(x => FormatFilterValue("Igual a", NormalizeDate(x.Trim()), datosTabla.Type));

                line = $"({string.Join(", ", items)})";
            }
            else
            {
                // Caso normal (Igual, mayor, menor, contiene)
                line = FormatFilterValue(operador, NormalizeDate(value), datosTabla.Type);
            }


            return line;
        }

        private QueryRequestDTO BuildQueryObject()
        {
            QueryRequestDTO? request = new QueryRequestDTO
            {
                BaseTable = GetNameTableBackend(BasesDatosSeleccionado!),
                // 1. Campos Seleccionados
                Fields = _selectedFields.Select(f => new SelectedFieldDTO(
                    GetNameTableBackendRT(_tableMap.FirstOrDefault(x => x.Value == f.Key.Table).Key),
                    f.Key.Field,
                    f.Value)).ToList(),

                // 2. Joins
                Joins = ActiveJoins.Select(j => new JoinDTO(
                    GetNameTableBackendRT(_tableMap.FirstOrDefault(x => x.Value == j.Table).Key).ToLowerInvariant(),
                    j.ParentTable?.ToString().ToLowerInvariant() ?? "", // Ajustar según tu objeto ReportTable
                    ToSql(j.JoinType),
                    j.Field,
                    j.TargetField)).ToList(),

                // 3. Métricas
                Metrics = Metrics.Where(m => !string.IsNullOrEmpty(m.Campo))
                    .Select(m => new MetricDTO(
                        GetNameTableBackend(m.Tabla!),
                        GetNameFieldBackendL(m.Campo!, m.Tabla!),
                        GetSqlAggregation(m.Agregacion!), 
                        m.Etiqueta)
                    ).ToList(),

                // 4. Filtros
                Filters = Filters.Where(f => !string.IsNullOrEmpty(f.Valor))
                    .Select(f => new FilterDTO(
                        GetNameTableBackend(f.Tabla!),
                        GetNameFieldBackend(f.Campo!, f.Tabla!),
                        GetSqlOperator(f.Operador!),
                        GetFilterValues(f.Valor!, f.Operador!, f.Tabla!, f.Campo!)
                        )
                    ).ToList()
            };

            // 5. Agrupación (Tu lógica de sincronización automática)
            if (request.Metrics.Any())
            {
                request.GroupBy = _groupedFields.Select(g => new GroupFieldDTO(
                    GetNameTableBackend(g.Tabla),
                    g.Campo
                    )
                ).ToList();
            }

            return request;
        }

        /*private ScheduleReportDto BuildScheduledObject() {
            ScheduleReportDto? dto = new ScheduleReportDto()
            {
                Scheduled = SchecheduledProgramedCustom,
                Frecuency = FrecuenciaSeleccionada,
                Destinations = RecipientsCustom
            };

            return dto;
        }*/


        private ScheduleReportDto BuildScheduledObject()
        {
            List<string>? destinatarios = Recipients?.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();

            return new ScheduleReportDto()
            {
                Scheduled = SchecheduledProgramed,

                Frecuency = FrecuenciaSeleccionada,

                Destinations = destinatarios
            };
        }

        private ScheduleReportDto BuildScheduledCustomObject()
        {
            List<string>? destinatarios = RecipientsCustom?.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();

            return new ScheduleReportDto()
            {
                Scheduled = SchecheduledProgramedCustom,

                Frecuency = FrecuenciaSeleccionadaCustom,

                Destinations = destinatarios
            };
        }

        private void OnQueryCustomChanged(string value)
        {
            createNewReport.QueryRaw = value;
        }


        private async Task SendCreateNewReport()
        {
            IsSendingReport = true;
            try
            {
                if (_reportMode == "visual")
                {
                    createNewReport.QueryRequest = BuildQueryObject();
                    createNewReport.ReportMode = "visual";
                    createNewReport.Schedule = BuildScheduledObject();
                    createNewReport.IsScheduled = SchecheduledProgramed;
                }
                else
                {
                    createNewReport.Schedule = BuildScheduledCustomObject();
                    createNewReport.ReportMode = "custom";
                    createNewReport.IsScheduled = SchecheduledProgramedCustom;
                }

                GeneralResponse? response = await ReportsService!.CreateNewReport(createNewReport);

                if (response.Flag)
                {
                    IsSendingReport = false;
                    Toltip.Success("Éxito!", response.Message);
                    Router!.NavigateTo("/app/reportes");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: Error al enviar el reporte: {ex.Message}");
                throw;
            }
            finally
            {
                IsSendingReport = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        

        #endregion

        #region Limpiar
        public void Dispose()
        {
            ColorService.OnColorChanged -= HandleColorChanged;
            State.OnSidebarChanged -= StateHasChanged;
        }
        #endregion
    }
}