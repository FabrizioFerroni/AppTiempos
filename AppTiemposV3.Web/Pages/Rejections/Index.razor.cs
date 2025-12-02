using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Filtros;
using AppTiemposV3.SharedClases.DTOs.RejectionDetails;
using AppTiemposV3.SharedClases.DTOs.Rejections;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.Web.Pages.Rejections.ModalRejectionDetails;
using AppTiemposV3.Web.Pages.Rejections.ModalRejections;
using AppTiemposV3.Web.Pages.Requeriments.Modals;
using AppTiemposV3.Web.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;

namespace AppTiemposV3.Web.Pages.Rejections;

public partial class Index : ComponentBase, IDisposable
{
    #region Variables
    #region InyeccionDependencias
    [Inject] LayoutState State { get; set; } = null!;
    [Inject] public IJSRuntime? JS { get; set; }
    
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private IRejectionContract<RejectionResponseDto> RejectionService { get; set; } = default!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private IRequerimentContract<RequerimentResponseDto> RequerimentsService { get; set; } = null!;
    [Inject] private ILocalStorageService _localStorageService { get; set; } = default!;
    private string? theme = "light";
    #endregion
    
    private bool IsSelectClosed = true;
    private List<string> OptionsEstados = new() {"Pendiente", "En Progreso", "Completado"};
    private string? EstadoSeleccionado = "";
    private List<string> OptionsResueltos = new() {"Todos", "Resueltos por mi", "No resueltos por mi"};
    private string? ResolvedSelected = "Todos";
    private int activeFiltersCount = 0;
    private bool isOpenFiltros = false;
    private bool isLoadingFiltros = false;
    private string filtroSelected = "";

    private bool IsLoading = false;
    private bool IsLoadingKpis = false;
    private bool IsRequerimentsLoaded = false;
    private bool SearchEmpty = false;
    private List<AdvancedFilters>  filters = new();
    private RejectionsFiltrosDto filtrosForm = new();
    private List<RejectionResponseDto> Rejections = [];
    private RejectionKpiResponse kpis = new();
    private bool IsLoadingDelete = false;
    private bool IsLoadingBackend = true;
    private bool IsOpenDetails = false;
    
    HashSet<Guid> ExpandedRejections = new();
    
    #region Paginado
    private List<int> OptionsPaginated = new() { 1, 5, 10, 15, 50 };
    private List<string> OptionsOrders = new() { "Fecha", "ReqID", "Capacitador", "Tiempo cargado", "Estado" };
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
    
    #region Modales
    private Guid IdModalEdit = Guid.NewGuid();
    private Guid IdModalNew = Guid.NewGuid();
    private ShowRequeriment? showModalRef;
    private NewRejectionModal? newModalRef;
    private NewRejectionDetailModal? newDetailModalRef;
    private EditRejectionModal? editModalRef;
    private EditRejectionDetailModal? editDetailModalRef;
    private DeleteRejectionModal? deleteModalRef;
    private DeleteRejectionDetailModal? deleteDetailModalRef;
    
    #endregion
    
    #endregion
    
