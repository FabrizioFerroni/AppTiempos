using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Categories;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.Web.Pages.Requeriments.Documents;
using AppTiemposV3.Web.Pages.Requeriments.Modals;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppTiemposV3.Web.Pages.Requeriments;

public partial class Index : ComponentBase,  IDisposable
{
    #region  Variables
    #region InyeccionDependencias
    [Inject] LayoutState State { get; set; } = null!;
    [Inject] private IJSRuntime? JS { get; set; }
    
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private IRequerimentContract<RequerimentResponseDto> RequerimentsService { get; set; } = null!;
    
    [Inject] private ICategoryContract<CategoryResponseDto> CategoriesService { get; set; } = null!;
    #endregion
    
    private List<string> OptionsSearch = new() { "Cliente", "ReqID", "Titulo", "Descripcion", "Categoria", "Conjunto de Cambios" };
    private List<string> OptionsOrders = new() { "Cliente", "ReqID", "Titulo", "Descripcion", "Categoria", "Conjunto de Cambios" };

    private List<int> OptionsPaginated = new() { 1, 5, 10, 15, 50 };

    private List<RequerimentResponseDto> Requeriments = [];
    private string? BuscarPor = "";
    
    private bool IsLoading = true;
    private bool IsLoadingDelete = false;
    private bool IsLoadingBackend = true;
   
    private bool SearchEmpty = false;
    
    private int pagina = 1;
    private int registrosPorPagina = 5;
    private string ordenar = "";
    private bool ascending = true;
    private string? search { get; set; } = string.Empty;
    
    private bool IsSelectClosed = true;
    
    #region Paginado
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
    private Guid IdModal = Guid.NewGuid();
    private NewRequeriment? newModalRef;
    private DocumentModal? docModalRef;
    private ShowRequeriment? showModalRef;
    private EditRequeriment? editModalRef;
    private DeleteRequeriment? deleteModalRef;
    #endregion
    
    private int filesUploaded = 0;
    
    private bool isOpenBuscar = false;
    #endregion

    #region Inicializacion
    protected override async Task OnInitializedAsync()
    {
        ColorService.OnColorChanged += HandleColorChanged;
        await GetAllRequeriments(true);
        State.OnSidebarChanged += StateHasChanged;
        await State.InitializeAsync();
        SearchEmpty = false;
    }
    #endregion
    
    #region Funciones
    private async Task HandleRequerimentDeleted()
    {
        pagina = 1;
        await GetAllRequeriments(false);
    }
    
    private async Task HandleRequerimentSaved(SavedEventArgs args)
    {
        pagina = 1;
        if (args.Success)
        {
            Toltip.Success("Éxito!", args.Message);
        }
        else
        {
            Toltip.Error("Error", args.Message);
        }
        
        await GetAllRequeriments(false);
    }
    
    private void HandleSidebarToggle()
    {
        _ = State.ToggleSidebar();
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }

    private async Task GetAllRequeriments(bool isLoading)
    {
        IsLoading = isLoading;
        StateHasChanged();
        try
        {
            IsLoadingBackend = true;
            PaginationDto pagination = new PaginationDto()
            {
                Pagina = pagina,
                RegistrosPorPagina = registrosPorPagina,
                Ordenar = ordenar,
                Ascending = ascending,
                Search = search
            };

            Pageable<List<RequerimentResponseDto>> requeriments =
                await RequerimentsService.GetAllRequerimentsPag(pagination, BuscarPor!.ToLower());

            
            Requeriments = requeriments.Content;
        
            isFirst = requeriments.First;
            isLast = requeriments.Last;
            totalPages = requeriments.TotalPages;
            totalElements = requeriments.TotalElements;

            SearchEmpty = search is not null && requeriments.Content.Count < 1;
           

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            totalElements = 0;
            totalPages = 0;
            IsLoading = false;
            IsLoadingBackend = false;
            throw;
        }
        finally
        {
            UpdatePageStatus();
            IsLoading = false;
            IsLoadingBackend = false;
            StateHasChanged();
        }
    }

    private Task UpdateSearch()
    {
        BuscarPor = "";
        search = "";
        SearchEmpty = false;
        StateHasChanged();
        _ = SafeRunAsync(async () => await GetAllRequeriments(false));
        return Task.CompletedTask;
    }
    
    private async Task DoSearch(string? query)
    {
        search = query;
        pagina = 1;
        
        await GetAllRequeriments(false);
    }

    private Task OnAscendingChanged()
    {
        ascending = !ascending;
        pagina = 1;
        
        _ = GetAllRequeriments(false);

        return Task.CompletedTask;
    }

    private Task OnOrderSelectedChanged(string nuevoOrden)
    {
        ordenar = nuevoOrden;
        pagina = 1;
        
        _ = GetAllRequeriments(false);

        return Task.CompletedTask;
    }

    private Task OnSearchSelectedChanged(string? value)
    {
        
        BuscarPor = value;
        pagina = 1;
        StateHasChanged();
        return Task.CompletedTask;
    }
    
    private Task OnPaginatedSelectedChanged(int value)
    {
        registrosPorPagina = value;
        pagina = 1;

        _ = SafeRunAsync(() => GetAllRequeriments(false));

        return Task.CompletedTask;
    }
    
    private void HandleDropdownState(bool closed)
    {
        IsSelectClosed = closed;
    }
    #endregion
    
    #region Limpiar
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
        State.OnSidebarChanged -= StateHasChanged;
    }
    #endregion
    
    #region PaginadoFunciones
    protected async Task First()
    {
        if (!isFirst)
        {
            currentPage = 1;
            pagina = 1;
            UpdatePageStatus();
            await GetAllRequeriments(false);
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
            await GetAllRequeriments(false);
        }
    }

    protected async Task Rewind()
    {
        if (currentPage > 1 && !isFirst)
        {
            currentPage--;
            pagina--;
            UpdatePageStatus();
            await GetAllRequeriments(false);
        }
    }

    protected async Task Forward()
    {
        if (currentPage < totalPages && !isLast)
        {
            currentPage++;
            pagina++;
            UpdatePageStatus();
            await GetAllRequeriments(false);
        }
    }

    protected async Task SetPage(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= totalPages)
        {
            currentPage = pageNumber;
            pagina = pageNumber;
            UpdatePageStatus();
            await GetAllRequeriments(false);
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
    
    #region ModalesFunciones
    private async Task NewModal()
    {
        await newModalRef!.ShowAsync(IdModal);
    }

    private async Task DocumentsModal()
    {
        await docModalRef!.ShowAsync(IdModal);
    }
    
    private async Task ShowModal(Guid id)
    {
        await showModalRef!.ShowAsync(id);
    }

    private async Task EditModal(Guid id)
    {
        await editModalRef!.ShowAsync(id);
    }

    private async Task DeleteModal(Guid id, string reqId)
    {
        await deleteModalRef!.ShowAsync(id, reqId);
    }
    #endregion
}