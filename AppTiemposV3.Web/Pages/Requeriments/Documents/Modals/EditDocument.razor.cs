using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.RequerimentsAttachments;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.Web.Components.UI;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;

namespace AppTiemposV3.Web.Pages.Requeriments.Documents.Modals
{
    public partial class EditDocument : ComponentBase
    {
        #region "Variables" 
        public Guid Id { get; set; }
        public Guid IdReq { get; set; }
        [Inject] private IJSRuntime? JS { get; set; }
        [Inject] private IRequerimentAttachmentContract<RequerimentsAttachmentsDto> DocumentService { get; set; } = null!;
        [Inject] private NavigationManager? Router { get; set; }
        [Inject] private NotificationService Toltip { get; set; } = default!;
        [Inject] private ColorService ColorService { get; set; } = null!;
        private Guid DocumentId { get; set; }

        private Dictionary<string, (string Mensaje, bool EsExitoso)> mensajes = new();

        private List<Etapas> OptionsEtapas = Enum.GetValues(typeof(Etapas))
                                          .Cast<Etapas>()
                                          .Where(e => e != Etapas.None)
                                          .ToList();

        private Etapas? EtapaSeleccionadaNullable = Etapas.None;

        private Etapas EtapaSeleccionada
        {
            get => EtapaSeleccionadaNullable ?? default;
            set => EtapaSeleccionadaNullable = value;
        }

        public bool IsLoadingData { get; set; } = true;
        private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
        private string IdModalFile = Generate(LowercaseLettersAndDigits, 10);

        private DateTime? NewDocumentDate;


        private ElementReference showModalRef;
        private ElementReference closeModalRef;

        private bool isError = false;
        private MarkupString messageError = new("");

        private bool isSuccessReq;
        private MarkupString? messageSuccessReq = new("");
        private bool IsPastDateTime { get; set; } = false;

        private string FileNameFinal { get; set; } = string.Empty;
        private byte[] bytesFile { get; set; } = null!;

        private CreateOrUpdateRequerimentAttachmentDto createDto = new()
        {
            AttachmentAt = DateTime.Now
        };

        private bool IsLoadingEdit = false;
        [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }

        private bool IsSelectClosed = true;

        private Input fileInputRefEdit = null!;

        private string ClassSelect => "font-medium bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 dark:hover:bg-gray-500";
        #endregion

        public async Task ShowAsync(Guid id, Guid idReq)
        {
            Id = id;
            IdReq = idReq;
            createDto.RequerimentId = id;
            await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
            await GetDocument(id);
            StateHasChanged();
        }

        private async Task GetDocument(Guid id)
        {
            IsLoadingData = true;
            StateHasChanged();

            try
            {
                DataResponse<RequerimentsAttachmentsDto> response = await DocumentService.GetRAttachmentById(id);

                if (response.Success)
                {
                    createDto = new CreateOrUpdateRequerimentAttachmentDto
                    {
                        RequerimentId = response.Data.RequerimentId,
                        AttachmentBy = response.Data.AttachmentBy,
                        AttachmentAt = response.Data.AttachmentAt,
                        Etapa = response.Data.Etapa,
                        Descripcion = response.Data.Descripcion
                    };
                    await OnEtapaSelectedChanged(response.Data.Etapa);
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al subir el archivo. {ex.Message}");
                throw;
            }
            finally
            {
                IsLoadingData = false;
                StateHasChanged();
            }
        }

        private async Task SendUploadDocument()
        {
            try
            {
                IsLoadingEdit = true;
                StateHasChanged();
                
                createDto.AttachmentBy = ApplyFormat(createDto.AttachmentBy);

                GeneralResponse response = await DocumentService.UpdateRAttachment(Id, createDto, bytesFile, FileNameFinal);

                if (response.Flag)
                {
                    await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);

                    SavedEventArgs? args = new SavedEventArgs
                    {
                        Message = response.Message,
                        Success = response.Flag,
                        IdResponse = IdReq
                    };

                    await OnSaved.InvokeAsync(args);
                    await GetDocument(Id);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al subir el archivo. {ex.Message}");
                throw;
            }
            finally
            {
                IsLoadingEdit = false;
                StateHasChanged();
            }
        }

        private Task OnEtapaSelectedChanged(Etapas value)
        {
            EtapaSeleccionada = value;
            createDto!.Etapa = value;
            return Task.CompletedTask;
        }

        private async Task CargarArchivo(IBrowserFile file)
        {
            long maxAllowedSize = 1024 * 1024; // 1 MB

            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                await file.OpenReadStream(maxAllowedSize).CopyToAsync(memoryStream);

                bytesFile = memoryStream.ToArray();
                FileNameFinal = file.Name;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CargarArchivo: {ex.Message}");
            }
            finally
            {
                StateHasChanged();
            }
        }


        private string ApplyFormat(string val)
        {
            if (string.IsNullOrWhiteSpace(val) || val.Contains(@"\"))
                return val;

            string[] partes = val.TrimStart().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (partes.Length >= 2)
            {
                string inicial = partes[0][0].ToString().ToUpper();
                string apellido = partes[1];

                return $@"ROJOSOFT\{inicial}{apellido}";
            }

            return val;
        }

        private void ClearInputs()
        {
            createDto = new CreateOrUpdateRequerimentAttachmentDto()
            {
                AttachmentAt = DateTime.Now,
                Etapa = Etapas.None
            };

            OnEtapaSelectedChanged(Etapas.None);

            fileInputRefEdit?.Clear();

            bytesFile = null!;
            FileNameFinal = string.Empty;
            StateHasChanged();
        }
    }
}