    #region Inicializacion
    protected override async Task OnInitializedAsync()
    {
        await HandleThemeChanged();
        await JS!.InvokeVoidAsync("registerThemeChangeHandler", DotNetObjectReference.Create(this));
        ColorService.OnColorChanged += HandleColorChanged;
        State.OnSidebarChanged += StateHasChanged;
        await State.InitializeAsync();
        // initial refresh
        await GetAllRejects();
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

    #endregion
    
    #region Functions
    private async Task FilterSearch()
    {
        isLoadingFiltros = true;
        IsLoadingKpis = false;
        pagina = 1;
        _ = SetPage(1);
        StateHasChanged();
        UpdatePageStatus();

        try
        {
            await BuildFilters();    
            pagina = 1;         
            await GetAllRejects(false);
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
    
    private async Task GetAllRejects(bool isLoadingKpis = true)
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

                Pageable<List<RejectionResponseDto>> response = await RejectionService.GetAllRejections(pagination);

                foreach (RejectionResponseDto req in response.Content)
                {
                    req.RejectionsDetails = req.RejectionsDetails
                        .OrderByDescending(d => d.RechazoNro)
                        .ThenByDescending(d => d.SolutionDate.HasValue)
                        .ThenByDescending(d => d.RejectionDate)   
                        .ThenByDescending(d => d.SolutionDate ?? DateOnly.MinValue)
                        .ToList();
                }
                
                
                Rejections = response.Content;

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
                    DataResponse<RejectionKpiResponse> kpiResponse = await RejectionService.GetRejectionKpi();

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
    
    private string GetStatusColor(string status)
    {
        switch (status)
        {
            case "completed":
                return "bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-200";
            case "in-progress":
                return "bg-yellow-100 dark:bg-yellow-900/30 text-yellow-800 dark:text-yellow-200";
            case "pending":
                return "bg-purple-100 dark:bg-purple-700/50 text-purple-800 dark:text-purple-200"; 
            default:
                return "";
        }
    }
    
    private void ShowLoadersLoading(bool show)
    {
        IsLoading = show;
        IsLoadingBackend = show;
        StateHasChanged();
    }
    
    private async Task HandleDeleted()
    {
        pagina = 1;
        await GetAllRejects();
    }

    private async Task HandleNewSaved(SavedEventArgs args)
    {
        pagina = 1;
        StateHasChanged();
        if (args.Success)
        {
            if (args.IdResponse.HasValue && args.Obs is not null)
            {
                await NewDetailModal(args.IdResponse.Value, args.Obs);
            }
            else
            {
                Console.WriteLine("IdResponse es null y reqid");
            }
        }
        else
        {
            Toltip.Error("Error", args.Message);
        }
        
        StateHasChanged(); 
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
        
        await GetAllRejects();
        StateHasChanged(); 
    }
    #endregion
    
    
    #region Limpiar
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
        State.OnSidebarChanged -= StateHasChanged;
    }

    private async void LimpiarFiltros()
    {
        try
        {
            EstadoSeleccionado = "";
            ResolvedSelected = "Todos";
            filtrosForm.ReqId = "";
            filtrosForm.Estado = "";
            filtrosForm.Resolved = "";
            filters.Clear();   
            activeFiltersCount = 0;
            pagina = 1;
            _ = SetPage(1);
            UpdatePageStatus();
            await GetAllRejects(false);
            //_ = SafeRunAsync(async () => await GetAllRejects(false));
            StateHasChanged();
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }
    
    private Task OnResueltosSelectedChanged(string estado)
    {
        ResolvedSelected = estado;
        
        switch (estado)
        {
            case "Resueltos por mi":
                filtrosForm.Resolved = "true";
                break;
            
            case "No resueltos por mi":
                filtrosForm.Resolved = "false";
                break;
            
            default:
                filtrosForm.Resolved = "";
                break;
        }
        return Task.CompletedTask;
    }
    
    private Task OnEstadoSelectedChanged(string estado)
    {
        EstadoSeleccionado = OnEstadoSelectedChangedBD(estado);
        filtrosForm.Estado = GetStatusName(estado);
        return Task.CompletedTask;
        
    }
    
    private string OnEstadoSelectedChangedBD(string estado)
    {
        EstadoSeleccionado = GetStatusNameR(estado);
        return GetStatusNameR(estado);
        
    }
    
    private string GetStatusName(string status)
    {
        switch (status) {
            case "Completado":
                return "completed";
            case "En Progreso":
                return "in-progress";
            case "Pendiente":
                return "pending";
            default:
                return status;
        }
    }

    private string GetStatusNameR(string status)
    {
        switch (status) {
            case "completed":
                return "Completado";
            case "in-progress":
                return "En Progreso";
            case "pending":
                return "Pendiente";
            default:
                return status;
        }
    }
    
    private async Task BuildFilters()
    {
        List<AdvancedFilters> list = new();

        if (!string.IsNullOrWhiteSpace(filtrosForm.Estado))
        {
            list.Add(new AdvancedFilters { Key = "Status", Value = filtrosForm.Estado });
            filtroSelected = "Estado";
        }

        if (!string.IsNullOrWhiteSpace(filtrosForm.Resolved))
        {
            list.Add(new AdvancedFilters { Key = "IsResolve", Value = filtrosForm.Resolved });
            filtroSelected = "Estado de Resolución";
        }

        if (!string.IsNullOrWhiteSpace(filtrosForm.ReqId))
        {
            DataResponse<Guid> req = await RequerimentsService.GetIdByReqId(filtrosForm.ReqId);
            list.Add(new AdvancedFilters { Key = "RequerimentId", Value = req.Data.ToString() });
            filtroSelected = "ReqID";
        }

        filters = list;
        
        activeFiltersCount = filters.Count;
    }
    
    void ToggleDetails(Guid rejectionId)
    {
        if (ExpandedRejections.Contains(rejectionId))
            ExpandedRejections.Remove(rejectionId);
        else
            ExpandedRejections.Add(rejectionId);
    }

    // Estos equivalen a tus funciones de React
    void OnNewDetail(Guid parentId) { /* abrir modal */ }
    void OnEditDetail(RejectionDetailResponseDto detail, Guid parentId) { /* editar */ }
    void OnDeleteDetail(Guid id, Guid parentId) { /* eliminar */ }
    
    
    private async Task RefreshData(bool includeKpis = false)
    {
        pagina = 1;
        IsLoadingKpis = includeKpis;
        StateHasChanged();
        await GetAllRejects(includeKpis);
    }
    #endregion

    #region Scripts
    private async Task SafeRunAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error en fire & forget: {ex}");
        }
    }
    #endregion
    
    #region Modales
    private async Task NewModal()
    {
        await newModalRef!.ShowAsync(IdModalNew);
    }
    
    private async Task EditModal(Guid id, string reqId)
    {
        await editModalRef!.ShowAsync(id, reqId);
    }
    
    private async Task ShowModal(Guid id)
    {
        await showModalRef!.ShowAsync(id);
    }
    
    private async Task NewDetailModal(Guid rejectionId, string reqId)
    {
        await newDetailModalRef!.ShowAsync(IdModalNew, rejectionId, reqId);
    }
    private async Task DeleteModal(Guid id, string reqId)
    {
        await deleteModalRef!.ShowAsync(id, reqId);
    }

    private async Task EditDetailModal(Guid idDetail, string reqId, int rejectionNumber)
    {
        await editDetailModalRef!.ShowAsync(idDetail, reqId, rejectionNumber);
    }
    
    private async Task DeleteDetailModal(Guid idDetail, string reqId)
    {
       
        await deleteDetailModalRef!.ShowAsync(idDetail, reqId);
    }
    #endregion
    
    
    #region PaginadoFunciones
    private Task OnOrderSelectedChanged(string nuevoOrden)
    {
        ordenar = nuevoOrden;
        pagina = 1;
        IsLoadingKpis = false;
        StateHasChanged();
        _ = SafeRunAsync(async () => await GetAllRejects(false));

        return Task.CompletedTask;
    }
    
    private Task OnAscendingChanged()
    {
        ascending = !ascending;
        pagina = 1;
        IsLoadingKpis = false;
        StateHasChanged();
        _ = SafeRunAsync(async () => await GetAllRejects(false));

        return Task.CompletedTask;
    }
    private Task OnPaginatedSelectedChanged(int value)
    {
        registrosPorPagina = value;
        pagina = 1;
        IsLoadingKpis = false;
        StateHasChanged();
        _ = SafeRunAsync(async () => await GetAllRejects(false));

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
            await GetAllRejects(false);
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
            await GetAllRejects(false);
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
            await GetAllRejects(false);
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
            await GetAllRejects(false);
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
            await GetAllRejects(false);
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