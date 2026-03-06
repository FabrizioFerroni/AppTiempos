using System.Globalization;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.Web.Pages.Activities.Modales;
using AppTiemposV3.Web.Pages.Requeriments.Modals;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using static AppTiemposV3.Web.Utils.Helpers;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Pages.Activities;

public partial class Index : ComponentBase, IDisposable
{
    #region  Variables
    #region InyeccionDependencias
    [Inject] LayoutState State { get; set; } = null!;
    [Inject] private IJSRuntime? JS { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private IRequerimentContract<RequerimentResponseDto> RequerimentsService { get; set; } = null!;
    [Inject] private IActivityContract<ActivityResponseDto>  ActivityService { get; set; } = null!;
    #endregion
    
    private List<int> OptionsPaginated = new() { 1, 5, 10, 15, 50 };

    private List<ActivityResponseDto> Activities = [];
    
    private bool IsLoading = true;
    private bool IsLoadingDelete = false;
    private bool IsLoadingBackend = true;
    
    private int pagina = 1;
    private int registrosPorPagina = 5;
    private string ordenar = "";
    private bool ascending = true;
    
    private string DateSelected = string.Empty;
    private bool IsNewButtonDisabled = false;
    private bool IsNewButtonNavbarDisabled = false;
    private bool IsUpdatingTime = false;

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
    private Guid IdModalEdit = Guid.NewGuid();
    private NewActivity? newModalRef;
    private ShowRequeriment? showModalRef;
    private EditActivity? editModalRef;
    private DeleteActivity? deleteModalRef;
    #endregion
    #endregion
    
    #region Inicializacion

    protected async override Task OnInitializedAsync()
    {
        ColorService.OnColorChanged += HandleColorChanged;
        FechaString();
        State.OnSidebarChanged += StateHasChanged;
        State.OnActivityCreated += HandleActiviyCreated;
        await GetAllActivities(true);
        CheckIfDisableNewButton();
        CheckIfDisableNewButtonNavbar();
        await State.InitializeAsync();
    }
    #endregion
    
    #region Funciones
    private Task OnPaginatedSelectedChanged(int value)
    {
        registrosPorPagina = value;
        pagina = 1;

        _ = SafeRunAsync(() => GetAllActivities(true));

        return Task.CompletedTask;
    }
    
    private async Task HandleActivitySavedNew(SavedEventArgs args)
    {
        try
        {
            pagina = 1;
            StateHasChanged();

            DateTime dateToUse = DateTime.Now;
            try
            {
                if (args.StartDate != null)
                {
                    DateOnly startDate = (DateOnly)args.StartDate;
                    dateToUse = DateTime.Parse(startDate.ToString("yyyy-MM-dd"), CultureInfo.InvariantCulture);
                }
                DateChanged(dateToUse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Parent] ❌ Error en DateChanged: {ex}");
            }

            if (args.Success)
            {
                Toltip.Success("Éxito!", args.Message);
                EnableButtonChanged();
            }
            else
            {
                Toltip.Error("Error", args.Message);
            }

            await GetAllActivities(true);
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Parent] ❌ Error general: {ex}");
        }
    }


    private async Task HandleActivitySaved(SavedEventArgs args)
    {
        pagina = 1;
        StateHasChanged();
        DateOnly startDate = (DateOnly)args.StartDate!;
        DateChanged(DateTime.Parse(startDate.ToString("yyyy-MM-dd"), CultureInfo.InvariantCulture));
        
        
        if (args.Success)
        {
            Toltip.Success("Éxito!", args.Message);
            EnableButtonChanged();
        }
        else
        {
            Toltip.Error("Error", args.Message);
        }
        
        await GetAllActivities(true);
        StateHasChanged(); 
    }
    
    private async Task HandleActivityDeleted(DateOnly startDate)
    {
        pagina = 1;
        DateChanged(DateTime.Parse(startDate.ToString("yyyy-MM-dd"), CultureInfo.InvariantCulture));
        await GetAllActivities(true);
    }

