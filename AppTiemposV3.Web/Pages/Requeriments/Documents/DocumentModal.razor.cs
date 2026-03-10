using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.RequerimentsAttachments;
using AppTiemposV3.Web.Components.UI;
using AppTiemposV3.Web.Pages.Requeriments.Documents.Modals;
using AppTiemposV3.Web.Services;
using ChartJs.Blazor.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.GenericModels.Generics;
using static AppTiemposV3.Web.Utils.Helpers;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;

namespace AppTiemposV3.Web.Pages.Requeriments.Documents;

public partial class DocumentModal : ComponentBase
{
    public Guid Id { get; set; }
    public Guid IdReq { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    [Inject] private IRequerimentAttachmentContract<RequerimentsAttachmentsDto> documentService { get; set; } = default!;
    [Inject] private NotificationService Toltip { get; set; } = default!;
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    
    private ElementReference showModalRef;
    private ElementReference closeModalRef;

    private List<RequerimentsAttachmentsDto> respDto = new();

    private int countData = 0;
    private bool IsLoadingData = false; 
    private bool IsDownloadingDocument = false;
    private bool IsDeletingDocument = false;
    private HashSet<Guid> DeletingIds = new();

    #region Modales
    private NewDocument? newModalRef;
    private EditDocument? editModalRef;


    #endregion
    public async Task ShowAsync(Guid id, Guid idReq)
    {
        Id = id;
        IdReq = idReq;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        StateHasChanged();
        await GetAllDocuments(idReq);
    }


    private async Task GetAllDocuments(Guid id)
    {
        IsLoadingData = true;
        StateHasChanged();
        try
        {
            DataAResponse<RequerimentsAttachmentsDto> response = await documentService.GetAllRAttachmentsByRequerimentId(id);

            if (response.Success)
            {
                countData = response.Data.Count();
                respDto = response.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        finally
        {
            IsLoadingData = false;
            StateHasChanged();
        }
    }

    private async Task DownloadFile(string fileName, string base64File)
    {
        IsDownloadingDocument = true;
        StateHasChanged();

        try
        {
            await JS!.InvokeVoidAsync("downloadFileAttachment", fileName, base64File);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error al descargar el archivo: {ex.Message}");
            throw;
        }
        finally
        {
            IsDownloadingDocument = false;
            StateHasChanged();
        }
    }

    private async Task DeleteFile(Guid id, Guid idReq)
    {
        IsDeletingDocument = true;
        DeletingIds.Add(id);
        StateHasChanged();

        try
        {
            GeneralResponse response = await documentService.DeleteRAttachment(id);

            if (response.Flag)
            {
                await GetAllDocuments(idReq);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error borrando el archivo: {ex.Message}");
            throw;
        }
        finally
        {
            IsDeletingDocument = false;
            StateHasChanged();
            DeletingIds.Remove(id);
        }
    }

    private async Task HandleSaved(SavedEventArgs args)
    {
        Guid idReq = args.IdResponse ?? Guid.Empty;

        await GetAllDocuments(idReq);
        StateHasChanged();
    }

    #region Modales
    private async Task EditAttachment(Guid id, Guid idReq)
    {
        await editModalRef!.ShowAsync(id, idReq);
    }

    private async Task NewModal()
    {
        //await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);
        await newModalRef!.ShowAsync(IdReq);
    }
    #endregion
}