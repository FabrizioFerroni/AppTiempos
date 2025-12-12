using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Invitations;
using AppTiemposV3.SharedClases.DTOs.Rejections;
using AppTiemposV3.Web.Pages.Invitations.Modales;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using static System.Globalization.CultureInfo;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;
using Microsoft.JSInterop;

namespace AppTiemposV3.Web.Pages.Invitations;

public partial class Index : ComponentBase, IDisposable
{
    #region  Variables
    #region InyeccionDependencias
    [Inject] LayoutState State { get; set; } = null!;
    [Inject] private IJSRuntime? JS { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private IInvitationContract<InvitationResponseDto> InvitationService { get; set; } = default!;
    #endregion
    
    private bool IsLoading = true;
    private InvitationFilterDto filterDto = new InvitationFilterDto();
    
    private DateTime currentDate = DateTime.Today;
    private string weekRange = string.Empty;
    private  bool isNextDisabled = false;
    private bool IsSelectClosed = true;
    private int weekNumberSelected = 45;
    
    private List<DateTime> WeekDays = new();
    
    private bool isOpenFiltros = false;
    private bool isLoadingFiltros = false;
    private int activeFiltersCount = 0;
    private string? search { get; set; } = string.Empty;
    private bool SearchEmpty = false;
    private List<AdvancedFilters>  filters = new();
    private string filtroSelected = string.Empty;
    private Guid IdModalNew = Guid.NewGuid();
    private bool IsLoadingBackend = false;
    private List<InvitationResponseDto> Invitations = [];
    
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

    #region Modales
    private NewInvitationAdminModal? newModalRef;
    private DetailsInvitationModal? detailModalRef;
    

    #endregion
    #endregion
    
    #region Inicializacion

    protected async override Task OnInitializedAsync()
    {
        ColorService.OnColorChanged += HandleColorChanged;
        State.OnSidebarChanged += StateHasChanged;
        await State.InitializeAsync();
        await GetAllInvitations();
    }
    #endregion

    #region Properties
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
        
        await GetAllInvitations();
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
        filterDto.NameOrEmail = "";
        filterDto.DateReceivedFrom = null;
        filterDto.DateReceivedTo = null;
        filters.Clear();   
        pagina = 1;
        _ = SetPage(1);
        await DoSearch(null);
        activeFiltersCount = 0;
        await GetAllInvitations();
        StateHasChanged();
    }
    #endregion

    #region Filtrado

    private Task DoSearch(string? query)
    {
        search = query;

        if (query is not null && query.Contains('@'))
        {
            filterDto.TypeNameOrEmail = "Email";
        }
        else
        {
            filterDto.TypeNameOrEmail = "FullName";
        }
        
        filterDto.NameOrEmail = query;
        return Task.CompletedTask;
    }
    
    private void DateChangedFrom(DateTime?  date)
    {
        if (date.HasValue)
        {
            filterDto.DateReceivedFrom = date;
        }
        else
        {
            filterDto.DateReceivedFrom = new DateTime();
        }
    }
    
    private void DateChangedTo(DateTime?  date)
    {
        if (date.HasValue)
        {
            filterDto.DateReceivedTo = date;
        }
        else
        {
            filterDto.DateReceivedTo = new DateTime();
        }
        
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
            await GetAllInvitations();
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

        if (!string.IsNullOrWhiteSpace(filterDto.NameOrEmail))
        {
            list.Add(new AdvancedFilters{ Key = filterDto.TypeNameOrEmail, Value = filterDto.NameOrEmail });
            filtroSelected = "Nombre O Email";
        }
        
        bool hasDateFilterFrom = filterDto.DateReceivedFrom.HasValue;
        if (hasDateFilterFrom)
        {
            list.Add(new AdvancedFilters{ Key = "DateReceived", Value = filterDto.DateReceivedFrom.ToString() });
            filtroSelected = "Fecha desde";
        }
        
        bool hasDateFilterTo = filterDto.DateReceivedTo.HasValue;
        if (hasDateFilterTo)
        {
            list.Add(new AdvancedFilters{ Key = "DateReceived", Value = filterDto.DateReceivedTo.ToString() });
            filtroSelected = "Fecha hasta";
        }

        filters = list;
        
        activeFiltersCount = filters.Count;
    }
    
    
    private async Task GetAllInvitations()
    {
        ShowLoadersLoading(true);
        StateHasChanged();

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

            Pageable<List<InvitationResponseDto>> response = await InvitationService.GetAllInvitations(pagination);


            Invitations = response.Content;

            isFirst = response.First;
            isLast = response.Last;
            totalPages = response.TotalPages;
            totalElements = response.TotalElements;

            SearchEmpty = pagination.Filters is not null && response.Content.Count < 1;

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
    
    #region Paginado
    private Task OnPaginatedSelectedChanged(int value)
    {
        registrosPorPagina = value;
        pagina = 1;
        StateHasChanged();
        _ = SafeRunAsync(async () => await GetAllInvitations());

        return Task.CompletedTask;
    }
    
    private Task OnOrderSelectedChanged(string nuevoOrden)
    {
        ordenar = nuevoOrden;
        pagina = 1;
        StateHasChanged();
        _ = SafeRunAsync(async () => await GetAllInvitations());

        return Task.CompletedTask;
    }
    
    private Task OnAscendingChanged()
    {
        ascending = !ascending;
        pagina = 1;
        StateHasChanged();
        _ = SafeRunAsync(async () => await GetAllInvitations());

        return Task.CompletedTask;
    }
    
    protected async Task First()
    {
        if (!isFirst)
        {
            currentPage = 1;
            pagina = 1;
            UpdatePageStatus();
            StateHasChanged();
            await GetAllInvitations();
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
            StateHasChanged();
            await GetAllInvitations();
        }
    }

    protected async Task Rewind()
    {
        if (currentPage > 1 && !isFirst)
        {
            currentPage--;
            pagina--;
            UpdatePageStatus();
            StateHasChanged();
            await GetAllInvitations();
        }
    }

    protected async Task Forward()
    {
        if (currentPage < totalPages && !isLast)
        {
            currentPage++;
            pagina++;
            UpdatePageStatus();
            StateHasChanged();
            await GetAllInvitations();
        }
    }

    protected async Task SetPage(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= totalPages)
        {
            currentPage = pageNumber;
            pagina = pageNumber;
            UpdatePageStatus();
            StateHasChanged();
            await GetAllInvitations();
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

    #region Modales
    private async Task NewModal()
    {
        await newModalRef!.ShowAsync(IdModalNew);
    }
    
    private async Task ShowModal(Guid id)
    {
        await detailModalRef!.ShowAsync(id);
    }
    #endregion
}