    private async Task GetAllActivities(bool isLoading)
    {
        IsLoading = isLoading;
        IsLoadingBackend = true;
        StateHasChanged();
        
        try
        {
            PaginationDto pagination = new PaginationDto()
            {
                Pagina = pagina,
                RegistrosPorPagina = registrosPorPagina,
                Ordenar = ordenar,
                Ascending = ascending,
                Search = ""
            };
            
            Pageable<List<ActivityResponseDto>> activities =
                await ActivityService.GetAllActivitiesPerDayPag(pagination, DateOnly.Parse(DateSelected));
            
            Activities = activities.Content;
            
            isFirst = activities.First;
            isLast = activities.Last;
            totalPages = activities.TotalPages;
            totalElements = activities.TotalElements;
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
            CheckIfDisableNewButton();
            CheckIfDisableNewButtonNavbar();
            IsLoading = false;
            IsLoadingBackend = true;
            StateHasChanged();
        }
    }
    private void FechaString(DateTime? fecha = null, bool firstRender = true)
    {
        DateTime fechaSeleccionada = fecha ?? DateTime.Now;
        CultureInfo? cultura = new CultureInfo("es-ES"); 
        DateSelected =  fechaSeleccionada.ToString("dddd, dd 'de' MMMM 'de' yyyy", cultura);
        if (!firstRender)
        {
            _ = SafeRunAsync(async () => await GetAllActivities(true));
        }
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
    
    private void HandleActiviyCreated(SavedEventArgs args)
    {
        // Refresca los datos o actualiza el estado del botón
        try
        {
            if (args.StartDate != null)
            {
                DateOnly startDate = (DateOnly)args.StartDate;
                
                InvokeAsync(async () =>
                {
                    pagina = 1;
                    DateChanged(DateTime.Parse(startDate.ToString("yyyy-MM-dd"), CultureInfo.InvariantCulture));
                    await GetAllActivities(true);
                    StateHasChanged();
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Parent] ❌ Error en DateChanged: {ex}");
        }
        
    }

    private void EnableButtonChanged()
    {
        State.NotifyButtonStatusChanged();
    }

    private static string Plural(int count, string singular, string plural) =>
        count == 1 ? singular : plural;
    
    private void DateChanged(DateTime?  date)
    {
         FechaString(date, false);
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

    private void CheckIfDisableNewButton()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        DateOnly selectedDate = DateOnly.Parse(DateSelected);
        IsNewButtonDisabled = selectedDate < today || Activities.Any(a => a.EndTime is null);
    }
    
    private void CheckIfDisableNewButtonNavbar()
    {
        IsNewButtonNavbarDisabled = Activities.Any(a => a.EndTime is null);
    }
    
    private async Task EndTimeAutomate(ActivityResponseDto activity)
    {
        IsUpdatingTime = true;
        StateHasChanged();

        try
        {
            UpdateActivityDto updateDto = new UpdateActivityDto()
            {
                Id = activity.Id,
                StartDate = activity.StartDate,
                StartTime = TimeOnly.Parse(activity.StartTime),
                EndTime = TimeOnly.FromDateTime(DateTime.Now),
                RequerimentId = activity.Requeriment.Id,
                Description = activity.Description,
                IsLoaded = false,
                StatusMessage = "in-progress",
                Comment = activity.Comment,
                Etapa = activity.Etapa
            };

            GeneralResponse response = await ActivityService.UpdateActivity(activity.Id, updateDto);

            if (response.Flag)
            {
                Toltip.Success("Éxito!", response.Message);
                await GetAllActivities(true);
            }
            else
            {
                Toltip.Error("Upss... hubo un error", response.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hubo un error al actualizar el tiempo final automatico, {ex.Message}");
            throw;
        }
        finally
        {
            IsUpdatingTime = false;
            StateHasChanged();
        }
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
            await GetAllActivities(true);
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
            await GetAllActivities(true);
        }
    }

    protected async Task Rewind()
    {
        if (currentPage > 1 && !isFirst)
        {
            currentPage--;
            pagina--;
            UpdatePageStatus();
            await GetAllActivities(true);
        }
    }

    protected async Task Forward()
    {
        if (currentPage < totalPages && !isLast)
        {
            currentPage++;
            pagina++;
            UpdatePageStatus();
            await GetAllActivities(true);
        }
    }

    protected async Task SetPage(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= totalPages)
        {
            currentPage = pageNumber;
            pagina = pageNumber;
            UpdatePageStatus();
            await GetAllActivities(true);
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
    
    #region Limpiar
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
        State.OnSidebarChanged -= StateHasChanged;
        State.OnActivityCreated -= HandleActiviyCreated;
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
    private async Task NewActivity()
    {
        DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);
        await newModalRef!.ShowAsync(IdModal, hoy);
    }
    
    private async Task ShowModal(Guid id)
    {
        await showModalRef!.ShowAsync(id);
    }
    
    private async Task EditModal(Guid idActivity)
    {
        await editModalRef!.ShowAsync(IdModalEdit, idActivity);
    }
    
    private async Task DeleteModal(Guid id, string reqId, DateOnly startDate)
    {
        await deleteModalRef!.ShowAsync(id, reqId, startDate);
    }
    #endregion
}