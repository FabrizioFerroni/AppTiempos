using System.Globalization;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.RejectionDetails;
using AppTiemposV3.SharedClases.DTOs.Rejections;
using AppTiemposV3.Web.Pages.Rejections.ModalRejectionDetails;
using AppTiemposV3.Web.Pages.Requeriments.Modals;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static System.Globalization.CultureInfo;

namespace AppTiemposV3.Web.Pages.Rejections.Details;

public partial class Index : ComponentBase, IDisposable
{
    [Parameter] public string urlId { get; set; } = string.Empty;
    
    [Inject] LayoutState State { get; set; } = null!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private IRejectionContract<RejectionResponseDto> RejectionService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    
    private bool IsLoadingData { get; set; } = false;
    private RejectionResponseDto RejectionRes { get; set; } = new RejectionResponseDto();
    
    private record StatusItem(RenderFragment Icon, string Result);
    #region Modales
    private Guid IdModalEdit = Guid.NewGuid();
    private Guid IdModalNew = Guid.NewGuid();
    private ShowRequeriment? showModalRef;
    private NewRejectionDetailModal? newDetailModalRef;
    private EditRejectionDetailModal? editDetailModalRef;
    private DeleteRejectionDetailModal? deleteDetailModalRef;
    
    #endregion

    
    #region Inicializacion

    protected async override Task OnInitializedAsync()
    {
        ColorService.OnColorChanged += HandleColorChanged;
        State.OnSidebarChanged += StateHasChanged;
        
        await State.InitializeAsync();
        await GetRejection();
    }
    #endregion
    
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }
    private void HandleSidebarToggle()
    {
        _ = State.ToggleSidebar();
    }
    
    
    #region Limpiar
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
        State.OnSidebarChanged -= StateHasChanged;
    }
    #endregion
    
   

    
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
    
    
    
    private async Task GetRejection()
    {
        IsLoadingData = true;
        StateHasChanged();
        try
        {
            DataResponse<RejectionResponseDto> rejection = await RejectionService.GetRejectionPorUrl(urlId);

            if (!rejection.Success)
            {
                Nav.NavigateTo("/app/rechazos");
            }
            
            rejection.Data.RejectionsDetails = rejection.Data.RejectionsDetails
                .OrderByDescending(d => d.RechazoNro)
                .ThenByDescending(d => d.SolutionDate.HasValue)
                .ThenByDescending(d => d.RejectionDate)
                .ThenByDescending(d => d.SolutionDate ?? DateOnly.MinValue)
                .ToList();
            
            RejectionRes = rejection.Data;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            IsLoadingData = false;
            StateHasChanged();
        }
    }
    
    
    private async Task HandleDeleted()
    {
        await GetRejection();
    }
    
    private async Task HandleSaved(SavedEventArgs args)
    {
        if (args.Success)
        {
            Toltip.Success("Éxito!", args.Message);
        }
        else
        {
            Toltip.Error("Error", args.Message);
        }
        
        await GetRejection();
        StateHasChanged(); 
    }
    
    
    #region Modales
    private async Task ShowModal(Guid id)
    {
        await showModalRef!.ShowAsync(id);
    }
    
    private async Task NewDetailModal(Guid rejectionId, string reqId)
    {
        await newDetailModalRef!.ShowAsync(IdModalNew, rejectionId, reqId);
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
}