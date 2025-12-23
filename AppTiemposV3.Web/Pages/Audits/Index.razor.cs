using System.Text.Json;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Audits;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.Web.Components.Icons;
using AppTiemposV3.Web.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;
using static AppTiemposV3.Web.Utils.AuditActionHelper;
using static AppTiemposV3.Web.Utils.RenderHFunction;

namespace AppTiemposV3.Web.Pages.Audits;

public partial class Index : ComponentBase, IDisposable
{
    #region  Variables
    #region InyeccionDependencias
    [Inject] LayoutState State { get; set; } = null!;
    [Inject] private IJSRuntime? JS { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private IAuditContract<AuditsResponseDto> AuditService { get; set; } = default!;
    [Inject] private ILocalStorageService _localStorageService { get; set; } = default!;
    #endregion
    private string? theme = "light";
    
    private bool IsLoading = true;
    private AuditFilterDto filterDto = new AuditFilterDto();
    
    private DateTime currentDate = DateTime.Today;
    private string weekRange = string.Empty;
    private  bool isNextDisabled = false;
    private bool IsSelectClosed = true;
    private int weekNumberSelected = 45;
    
    private List<DateTime> WeekDays = new();
    
    private bool isOpenFiltros = false;
    private bool isOpenAuditsChanges = false;
    private bool isLoadingFiltros = false;
    private int activeFiltersCount = 0;
    private string? search { get; set; } = string.Empty;
    private bool SearchEmpty = false;
    private List<AdvancedFilters>  filters = new();
    private string filtroSelected = string.Empty;
    private Guid IdModalNew = Guid.NewGuid();
    private bool IsLoadingBackend = false;
    private List<AuditsResponseDto> Audits = [];
    
    private bool IsLoadingKpis = false;
    private AuditKpiResponse kpis = new();
    HashSet<Guid> ExpandedAudits = new();
    
    private List<string> OptionsEntidad = new() {"Todas las entidades", "Actividades", "Capacitaciones", "Invitaciones", "Requerimientos", "Rechazos", "Usuarios"};
    private string? EntitySelected = "Todas las entidades";
    private List<string> OptionsAction = new() {"Todas las acciones", "Creado", "Actualizado", "Eliminado", "Estado Cambiado", "Etapa Cambiada", "Completado", "Rechazado", "Aprobado", "Asignado"};
    private string? ActionSelected = "Todas las acciones";
    
    private static readonly JsonSerializerOptions _jsonPretty =
        new() { WriteIndented = true };
    
    #region Paginado
    private List<int> OptionsPaginated = new() { 1, 5, 10, 15, 50 };
    private List<string> OptionsOrders = new() { "Nombre Completo", "Email",  "Fecha Recibido"};
    private int pagina = 1;
    private int registrosPorPagina = 5;
    private string ordenar = "";
    private bool ascending = false;
    protected int totalPages = 0;
    protected int totalElements = 0;
    protected int currentPage = 1; 
    protected int maxVisiblePages = 5;
    protected bool isFirst = true;
    protected bool isLast = false;
    protected bool isFirstPage = true;
    protected bool isLastPage = false;
    #endregion

    // #region Modales
    // #endregion
    #endregion
    
    #region Inicializacion
    protected async override Task OnInitializedAsync()
    {
        await HandleThemeChanged();
        await JS!.InvokeVoidAsync("registerThemeChangeHandler", DotNetObjectReference.Create(this));
        ColorService.OnColorChanged += HandleColorChanged;
        State.OnSidebarChanged += StateHasChanged;
        await State.InitializeAsync();
        await GetAllAudits();
    }
    #endregion
    
    #region Properties
    [JSInvokable("OnThemeChanged")]
    public async Task OnThemeChanged()
    {
        await HandleThemeChanged();
    }

    
    private async Task HandleThemeChanged()
    {
        theme = await _localStorageService.GetItemAsync<string>("color-theme")!;
        StateHasChanged();
    }
    
    private void HandleSidebarToggle()
    {
        _ = State.ToggleSidebar();
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }
    
    private void HandleDropdownState(bool closed)
    {
        IsSelectClosed = closed;
    }
    
    private async Task HandleSaved(SavedEventArgs args)
    {
        pagina = 1;
        StateHasChanged();
        
        if (args.Success)
        {
            Toltip.Success("Éxito!", args.Message);
        }
        else
        {
            Toltip.Error("Error", args.Message);
        }
        
        await GetAllAudits();
        StateHasChanged(); 
    }
    #endregion

    #region Filtros
    private Task DoSearch(string? query)
    {
        search = query;
        
        filterDto.ActivityActionSearch = query;
        return Task.CompletedTask;
    }
    
    private void DateChangedFrom(DateTime?  date)
    {
        if (date.HasValue)
        {
            filterDto.StartDate = date;
        }
        else
        {
            filterDto.StartDate = new DateTime();
        }
    }
    
    private void DateChangedTo(DateTime?  date)
    {
        if (date.HasValue)
        {
            filterDto.EndDate = date;
        }
        else
        {
            filterDto.EndDate = new DateTime();
        }
        
    }
    
    private Task OnEntitySelectedChanged(string entity)
    {
        EntitySelected = entity;
        
        switch (entity)
        {
            case "Todas las entidades":
                filterDto.EntitySearch = null;
                break;
            
            default:
                filterDto.EntitySearch = entity;
                break;
        }
        return Task.CompletedTask;
    }
    
    private Task OnActionSelectedChanged(string action)
    {
        ActionSelected = action;
        
        switch (action)
        {
            case "Todas las acciones":
                filterDto.ActionSearch = null;
                break;
            
            default:
                filterDto.ActionSearch = action;
                break;
        }
        return Task.CompletedTask;
    }
    
    private async Task FilterSearch()
    {
        isLoadingFiltros = true;
        pagina = 1;
        _ = SetPage(1);
        UpdatePageStatus();
        StateHasChanged();

        try
        {
            BuildFilters();    
            pagina = 1;
            GetAllAudits(false).RunSynchronously();
            StateHasChanged();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            isLoadingFiltros = false;
            StateHasChanged();
        }
    }
    
    private void BuildFilters()
    {
        List<AdvancedFilters> list = new();

        if (!string.IsNullOrWhiteSpace(filterDto.ActivityActionSearch))
        {
             list.Add(new AdvancedFilters{ Key = "ActionActivity", Value = filterDto.ActivityActionSearch });
             filtroSelected = "Actividad";
        }
        
        if (!string.IsNullOrWhiteSpace(filterDto.EntitySearch))
        {
            list.Add(new AdvancedFilters{ Key = "EntityName", Value = filterDto.EntitySearch });
            filtroSelected = "Entidad";
        }
        
        if (!string.IsNullOrWhiteSpace(filterDto.ActionSearch))
        {
            list.Add(new AdvancedFilters{ Key = "Action", Value = filterDto.ActionSearch });
            filtroSelected = "Accion";
        }
        
        bool hasDateFilterFrom = filterDto.StartDate.HasValue;
        if (hasDateFilterFrom)
        {
             list.Add(new AdvancedFilters{ Key = "StartDate", Value = filterDto.StartDate.ToString() });
             filtroSelected = "Fecha desde";
        }
        
        bool hasDateFilterTo = filterDto.EndDate.HasValue;
        if (hasDateFilterTo)
        {
            list.Add(new AdvancedFilters{ Key = "EndDate", Value = filterDto.EndDate.ToString() });
            filtroSelected = "Fecha hasta";
        }

        filters = list;
        
        activeFiltersCount = filters.Count;
    }
    #endregion

    #region Data
    private async Task GetAllAudits(bool isLoadingKpis = true)
    {
        ShowLoadersLoading(true);
        IsLoadingKpis = isLoadingKpis;
        StateHasChanged();

        try
        {
            try
            {
                string orderDefault = !string.IsNullOrWhiteSpace(ordenar) ? ordenar : "CreatedAt";

                PaginationDtoAdvanced pagination = new()
                {
                    Pagina = pagina,
                    RegistrosPorPagina = registrosPorPagina,
                    Ordenar = orderDefault,
                    Ascending = ascending,
                    Filters = filters.ToArray()
                };

                Pageable<List<AuditsResponseDto>> response = await AuditService.GetAllAudits(pagination);


                Audits = response.Content;

                isFirst = response.First;
                isLast = response.Last;
                totalPages = response.TotalPages;
                totalElements = response.TotalElements;

                SearchEmpty = pagination.Filters is not null && response.Content.Count < 1;
            } 
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (isLoadingKpis)
            {
                try
                {
                    DataResponse<AuditKpiResponse> kpiResponse = await AuditService.GetKpis();

                    kpis = kpiResponse.Data;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    IsLoadingKpis = false;
                    StateHasChanged();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        finally
        {
            ShowLoadersLoading(false);
        }
    }
    
    private void ShowLoadersLoading(bool show)
    {
        IsLoading = show;
        IsLoadingBackend = show;
        StateHasChanged();
    }
    #endregion

    #region Functions
    void ToggleDetails(Guid rejectionId)
    {
        if (ExpandedAudits.Contains(rejectionId))
            ExpandedAudits.Remove(rejectionId);
        else
            ExpandedAudits.Add(rejectionId);
    }
    
    private string GetActionColor(string action)
    {
        
        if (!Enum.TryParse(action, out AuditAction auditAction))
        {
            return "bg-gray-100 dark:bg-gray-900/30 text-gray-800 dark:text-gray-200 border-gray-200 dark:border-gray-800";;
        }

        switch (auditAction)
        {
            case AuditAction.Created:
                return
                    "bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-200 border-green-200 dark:border-green-800";

            case AuditAction.Updated:
                return
                    "bg-blue-100 dark:bg-blue-900/30 text-blue-800 dark:text-blue-200 border-blue-200 dark:border-blue-800";

            case AuditAction.Deleted:
                return
                    "bg-red-100 dark:bg-red-900/30 text-red-800 dark:text-red-200 border-red-200 dark:border-red-800";

            case AuditAction.StageChanged:
            case AuditAction.StatusChanged:
                return
                    "bg-purple-100 dark:bg-purple-900/30 text-purple-800 dark:text-purple-200 border-purple-200 dark:border-purple-800";

            case AuditAction.Completed:
            case AuditAction.Approved:
                return
                    "bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-200 border-green-200 dark:border-green-800";

            case AuditAction.Rejected:
                return
                    "bg-red-100 dark:bg-red-900/30 text-red-800 dark:text-red-200 border-red-200 dark:border-red-800";

            case AuditAction.Assigned:
                return
                    "bg-yellow-100 dark:bg-yellow-900/30 text-yellow-800 dark:text-yellow-200 border-yellow-200 dark:border-yellow-800";

            default:
                return
                    "bg-gray-100 dark:bg-gray-900/30 text-gray-800 dark:text-gray-200 border-gray-200 dark:border-gray-800";
        }
        
    }
    
    private static RenderFragment GetActionIcon(string action) => builder =>
    {
        if (!Enum.TryParse(action, out AuditAction auditAction))
        {
            RenderDefault(builder);
            return;
        }
        
        switch (auditAction)
        {
            case AuditAction.Created:
                Render<Plus>(builder);
                break;

            case AuditAction.Updated:
                Render<Edit>(builder);
                break;

            case AuditAction.Deleted:
                Render<Trash2>(builder);
                break;

            case AuditAction.StageChanged:
            case AuditAction.StatusChanged:
                Render<ArrowRight>(builder);
                break;

            case AuditAction.Completed:
            case AuditAction.Approved:
                Render<CheckCircle>(builder);
                break;

            case AuditAction.Rejected:
                Render<XCircle>(builder);
                break;

            case AuditAction.Assigned:
                Render<User>(builder);
                break;

            default:
                RenderDefault(builder);
                break;
        }
    };
    
    private static RenderFragment GetEntityIcon(string entityName) => builder =>
    {
        switch (entityName)
        {
            case "Dashboard":
                Render<HomeIcon>(builder);
                break;
            case "Semanal":
                Render<CalendarClock>(builder);
                break;
            case "Actividades":
                Render<Clock>(builder);
                break;
            case "Requerimientos":
                Render<FileText>(builder);
                break;
            case "Capacitaciones":
                Render<GraduationCap>(builder);
                break;
            case "Rechazos":
                Render<XCircle>(builder);
                break;
            case "Reportes":
                Render<BarChart3>(builder);
                break;
            case "Configuración":
                Render<Settings>(builder);
                break;
            case "Invitaciones":
                Render<UserPlus>(builder);
                break;
            case "Auditorias":
                Render<BookLock>(builder);
                break;
            case "Usuarios":
                Render<Users>(builder);
                break;
            default:
                break;
        }
    };
    
    private string GetActionText(string action) => Get(action);

    private string GetEntityName(string entity)
    {
        switch (entity)
        {
            case "activities":
                return "Actividades";
            case "trainings":
                return "Capacitaciones";
            case "requeriments":
                return "Requerimientos";
            case "rechazos":
                return "Rechazos";
            case "usuarios":
                return "Usuarios";
            default:
                return entity;
        }
    }

    #endregion

    #region Limpiar
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
        State.OnSidebarChanged -= StateHasChanged;
    }
    
    private void LimpiarFiltros()
    {
        filterDto.ActivityActionSearch = "";
        filterDto.EntitySearch = "";
        filterDto.ActionSearch = "";
        filterDto.StartDate = null;
        filterDto.EndDate = null;
        filters.Clear();   
        pagina = 1;
        _ = SetPage(1);
        DoSearch(null).RunSynchronously();
        activeFiltersCount = 0;
        GetAllAudits(false).RunSynchronously();
        StateHasChanged();
    }
    #endregion
    
    #region PaginadoFunciones
    private Task OnOrderSelectedChanged(string nuevoOrden)
    {
        ordenar = nuevoOrden;
        pagina = 1;
        IsLoadingKpis = false;
        StateHasChanged();
        _ = SafeRunAsync(async () => await GetAllAudits(false));

        return Task.CompletedTask;
    }
    
    private Task OnAscendingChanged()
    {
        ascending = !ascending;
        pagina = 1;
        IsLoadingKpis = false;
        StateHasChanged();
        _ = SafeRunAsync(async () => await GetAllAudits(false));

        return Task.CompletedTask;
    }
    private Task OnPaginatedSelectedChanged(int value)
    {
        registrosPorPagina = value;
        pagina = 1;
        IsLoadingKpis = false;
        StateHasChanged();
        _ = SafeRunAsync(async () => await GetAllAudits(false));

        return Task.CompletedTask;
    }

    protected async Task First()
    {
        if (!isFirst)
        {
            currentPage = 1;
            pagina = 1;
            UpdatePageStatus();
            IsLoadingKpis = false;
            StateHasChanged();
            await GetAllAudits(false);
        }
    }

    protected async Task Last()
    {
        if (!isLast)
        {
            int totalPagesCalc = (int)Math.Ceiling((double)totalElements / (registrosPorPagina == 0 ? 1 : registrosPorPagina));
            currentPage = totalPagesCalc;
            pagina = totalPagesCalc;
            UpdatePageStatus();
            IsLoadingKpis = false;
            StateHasChanged();
            await GetAllAudits(false);
        }
    }

    protected async Task Rewind()
    {
        if (currentPage > 1 && !isFirst)
        {
            currentPage--;
            pagina--;
            UpdatePageStatus();
            IsLoadingKpis = false;
            StateHasChanged();
            await GetAllAudits(false);
        }
    }

    protected async Task Forward()
    {
        if (currentPage < totalPages && !isLast)
        {
            currentPage++;
            pagina++;
            UpdatePageStatus();
            IsLoadingKpis = false;
            StateHasChanged();
            await GetAllAudits(false);
        }
    }

    protected async Task SetPage(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= totalPages)
        {
            currentPage = pageNumber;
            pagina = pageNumber;
            UpdatePageStatus();
            IsLoadingKpis = false;
            StateHasChanged();
            await GetAllAudits(false);
        }
    }

    private void UpdatePageStatus()
    {
        isFirst = currentPage <= 1;
        isLast = currentPage >= totalPages;
        isFirstPage = isFirst;
        isLastPage = isLast;
    }
    #endregion
}