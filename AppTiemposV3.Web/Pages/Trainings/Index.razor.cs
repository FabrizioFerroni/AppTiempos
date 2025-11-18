using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.Web.Pages.Requeriments.Modals;
using AppTiemposV3.Web.Pages.Trainings.Modales;
using AppTiemposV3.Web.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static System.Globalization.CultureInfo;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Pages.Trainings;

public partial class Index : ComponentBase, IDisposable
{
    #region Variables
    #region InyeccionDependencias
    [Inject] LayoutState State { get; set; } = null!;
    [Inject] public IJSRuntime? JS { get; set; }
    
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private ITrainingContract<TrainingResponseDto> TrainingService { get; set; } = default!;
    [Inject] private IRequerimentContract<RequerimentResponseDto> RequerimentsService { get; set; } = null!;
    [Inject] private ILocalStorageService _localStorageService { get; set; } = default!;
    private string? theme = "light";
    #endregion
    
    private bool IsSelectClosed = true;
    private List<string> OptionsEstados = new() {"Pendiente", "En Progreso", "Completado"};
    private string? EstadoSeleccionado = "";
    private List<string> OptionsCargados = new() {"Todos", "Tiempo cargado", "Tiempo no cargado"};
    private string? LoadedSeleccionado = "Todos";
    private int activeFiltersCount = 0;
    private bool isOpenFiltros = false;
    private bool isLoadingFiltros = false;
    private string filtroSelected = "";

    private bool IsLoading = false;
    private bool IsLoadingKpis = false;
    private List<TrainingResponseDto> Trainings = [];
    private TrainingKpiResponse kpis = new();
    private bool IsRequerimentsLoaded = false;
    private bool SearchEmpty = false;
    private List<AdvancedFilters>  filters = new();
    private FiltrosTrainingDto filtrosForm = new();
    private bool IsLoadingDelete = false;
    private bool IsLoadingBackend = true;
    
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
    private ShowRequeriment? showModalRef;
    private ShowTrainingModal? showTModalRef;
    private NewTrainingModal? newModalRef;
    private EditTrainingModal? editModalRef;
    private DeleteTrainingModal? deleteModalRef;
    private Guid IdModalEdit = Guid.NewGuid();
    private Guid idModalNew = Guid.NewGuid();
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
        await GetAllTraining();

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
    
    private Task OnEstadoSelectedChanged(string estado)
    {
        EstadoSeleccionado = OnEstadoSelectedChangedBD(estado);
        filtrosForm.Estado = GetStatusName(estado);
        return Task.CompletedTask;
        
    }

    private Task OnLoadedSelectedChanged(string estado)
    {
        LoadedSeleccionado = estado;
        switch (estado)
        {
            case "Tiempo cargado":
                filtrosForm.Loaded = "true";
                break;
            
            case "Tiempo no cargado":
                filtrosForm.Loaded = "false";
                break;
            
            default:
                filtrosForm.Loaded = "";
                break;
        }
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

    private void ShowLoadersLoading(bool show)
    {
        IsLoading = show;
        IsLoadingBackend = show;
        StateHasChanged();
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
        EstadoSeleccionado = "";
        LoadedSeleccionado = "Todos";
        filtrosForm.Loaded = "";
        filtrosForm.Capacitador = "";
        filtrosForm.ReqId = "";
        filters.Clear();   
        activeFiltersCount = 0;
        pagina = 1;
        _ = SetPage(1);
        UpdatePageStatus();
        _ = SafeRunAsync(async () => await GetAllTraining(false));
        StateHasChanged();
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
    
    #region Funciones
    private async Task BuildFilters()
    {
        List<AdvancedFilters> list = new();

        if (!string.IsNullOrWhiteSpace(filtrosForm.Estado))
        {
            list.Add(new AdvancedFilters { Key = "Status", Value = filtrosForm.Estado });
            filtroSelected = "Estado";
        }

        if (!string.IsNullOrWhiteSpace(filtrosForm.Capacitador))
        {
            list.Add(new AdvancedFilters { Key = "Capacitator", Value = filtrosForm.Capacitador });
            filtroSelected = "Capacitador";
        }

        if (!string.IsNullOrWhiteSpace(filtrosForm.Loaded))
        {
            list.Add(new AdvancedFilters { Key = "IsLoaded", Value = filtrosForm.Loaded });
            filtroSelected = "Tiempo cargado";
        }

        if (!string.IsNullOrWhiteSpace(filtrosForm.ReqId))
        {
            DataResponse<Guid> req = await RequerimentsService.GetIdByReqId(filtrosForm.ReqId);
            list.Add(new AdvancedFilters { Key = "ReqId", Value = req.Data.ToString() });
            filtroSelected = "ReqID";
        }

        filters = list;
        
        activeFiltersCount = filters.Count;
    }
    
    #region Modales
    private async Task NewModal()
    {
        await newModalRef!.ShowAsync(idModalNew);
    }
    
    private async Task EditModal(Guid idTraining)
    {
        await editModalRef!.ShowAsync(IdModalEdit, idTraining);
    }
    
    private async Task ShowModal(Guid id)
    {
        await showModalRef!.ShowAsync(id);
    }
    
    private async Task ShowTrainingModal(Guid id)
    {
        await showTModalRef!.ShowAsync(id);
    }
    
    private async Task DeleteModal(Guid id, string reqId)
    {
        await deleteModalRef!.ShowAsync(id, reqId);
    }
    #endregion

    private async Task FilterSearch()
    {
        isLoadingFiltros = true;
        IsLoadingKpis = false;
        pagina = 1;
        _ = SetPage(1);
        UpdatePageStatus();
        StateHasChanged();

        try
        {
            await BuildFilters();    
            pagina = 1;         
            await GetAllTraining(false);
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

    private async Task GetAllTraining(bool isLoadingKpis = true)
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

                Pageable<List<TrainingResponseDto>> response = await TrainingService.GetAllTrainings(pagination);

                Trainings = response.Content;

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
                    DataResponse<TrainingKpiResponse> kpiResponse = await TrainingService.GetTrainingKpi();

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
                return "bg-gray-100 dark:bg-gray-700/50 text-gray-800 dark:text-gray-200"; 
            default:
                return "";
        }
    }
    
    private async Task HandleActivitySaved(SavedEventArgs args)
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
        
        await GetAllTraining();
        StateHasChanged(); 
    }
    
    private async Task HandleActivityDeleted()
    {
        pagina = 1;
        await GetAllTraining();
    }
    #endregion
    
    #region PaginadoFunciones
    private Task OnOrderSelectedChanged(string nuevoOrden)
    {
        ordenar = nuevoOrden;
        pagina = 1;
        IsLoadingKpis = false;
        StateHasChanged();
        _ = SafeRunAsync(async () => await GetAllTraining(false));

        return Task.CompletedTask;
    }
    
    private Task OnAscendingChanged()
    {
        ascending = !ascending;
        pagina = 1;
        IsLoadingKpis = false;
        StateHasChanged();
        _ = SafeRunAsync(async () => await GetAllTraining(false));

        return Task.CompletedTask;
    }
    private Task OnPaginatedSelectedChanged(int value)
    {
        registrosPorPagina = value;
        pagina = 1;
        IsLoadingKpis = false;
        StateHasChanged();
        _ = SafeRunAsync(async () => await GetAllTraining(false));

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
            await GetAllTraining(false);
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
            await GetAllTraining(false);
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
            await GetAllTraining(false);
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
            await GetAllTraining(false);
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
            await GetAllTraining(false);
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